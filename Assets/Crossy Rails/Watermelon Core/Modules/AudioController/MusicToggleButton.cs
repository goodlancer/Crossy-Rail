#pragma warning disable 0649 

using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class MusicToggleButton : MonoBehaviour
    {
        [SerializeField]
        public Image graphic;

        [Space]
        [SerializeField]
        private Sprite musicOnIcon;
        [SerializeField]
        private Sprite musicOffIcon;

        [Space]
        [SerializeField]
        private Color activeColor = Color.white;
        [SerializeField]
        private Color disableColor = Color.white;

        private bool isActive = true;

        private void Start()
        {
            isActive = AudioController.GetMusicVolume() == 1.0f;

            if (isActive)
                graphic.color = activeColor;
            else
                graphic.color = disableColor;
        }

        public void SwitchState()
        {
            isActive = !isActive;

            if (isActive)
            {
                graphic.sprite = musicOnIcon;
                graphic.color = activeColor;

                AudioController.SetMusicVolume(1.0f);
            }
            else
            {
                graphic.sprite = musicOffIcon;
                graphic.color = disableColor;

                AudioController.SetMusicVolume(0.0f);
            }
        }
    }
}
