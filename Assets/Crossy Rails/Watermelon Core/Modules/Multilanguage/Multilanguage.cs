#pragma warning disable 0649

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon.Core;

namespace Watermelon
{
    //Multilanguage version 1.0.2
    public sealed class Multilanguage : MonoBehaviour
    {
        private static Multilanguage instance;

        //Path to text and audio folders (Resources/)
        public const string TEXT_PATH = "Language/Text/";
        public const string AUDIO_PATH = "Language/Audio/";
        public const string FONT_PATH = "Language/Fonts/";

        /// <summary>
        /// Player prefs language key
        /// </summary>
        public const string PREFS_KEY_NAME = "settings.Language";

        public static readonly string[] REQUIRED_FONTS = new string[]
        {
            "default",
        };
        
        public static OnLanguageChangedCallback onLanguageChanged;

        private readonly Dictionary<ProjectLanguages, ProjectLanguages> linkLanguages = new Dictionary<ProjectLanguages, ProjectLanguages>()
        {
            //CIS Countries
            { ProjectLanguages.Belarusian, ProjectLanguages.Russian  },

            { ProjectLanguages.Catalan, ProjectLanguages.Spanish  },
            { ProjectLanguages.Chinese, ProjectLanguages.ChineseSimplified }
        };

        /// <summary>
        /// Current language
        /// </summary>
        private ProjectLanguages currentLanguage;
        public static ProjectLanguages CurrentLanguage
        {
            set { instance.SetLanguage(value); }
            get { return instance.currentLanguage; }
        }

        /// <summary>
        /// All loaded words
        /// </summary>
        private Dictionary<string, string> loadedWords = new Dictionary<string, string>();

        /// <summary>
        /// All loaded audio
        /// </summary>
        private Dictionary<string, AudioClip> loadedAudio = new Dictionary<string, AudioClip>();

        private Dictionary<string, Font> loadedFonts = new Dictionary<string, Font>();

        [SerializeField]
        private MultilanguageSettings multilanguageSettings;

        private void Awake()
        {
#if UNITY_EDITOR
            if(multilanguageSettings.activeLanguages.IsNullOrEmpty())
            {
                Debug.LogError("[Multilanguage]: There are no active languages!");
            }
#endif

            instance = this;

            //On start check if any language saved in player prefs
            if (!PlayerPrefs.HasKey(PREFS_KEY_NAME))
            {
                ProjectLanguages systemSelectedLanguage = LangUtils.GetSystemLanguage();

                if (linkLanguages.ContainsKey(systemSelectedLanguage))
                    systemSelectedLanguage = linkLanguages[systemSelectedLanguage];

                if (!IsLanguageActive(systemSelectedLanguage))
                    systemSelectedLanguage = multilanguageSettings.defaultLanguage;

                SetLanguage(systemSelectedLanguage);
            }
            else
            {
                ProjectLanguages projectLanguages = (ProjectLanguages)PlayerPrefs.GetInt(PREFS_KEY_NAME);

                if (!IsLanguageActive(projectLanguages))
                    projectLanguages = multilanguageSettings.defaultLanguage;

                SetLanguage(projectLanguages);
            }
        }

        /// <summary>
        /// Get word by the key
        /// </summary>
        public static string GetWord(string key)
        {
            if (key == string.Empty)
            {
                return string.Empty;
            }
            else
            {
                return instance.loadedWords[key];
            }
        }

        /// <summary>
        /// Get audio clip by the key
        /// </summary>
        public static AudioClip GetAudio(string key)
        {
            return instance.loadedAudio[key];
        }

        /// <summary>
        /// Get font by the name
        /// </summary>
        public static Font GetFont(string name)
        {
            if (instance.loadedFonts.ContainsKey(name))
            {
                return instance.loadedFonts[name];
            }

            return null;
        }

        /// <summary>
        /// Get font by the name and language
        /// </summary>
        public static Font GetFont(string name, ProjectLanguages language)
        {
#if UNITY_EDITOR
            if (!instance.loadedFonts.ContainsKey(name))
            {
                Debug.LogError("[Multilanguage]: Wrong font name! - " + name);

                return null;
            }
#endif

            Font languageFont = Resources.Load<Font>(FONT_PATH + language + "/" + name);
            if (languageFont == null)
            {
                languageFont = Resources.Load<Font>(FONT_PATH + instance.multilanguageSettings.defaultLanguage + "/" + name);
            }

            return languageFont;
        }

        /// <summary>
        /// Change current language
        /// </summary>
        public void SetLanguage(ProjectLanguages language)
        {
            if (IsLanguageActive(language))
            {
                Debug.Log("[Multilanguage]: Selected language - " + language.ToString());

                PlayerPrefs.SetInt(PREFS_KEY_NAME, (int)language);

                currentLanguage = language;

                LoadResources(language);

                if (onLanguageChanged != null)
                    onLanguageChanged.Invoke();
            }
        }

        public static void ChangeLanguage(ProjectLanguages language)
        {
            instance.SetLanguage(language);
        }

        /// <summary>
        /// Load words and audio from resources 
        /// </summary>
        private void LoadResources(ProjectLanguages language)
        {
            //Load words
            loadedWords = GetWords(language);
            
            Debug.Log("[Multilanguage]: Loaded " + loadedWords.Count + " words!");

            //Load audio
            loadedAudio = new Dictionary<string, AudioClip>();
            AudioClip[] languageAudio = Resources.LoadAll<AudioClip>(AUDIO_PATH + language + "/");
            
            Debug.Log("[Multilanguage]: Loaded " + languageAudio.Length + " audio files!");

            for (int i = 0; i < languageAudio.Length; i++)
            {
                loadedAudio.Add(languageAudio[i].name, languageAudio[i]);
            }

            //Load fonts
            loadedFonts = new Dictionary<string, Font>();
            for (int i = 0; i < REQUIRED_FONTS.Length; i++)
            {
                Font languageFont = Resources.Load<Font>(FONT_PATH + language + "/" + REQUIRED_FONTS[i]);
                if (languageFont == null)
                {
                    languageFont = Resources.Load<Font>(FONT_PATH + multilanguageSettings.defaultLanguage + "/" + REQUIRED_FONTS[i]);
                }

                loadedFonts.Add(REQUIRED_FONTS[i], languageFont);
            }
        }

        public static Dictionary<string, string> GetWords(ProjectLanguages language)
        {
            Dictionary<string, string> words = new Dictionary<string, string>();
            TextAsset languageText = Resources.Load(TEXT_PATH + language) as TextAsset;
            if (languageText != null)
            {
                string[] lines = languageText.text.Split(new[] { '\r', '\n' });

                for (int i = 1; i < lines.Length; i++)
                {
                    if (!string.IsNullOrEmpty(lines[i]))
                    {
                        string[] entries = lines[i].Split(new char[] { '=' }, 2);

                        words.Add(entries[0], entries[1]);
                    }
                }
            }

            return words;
        }

        /// <summary>
        /// Get language and check if it exist
        /// </summary>
        private bool IsLanguageActive(ProjectLanguages language)
        {
            if (!multilanguageSettings.IsLanguageActive(language))
            {
                //Debug.LogError("Language " + language.ToString() + " doesn't exist!");

                return false;
            }

            return true;
        }

        public delegate void OnLanguageChangedCallback();

        public enum ProjectLanguages
        {
            Afrikaans = 0,
            Arabic = 1,
            Basque = 2,
            Belarusian = 3,
            Bulgarian = 4,
            Catalan = 5,
            Chinese = 6,
            Czech = 7,
            Danish = 8,
            Dutch = 9,
            English = 10,
            Estonian = 11,
            Faroese = 12,
            Finnish = 13,
            French = 14,
            German = 15,
            Greek = 16,
            Hebrew = 17,
            Hugarian = 18,
            Hungarian = 18,
            Icelandic = 19,
            Indonesian = 20,
            Italian = 21,
            Japanese = 22,
            Korean = 23,
            Latvian = 24,
            Lithuanian = 25,
            Norwegian = 26,
            Polish = 27,
            Portuguese = 28,
            Romanian = 29,
            Russian = 30,
            SerboCroatian = 31,
            Slovak = 32,
            Slovenian = 33,
            Spanish = 34,
            Swedish = 35,
            Thai = 36,
            Turkish = 37,
            Ukrainian = 38,
            Vietnamese = 39,
            ChineseSimplified = 40,
            ChineseTraditional = 41,
            Hindi = 42,
            Telugu = 43,
            Bangla = 44,
            Unknown = 45
        }

        [System.Serializable]
        public class LanguageCase
        {
            public ProjectLanguages projectLanguages;

            public LanguageCase(ProjectLanguages projectLanguages)
            {
                this.projectLanguages = projectLanguages;
            }
        }
    }
}
//Changelog
//v1.0.0 - base multilanguage system
//v1.0.1 - load words from ScriptableObject (LanguageText), export and import words data
//v1.0.1 - added link languages
//v1.0.2 - remove MODULE_MULTILANGUAGE define