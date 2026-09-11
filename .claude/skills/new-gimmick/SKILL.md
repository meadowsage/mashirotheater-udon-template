---
name: new-gimmick
description: 新しい Udon ギミック（UdonSharp）を 1 フォルダ分作る。利用者が「〜するギミックを作りたい」と言ったとき、または /new-gimmick と入力したときに使う。
---

# 新規ギミックの作成

`CLAUDE.md` の「新規ギミックの作り方」に従って進めます。引数に説明があればそれを出発点にし、無ければ最初に聞きます。

## 手順

1. **要件を確認する。** 次の 3 点を、利用者の言葉から埋める。足りない項目だけ、まとめて 1 回で質問する。
   - 何が起きるか（例: 近づくとドアが開く）
   - 誰に見えるか（自分だけ = ローカル / 全員 = 同期）
   - きっかけ（Interact で押す / エリアに入る / 常時）
2. **ギミック名を決める。** 英語 PascalCase（例: `AutoDoor`）。利用者に一言で確認する。
3. **フォルダと `.cs` を作る。** `<ギミック名>/Mashiro<ギミック名>.cs`。
   `Samples/SyncedToggleButton/MashiroSyncedToggleButton.cs` を先に読み、ヘッダ・属性・書式をそろえる。
   同期が必要なら `[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]` と `[UdonSynced]`、不要なら `NoVariableSync`。
4. **`README.md` を作る。** `CLAUDE.md` の「README の書き方」の構成で、`Samples/SyncedToggleButton/README.md` と同じ粒度にする。
5. **利用者に次の操作を伝える。** 短く、この順で。
   1. Unity に戻り、対象 GameObject に `Add Component > MashiroTheater > Mashiro <ギミック名>` を追加する
   2. インスペクタの各項目を設定する（README の「セットアップ」を指す）
   3. Console に赤いエラーがあれば、その文面を貼り付けてもらう
   4. 動いたら、`.cs` / `README.md` と Unity が生成した `.meta` / `.asset` をまとめてコミットする

## やってはいけないこと

- `.meta` / `.asset` / `.prefab` / `.unity` を作る・編集する
- `Samples/` の中にギミックを作る
- 依頼されていない機能を足す
