using Trea;
using UnityEditor;
using UnityEngine;

namespace Trea.Editor
{
    [CustomEditor(typeof(BehaviourTreeRunner))]
    public class BehaviourTreeRunnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var runner = (BehaviourTreeRunner)target;

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("root"), new GUIContent("根节点"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tickInterval"), new GUIContent("Tick 间隔（秒）"));
            serializedObject.ApplyModifiedProperties();

            if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                if (GUILayout.Button("重置行为树"))
                    runner.ResetTree();

                if (runner.blackboard != null)
                {
                    EditorGUILayout.LabelField("黑板数据", EditorStyles.boldLabel);
                    foreach (var key in runner.blackboard.Keys)
                    {
                        var value = runner.blackboard.Get<object>(key);
                        EditorGUILayout.LabelField(key, value != null ? value.ToString() : "null");
                    }
                }
            }
        }
    }
}
