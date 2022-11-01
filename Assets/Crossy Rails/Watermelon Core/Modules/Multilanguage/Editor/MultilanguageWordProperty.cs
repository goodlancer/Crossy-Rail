using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Watermelon.Core;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(MultilanguageWordAttribute))]
    public class MultilanguageWordProperty : UnityEditor.PropertyDrawer
    {
        private bool m_IsInited = false;
        private string[] m_EnumWords;
        private List<string> wordsKeys;

        private const int MAX_WORD_LENGTH = 30;

        private MultilanguageSettings multilanguageSettings;

        private void Init(SerializedProperty property)
        {
            multilanguageSettings = EditorUtils.GetAsset<MultilanguageSettings>();

            wordsKeys = new List<string>();

            Dictionary<string, string> words = Multilanguage.GetWords(multilanguageSettings.defaultLanguage);

            MultilanguageWordAttribute wordAttribute = attribute as MultilanguageWordAttribute;

            wordsKeys = words.Keys.ToList();

            if (!string.IsNullOrEmpty(wordAttribute.filter))
            {
                wordsKeys = wordsKeys.FindAll(x => x.Contains(wordAttribute.filter));
            }

            int wordsCount = wordsKeys.Count;
            m_EnumWords = new string[wordsCount];
            for (int i = 0; i < wordsCount; i++)
            {
                string word = words[wordsKeys[i]].Replace("/", "\\");
                if (word.Length > MAX_WORD_LENGTH)
                    word = word.Substring(0, MAX_WORD_LENGTH);

                m_EnumWords[i] = word + " - (" + wordsKeys[i] + ")";
            }

            m_IsInited = true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!m_IsInited)
            {
                Init(property);
            }

            string propertyValue = property.stringValue;
            int m_SelectedWordId = 0;

            if (string.IsNullOrEmpty(propertyValue))
            {
                property.stringValue = null;
                m_SelectedWordId = -1;
            }
            else
            {
                int foundedKey = wordsKeys.FindIndex(x => x == property.stringValue);

                if (foundedKey != -1)
                {
                    m_SelectedWordId = foundedKey;
                }
                else
                {
                    property.stringValue = "Null";
                    m_SelectedWordId = -1;
                }
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var amountRect = new Rect(position.x, position.y, position.width, position.height);

            m_SelectedWordId = EditorGUI.Popup(amountRect, m_SelectedWordId, m_EnumWords);

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();

            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = wordsKeys[m_SelectedWordId];
            }
        }
    }
}