using System;
using System.Collections.Generic;
using Trea;
using UnityEditor;
using UnityEngine;

namespace Trea.Editor
{
    public static class TreaMenu
    {
        [MenuItem("Trea/示例/创建示例节点资产")]
        public static void CreateExampleAssets()
        {
            var dir = "Assets/TreaExample";
            if (!AssetDatabase.IsValidFolder(dir))
                AssetDatabase.CreateFolder("Assets", "TreaExample");

            var root = CreateAsset<TreaAction>(dir + "/Root_Selector.asset", a =>
            {
                a.actionType = ActionType.Selector;
                a.actionName = "根节点";
            });
            var chase = CreateAsset<TreaAction>(dir + "/Chase_Sequence.asset", a =>
            {
                a.actionType = ActionType.Sequence;
                a.actionName = "追击";
            });
            var seePlayer = CreateAsset<TreaAction>(dir + "/SeePlayer_Chance.asset", a =>
            {
                a.actionType = ActionType.Chance;
                a.actionName = "发现玩家";
                a.probability = 0.8f;
            });
            var wait = CreateAsset<TreaAction>(dir + "/Wait_1s.asset", a =>
            {
                a.actionType = ActionType.Wait;
                a.actionName = "等待1秒";
                a.duration = 1f;
            });
            var attack = CreateAsset<TreaAction>(dir + "/Attack_Animation.asset", a =>
            {
                a.actionType = ActionType.Animation;
                a.actionName = "攻击动画";
            });
            var patrol = CreateAsset<TreaAction>(dir + "/Patrol_Sequence.asset", a =>
            {
                a.actionType = ActionType.Sequence;
                a.actionName = "巡逻";
            });
            var log = CreateAsset<TreaAction>(dir + "/Log_Patrol.asset", a =>
            {
                a.actionType = ActionType.Log;
                a.actionName = "打印巡逻";
                a.message = "巡逻中...";
            });
            var wait2 = CreateAsset<TreaAction>(dir + "/Wait_2s.asset", a =>
            {
                a.actionType = ActionType.Wait;
                a.actionName = "等待2秒";
                a.duration = 2f;
            });

            root.children = new List<TreaAction> { chase, patrol };
            chase.children = new List<TreaAction> { seePlayer, wait, attack };
            patrol.children = new List<TreaAction> { log, wait2 };

            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(chase);
            EditorUtility.SetDirty(patrol);
            AssetDatabase.SaveAssets();
            Selection.activeObject = root;
            Debug.Log("示例节点资产已创建到 Assets/TreaExample，可将 Root_Selector 挂到 TreaActionReg 的根节点资产上");
        }

        private static T CreateAsset<T>(string path, Action<T> setup) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            setup(asset);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
