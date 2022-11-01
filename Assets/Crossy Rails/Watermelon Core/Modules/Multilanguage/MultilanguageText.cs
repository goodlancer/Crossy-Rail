#pragma warning disable 0649

using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [RequireComponent(typeof(Text))]
    public class MultilanguageText : MonoBehaviour
    {
        [SerializeField]
        [MultilanguageWord]
        private string key;

        [SerializeField]
        [MultilanguageFont]
        private string font;

        private void Start()
        {
            Text textComponent = GetComponent<Text>();

            textComponent.text = Multilanguage.GetWord(key);
            if (!string.IsNullOrEmpty(font))
            {
                textComponent.font = Multilanguage.GetFont(font);
            }

            Destroy(this);
        }
    }
}