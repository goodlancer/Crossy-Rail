using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using Watermelon.Core;

namespace Watermelon
{
    [CustomEditor(typeof(MultilanguageSettings))]
    public class MultilanguageSettingsEditor : WatermelonEditor
    {
        private string TEXT_FOLDER_PATH
        {
            get { return "/" + ApplicationConsts.PROJECT_FOLDER + "/Resources/" + Multilanguage.TEXT_PATH; }
        }

        private List<LanguageKey> multilanguageData = new List<LanguageKey>();
                
        private bool fileChanged = false;

        private Multilanguage.ProjectLanguages[] activeLanguages;
        private string[] activeLanguageNames;
        private int activeSystemLanguage = 0;
        private int defaultLanguage = 0;
        
        private MultilanguageSettings multilanguageSettings;

        private string defaultLanguagePropertyName = "defaultLanguage";
        private string activeLanguagesPropertyName = "activeLanguages";

        private SerializedProperty defaultLanguageProperty;
        private SerializedProperty activeLanguagesProperty;

        private GUIContent addButton;

        private Multilanguage.LanguageCase tempLanguageCase = null;

        protected override void OnEnable()
        {
            base.OnEnable();

            // Get properties
            defaultLanguageProperty = serializedObject.FindProperty(defaultLanguagePropertyName);
            activeLanguagesProperty = serializedObject.FindProperty(activeLanguagesPropertyName);

            // Get Multilanguage settings from target
            multilanguageSettings = target as MultilanguageSettings;

            // Reset temp language
            tempLanguageCase = null;
            
            // Initialize languages enums
            Init();

            // Load data from file
            Load();

            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        protected override void Styles()
        {
            // Get styles
            addButton = new GUIContent(EditorStylesExtended.ICON_SPACE + "Add Language", EditorStylesExtended.GetTexture("icon_add", EditorStylesExtended.IconColor));
        }

        private void PlayModeStateChanged(PlayModeStateChange state)
        {
            if(state == PlayModeStateChange.EnteredPlayMode)
            {
                if (fileChanged)
                {
                    AssetDatabase.Refresh();

                    fileChanged = false;
                }
            }
        }

        private void OnDisable()
        {
            if (fileChanged)
            {
                AssetDatabase.Refresh();

                fileChanged = false;
            }

            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
        }

        private void Init()
        {
            activeLanguages = multilanguageSettings.ActiveLanguages();
            activeLanguageNames = new string[activeLanguages.Length];
            for (int i = 0; i < activeLanguages.Length; i++)
            {
                activeLanguageNames[i] = activeLanguages[i].ToString();
            }

            defaultLanguage = System.Array.FindIndex(activeLanguages, x => x == multilanguageSettings.defaultLanguage);

            if(activeLanguages.Length > 0)
            {
                activeSystemLanguage = 0;
            }
            else
            {
                activeSystemLanguage = -1;
            }
        }
        
        public override void OnInspectorGUI()
        {
            InitStyles();

            serializedObject.Update();

            Rect editorRect = EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);
            EditorGUILayoutCustom.Header("MULTILANGUAGE");

            EditorGUI.BeginChangeCheck();
            defaultLanguage = EditorGUILayout.Popup("Default Language", defaultLanguage, activeLanguageNames);
            if(EditorGUI.EndChangeCheck())
            {
                defaultLanguageProperty.intValue = (int)multilanguageSettings.activeLanguages[defaultLanguage].projectLanguages;
            }

            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Active Languages", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            int musicArraySize = activeLanguagesProperty.arraySize;
            if (musicArraySize > 0)
            {
                for (int i = 0; i < musicArraySize; i++)
                {
                    SerializedProperty arrayElementProperty = activeLanguagesProperty.GetArrayElementAtIndex(i);
                    
                    EditorGUILayout.BeginHorizontal();

                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(arrayElementProperty.FindPropertyRelative("projectLanguages"), GUIContent.none);
                    GUI.enabled = true;

                    if (GUILayout.Button("X", EditorStylesExtended.button_04_mini, GUILayout.Height(18), GUILayout.Width(18)))
                    {
                        if (EditorUtility.DisplayDialog("Remove language", "Are you sure you want to remove language?", "Remove", "Cancel"))
                        {
                            activeLanguagesProperty.RemoveFromVariableArrayAt(i);

                            EditorApplication.delayCall += delegate
                            {
                                Init();
                            };

                            return;
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            if(tempLanguageCase != null)
            {
                EditorGUILayout.BeginHorizontal();
                tempLanguageCase.projectLanguages = (Multilanguage.ProjectLanguages)EditorGUILayout.EnumPopup(GUIContent.none, tempLanguageCase.projectLanguages, GUILayout.MinWidth(10));

                if (GUILayout.Button("✓", EditorStylesExtended.button_03_mini, GUILayout.Height(18), GUILayout.Width(18)))
                {
                    if (System.Array.FindIndex(activeLanguages, x => x == tempLanguageCase.projectLanguages) != -1)
                    {
                        EditorUtility.DisplayDialog("Wrong Language", tempLanguageCase.projectLanguages + " language already exists!", "Close");

                        return;
                    }

                    int index = activeLanguagesProperty.arraySize;

                    activeLanguagesProperty.arraySize++;

                    SerializedProperty newElementProperty = activeLanguagesProperty.GetArrayElementAtIndex(index);
                    
                    newElementProperty.FindPropertyRelative("projectLanguages").intValue = (int)tempLanguageCase.projectLanguages;

                    Save(tempLanguageCase.projectLanguages);

                    tempLanguageCase = null;

                    EditorApplication.delayCall += delegate
                    {
                        Init();
                    };
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(addButton, EditorStylesExtended.button_01, GUILayout.Width(120)))
            {
                tempLanguageCase = new Multilanguage.LanguageCase((Multilanguage.ProjectLanguages)0);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndVertical();

            GUILayout.Space(8);

            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            EditorGUILayoutCustom.Header("WORDS");

            activeSystemLanguage = EditorGUILayout.Popup("Selected Language", activeSystemLanguage, activeLanguageNames);
            
            if(activeSystemLanguage != -1)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Key", EditorStyles.boldLabel);
                GUILayout.Label("Value", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                bool enableState = GUI.enabled;

                EditorGUI.BeginChangeCheck();
                for (int i = 0; i < multilanguageData.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    GUI.enabled = false;
                    multilanguageData[i].key = EditorGUILayout.TextField(multilanguageData[i].key);
                    GUI.enabled = enableState;

                    int index = multilanguageData[i].values.FindIndex(x => x.language == activeLanguages[activeSystemLanguage]);
                    if (index == -1)
                    {
                        multilanguageData[i].values.Add(new LanguageValue(activeLanguages[activeSystemLanguage], ""));

                        index = multilanguageData[i].values.Count - 1;
                    }

                    multilanguageData[i].values[index].value = EditorGUILayout.TextField(multilanguageData[i].values[index].value);

                    EditorGUILayout.EndHorizontal();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    Save(activeLanguages[activeSystemLanguage]);
                }
                
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();

            EditorGUILayoutCustom.DrawCompileWindow(editorRect);
        }

        private void Save()
        {
            foreach (Multilanguage.ProjectLanguages language in activeLanguages)
            {
                Save(language);
            }
        }

        private void Save(Multilanguage.ProjectLanguages language)
        {
            fileChanged = true;

            string name = language.ToString();

            StringBuilder exportText = new StringBuilder();

            exportText.AppendLine("[" + name + "]");

            for (int i = 0; i < multilanguageData.Count; i++)
            {
                string word = "";

                int wordIndex = multilanguageData[i].values.FindIndex(x => x.language == language);
                if (wordIndex != -1)
                {
                    word = multilanguageData[i].values[wordIndex].value;
                }

                exportText.AppendLine(multilanguageData[i].key + "=" + word);
            }

            File.WriteAllText(Application.dataPath + TEXT_FOLDER_PATH + name + ".txt", exportText.ToString(), Encoding.UTF8);
        }

        private void Load()
        {
            multilanguageData = new List<LanguageKey>();
            
            foreach (string file in Directory.GetFiles(Application.dataPath + TEXT_FOLDER_PATH, "*.txt"))
            {
                string[] contents = File.ReadAllLines(file);

                Multilanguage.ProjectLanguages language = (Multilanguage.ProjectLanguages)System.Enum.Parse(typeof(Multilanguage.ProjectLanguages), Regex.Match(contents[0], @"\[([^)]*)\]").Result("$1"));

                for (int i = 1; i < contents.Length; i++)
                {
                    if (!string.IsNullOrEmpty(contents[i]))
                    {
                        string[] entries = contents[i].Split(new char[] { '=' }, 2);

                        int keyIndex = multilanguageData.FindIndex(x => x.key == entries[0]);
                        if (keyIndex == -1)
                        {
                            multilanguageData.Add(new LanguageKey(entries[0]));

                            keyIndex = multilanguageData.Count - 1;
                        }

                        multilanguageData[keyIndex].SetOrAdd(language, entries[1]);
                    }
                }
            }
        }

        [System.Serializable]
        public class LanguageKey
        {
            [SerializeField]
            private string m_Key;
            public string key
            {
                get { return m_Key; }
                set { m_Key = value; }
            }

            [SerializeField]
            private List<LanguageValue> m_Values = new List<LanguageValue>();
            public List<LanguageValue> values
            {
                get { return m_Values; }
                set { m_Values = value; }
            }

            public LanguageKey(string key)
            {
                m_Key = key;
            }

            public LanguageKey SetOrAdd(Multilanguage.ProjectLanguages language, string value)
            {
                int valueIndex = m_Values.FindIndex(x => x.language == language);

                if (valueIndex == -1)
                    m_Values.Add(new LanguageValue(language, value));
                else
                    m_Values[valueIndex].value = value;

                return this;
            }
        }

        [System.Serializable]
        public class LanguageValue
        {
            [SerializeField]
            private Multilanguage.ProjectLanguages m_Language;
            public Multilanguage.ProjectLanguages language
            {
                get { return m_Language; }
            }

            [SerializeField]
            private string m_Value;
            public string value
            {
                get { return m_Value; }
                set { m_Value = value; }
            }

            public LanguageValue(Multilanguage.ProjectLanguages language, string value)
            {
                m_Language = language;
                m_Value = value;
            }
        }
    }
}