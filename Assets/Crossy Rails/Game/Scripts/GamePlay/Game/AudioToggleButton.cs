#pragma warning disable 0649 

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Watermelon;

public class AudioToggleButton : MonoBehaviour
{
    public Type type;

    [Space(5)]
    public Sprite activeImage;
    public Sprite disabledImage;
    public Image imageRef;

    public enum Type
    {
        Music,
        Sound,
    }

    private bool isActive = true;

    public void Awake()
    {
        gameObject.SetActive(type == Type.Music ? AudioController.Settings.isMusicEnabled : AudioController.Settings.isAudioEnabled); 

        isActive = type == Type.Music ? AudioController.GetMusicVolume() == 1.0f : AudioController.GetSoundVolume() == 1.0f;

        if (isActive)
            imageRef.sprite = activeImage;
        else
            imageRef.sprite = disabledImage;
    }


    public void OnClick()
    {
        isActive = !isActive;

        if (isActive)
        {
            imageRef.sprite = activeImage;

            if (type == Type.Music)
            {
                AudioController.SetMusicVolume(1f);
            }
            else
            {
                AudioController.SetSoundVolume(1f);
            }
        }
        else
        {
            imageRef.sprite = disabledImage;

            if (type == Type.Music)
            {
                AudioController.SetMusicVolume(0f);
            }
            else
            {
                AudioController.SetSoundVolume(0f);
            }
        }

        AudioController.PlaySound(AudioController.Settings.sounds.buttonClip, AudioController.AudioType.Sound, 1f);
    }
}
