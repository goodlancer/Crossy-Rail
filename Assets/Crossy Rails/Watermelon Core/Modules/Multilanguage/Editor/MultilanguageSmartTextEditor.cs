using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Watermelon
{
    [CustomEditor(typeof(MultilanguageSmartText))]
    public class MultilanguageSmartTextEditor : Editor
    {
        private ReorderableList list;

        private void OnEnable()
        {
            list = new ReorderableList(serializedObject, serializedObject.FindProperty("fontSizeOverrides"), true, true, true, true);
            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - 60, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("language"), GUIContent.none);
                EditorGUI.PropertyField(new Rect(rect.x + rect.width - 60, rect.y, 60, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("fontSize"), GUIContent.none);
            };
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            GUILayout.Space(10);

            list.DoLayoutList();

            if (GUI.changed)
                serializedObject.ApplyModifiedProperties();
        }
    }
}
