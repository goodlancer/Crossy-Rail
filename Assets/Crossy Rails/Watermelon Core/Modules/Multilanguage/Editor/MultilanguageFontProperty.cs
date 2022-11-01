using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(MultilanguageFontAttribute))]
    public class MultilanguageFontProperty : UnityEditor.PropertyDrawer
    {
        private bool m_IsInited = false;
        private string[] fontKeys;

        private void Init(SerializedProperty property)
        {
            List<string> fontKeysList = new List<string>();
            fontKeysList.Add("None");
            fontKeysList.AddRange(Multilanguage.REQUIRED_FONTS);

            fontKeys = fontKeysList.ToArray();

            m_IsInited = true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!m_IsInited)
                Init(property);

            string propertyValue = property.stringValue;
            int selectedFontId = 0;

            if (string.IsNullOrEmpty(propertyValue))
            {
                property.stringValue = "";
                selectedFontId = 0;
            }
            else
            {
                int foundedKey = System.Array.FindIndex(fontKeys, x => x == property.stringValue);

                if (foundedKey != -1)
                {
                    selectedFontId = foundedKey;
                }
                else
                {
                    property.stringValue = "";
                    selectedFontId = 0;
                }
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var amountRect = new Rect(position.x, position.y, position.width, position.height);

            selectedFontId = EditorGUI.Popup(amountRect, selectedFontId, fontKeys);

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();

            if (GUI.changed)
            {
                if (fontKeys[selectedFontId] == "None")
                {
                    property.stringValue = "";
                }
                else
                {
                    property.stringValue = fontKeys[selectedFontId];
                }
            }
        }
    }
}