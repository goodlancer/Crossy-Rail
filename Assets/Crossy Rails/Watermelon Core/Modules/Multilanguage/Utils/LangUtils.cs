using UnityEngine;

namespace Watermelon
{
    public static class LangUtils
    {
        public static Multilanguage.ProjectLanguages GetSystemLanguage()
        {
            Multilanguage.ProjectLanguages defaultLanguage = Multilanguage.ProjectLanguages.English;

            try
            {
                defaultLanguage = (Multilanguage.ProjectLanguages)System.Enum.Parse(typeof(Multilanguage.ProjectLanguages), Application.systemLanguage.ToString(), true);
            }
            catch
            {
                try
                {
                    defaultLanguage = (Multilanguage.ProjectLanguages)System.Enum.Parse(typeof(Multilanguage.ProjectLanguages), GetDeviceDisplayLanguage(), true);
                }
                catch
                {
                    Debug.LogWarning("[Multilanguage]: Unable to get device language!");
                }
            }

            return defaultLanguage;
        }

        public static string GetDeviceDisplayLanguage()
        {
#if UNITY_EDITOR
            return Application.systemLanguage.ToString();
#elif UNITY_ANDROID
            AndroidJavaClass localeClass = new AndroidJavaClass ( "java/util/Locale" );
            AndroidJavaObject defaultLocale = localeClass.CallStatic<AndroidJavaObject> ( "getDefault" );
            AndroidJavaObject usLocale = localeClass.GetStatic<AndroidJavaObject> ( "US" );
            string systemLanguage = defaultLocale.Call<string> ( "getDisplayLanguage", usLocale );
            Debug.Log ( "Android language is " + systemLanguage + " detected as " + systemLanguage );
            return systemLanguage;
#else
            return "";
#endif
        }

        public static bool IsMuslim(string language)
        {
            string[] muslimLanguages = new string[]
            {
                "Arabic",
                "Albanian",
                "Bengali",
                "Indonesian",
                "Kyrgyz",
                "Kazakh",
                "Sinhala",
                "Turkish",
            };

            return System.Array.FindIndex(muslimLanguages, x => x == language) != -1;
        }

        public static bool IsMuslim(SystemLanguage language)
        {
            return IsMuslim(language.ToString());
        }
    }
}
