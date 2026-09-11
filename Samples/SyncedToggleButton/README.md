# Mashiro Synced Toggle Button

押すたびに対象 GameObject の**表示 / 非表示を切り替え**、その状態を**全員に同期**するボタンです。
Interact・同期変数・所有権という Udon ギミックの基本が 1 ファイルに入った、お手本用のギミックです。

## できること

- Interact するたびに Target の表示 / 非表示が反転する。
- 誰が押しても、全プレイヤーで同じ状態になる。
- 途中から入ったプレイヤーにも、そのときの状態が届く。

## セットアップ

1. ボタンにする GameObject（Cube など）に `Add Component > MashiroTheater > Mashiro Synced Toggle Button` を追加します。
2. 押せるように、その GameObject に `Collider`（Box Collider など）が付いていることを確認します。
3. インスペクタの **Target** に、表示 / 非表示を切り替えたい GameObject を入れます。ボタン自身とは別のオブジェクトにしてください。
4. Play（ClientSim）でボタンに近づき Interact すると、Target が消えたり出たりします。

## インスペクタ項目

| 項目 | 説明 |
| --- | --- |
| Target | 表示 / 非表示を切り替える GameObject |
| Initially Active | ワールド開始時に Target を表示しておくか（既定: オン） |

## API リファレンス

### MashiroSyncedToggleButton

| メソッド | 用途 | 呼び出し種別 |
| --- | --- | --- |
| `Interact()` | 表示 / 非表示を切り替える | オブジェクトへの Interact（配線不要） |
| `Toggle()` | 同上 | UI Button や他の Udon から `SendCustomEvent("Toggle")` で呼ぶ |
| `OnDeserialization()` | 同期された状態を反映する | 内部専用（配線不要） |

## 注意点

- Target をボタン自身にすると、非表示にした瞬間に押せなくなり戻せません。
- 同期は `[UdonSynced]` 変数 1 つで行っています。切り替えの瞬間だけ通信が発生し、常時の負荷はありません。
