using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Watermelon.Core;

namespace Watermelon
{
    public class MultilanguageFileMerger : EditorWindow
    {
        private Multilanguage.ProjectLanguages selectedLanguage = Multilanguage.ProjectLanguages.Unknown;

        private string firstFilePath;
        private string secondFilePath;

        [MenuItem("Tools/Editor/Multilanguage File Merger")]
        public static void InitWindow()
        {
            MultilanguageFileMerger window = (MultilanguageFileMerger)GetWindow(typeof(MultilanguageFileMerger), false, "Multilanguage File Merger");
            window.Show();
        }

        public void OnGUI()
        {
            selectedLanguage = (Multilanguage.ProjectLanguages)EditorGUILayout.EnumPopup("Selected Language: ", selectedLanguage);

            if (selectedLanguage != Multilanguage.ProjectLanguages.Unknown)
            {
                firstFilePath = EditorGUILayoutCustom.FileField(new GUIContent("Keys: "), firstFilePath, "", "txt");
                secondFilePath = EditorGUILayoutCustom.FileField(new GUIContent("Values: "), secondFilePath, "", "txt");

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Merge"))
                {
                    Debug.Log(firstFilePath);
                    Debug.Log(secondFilePath);

                    if (File.Exists(firstFilePath) && File.Exists(secondFilePath))
                    {
                        string[] keys = File.ReadAllLines(firstFilePath);
                        string[] words = File.ReadAllLines(secondFilePath);

                        Debug.Log(keys.Length);
                        Debug.Log(words.Length);

                        if (keys.Length == words.Length)
                        {
                            string[] result = new string[keys.Length + 1];

                            result[0] = "[" + selectedLanguage + "]";
                            for (int i = 0; i < keys.Length; i++)
                            {
                                result[i + 1] = keys[i] + "=" + words[i];
                            }

                            Debug.Log(Application.dataPath + ApplicationConsts.PROJECT_FOLDER + "/Resources/" + Multilanguage.TEXT_PATH + selectedLanguage.ToString() + ".txt");

                            File.WriteAllLines(Application.dataPath + ApplicationConsts.PROJECT_FOLDER + "/Resources/" + Multilanguage.TEXT_PATH + selectedLanguage.ToString() + ".txt", result);
                        }
                    }
                }
            }
        }
    }
}