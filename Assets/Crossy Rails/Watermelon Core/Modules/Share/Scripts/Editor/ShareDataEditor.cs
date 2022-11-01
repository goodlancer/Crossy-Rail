using UnityEditor;
using UnityEngine;
using Watermelon.Core;

namespace Watermelon
{
    [CustomEditor(typeof(ShareSettings))]
    public class ShareDataEditor : Editor
    {
        private const string shareEnabledPropertyName = "isShareEnabled";
        private const string shareTextAndroidSerializedPropertyName = "shareMessageAndroid";
        private const string shareTextIOSSerializedPropertyName = "shareMessageIOS";

        private SerializedProperty shareEnabledSerializedProperty;
        private SerializedProperty shareTextAndroidSerializedProperty;
        private SerializedProperty shareTextIOSSerializedProperty;
        
        private void OnEnable()
        {
            shareEnabledSerializedProperty = serializedObject.FindProperty(shareEnabledPropertyName);
            shareTextAndroidSerializedProperty = serializedObject.FindProperty(shareTextAndroidSerializedPropertyName);
            shareTextIOSSerializedProperty = serializedObject.FindProperty(shareTextIOSSerializedPropertyName);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorStyles.textArea.wordWrap = true;

            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            shareEnabledSerializedProperty.boolValue = EditorGUILayoutCustom.HeaderToggle("SHARE", shareEnabledSerializedProperty.boolValue);

            if (!shareEnabledSerializedProperty.boolValue)
            {
                EditorGUILayout.HelpBox("Share is disabled!", MessageType.Warning);
            }

            EditorGUILayout.PrefixLabel("Android Share:");
            shareTextAndroidSerializedProperty.stringValue = EditorGUILayout.TextArea(shareTextAndroidSerializedProperty.stringValue, EditorStyles.textArea, GUILayout.Height(80));
            EditorGUILayout.PrefixLabel("IOS Share:");
            shareTextIOSSerializedProperty.stringValue = EditorGUILayout.TextArea(shareTextIOSSerializedProperty.stringValue, EditorStyles.textArea, GUILayout.Height(80));
            
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}