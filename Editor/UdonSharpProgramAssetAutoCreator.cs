using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MashiroTheater.UdonTemplate
{
    /// <summary>
    /// UdonSharpBehaviour の .cs が読み込まれたとき、同じフォルダに同名の UdonSharp Program Asset が
    /// 無ければ自動で作る。AI がブラウザ上で .cs だけを作る運用のため、利用者が Unity 上で
    /// Program Asset を手作りしなくて済むようにしている。
    ///
    /// UdonSharp の型はリフレクションで参照し、アセンブリ定義への依存を持たない
    /// （UdonSharp が未導入のプロジェクトでもコンパイルエラーにならない）。
    /// </summary>
    public class UdonSharpProgramAssetAutoCreator : AssetPostprocessor
    {
        private const string BehaviourTypeName = "UdonSharp.UdonSharpBehaviour, UdonSharp.Runtime";
        private const string ProgramAssetTypeName = "UdonSharp.UdonSharpProgramAsset, UdonSharp.Editor";

        private static void OnPostprocessAllAssets(
            string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths,
            bool didDomainReload)
        {
            var behaviourType = Type.GetType(BehaviourTypeName);
            var programAssetType = Type.GetType(ProgramAssetTypeName);
            if (behaviourType == null || programAssetType == null) return;

            // このリポジトリ（この Editor スクリプトの親フォルダ）配下だけを対象にし、
            // 同じプロジェクト内の他のアセットには手を出さない。
            string root = FindRepositoryRoot();
            if (string.IsNullOrEmpty(root)) return;

            bool created = false;
            foreach (string path in importedAssets)
            {
                if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) continue;
                if (!path.StartsWith(root + "/", StringComparison.Ordinal)) continue;
                if (path.Contains("/Editor/")) continue;

                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script == null) continue;

                // クラス名とファイル名が一致しない、コンパイルエラー中などは GetClass が null になるので見送る。
                var type = script.GetClass();
                if (type == null || type.IsAbstract || !behaviourType.IsAssignableFrom(type)) continue;

                string assetPath = Path.ChangeExtension(path, ".asset");
                if (File.Exists(assetPath)) continue;

                var asset = ScriptableObject.CreateInstance(programAssetType);
                var field = programAssetType.GetField("sourceCsScript", BindingFlags.Public | BindingFlags.Instance);
                if (field == null)
                {
                    UnityEngine.Object.DestroyImmediate(asset);
                    Debug.LogWarning("[UdonSharpProgramAssetAutoCreator] UdonSharpProgramAsset の構造が想定と異なるため、自動生成を中止しました。");
                    return;
                }
                field.SetValue(asset, script);
                AssetDatabase.CreateAsset(asset, assetPath);
                created = true;
                Debug.Log("[UdonSharpProgramAssetAutoCreator] Program Asset を作成しました: " + assetPath, asset);
            }

            if (!created) return;
            AssetDatabase.SaveAssets();
            // 作った直後にコンパイルしておくと、Add Component の時点で使える状態になる。
            // メソッドが見つからない場合でも、Play 開始時に UdonSharp が自動でコンパイルするので致命的ではない。
            EditorApplication.delayCall += () => TryCompileAll(programAssetType);
        }

        private static string FindRepositoryRoot()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:MonoScript " + nameof(UdonSharpProgramAssetAutoCreator)))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) != nameof(UdonSharpProgramAssetAutoCreator)) continue;
                string editorDir = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(editorDir)) return null;
                return Path.GetDirectoryName(editorDir)?.Replace('\\', '/');
            }
            return null;
        }

        private static void TryCompileAll(Type programAssetType)
        {
            try
            {
                var method = programAssetType.GetMethod("CompileAllCsPrograms", BindingFlags.Public | BindingFlags.Static);
                if (method == null) return;
                var parameters = method.GetParameters();
                var args = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    args[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue : null;
                }
                method.Invoke(null, args);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[UdonSharpProgramAssetAutoCreator] 自動コンパイルに失敗しました。Play 開始時に再コンパイルされます: " + e.Message);
            }
        }
    }
}
