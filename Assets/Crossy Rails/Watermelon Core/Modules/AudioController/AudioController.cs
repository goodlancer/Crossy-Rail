using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using Watermelon.Core;

namespace Watermelon
{
    [Define("MODULE_VIBRATION")]
    public class AudioController : MonoBehaviour
    {
        private static AudioController instance;

        [SerializeField]
        private AudioSettings audioSettings;
        public static AudioSettings Settings
        {
            get { return instance.audioSettings; }
        }

        private List<AudioSource> audioSources = new List<AudioSource>();

        private List<AudioSource> activeSounds = new List<AudioSource>();
        private List<AudioSource> activeMusic = new List<AudioSource>();

        private List<AudioSource> customSources = new List<AudioSource>();
        private List<AudioCaseCustom> activeCustomSourcesCases = new List<AudioCaseCustom>();

        private const int AUDIO_SOURCES_AMOUNT = 4;
        private int lastSourceID = 0;
        
        private void Awake()
        {
            instance = this;

            if(audioSettings == null)
            {
                Debug.LogError("[AudioController]: Audio Settings is NULL!");

                return;
            }

            //Create audio source objects
            for (int i = 0; i < AUDIO_SOURCES_AMOUNT; i++)
            {
                audioSources.Add(CreateAudioSourceObject(false));
            }

            SceneLoader.OnSceneChanged += ReleaseStreams;
        }

        private void OnEnable()
        {
#if MODULE_PAUSE_MANAGER
            PauseManager.OnPauseStateChanged += PauseAudio;
#endif
        }

        private void OnDisable()
        {
#if MODULE_PAUSE_MANAGER
            PauseManager.OnPauseStateChanged -= PauseAudio;
#endif
        }

#if MODULE_PAUSE_MANAGER
        private void PauseAudio(bool state)
        {
            if (state)
            {
                int activeSoundsAmount = instance.activeSounds.Count;
                for (int i = 0; i < activeSoundsAmount; i++)
                {
                    instance.activeSounds[i].Pause();
                }

                int activeMusicAmount = instance.activeMusic.Count;
                for (int i = 0; i < activeMusicAmount; i++)
                {
                    instance.activeMusic[i].Pause();
                }

                int activeCusomSourcesAmount = instance.activeCustomSourcesCases.Count;
                for (int i = 0; i < activeCusomSourcesAmount; i++)
                {
                    instance.activeCustomSourcesCases[i].source.Pause();
                }
            }
            else
            {
                int activeSoundsAmount = instance.activeSounds.Count;
                for (int i = 0; i < activeSoundsAmount; i++)
                {
                    instance.activeSounds[i].UnPause();
                }

                int activeMusicAmount = instance.activeMusic.Count;
                for (int i = 0; i < activeMusicAmount; i++)
                {
                    instance.activeMusic[i].UnPause();
                }

                int activeCusomSourcesAmount = instance.activeCustomSourcesCases.Count;
                for (int i = 0; i < activeCusomSourcesAmount; i++)
                {
                    instance.activeCustomSourcesCases[i].source.UnPause();
                }
            }
        }
#endif

        public static void PlayRandomMusic()
        {
            PlayMusic(instance.audioSettings.musicAudioClips.GetRandomItem());
        }

        /// <summary>
        /// Stop all active streams
        /// </summary>
        public static void ReleaseStreams()
        {
            ReleaseMusic();
            ReleaseSounds();
            ReleaseCustomStreams();
        }

        /// <summary>
        /// Releasing all active music.
        /// </summary>
        public static void ReleaseMusic()
        {
            int activeMusicCount = instance.activeMusic.Count - 1;
            for (int i = activeMusicCount; i >= 0; i--)
            {
                instance.activeMusic[i].Stop();
                instance.activeMusic[i].clip = null;
                instance.activeMusic.RemoveAt(i);
            }
        }

        /// <summary>
        /// Releasing all active sounds.
        /// </summary>
        public static void ReleaseSounds()
        {
            int activeStreamsCount = instance.activeSounds.Count - 1;
            for (int i = activeStreamsCount; i >= 0; i--)
            {
                instance.activeSounds[i].Stop();
                instance.activeSounds[i].clip = null;
                instance.activeSounds.RemoveAt(i);
            }
        }

        /// <summary>
        /// Releasing all active custom sources.
        /// </summary>
        public static void ReleaseCustomStreams()
        {
            int activeStreamsCount = instance.activeCustomSourcesCases.Count - 1;
            for (int i = activeStreamsCount; i >= 0; i--)
            {
                if (instance.activeCustomSourcesCases[i].autoRelease)
                {
                    AudioSource source = instance.activeCustomSourcesCases[i].source;
                    instance.activeCustomSourcesCases[i].source.Stop();
                    instance.activeCustomSourcesCases[i].source.clip = null;
                    instance.activeCustomSourcesCases.RemoveAt(i);
                    instance.customSources.Add(source);
                }
            }
        }

        public static void StopStream(AudioCase audioCase, float fadeTime = 0)
        {
            if (audioCase.type == AudioType.Sound)
            {
                instance.StopSound(audioCase.source, fadeTime);
            }
            else
            {
                instance.StopMusic(audioCase.source, fadeTime);
            }
        }

        public static void StopStream(AudioCaseCustom audioCase, float fadeTime = 0)
        {
            ReleaseCustomSource(audioCase, fadeTime);
        }

        private void StopSound(AudioSource source, float fadeTime = 0)
        {
            int streamID = activeSounds.FindIndex(x => x == source);
            if (streamID != -1)
            {
                if (fadeTime == 0)
                {
                    activeSounds[streamID].Stop();
                    activeSounds[streamID].clip = null;
                    activeSounds.RemoveAt(streamID);
                }
                else
                {
                    activeSounds[streamID].DOVolume(0f, fadeTime).OnComplete(() =>
                    {
                        activeSounds.Remove(source);
                        source.Stop();
                    });
                }
            }
        }

        private void StopMusic(AudioSource source, float fadeTime = 0)
        {
            int streamID = activeMusic.FindIndex(x => x == source);
            if (streamID != -1)
            {
                if (fadeTime == 0)
                {
                    activeMusic[streamID].Stop();
                    activeMusic[streamID].clip = null;
                    activeMusic.RemoveAt(streamID);
                }
                else
                {
                    activeMusic[streamID].DOVolume(0f, fadeTime).OnComplete(() =>
                    {
                        activeMusic.Remove(source);
                        source.Stop();
                    });
                }
            }
        }

        private static void AddMusic(AudioSource source)
        {
            if (!instance.activeMusic.Contains(source))
            {
                instance.activeMusic.Add(source);
            }
        }

        private static void AddSound(AudioSource source)
        {
            if (!instance.activeSounds.Contains(source))
            {
                instance.activeSounds.Add(source);
            }
        }


        public static void PlayMusic(AudioClip clip, float volumePercentage = 1.0f)
        {
            if (!instance.audioSettings.isMusicEnabled)
                return;

            if (clip == null)
                Debug.LogError("[AudioController]: Audio clip is null");

            AudioSource source = instance.GetAudioSource();

            SetSourceDefaultSettings(source, AudioType.Music);

            source.volume *= volumePercentage;
            source.clip = clip;
            source.Play();

            AddMusic(source);
        }

        public static AudioCase PlaySmartMusic(AudioClip clip, float volumePercentage = 1.0f)
        {
            if (!instance.audioSettings.isMusicEnabled)
                return null;

            if (clip == null)
                Debug.LogError("[AudioController]: Audio clip is null");

            AudioSource source = instance.GetAudioSource();

            SetSourceDefaultSettings(source, AudioType.Music);

            source.volume *= volumePercentage;
            source.clip = clip;

            AudioCase audioCase = new AudioCase(clip, source, AudioType.Music);

            audioCase.Play();

            AddMusic(source);

            return audioCase;
        }

        public static void PlaySound(AudioClip clip, AudioType type = AudioType.Sound, float volumePercentage = 1.0f)
        {
            if (!instance.audioSettings.isAudioEnabled)
                return;

            if (clip == null)
                Debug.LogError("[AudioController]: Audio clip is null");

            AudioSource source = instance.GetAudioSource();

            SetSourceDefaultSettings(source, type);

            source.volume *= volumePercentage;
            source.clip = clip;
            source.Play();

            AddSound(source);
        }

        public static void PlaySound(AudioClip clip, AudioType type = AudioType.Sound, float volumePercentage = 1.0f, float pitch = 1.0f)
        {
            if (!instance.audioSettings.isAudioEnabled)
                return;

            if (clip == null)
                Debug.LogError("[AudioController]: Audio clip is null");

            AudioSource source = instance.GetAudioSource();

            SetSourceDefaultSettings(source, type);

            source.volume *= volumePercentage;
            source.clip = clip;
            source.pitch = pitch;
            source.Play();

            AddSound(source);
        }

        public static AudioCase PlaySmartSound(AudioClip clip, AudioType type = AudioType.Sound)
        {
            if (!instance.audioSettings.isAudioEnabled)
                return null;

            if (clip == null)
                Debug.LogError("[AudioController]: Audio clip is null");

            AudioSource source = instance.GetAudioSource();

            SetSourceDefaultSettings(source, type);

            AudioCase audioCase = new AudioCase(clip, source, type);
            audioCase.Play();

            AddSound(source);

            return audioCase;
        }

        public static AudioCaseCustom GetCustomSource(bool autoRelease, AudioType audioType = AudioType.Sound)
        {
            AudioSource source = null;

            if (!instance.customSources.IsNullOrEmpty())
            {
                source = instance.customSources[0];
                instance.customSources.RemoveAt(0);
            }
            else
            {
                source = instance.CreateAudioSourceObject(true);
            }

            SetSourceDefaultSettings(source, audioType);

            AudioCaseCustom audioCase = new AudioCaseCustom(null, source, AudioType.Sound, autoRelease);

            instance.activeCustomSourcesCases.Add(audioCase);

            return audioCase;
        }

        public static void ReleaseCustomSource(AudioCaseCustom audioCase, float fadeTime = 0)
        {
            int streamID = instance.activeCustomSourcesCases.FindIndex(x => x.source == audioCase.source);
            if (streamID != -1)
            {
                if (fadeTime == 0)
                {
                    instance.activeCustomSourcesCases[streamID].source.Stop();
                    instance.activeCustomSourcesCases[streamID].source.clip = null;
                    instance.activeCustomSourcesCases.RemoveAt(streamID);
                    instance.customSources.Add(audioCase.source);
                }
                else
                {
                    instance.activeCustomSourcesCases[streamID].source.DOVolume(0f, fadeTime).OnComplete(() =>
                    {
                        instance.activeCustomSourcesCases.Remove(audioCase);
                        audioCase.source.Stop();
                        instance.customSources.Add(audioCase.source);
                    });
                }
            }
        }
        
        private AudioSource GetAudioSource()
        {
            int sourcesAmount = audioSources.Count;
            for (int i = 0; i < sourcesAmount; i++)
            {
                if (!audioSources[i].isPlaying)
                {
                    return audioSources[i];
                }
            }

            AudioSource createdSource = CreateAudioSourceObject(false);
            audioSources.Add(createdSource);

            return createdSource;
        }
        
        private AudioSource CreateAudioSourceObject(bool isCustom)
        {
            lastSourceID++;

            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            SetSourceDefaultSettings(audioSource);

            return audioSource;
        }

        public static void SetMusicVolume(float volume)
        {
            GameSettingsPrefs.Set("music", volume);

            instance.SetVolumeForAudioType(volume, AudioType.Music);
        }

        public static void SetSoundVolume(float volume)
        {
            GameSettingsPrefs.Set("sound", volume);

            instance.SetVolumeForAudioType(volume, AudioType.Sound);
        }

        private void SetVolumeForAudioType(float volume, AudioType type)
        {
            List<AudioSource> activeSourcesList = type == AudioType.Sound ? activeSounds : activeMusic;

            // setuping all active sound sources
            int activeSoundSourcesCount = activeSourcesList.Count;
            for (int i = 0; i < activeSoundSourcesCount; i++)
            {
                activeSourcesList[i].volume = volume;
            }

            // setuping all custom sound sources
            int activeCustomSourcesCount = activeCustomSourcesCases.Count;
            for (int i = 0; i < activeCustomSourcesCount; i++)
            {
                if (activeCustomSourcesCases[i].type == type)
                {
                    activeCustomSourcesCases[i].source.volume = volume;
                }
            }
        }

        public static float GetMusicVolume()
        {
            return GameSettingsPrefs.Get<float>("music");
        }

        public static float GetSoundVolume()
        {
            return GameSettingsPrefs.Get<float>("sound");
        }

        public static bool IsVibrationEnabled()
        {
#if MODULE_VIBRATION
            if (GameSettingsPrefs.Get<bool>("vibration"))
            {
                return true;
            }
#endif

            return false;
        }

        public static void SetSourceDefaultSettings(AudioSource source, AudioType type = AudioType.Sound)
        {
            float volume = 1.0f;

            if (type == AudioType.Sound)
            {
                volume = GameSettingsPrefs.Get<float>("sound");
                source.loop = false;
            }
            else if (type == AudioType.Music)
            {
                volume = GameSettingsPrefs.Get<float>("music");
                source.loop = true;
            }

            source.clip = null;

            source.volume = volume;
            source.pitch = 1.0f;
            source.spatialBlend = 0; // 2D Sound
            source.mute = false;
            source.playOnAwake = false;
            source.outputAudioMixerGroup = null;
        }

        public static void LoadAudioClipFromStreamingAssets(string fileName, Action<AudioClip> OnAudioLoaded)
        {
            instance.StartCoroutine(instance.LoadAudioClipFromSACoroutine(fileName, OnAudioLoaded));
        }

        private IEnumerator LoadAudioClipFromSACoroutine(string fileName, Action<AudioClip> OnAudioLoaded)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Application.streamingAssetsPath);
            FileInfo[] allFiles = directoryInfo.GetFiles("*.wav");

            foreach (FileInfo file in allFiles)
            {
                if (file.Name.Contains(fileName))
                {
                    if (file.Name.Contains("meta"))
                    {
                        yield break;
                    }
                    else
                    {
                        string musicFilePath = file.FullName.ToString();
                        string url = string.Format("file://{0}", musicFilePath);

                        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, UnityEngine.AudioType.WAV))
                        {
                            www.SendWebRequest();

                            while (!www.isDone)
                            {
                                yield return null;
                            }

                            if (www.isNetworkError)
                            {
                                Debug.Log(www.error);
                            }
                            else
                            {
                                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                                clip.name = fileName;

                                if (OnAudioLoaded != null)
                                {
                                    OnAudioLoaded(clip);
                                }
                            }
                        }
                    }
                }
            }
        }
        
        public enum AudioType
        {
            Music = 0,
            Sound = 1
        }
    }
}