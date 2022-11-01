#if MODULE_AUDIO_NATIVE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using E7.Native;

namespace Watermelon
{
    [System.Serializable]
    public class NativeAudioCase
    {
        public string audioName;
        public AudioController.AudioType audioType;

#if UNITY_EDITOR
        private AudioCaseCustom customAudioCase;
#elif UNITY_ANDROID
    private NativeAudioPointer nativeAudioPointer;
    //private NativeAudioController controller;
    //private NativeAudio.PlayOptions playOptions;

    private bool audioEnabled;
#endif

        public NativeAudioCase(string fileName, AudioController.AudioType type)
        {
            audioName = fileName;
            audioType = type;

#if UNITY_EDITOR
            AudioController.LoadAudioClipFromStreamingAssets(fileName, OnClipLoaded);
#elif UNITY_ANDROID
        float currentVolume = type == AudioController.AudioType.Sound ? AudioController.GetSoundVolume() : AudioController.GetMusicVolume();
        //var adjustment = new NativeAudio.PlayAdjustment { volume = currentVolume, pan = 1f };
        //playOptions = new NativeAudio.PlayOptions { playAdjustment = adjustment };
        //Debug.Log("Created native audio case for:" + fileName + "  volume: " + currentVolume);

        audioEnabled = currentVolume == 0 ? false : true;

        nativeAudioPointer = NativeAudio.Load(fileName);
#endif
        }

#if UNITY_EDITOR
        private void OnClipLoaded(AudioClip audioClip)
        {
            customAudioCase = AudioController.GetCustomSource(false, audioType);

            customAudioCase.source.clip = audioClip;
        }
#endif

        public void Play()
        {
#if UNITY_EDITOR
            if (customAudioCase != null)
            {
                customAudioCase.Play();
            }
#elif UNITY_ANDROID
        if (audioEnabled)
        {
            /*controller = */nativeAudioPointer.Play(/*playOptions*/);
        }
#endif
        }

        // !NOTE! now supports only enabled/disabled sound (1/0 volume)
        public void SetVolume(float volumeToSet)
        {
#if !UNITY_EDITOR && UNITY_ANDROID

        audioEnabled = volumeToSet == 0 ? false : true;

        //var adjustment = new NativeAudio.PlayAdjustment { volume = volumeToSet, pan = 1f };
        //playOptions = new NativeAudio.PlayOptions { playAdjustment = adjustment };

        //Debug.Log("NA set volume: " + volumeToSet + "  for " + audioName);

        //if(controller != null)
        //{
        //controller.SetVolume(volumeToSet);
        //}
#endif
        }
    }
}
#endif