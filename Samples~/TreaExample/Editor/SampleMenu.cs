using Trea;
using Trea.Example;
using UnityEditor;
using UnityEngine;

namespace Trea.Example.Editor
{
    public static class SampleMenu
    {
        [MenuItem("Trea/创建示例敌人")]
        public static void CreateExampleEnemy()
        {
            var go = new GameObject("ExampleEnemy");
            var runner = go.AddComponent<BehaviourTreeRunner>();
            var ai = go.AddComponent<ExampleEnemyAI>();
            ai.runner = runner;
            ai.BuildTree();

            EditorUtility.SetDirty(go);
            Selection.activeGameObject = go;
            Debug.Log("示例敌人已创建：行为树已配置在 BehaviourTreeRunner 的 Inspector 中，请在 ExampleEnemyAI 上指定 Player 或使用 Tag 为 Player 的物体");
        }
    }
}
