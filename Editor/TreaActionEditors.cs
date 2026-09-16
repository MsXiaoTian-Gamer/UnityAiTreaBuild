using System;
using System.Collections.Generic;
using System.Linq;
using Trea;
using UnityEditor;
using UnityEngine;

namespace Trea.Editor
{
    [CustomEditor(typeof(TreaAction))]
    public class TreaActionEditor : UnityEditor.Editor
    {
        private static readonly List<Type> NodeTypes = TypeCache.GetTypesDerivedFrom<BTNode>()
            .Where(t => !t.IsAbstract && !t.ContainsGenericParameters)
            .OrderBy(t => t.FullName)
            .ToList();

        public override void OnInspectorGUI()
        {
            var action = (TreaAction)target;

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("actionType"), new GUIContent("节点类型"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("actionName"), new GUIContent("节点名称"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("children"), new GUIContent("子节点"));

            switch (action.actionType)
            {
                case ActionType.Repeater:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("repeatCount"), new GUIContent("重复次数（-1 无限）"));
                    break;
                case ActionType.Timeout:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("duration"), new GUIContent("超时时间"));
                    break;
                case ActionType.Wait:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("duration"), new GUIContent("等待时间"));
                    break;
                case ActionType.Cooldown:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("cooldown"), new GUIContent("冷却时间"));
                    break;
                case ActionType.Chance:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("probability"), new GUIContent("概率"));
                    break;
                case ActionType.Log:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("message"), new GUIContent("日志内容"));
                    break;
                case ActionType.Animation:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("animationClip"), new GUIContent("动画片段"));
                    break;
                case ActionType.MoveTo:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("targetPosition"), new GUIContent("目标位置"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpeed"), new GUIContent("移动速度"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("arriveDistance"), new GUIContent("到达判定距离"));
                    break;
                case ActionType.Custom:
                    DrawCustomTypeField(action);
                    break;
            }
            serializedObject.ApplyModifiedProperties();

            if (IsComposite(action.actionType) && action.children.Count == 0)
                EditorGUILayout.HelpBox("组合节点至少需要 1 个子节点", MessageType.Warning);
            if (IsDecorator(action.actionType) && action.children.Count > 1)
                EditorGUILayout.HelpBox("装饰节点只能有 1 个子节点", MessageType.Warning);
            if (IsLeaf(action.actionType) && action.children.Count > 0)
                EditorGUILayout.HelpBox("叶节点不能有子节点", MessageType.Warning);
        }

        private static void DrawCustomTypeField(TreaAction action)
        {
            int index = -1;
            for (int i = 0; i < NodeTypes.Count; i++)
            {
                if (NodeTypes[i].AssemblyQualifiedName == action.customTypeName)
                {
                    index = i;
                    break;
                }
            }

            var displayNames = NodeTypes.Select(t => t.FullName).ToArray();
            int newIndex = EditorGUILayout.Popup("自定义节点类", Mathf.Max(0, index), displayNames);
            if (newIndex != index)
            {
                action.customTypeName = NodeTypes[newIndex].AssemblyQualifiedName;
                EditorUtility.SetDirty(action);
            }
            if (index < 0 && !string.IsNullOrEmpty(action.customTypeName))
                EditorGUILayout.HelpBox("找不到类型：" + action.customTypeName, MessageType.Error);

            EditorGUILayout.HelpBox("自定义节点的参数请在构建后的行为树中配置（Runner 的 Inspector）", MessageType.Info);
        }

        private static bool IsComposite(ActionType type)
        {
            return type == ActionType.Sequence || type == ActionType.Selector ||
                   type == ActionType.Parallel || type == ActionType.RandomSelector;
        }

        private static bool IsDecorator(ActionType type)
        {
            return type == ActionType.Inverter || type == ActionType.Repeater ||
                   type == ActionType.UntilSuccess || type == ActionType.Timeout ||
                   type == ActionType.Cooldown || type == ActionType.Succeeder;
        }

        private static bool IsLeaf(ActionType type)
        {
            return type == ActionType.Wait || type == ActionType.Log ||
                   type == ActionType.Chance || type == ActionType.Animation ||
                   type == ActionType.MoveTo;
        }
    }

    [CustomEditor(typeof(TreaActionReg))]
    public class TreaActionRegEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rootAction"), new GUIContent("根节点资产"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("runner"), new GUIContent("行为树 Runner"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("buildOnStart"), new GUIContent("启动时自动构建（树为空时）"));
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (GUILayout.Button("构建行为树"))
                ((TreaActionReg)target).BuildAction();
        }
    }
}
