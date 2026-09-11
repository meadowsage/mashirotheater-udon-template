# Udon ギミック勉強会テンプレート

VRChat ワールド用の Udon ギミック（UdonSharp）を、AI コーディングツール（Claude Code / Codex）と一緒に作るためのテンプレートリポジトリです。
Unity プロジェクトの `Assets/` 直下に clone して使います。

## 事前準備（勉強会の前に済ませておくこと）

| 項目 | 内容 |
| --- | --- |
| GitHub アカウント | https://github.com/ で作成 |
| GitHub Desktop | https://desktop.github.com/ をインストールし、GitHub アカウントでサインイン |
| Unity プロジェクト | VRChat Creator Companion（VCC）で **World** テンプレートから新規プロジェクトを作成し、一度開けることを確認 |
| AI コーディングツール | Claude Code（デスクトップアプリ または VS Code 拡張）か、ChatGPT Codex のどちらか。有料プランへの加入が必要 |

## セットアップ

1. このページ右上の **Use this template → Create a new repository** で、自分のアカウントにリポジトリを作ります（名前は自由。Public / Private どちらでも可）。
2. GitHub Desktop で **File → Clone repository** から作ったリポジトリを選び、**Local path** に Unity プロジェクトの `Assets` フォルダを指定して clone します。
   例: `C:\Users\you\MyWorld\Assets\my-udon-gimmicks`
3. Unity でプロジェクトを開きます。Project ウィンドウに clone したフォルダが見え、Console にエラーが出ていなければ準備完了です。
4. GitHub Desktop の Changes に `.meta` ファイルが並んでいたら、そのまま **Commit to main → Push origin** してください（Unity が生成した `.meta` は必ずコミットします）。

## お手本を動かしてみる

`Samples/SyncedToggleButton/` に、押すと対象の表示 / 非表示が切り替わり全員に同期されるボタンが入っています。

1. Hierarchy で Cube を作り、`Add Component > MashiroTheater > Mashiro Synced Toggle Button` を追加します。
2. インスペクタの **Target** に、切り替えたい別の GameObject を入れます。
3. Play ボタンを押し（ClientSim が起動します）、Cube に近づいて Interact（キーボードなら E）すると Target が消えたり出たりします。

詳しい説明は [`Samples/SyncedToggleButton/README.md`](Samples/SyncedToggleButton/README.md) を見てください。

## AI に新しいギミックを作らせる

1. AI ツールを、**clone したフォルダ**（`Assets/<リポジトリ名>`）を開いた状態で起動します。
   - Claude Code: デスクトップアプリでそのフォルダを開くか、ターミナルでそのフォルダに移動して `claude` を実行
   - Codex: 同様にそのフォルダで起動
2. 作りたいものを日本語で伝えます。例:
   > 近づくと自動で開くドアを作って。開閉は全員に同期してほしい。
3. AI が `<ギミック名>/Mashiro<ギミック名>.cs` と `README.md` を作ります。**AI は `.cs` と `.md` しか作りません。**
4. Unity に戻り、README の手順どおり `Add Component` します。この時点で Unity が `.meta` と `.asset`（UdonSharp の ProgramAsset）を自動生成します。
5. Console に赤いエラーが出たら、その文面をコピーして AI に貼り付けます。AI が `.cs` を直すので、Unity に戻って再確認します。
6. 動いたら GitHub Desktop で **Commit → Push** します。Changes に出ている `.cs` / `.md` / `.meta` / `.asset` を全部含めてください。

Claude Code では `/new-gimmick` と入力すると、手順 2〜3 を対話形式で進められます。

## フォルダ構成

```
<リポジトリ>/
├── CLAUDE.md          AI 向けの規約（Claude Code が読む）
├── AGENTS.md          同上（Codex が読む。中身は CLAUDE.md を参照するだけ）
├── Samples/           お手本ギミック
│   └── SyncedToggleButton/
├── <ギミック名>/       自分で作ったギミック（1 ギミック = 1 フォルダ）
│   ├── Mashiro<ギミック名>.cs
│   ├── Mashiro<ギミック名>.asset   Unity が自動生成（コミットする）
│   └── README.md
└── .claude/           Claude Code の設定とスキル
```

## Git の最低限

| やりたいこと | GitHub Desktop の操作 |
| --- | --- |
| 変更を記録する | Changes で対象にチェック → Summary を書く → **Commit to main** |
| GitHub に送る | **Push origin** |
| 他の PC で続きをやる | そちらでも同じ手順で clone → 作業前に **Fetch origin / Pull** |

コミットに含めるもの: `.cs`、`.md`、`.meta`、`.asset`。
含めないもの: Unity プロジェクト側のファイル（`Library/` など。clone したフォルダの外なので通常は出てきません）。

## トラブルシュート

| 症状 | 原因と対処 |
| --- | --- |
| Play を押した瞬間に Unity が落ちる | `.cs` と同名の `.asset` が無い。Unity 上で一度 `Add Component` し直し、生成された `.asset` をコミットする |
| Add Component のメニューに出てこない | Console にコンパイルエラーが出ている。エラー文を AI に貼る |
| 別の PC で開いたら参照が外れている | `.meta` をコミットし忘れている。元の PC で `.meta` をコミット・Push する |
| Interact しても反応しない | GameObject に `Collider` が無い。Box Collider などを追加する |
| ClientSim で同期の動作を確かめたい | VRChat SDK → Build & Test で複数クライアントを起動できる |

## ライセンス

MIT License（[`LICENSE.md`](LICENSE.md)）。
