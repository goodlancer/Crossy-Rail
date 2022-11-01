#pragma warning disable 0649

using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [RequireComponent(typeof(Text))]
    public class MultilanguageTextProperties : MonoBehaviour
    {
        [SerializeField]
        [MultilanguageFont]
        private string font;

        [SerializeField]
        [HideInInspector]
        private MultilanguageFontSizeOverride[] fontSizeOverrides;

        private Text textComponent;

        private bool inited = false;
        private int defaultFontSize;

        public Text TextComponent
        {
            get
            {
                if (textComponent == null)
                    textComponent = GetComponent<Text>();

                return textComponent;
            }
        }


        private void Init()
        {
            inited = true;
            textComponent = GetComponent<Text>();

            defaultFontSize = textComponent.fontSize;
        }

        public TextProcessor textProcessor;

        public void InitWord(string key)
        {
            if (!inited)
            {
                Init();
            }

            textComponent.text = Multilanguage.GetWord(key);

            if (!string.IsNullOrEmpty(font))
            {
                textComponent.font = Multilanguage.GetFont(font);
            }

            int fontOverrideIndex = System.Array.FindIndex(fontSizeOverrides, x => x.language == Multilanguage.CurrentLanguage);
            if (fontOverrideIndex != -1)
            {
                textComponent.fontSize = fontSizeOverrides[fontOverrideIndex].fontSize;
            }
            else
            {
                textComponent.fontSize = defaultFontSize;
            }
        }

        public delegate string TextProcessor(string text);
    }
}