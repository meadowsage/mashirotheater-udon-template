# 実装ガイドライン（AI エージェント向け）

このリポジトリは、VRChat ワールド用の Udon ギミック（UdonSharp）を置く場所です。
Unity プロジェクトの `Assets/` 直下に clone されており、1 ギミック = 1 フォルダで管理します。
利用者は 3D モデラーなどの非エンジニアです。説明は日本語で、専門用語には一言の補足を添えてください。

## 絶対ルール（これだけは守る）

1. **AI が作成・編集してよいのは `.cs` と `.md` だけです。**
   `.meta` / `.asset` / `.prefab` / `.unity` / `.mat` / `.anim` などの Unity アセットは作成も編集もしないでください。
   これらは Unity が生成・管理します。
2. **新しい `.cs` を作ったら、利用者に「Unity に戻って Add Component してください」と伝えてください。**
   UdonSharp は Add Component した時点で ProgramAsset（`.asset`）を自動生成します。
   `.cs` と同じ名前の `.asset` と、各ファイルの `.meta` が生成されたことを確認してもらってください。
3. **コミットには Unity が生成した `.meta` と `.asset` を必ず含めてください。**
   `.meta` が無いと他の環境で参照が切れ、`.asset` が無いと Play モードで Unity がクラッシュします。
4. **`git add -A` / `git commit -a` は使わないでください。** 対象ファイルを明示的に `git add` します。
5. **コンパイルエラーは利用者が Unity の Console から貼り付けてくれます。** 修正は `.cs` の範囲で行ってください。

## 新規ギミックの作り方（手順）

利用者から「〜するギミックを作って」と依頼されたら、次の順で進めてください。

1. ギミック名（英語 PascalCase）と、動作の要点を 1〜3 行で確認する。曖昧な点は最初にまとめて質問する。
2. リポジトリ直下に `<ギミック名>/` フォルダを作る（`Samples/` の中には作らない）。
3. `<ギミック名>/Mashiro<ギミック名>.cs` を作る。書き方は下記「コーディング規約」と `Samples/SyncedToggleButton/` を手本にする。
4. `<ギミック名>/README.md` を作る。構成は下記「README の書き方」に従う。
5. 利用者に次の 3 つを伝える。
   - Unity に戻り、対象 GameObject に `Add Component > MashiroTheater > ...` でコンポーネントを追加する
   - Console に赤いエラーが出ていたら、その文面をそのまま貼り付けてもらう
   - 動作確認後、`.cs` / `README.md` と、Unity が生成した `.meta` / `.asset` をまとめてコミットする

## コーディング規約

- 名前空間は `namespace MashiroTheater`。
- `UdonSharpBehaviour` を継承する。
- `[UdonBehaviourSyncMode(...)]` を**必ず明示**する。同期変数が無ければ `NoVariableSync`、`[UdonSynced]` を使うなら原則 `Manual`。
- `[AddComponentMenu("MashiroTheater/Mashiro Xxx Yyy")]` を付け、Add Component メニューから探せるようにする。
- クラス名は `Mashiro` で始める PascalCase。private フィールド・メソッドは camelCase。
- インスペクタに出すフィールドは `[SerializeField]` + `[Tooltip("日本語の説明")]` を付け、`[Header("...")]` でグループ化する。public フィールドで露出させない。
- コメント・XML ドキュメント（`/// <summary>`）は日本語。「何をしているか」ではなく「なぜそうしているか」を書く。
- ファイル先頭に MIT ライセンスヘッダを付ける（`Samples/SyncedToggleButton/MashiroSyncedToggleButton.cs` の先頭と同じ書式）。
- 1 ファイル 1 クラス。ファイル名とクラス名を一致させる。

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

- 作業前にリポジトリ全体を探索しない。まず読むのは CLAUDE.md と `Samples/SyncedToggleButton/MashiroSyncedToggleButton.cs` で足ります。必要になったら他のファイルを読んでください。
- コンパイルやテストは試みない。クラウド環境に Unity は無く、コンパイルは利用者が Unity で行います。
- 説明は要点だけにする。書いたコードをチャットに貼り直さない。
- README は利用者が読む分だけ書く（目安 40 行以内）。

## UI 文言

Tooltip やヘルプボックスなど利用者の目に触れる文言は、「利用者が次に取る行動を助けるか」で判断し、助けにならない文は書きません。内部挙動の説明はコードコメントや README に書きます。

## 言語

コメント、README、利用者への説明はすべて日本語です。識別子・技術用語・コミットメッセージの prefix（`feat:` など）は英語で構いません。
