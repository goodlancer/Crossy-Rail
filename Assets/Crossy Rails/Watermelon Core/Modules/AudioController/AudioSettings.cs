using UnityEngine;
using Watermelon.Core;

namespace Watermelon
{
    [SetupTab("Audio", texture = "icon_audio")]
    [CreateAssetMenu(fileName = "Audio Settings", menuName = "Settings/Audio Settings")]
    public class AudioSettings : ScriptableObject
    {
        [System.Serializable]
        public class Sounds
        {
            [Tooltip("Mining cart movement sound")]
            public AudioClip cartMovementClip;
            [Tooltip("Swipe sound")]
            public AudioClip swipeClip;
            [Tooltip("Button click sound")]
            public AudioClip buttonClip;
        }

        [System.Serializable]
        public class Vibrations
        {
            public int shortVibration = 20;
            public int longVibration = 60;
        }
        
        public bool isMusicEnabled = true;
        public bool isAudioEnabled = true;
        
        public AudioClip[] musicAudioClips;
        
        public Sounds sounds;
        public Vibrations vibrations;
    }
}