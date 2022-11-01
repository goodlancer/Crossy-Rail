using UnityEngine;
using UnityEditor;
using Watermelon.Core;

namespace Watermelon
{
    [CustomEditor(typeof(Multilanguage))]
    sealed internal class MultilanguageEditor : Editor
    {
        private Multilanguage.ProjectLanguages[] languages;
        private Multilanguage multilanguageTarget;

        private MultilanguageSettings multilanguageSettings;

        private int selectedLanguage;

        private void OnEnable()
        {
            multilanguageSettings = EditorUtils.GetAsset<MultilanguageSettings>();
            languages = multilanguageSettings.ActiveLanguages();

            multilanguageTarget = (Multilanguage)target;
            selectedLanguage = PlayerPrefs.GetInt(Multilanguage.PREFS_KEY_NAME, (int)multilanguageSettings.defaultLanguage);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(12);

            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);
            for (int i = 0; i < languages.Length; i++)
            {
                int languageValue = (int)languages[i];

                GUIStyle guiStyle = EditorStylesExtended.button_01;
                if (languageValue == selectedLanguage)
                    guiStyle = EditorStylesExtended.button_03;

                if (GUILayout.Button(languages[i].ToString() + (multilanguageSettings.defaultLanguage == languages[i] ? " (default)" : ""), guiStyle))
                {
                    selectedLanguage = languageValue;

                    if (Application.isPlaying)
                    {
                        multilanguageTarget.SetLanguage(languages[i]);
                    }
                    else
                    {
                        PlayerPrefs.SetInt(Multilanguage.PREFS_KEY_NAME, languageValue);
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }
        
        [InitializeOnLoadMethod]
        public static void MultilanguageImporter()
        {
            IOUtils.CreatePath(ApplicationConsts.PROJECT_FOLDER + "/Resources/" + Multilanguage.TEXT_PATH);
            IOUtils.CreatePath(ApplicationConsts.PROJECT_FOLDER + "/Resources/" + Multilanguage.AUDIO_PATH);
            IOUtils.CreatePath(ApplicationConsts.PROJECT_FOLDER + "/Resources/" + Multilanguage.FONT_PATH);
        }
    }
}

