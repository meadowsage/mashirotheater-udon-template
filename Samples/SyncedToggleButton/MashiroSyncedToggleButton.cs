/*
MIT License

Copyright (c) 2025 Mashiro Small Theater

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace MashiroTheater
{
    /// <summary>
    /// Interact するたびに対象 GameObject の表示 / 非表示を切り替え、その状態を全員に同期するボタン。
    ///
    /// 状態は所有者（Owner）だけが書き換えられる同期変数で持つ。誰が押しても動くように、
    /// 押した人がまず所有権を取ってから書き換える。途中参加者には最新の状態が自動で届く。
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [AddComponentMenu("MashiroTheater/Mashiro Synced Toggle Button")]
    public class MashiroSyncedToggleButton : UdonSharpBehaviour
    {
        [Header("対象設定")]
        [SerializeField, Tooltip("表示 / 非表示を切り替える GameObject。")]
        private GameObject target;

        [SerializeField, Tooltip("ワールド開始時に表示しておくか。")]
        private bool initiallyActive = true;

        // 全員で共有する ON / OFF 状態。所有者が書き換え、RequestSerialization で配る。
        [UdonSynced] private bool isActive;

        private void Start()
        {
            // 途中参加者は直後に OnDeserialization で上書きされるので、ここでは初期値を入れるだけでよい。
            isActive = initiallyActive;
            Apply();
        }

        // プレイヤーがボタンを押したとき（誰でも押せる）。
        public override void Interact()
        {
            Toggle();
        }

        // UI Button や他の Udon からも呼べる切り替えイベント。
        public void Toggle()
        {
            var local = Networking.LocalPlayer;
            // 同期変数を書き換えられるのは所有者だけなので、押した人が所有権を取ってから変更する。
            if (local != null && !Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(local, gameObject);
            }

            isActive = !isActive;
            Apply();
            RequestSerialization();
        }

        // 所有者が配った状態を受け取ったとき（自分以外が押した場合や途中参加時）。
        public override void OnDeserialization()
        {
            Apply();
        }

        private void Apply()
        {
            if (target == null) return;
            target.SetActive(isActive);
        }
    }
}
