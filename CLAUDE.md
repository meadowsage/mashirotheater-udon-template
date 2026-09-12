# 実装ガイドライン（AI エージェント向け）

このリポジトリは、VRChat ワールド用の Udon ギミック（UdonSharp）を置く場所です。
Unity プロジェクトの `Assets/` 直下に clone されており、1 ギミック = 1 フォルダで管理します。
利用者は 3D モデラーなどの非エンジニアです。説明は日本語で、専門用語には一言の補足を添えてください。

## 絶対ルール（これだけは守る）

1. **AI が直接書くのは `.cs` と `.md` だけです。**
   `.meta` / `.asset` / `.prefab` / `.unity` / `.mat` / `.anim` などの Unity アセットを YAML として手書き・編集しないでください。
   Prefab や UI 階層が必要なときは、下記「Prefab・雛形を生成する Editor 拡張」のとおり、Unity 上で生成するコードを書きます。
2. **新しい `.cs` を作ったら、利用者に「Unity に戻って Add Component してください」と伝えてください。**
   Unity が `.cs` を読み込むと、このリポジトリの Editor 拡張が同じフォルダに同名の ProgramAsset（`.asset`）を自動生成します。
   `.cs` と同じ名前の `.asset` と、各ファイルの `.meta` が生成されたことを確認してもらってください。
3. **コミットには Unity が生成した `.meta` と `.asset` を必ず含めてください。**
   `.meta` が無いと他の環境で参照が切れ、`.asset` が無いと Play モードで Unity がクラッシュします。
4. **`git add -A` / `git commit -a` は使わないでください。** 対象ファイルを明示的に `git add` します。
5. **コンパイルエラーは利用者が Unity の Console から貼り付けてくれます。** 修正は `.cs` の範囲で行ってください。

## 新規ギミックの作り方（手順）

利用者から「〜するギミックを作って」と依頼されたら、次の順で進めてください。

1. ギミック名（英語 PascalCase）と、動作の要点を 1〜3 行で確認する。曖昧な点は最初にまとめて質問する。
2. リポジトリ直下に `<ギミック名>/` フォルダを作る。
3. `<ギミック名>/<ギミック名>.cs` を作る。書き方は下記「コーディング規約」と「コードの雛形」に従う。
4. Prefab や UI 階層、複数オブジェクトの配線が必要なら、`<ギミック名>/Editor/<ギミック名>Editor.cs` に雛形を生成する Editor 拡張を作る（下記「Prefab・雛形を生成する Editor 拡張」）。1 つの GameObject にコンポーネントを付けるだけで済むギミックには不要。
5. `<ギミック名>/README.md` を作る。構成は下記「README の書き方」に従う。
6. 利用者に次の 3 つを伝える。
   - Unity に戻り、対象 GameObject に `Add Component > <名前空間> > <ギミック名>` でコンポーネントを追加する（Editor 拡張を作った場合は、その生成ボタンを押す）
   - Console に赤いエラーが出ていたら、その文面をそのまま貼り付けてもらう
   - 動作確認後、`.cs` / `README.md` と、Unity が生成した `.meta` / `.asset` をまとめてコミットする

## コーディング規約

- 名前空間は **利用者の GitHub ユーザー名** から作る。`git remote -v` に出る `github.com/<owner>/...` の `<owner>` を、英数字以外を取り除いて先頭を大文字にしたもの（例: `vivi` → `Vivi`、`meadow-sage` → `MeadowSage`）。数字で始まる場合は先頭に `U` を付ける。判断に迷ったら利用者に確認する。
- `UdonSharpBehaviour` を継承する。
- `[UdonBehaviourSyncMode(...)]` を**必ず明示**する。同期変数が無ければ `NoVariableSync`、`[UdonSynced]` を使うなら原則 `Manual`。
- `[AddComponentMenu("<名前空間>/<クラス名を空白区切りにしたもの>")]` を付け、Add Component メニューから探せるようにする（例: `"Vivi/Auto Door"`）。
- クラス名は PascalCase（例: `AutoDoor`）。プレフィックスは付けない。private フィールド・メソッドは camelCase。
- インスペクタに出すフィールドは `[SerializeField]` + `[Tooltip("日本語の説明")]` を付け、`[Header("...")]` でグループ化する。public フィールドで露出させない。
- コメント・XML ドキュメント（`/// <summary>`）は日本語。「何をしているか」ではなく「なぜそうしているか」を書く。
- 1 ファイル 1 クラス。ファイル名とクラス名を一致させる。

## コードの雛形

```csharp
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Vivi
{
    /// <summary>
    /// 近づくと自動で開き、離れると閉まるドア。開閉は全員に同期する。
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [AddComponentMenu("Vivi/Auto Door")]
    public class AutoDoor : UdonSharpBehaviour
    {
        [Header("対象設定")]
        [SerializeField, Tooltip("開閉するドアの GameObject。")]
        private GameObject door;

        [UdonSynced] private bool isOpen;

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            // 同期変数を書き換えられるのは所有者だけなので、先に所有権を取る。
            if (!player.isLocal) return;
            Networking.SetOwner(player, gameObject);
            isOpen = true;
            Apply();
            RequestSerialization();
        }

        public override void OnDeserialization()
        {
            Apply();
        }

        private void Apply()
        {
            if (door != null) door.SetActive(!isOpen);
        }
    }
}
```

## Prefab・雛形を生成する Editor 拡張

原則として、次のいずれかに当てはまるギミックには、Unity 上で雛形を生成する Editor 拡張を併せて作ってください。

- Canvas・ボタン・テキストなどの UI が要る
- 複数の GameObject を決まった階層で並べる必要がある
- 複数のコンポーネント間の参照を配線する必要がある

作り方の要点:

- 置き場所は `<ギミック名>/Editor/<ギミック名>Editor.cs`。ファイル全体を `#if !COMPILER_UDONSHARP && UNITY_EDITOR` 〜 `#endif` で囲む。
- ギミック本体のカスタムインスペクタ（`[CustomEditor(typeof(<ギミック名>))]` の `Editor` 派生）にして、「構成を生成」ボタンを置く。`OnInspectorGUI` では先に `UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)` を呼び、続けて `DrawDefaultInspector()` で通常の項目を出す。
- 生成は必ず Unity の API で行う。`new GameObject(...)`、`UdonSharpUndo.AddComponent<T>(gameObject)`、参照の書き込みは `SerializedObject` で行い `UdonSharpEditorUtility.CopyProxyToUdon(behaviour)` で反映、Prefab 化は `PrefabUtility.SaveAsPrefabAsset(go, path)`。`.prefab` の YAML を手書きしない（proxy と UdonBehaviour の対応が壊れ、Play 時にクラッシュする）。
- Prefab の保存先はギミックのフォルダ内（例: `<ギミック名>/<ギミック名>.prefab`）。同名があれば確認ダイアログを出してから作り直す。
- 生成物は「構造と配線が正しい状態」までを担保する。サイズ・色・フォントなどの見た目は利用者が Unity 上で調整する前提で、決め打ちの値で構わない。
- README の「セットアップ」に、どのボタンを押すと何が生成されるかを書く。

## UdonSharp で使えないもの（AI がよく間違える）

UdonSharp は C# のサブセットです。次は**使えません**。代わりに右の書き方をしてください。

| 使えない | 代わりに |
| --- | --- |
| `List<T>` / `Dictionary<K,V>` などのジェネリックコレクション | 配列（`T[]`）と件数カウンタ |
| LINQ（`.Where` / `.Select` など）、ラムダ式 | `for` ループ |
| `try` / `catch` | `null` チェックで防ぐ |
| インターフェース、ジェネリッククラスの定義、ジェネリックメソッドの定義 | 具象クラスに分ける |
| `static` フィールド（`const` は可） | インスタンスフィールド |
| `Instantiate` の多用、`new GameObject()` | 事前にシーンへ配置し、`SetActive` で切り替える |

覚えておく挙動:

- 同期変数は `[UdonSynced]` を付け、**所有者（Owner）だけが書き換え**られます。書き換える前に `Networking.SetOwner(Networking.LocalPlayer, gameObject)` で所有権を取り、書き換えた後に `RequestSerialization()` を呼びます。受信側は `OnDeserialization()` で反映します。
- 全員に「イベント」を飛ばすときは `SendCustomNetworkEvent(NetworkEventTarget.All, nameof(メソッド名))`。呼ばれるメソッドは `public` にします。
- プレイヤーが押すボタンは `public override void Interact()` で受け取ります。GameObject に `Collider` が必要です。
- `Networking.LocalPlayer` はエディタ上で `null` になることがあります。使う前に `null` チェックします。
- 他のコンポーネントは `GetComponent<T>()` で取得できます。VRChat SDK 型は `(VRCObjectSync)GetComponent(typeof(VRCObjectSync))` の形も使えます。

## README の書き方

各ギミックのフォルダに `README.md` を置きます。利用者向けマニュアルに限定し、設計の経緯や不採用案は書きません。次の構成にしてください。

1. `# Mashiro <ギミック名>` と 1〜2 行の概要
2. `## できること` 箇条書き
3. `## セットアップ` Unity での配線手順（Add Component、インスペクタ設定、必要な Collider など）
4. `## インスペクタ項目` 表（項目 / 説明）
5. `## API リファレンス` public メソッドの表（メソッド / 用途 / 呼び出し種別）。
   「UI Button や他 Udon から配線するもの」と「内部連携用で配線不要のもの」を区別して書く
6. `## 注意点`

public メソッドを追加・変更・削除したときは、同じコミットで API リファレンスも更新します。

## 無駄な消費を抑える（目安）

利用者の AI 利用枠は小さいことがあります。次を目安に、必要以上に読み書きしないでください。厳密な制限ではありません。

- 作業前にリポジトリ全体を探索しない。まず読むのは CLAUDE.md だけで足ります。必要になったら他のファイルを読んでください。リポジトリ直下の `Editor/` はテンプレート付属の補助スクリプトなので触りません。
- コンパイルやテストは試みない。クラウド環境に Unity は無く、コンパイルは利用者が Unity で行います。
- 説明は要点だけにする。書いたコードをチャットに貼り直さない。
- README は利用者が読む分だけ書く（目安 40 行以内）。

## UI 文言

Tooltip やヘルプボックスなど利用者の目に触れる文言は、「利用者が次に取る行動を助けるか」で判断し、助けにならない文は書きません。内部挙動の説明はコードコメントや README に書きます。

## 言語（日本語を使う）

人が読む文章はすべて日本語で書いてください。

- コードのコメント、XML ドキュメント、Tooltip、README
- 利用者への説明・質問
- **PR のタイトル・本文**、PR へのコメント、レビューの指摘
- コミットメッセージの本文（prefix の `feat:` / `fix:` / `docs:` などは英語のままで構いません）

識別子・ファイル名・技術用語・コードスニペットは英語のままで構いません。
