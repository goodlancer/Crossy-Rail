using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using System;
using Watermelon.Core;

namespace Watermelon
{
    public class MultilanguageDataWindow : EditorWindow
    {
        private readonly string TEXT_FOLDER_PATH = "/" + ApplicationConsts.PROJECT_FOLDER + "/Resources/" + Multilanguage.TEXT_PATH;

        private List<LanguageKey> multilanguageData = new List<LanguageKey>();

        private Vector2 scroll = Vector2.zero;

        private string inputKey = "";

        private bool fileChanged = false;

        private Multilanguage.ProjectLanguages[] activeLanguages;
        private string[] activeLanguageNames;
        private int activeSystemLanguage = 0;

        private MultilanguageSettings multilanguageSettings;

        private List<RequiredWord> requiredWords = new List<RequiredWord>();

        [MenuItem("Tools/Editor/Multilanguage Manager")]
        public static void InitWindow()
        {
            MultilanguageDataWindow window = (MultilanguageDataWindow)GetWindow(typeof(MultilanguageDataWindow), false, "Multilanguage Data");
            window.Show();
        }

        private void OnEnable()
        {
            Load();

            multilanguageSettings = EditorUtils.GetAsset<MultilanguageSettings>();

            activeLanguages = multilanguageSettings.ActiveLanguages();
            activeLanguageNames = new string[activeLanguages.Length];
            for (int i = 0; i < activeLanguages.Length; i++)
            {
                activeLanguageNames[i] = activeLanguages[i].ToString();
            }

            LoadRequiredWords();
        }

        private void LoadRequiredWords()
        {
            List<RequiredWord> requredWordsList = new List<RequiredWord>();

            //Get assembly
            Assembly assembly = Assembly.GetAssembly(typeof(RequiredWordAttribute));

            IEnumerable<Type> types = assembly.GetTypes().Where(x => x.IsDefined(typeof(RequiredWordAttribute), true));

            foreach (Type type in types)
            {
                //Get attribute
                RequiredWordAttribute requiredWordAttribute = (RequiredWordAttribute)Attribute.GetCustomAttribute(type, typeof(RequiredWordAttribute));

                //Add requred words
                if (requiredWordAttribute != null)
                {
                    for (int i = 0; i < requiredWordAttribute.requiredWords.Length; i++)
                    {
                        requredWordsList.Add(new RequiredWord(requiredWordAttribute.requiredWords[i], multilanguageData.FindIndex(x => x.key == requiredWordAttribute.requiredWords[i]) != -1));
                    }
                }
            }

            requiredWords = requredWordsList;
        }

        private void OnLostFocus()
        {
            if (fileChanged)
                AssetDatabase.Refresh();
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Tools", EditorStyles.toolbarDropDown)) { ShowToolsMenu(); }
            EditorGUILayout.EndHorizontal();

            activeSystemLanguage = EditorGUILayout.Popup("Current Language", activeSystemLanguage, activeLanguageNames);

            //Required words
            int requredWordsCount = requiredWords.Count();
            string requredText = "\n";
            bool showRequiredWords = false;

            for (int i = 0; i < requredWordsCount; i++)
            {
                if (!requiredWords[i].contains)
                {
                    requredText += requiredWords[i].word + "\n";
                    showRequiredWords = true;
                }
            }

            if (showRequiredWords)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Required words:", EditorStyles.boldLabel);
                if (GUILayout.Button("Add", EditorStyles.miniButton))
                {
                    for (int i = 0; i < requredWordsCount; i++)
                    {
                        if (!requiredWords[i].contains)
                            AddWord(requiredWords[i].word);
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.HelpBox(requredText, MessageType.Warning, true);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);
            scroll = EditorGUILayout.BeginScrollView(scroll, false, false);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Key", EditorStyles.boldLabel);
            GUILayout.Label("Value", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < multilanguageData.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                multilanguageData[i].key = EditorGUILayout.TextField(multilanguageData[i].key);

                int index = multilanguageData[i].values.FindIndex(x => x.language == activeLanguages[activeSystemLanguage]);
                if (index == -1)
                {
                    multilanguageData[i].values.Add(new LanguageValue(activeLanguages[activeSystemLanguage], ""));

                    index = multilanguageData[i].values.Count - 1;
                }

                multilanguageData[i].values[index].value = EditorGUILayout.TextField(multilanguageData[i].values[index].value);

                if (GUILayout.Button("=", EditorStyles.miniButton))
                {
                    int selectedIndex = i;

                    // create the menu and add items to it
                    GenericMenu menu = new GenericMenu();

                    // forward slashes nest menu items under submenus
                    menu.AddItem(new GUIContent("Remove"), false, delegate
                    {
                        if (EditorUtility.DisplayDialog("Are you sure?", "This element will be removed!", "Remove", "Cancel"))
                        {
                            InitRequiredWord(multilanguageData[selectedIndex].key, false);

                            multilanguageData.RemoveAt(selectedIndex);

                            Save();
                        }
                    });

                    // display the menu
                    menu.ShowAsContext();
                }

                EditorGUILayout.EndHorizontal();
            }
            if (EditorGUI.EndChangeCheck())
            {
                Save(activeLanguages[activeSystemLanguage]);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            inputKey = EditorGUILayout.TextField(inputKey);
            if (GUILayout.Button("Add"))
            {
                if (inputKey != string.Empty && multilanguageData.FindIndex(x => x.key == inputKey) == -1)
                {
                    AddWord(inputKey);

                    inputKey = string.Empty;

                    GUI.FocusControl(null);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void AddWord(string word)
        {
            InitRequiredWord(word, true);

            multilanguageData.Add(new LanguageKey(word));

            Save();
        }

        private void InitRequiredWord(string word, bool state)
        {
            int requiredWordIndex = requiredWords.FindIndex(x => x.word == word);
            if (requiredWordIndex != -1)
                requiredWords[requiredWordIndex].contains = state;
        }

        private void ShowToolsMenu()
        {
            GenericMenu toolsMenu = new GenericMenu();

            toolsMenu.AddItem(new GUIContent("Export"), false, delegate
            {
                Export();
            });
            toolsMenu.AddItem(new GUIContent("Import"), false, delegate
            {
                Import();
            });
            toolsMenu.AddSeparator("");
            toolsMenu.AddItem(new GUIContent("Sort by name"), false, delegate
            {
                List<LanguageKey> sortedList = multilanguageData.OrderBy(x => x.key).ToList();

                bool isSame = true;

                for (int i = 0; i < sortedList.Count; i++)
                {
                    if (sortedList[i].key != multilanguageData[i].key)
                    {
                        isSame = false;

                        break;
                    }
                }

                if (isSame)
                    multilanguageData = multilanguageData.OrderByDescending(x => x.key).ToList();
                else
                    multilanguageData = sortedList;

                Save();
            });
            toolsMenu.ShowAsContext();
            GUIUtility.ExitGUI();
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

        private void Export()
        {
            StringBuilder exportedData = new StringBuilder();

            exportedData.AppendLine("[" + activeLanguageNames[activeSystemLanguage] + "]");

            for (int i = 0; i < multilanguageData.Count; i++)
            {
                string word = "";

                int wordIndex = multilanguageData[i].values.FindIndex(x => x.language == activeLanguages[activeSystemLanguage]);
                if (wordIndex != -1)
                {
                    word = multilanguageData[i].values[wordIndex].value;
                }

                exportedData.AppendLine(multilanguageData[i].key + "=" + word);
            }

            string path = EditorUtility.SaveFilePanel("Save " + activeLanguageNames[activeSystemLanguage] + " language file", "", activeLanguageNames[activeSystemLanguage] + ".txt", "txt");

            if (!string.IsNullOrEmpty(path))
                File.WriteAllText(path, exportedData.ToString());
        }

        private void Import()
        {
            string path = EditorUtility.OpenFilePanel("Open language file", "", "txt");

            if (!string.IsNullOrEmpty(path))
            {
                string[] lines = File.ReadAllLines(path);

                try
                {
                    string languageName = Regex.Match(lines[0], @"\[([^)]*)\]").Result("$1");

                    Multilanguage.ProjectLanguages language = (Multilanguage.ProjectLanguages)System.Enum.Parse(typeof(Multilanguage.ProjectLanguages), languageName);

                    for (int i = 1; i < lines.Length; i++)
                    {
                        string[] values = lines[i].Split('=');

                        int keyIndex = multilanguageData.FindIndex(x => x.key == values[0]);
                        if (keyIndex != -1)
                        {
                            multilanguageData[keyIndex].SetOrAdd(language, values[1]);
                        }
                        else
                        {
                            multilanguageData.Add(new LanguageKey(values[0]).SetOrAdd(language, values[1]));
                        }
                    }

                    Save();
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private Dictionary<string, string> Read(Multilanguage.ProjectLanguages language)
        {
            Debug.Log(language.ToString());

            Dictionary<string, string> languageData = new Dictionary<string, string>();

            string[] lines = "".Split(new[] { '\r', '\n' });

            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(lines[i]))
                {
                    string[] entries = lines[i].Split(new char[] { '=' }, 2);

                    languageData.Add(entries[0], entries[1]);
                }
            }

            return languageData;
        }

        private class RequiredWord
        {
            public string word;
            public bool contains;

            public RequiredWord(string word, bool contains)
            {
                this.word = word;
                this.contains = contains;
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