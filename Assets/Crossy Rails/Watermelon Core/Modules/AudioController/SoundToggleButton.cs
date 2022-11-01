#pragma warning disable 0649 

using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class SoundToggleButton : MonoBehaviour
    {
        [SerializeField]
        public Image graphic;

        [Space]
        [SerializeField]
        private Sprite audioOnIcon;
        [SerializeField]
        private Sprite audioOffIcon;

        [Space]
        [SerializeField]
        private Color activeColor = Color.white;
        [SerializeField]
        private Color disableColor = Color.white;

        private bool isActive = true;

        private void Start()
        {
            isActive = AudioController.GetSoundVolume() == 1.0f;

            if (isActive)
            {
                graphic.sprite = audioOnIcon;
                graphic.color = activeColor;
            }
            else
            {
                graphic.sprite = audioOffIcon;
                graphic.color = disableColor;
            }
        }

        public void SwitchState()
        {
            isActive = !isActive;

            if (isActive)
            {
                graphic.sprite = audioOnIcon;
                graphic.color = activeColor;

                AudioController.SetSoundVolume(1.0f);
            }
            else
            {
                graphic.sprite = audioOffIcon;
                graphic.color = disableColor;

                AudioController.SetSoundVolume(0.0f);
            }
        }
    }
}
