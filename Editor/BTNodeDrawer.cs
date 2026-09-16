using System;
using System.Collections.Generic;
using System.Linq;
using Trea;
using UnityEditor;
using UnityEngine;

namespace Trea.Editor
{
    [CustomPropertyDrawer(typeof(BTNode), true)]
    public class BTNodeDrawer : PropertyDrawer
    {
        private const float ElementMinHeight = 32f;

        private static readonly List<Type> NodeTypes;

        static BTNodeDrawer()
        {
            NodeTypes = TypeCache.GetTypesDerivedFrom<BTNode>()
                .Where(t => !t.IsAbstract && !t.ContainsGenericParameters)
                .OrderBy(t => t.Name)
                .ToList();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var node = property.managedReferenceValue as BTNode;
            if (node == null || !property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            float height = EditorGUIUtility.singleLineHeight;
            foreach (var child in BodyProperties(property))
            {
                if (child.name == "children" && child.isArray)
                    height += GetChildrenListHeight(child);
                else
                    height += EditorGUI.GetPropertyHeight(child, true);
                height += EditorGUIUtility.standardVerticalSpacing;
            }
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            var node = property.managedReferenceValue as BTNode;

            if (node == null)
            {
                if (GUI.Button(position, "选择节点类型...", EditorStyles.popup))
                    ShowTypeMenu(property);
                return;
            }

            DrawHeader(position, property, node);
            if (!property.isExpanded) return;

            EditorGUI.indentLevel++;
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            foreach (var child in BodyProperties(property))
            {
                float h;
                if (child.name == "children" && child.isArray)
                {
                    h = GetChildrenListHeight(child);
                    DrawChildrenList(new Rect(position.x, position.y, position.width, h), child);
                }
                else
                {
                    h = EditorGUI.GetPropertyHeight(child, true);
                    EditorGUI.PropertyField(position, child, true);
                }
                position.y += h + EditorGUIUtility.standardVerticalSpacing;
            }
            EditorGUI.indentLevel--;
        }

        private void DrawHeader(Rect position, SerializedProperty property, BTNode node)
        {
            var color = GetNodeColor(node);
            EditorGUI.DrawRect(position, new Color(color.r, color.g, color.b, 0.22f));

            var foldoutRect = new Rect(position.x, position.y, 14, position.height);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none);

            var typeRect = new Rect(position.x + 14, position.y, 110, position.height);
            if (GUI.Button(typeRect, new GUIContent(node.GetType().Name, "点击更换节点类型（参数会重置）"), EditorStyles.popup))
                ShowTypeMenu(property);

            float stateWidth = EditorApplication.isPlaying ? 70f : 0f;
            var clearRect = new Rect(position.xMax - 18, position.y, 18, position.height);
            if (GUI.Button(clearRect, new GUIContent("×", "删除该节点")))
            {
                Undo.RecordObject(property.serializedObject.targetObject, "删除节点");
                if (!TryDeleteInList(property))
                {
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                }
                return;
            }

            var nameRect = new Rect(position.x + 128, position.y, position.width - 128 - 18 - stateWidth - 4, position.height);
            var nodeNameProp = property.FindPropertyRelative("nodeName");
            EditorGUI.BeginChangeCheck();
            var newName = EditorGUI.TextField(nameRect, nodeNameProp.stringValue);
            if (EditorGUI.EndChangeCheck())
                nodeNameProp.stringValue = newName;

            if (EditorApplication.isPlaying && (node.Started || node.State != NodeState.Running))
            {
                var stateRect = new Rect(position.xMax - 70, position.y, 70, position.height);
                var state = node.State;
                var stateColor = state == NodeState.Success ? new Color(0.3f, 0.85f, 0.35f)
                    : state == NodeState.Failure ? new Color(0.9f, 0.25f, 0.25f)
                    : new Color(0.95f, 0.8f, 0.2f);
                var oldColor = GUI.color;
                GUI.color = stateColor;
                GUI.Box(stateRect, new GUIContent(state.ToString()), EditorStyles.miniButton);
                GUI.color = oldColor;
            }
        }

        private static bool TryDeleteInList(SerializedProperty property)
        {
            var path = property.propertyPath;
            int idx = path.LastIndexOf(".Array.data[", StringComparison.Ordinal);
            if (idx < 0) return false;
            var list = property.serializedObject.FindProperty(path.Substring(0, idx));
            if (list == null) return false;
            int end = path.IndexOf(']', idx);
            if (end < 0) return false;
            if (!int.TryParse(path.Substring(idx + 12, end - idx - 12), out int index)) return false;
            list.DeleteArrayElementAtIndex(index);
            property.serializedObject.ApplyModifiedProperties();
            return true;
        }

        private static float GetChildrenListHeight(SerializedProperty listProp)
        {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            for (int i = 0; i < listProp.arraySize; i++)
            {
                var element = listProp.GetArrayElementAtIndex(i);
                if (element.managedReferenceValue == null)
                    height += EditorGUIUtility.singleLineHeight;
                else
                    height += Mathf.Max(EditorGUI.GetPropertyHeight(element, true), ElementMinHeight);
                height += EditorGUIUtility.standardVerticalSpacing;
            }
            return height;
        }

        private void DrawChildrenList(Rect position, SerializedProperty listProp)
        {
            float y = position.y;
            var headerRect = new Rect(position.x, y, position.width - 24, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(headerRect, "子节点");
            var addRect = new Rect(position.xMax - 24, y, 24, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(addRect, "+"))
                ShowAddMenu(listProp);
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            for (int i = 0; i < listProp.arraySize; i++)
            {
                var element = listProp.GetArrayElementAtIndex(i);
                if (element.managedReferenceValue == null)
                {
                    var nullRect = new Rect(position.x, y, position.width - 24, EditorGUIUtility.singleLineHeight);
                    EditorGUI.LabelField(nullRect, "（空槽位）");
                    var removeRect = new Rect(position.xMax - 24, y, 24, EditorGUIUtility.singleLineHeight);
                    if (GUI.Button(removeRect, "−"))
                    {
                        Undo.RecordObject(listProp.serializedObject.targetObject, "删除子节点");
                        listProp.DeleteArrayElementAtIndex(i);
                        listProp.serializedObject.ApplyModifiedProperties();
                        return;
                    }
                    y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    continue;
                }

                float h = Mathf.Max(EditorGUI.GetPropertyHeight(element, true), ElementMinHeight);
                var upRect = new Rect(position.x, y, 20, 16);
                var downRect = new Rect(position.x, y + 16, 20, 16);
                using (new EditorGUI.DisabledScope(i <= 0))
                {
                    if (GUI.Button(upRect, "▲"))
                    {
                        Undo.RecordObject(listProp.serializedObject.targetObject, "移动子节点");
                        listProp.MoveArrayElement(i, i - 1);
                        listProp.serializedObject.ApplyModifiedProperties();
                        return;
                    }
                }
                using (new EditorGUI.DisabledScope(i >= listProp.arraySize - 1))
                {
                    if (GUI.Button(downRect, "▼"))
                    {
                        Undo.RecordObject(listProp.serializedObject.targetObject, "移动子节点");
                        listProp.MoveArrayElement(i, i + 1);
                        listProp.serializedObject.ApplyModifiedProperties();
                        return;
                    }
                }

                EditorGUI.PropertyField(new Rect(position.x + 24, y, position.width - 24, h), element, true);
                y += h + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        private static void ShowAddMenu(SerializedProperty listProp)
        {
            var menu = new GenericMenu();
            foreach (var type in NodeTypes)
            {
                var captured = type;
                menu.AddItem(new GUIContent(captured.Name), false, () =>
                {
                    Undo.RecordObject(listProp.serializedObject.targetObject, "添加子节点");
                    int index = listProp.arraySize;
                    listProp.InsertArrayElementAtIndex(index);
                    listProp.GetArrayElementAtIndex(index).managedReferenceValue = Activator.CreateInstance(captured);
                    listProp.serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        }

        private void ShowTypeMenu(SerializedProperty property)
        {
            var menu = new GenericMenu();
            foreach (var type in NodeTypes)
            {
                var captured = type;
                menu.AddItem(new GUIContent(captured.Name), false, () =>
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "创建/更换节点");
                    property.managedReferenceValue = Activator.CreateInstance(captured);
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        }

        private static IEnumerable<SerializedProperty> BodyProperties(SerializedProperty property)
        {
            var copy = property.Copy();
            bool enterChildren = true;
            while (copy.NextVisible(enterChildren) && copy.depth > property.depth)
            {
                enterChildren = false;
                if (copy.depth != property.depth + 1) continue;
                if (copy.name == "nodeName") continue;
                yield return copy;
            }
        }

        private static Color GetNodeColor(BTNode node)
        {
            if (node is ConditionNode) return new Color(0.95f, 0.75f, 0.25f);
            if (node is ActionNode) return new Color(0.35f, 0.75f, 0.35f);
            if (node is CompositeNode) return new Color(0.35f, 0.55f, 0.85f);
            if (node is DecoratorNode) return new Color(0.45f, 0.75f, 0.75f);
            return Color.gray;
        }
    }
}
