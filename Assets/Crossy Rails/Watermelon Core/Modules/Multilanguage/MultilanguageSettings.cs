using System.Linq;
using UnityEngine;
using Watermelon.Core;

namespace Watermelon
{
    [SetupTab("Multilanguage", texture = "icon_language")]
    [CreateAssetMenu(fileName = "Multilanguage Settings", menuName = "Settings/Multilanguage Settings")]
    public class MultilanguageSettings : ScriptableObject
    {
        public Multilanguage.LanguageCase[] activeLanguages = new Multilanguage.LanguageCase[]
        {
            new Multilanguage.LanguageCase(Multilanguage.ProjectLanguages.English)
        };
        
        public Multilanguage.ProjectLanguages defaultLanguage = Multilanguage.ProjectLanguages.English;

        public bool IsLanguageActive(Multilanguage.ProjectLanguages language)
        {
            return System.Array.FindIndex(activeLanguages, x => x.projectLanguages == language) != -1;
        }
        
        public Multilanguage.ProjectLanguages[] ActiveLanguages()
        {
            return activeLanguages.Select(x => x.projectLanguages).ToArray();
        }
    }
}