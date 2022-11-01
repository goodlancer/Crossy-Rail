#pragma warning disable 0649 

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System;
using Watermelon.Core;

namespace Watermelon
{
    //Current version v1.0.2
    public class SceneLoader : MonoBehaviour
    {
        private static SceneLoader instance;

        [ValidateInput("ValidateFirstScene")]
        [SerializeField]
        private Scenes firstScene;
        [SerializeField]
        private Scenes loadingScene;

        private static Scenes currentScene;
        private Scenes prevScene;

        public static Scenes CurrentScene
        {
            get { return currentScene; }
#if UNITY_EDITOR
            set { currentScene = value; }
#endif
        }

        public static Scenes PrevScene
        {
            get { return instance.prevScene; }
        }

        [SerializeField]
        private Image fadePanel;

        [LineSpacer("Animation")]
        [SerializeField]
        private Animator transitionAnimator;

        private List<SceneEvent> sceneOpenEvents = new List<SceneEvent>();
        private List<SceneEvent> sceneLeaveEvents = new List<SceneEvent>();

        public static Action<float> onAsyncLoadProgressChanged;

        private const string ANIMATION_FADE_IN = "FadeIn";
        private const string ANIMATION_FADE_OUT = "FadeOut";

        public delegate void SceneLoaderCallback();

        private SceneLoaderCallback onSceneChanged;
        public static SceneLoaderCallback OnSceneChanged
        {
            get { return instance.onSceneChanged; }
            set { instance.onSceneChanged = value; }
        }

        private void Awake()
        {
            instance = this;

            DontDestroyOnLoad(gameObject);

            SceneManager.activeSceneChanged += OnActiveSceneChanged;

#if !UNITY_EDITOR
            if (loadingScene != Scenes.Init)
            {
                LoadSceneWithLoadingScene(firstScene, loadingScene);

                return;
            }

            LoadScene(firstScene);
#else
            if (SceneManager.sceneCount == 1)
                LoadScene(firstScene);
#endif
        }

        public static void OnSceneOpened(Scenes scene, SceneCallback callback, bool callOnce = false)
        {
            instance.sceneOpenEvents.Add(new SceneEvent(scene, callback, callOnce));
        }

        public static void OnSceneLeave(Scenes scene, SceneCallback callback, bool callOnce = false)
        {
            instance.sceneLeaveEvents.Add(new SceneEvent(scene, callback, callOnce));
        }

        private void OnActiveSceneChanged(Scene prevScene, Scene currentScene)
        {
            int eventsCount = sceneOpenEvents.Count;
            for (int i = eventsCount - 1; i >= 0; i--)
            {
                if (sceneOpenEvents[i].scene == currentScene.name)
                {
                    sceneOpenEvents[i].callback.Invoke();

                    if (sceneOpenEvents[i].callOnce)
                        sceneOpenEvents.RemoveAt(i);
                }
            }
        }

        public static void ReloadScene(SceneTransition transition = SceneTransition.Fade)
        {
            string currentSceneName = currentScene.ToString();

            int eventsCount = instance.sceneLeaveEvents.Count;
            for (int i = eventsCount - 1; i >= 0; i--)
            {
                if (instance.sceneLeaveEvents[i].scene == currentSceneName)
                {
                    instance.sceneLeaveEvents[i].callback.Invoke();

                    if (instance.sceneLeaveEvents[i].callOnce)
                        instance.sceneLeaveEvents.RemoveAt(i);
                }
            }
            
            Debug.Log("[SceneLoader] Loading scene: " + currentSceneName);

            if (transition == SceneTransition.Fade)
            {
                if (instance.onSceneChanged != null)
                {
                    instance.onSceneChanged();
                }

                FadePanel(() => SceneManager.LoadScene(currentSceneName, LoadSceneMode.Single));
            }
            else if (transition == SceneTransition.Animation)
            {
                if (instance.onSceneChanged != null)
                {
                    instance.onSceneChanged();
                }

                FadeAnimation(() => SceneManager.LoadScene(currentSceneName, LoadSceneMode.Single));
            }
            else
            {
                if (instance.onSceneChanged != null)
                {
                    instance.onSceneChanged();
                }

                SceneManager.LoadScene(currentSceneName, LoadSceneMode.Single);
            }
        }

        public static void LoadScene(Scenes scene, SceneTransition transition = SceneTransition.Fade)
        {
            string currentSceneName = currentScene.ToString();

            int eventsCount = instance.sceneLeaveEvents.Count;
            for (int i = eventsCount - 1; i >= 0; i--)
            {
                if (instance.sceneLeaveEvents[i].scene == currentSceneName)
                {
                    instance.sceneLeaveEvents[i].callback.Invoke();

                    if (instance.sceneLeaveEvents[i].callOnce)
                        instance.sceneLeaveEvents.RemoveAt(i);
                }
            }
            
            Debug.Log("[SceneLoader] Loading scene: " + currentSceneName);

            if (transition == SceneTransition.Fade)
            {
                FadePanel(delegate
                {
                    Tween.RemoveAll();

                    if (instance.onSceneChanged != null)
                    {
                        instance.onSceneChanged();
                    }

                    SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);
                });
            }
            else if (transition == SceneTransition.Animation)
            {
                if (instance.onSceneChanged != null)
                {
                    instance.onSceneChanged();
                }

                FadeAnimation(() => SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single));
            }
            else
            {
                if (instance.onSceneChanged != null)
                {
                    instance.onSceneChanged();
                }

                SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);
            }

            instance.prevScene = currentScene;
            currentScene = scene;
        }

        public static void LoadSceneWithLoadingScene(Scenes scene, Scenes loadingScene, SceneTransition transition = SceneTransition.Fade)
        {
            string currentSceneName = currentScene.ToString();

            int eventsCount = instance.sceneLeaveEvents.Count;
            for (int i = eventsCount - 1; i >= 0; i--)
            {
                if (instance.sceneLeaveEvents[i].scene == currentSceneName)
                {
                    instance.sceneLeaveEvents[i].callback.Invoke();

                    if (instance.sceneLeaveEvents[i].callOnce)
                        instance.sceneLeaveEvents.RemoveAt(i);
                }
            }
            
            Debug.Log("[SceneLoader] Loading scene: " + currentSceneName);

            instance.StartCoroutine(instance.AsyncLoading(scene, loadingScene, transition));
        }

        private IEnumerator AsyncLoading(Scenes scene, Scenes loadingScene, SceneTransition transition = SceneTransition.Fade)
        {
            SceneManager.LoadScene(loadingScene.ToString(), LoadSceneMode.Single);

            yield return new WaitForSeconds(2.0f);

            //AsyncOperation asyncLoading = SceneManager.LoadSceneAsync(loadingScene.ToString(), LoadSceneMode.Single);
            //asyncLoading.allowSceneActivation = false;

            //if (transition == SceneTransition.None)
            //{
            //    asyncLoading.allowSceneActivation = true;
            //}
            //else
            //{
            //    FadePanel(delegate
            //    {
            //        asyncLoading.allowSceneActivation = true;
            //    });
            //}

            //yield return new WaitUntil(() => !asyncLoading.isDone);
            
            Debug.Log("[SceneLoader] Loading scene: " + scene);

            AsyncOperation async = SceneManager.LoadSceneAsync(scene.ToString(), LoadSceneMode.Single);
            async.allowSceneActivation = false;
            while (async.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(async.progress / 0.9f);
                //Debug.Log("Loading progress: " + (progress * 100) + "%");

                if (onAsyncLoadProgressChanged != null)
                    onAsyncLoadProgressChanged.Invoke(progress);

                yield return null;
            }

            FadePanel(() => async.allowSceneActivation = true);

            instance.prevScene = currentScene;
            currentScene = scene;
        }

        public static void HideFadePanel()
        {
            instance.fadePanel.color.SetAlpha(0);
            instance.fadePanel.raycastTarget = false;
            instance.fadePanel.gameObject.SetActive(false);
        }

        public static void FadePanel(Tween.TweenCallback callback)
        {
            instance.fadePanel.raycastTarget = true;
            instance.fadePanel.color.SetAlpha(0);
            instance.fadePanel.gameObject.SetActive(true);
            instance.fadePanel.DOFade(1, 0.5f, true).OnComplete(delegate
            {
                callback.Invoke();

                instance.fadePanel.DOFade(0, 0.5f, true).OnComplete(delegate
                {
                    instance.fadePanel.color.SetAlpha(0);
                    instance.fadePanel.raycastTarget = false;
                    instance.fadePanel.gameObject.SetActive(false);
                });
            });
        }

        public static void FadeAnimation(Tween.TweenCallback callback)
        {
            instance.transitionAnimator.SetTrigger(ANIMATION_FADE_IN);

            instance.StartCoroutine(instance.FadeAnimationCoroutine(callback));
        }

        private IEnumerator FadeAnimationCoroutine(Tween.TweenCallback callback)
        {
            while (transitionAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
                yield return null;

            callback.Invoke();

            instance.transitionAnimator.SetTrigger(ANIMATION_FADE_OUT);
        }
        
        public ValidatorAttribute.ValidateResult ValidateFirstScene(Scenes value)
        {
            if (value == Scenes.Init)
            {
                return new ValidatorAttribute.ValidateResult(ValidatorAttribute.ValidateType.Error, "First scene can't be Init!");
            }

            return new ValidatorAttribute.ValidateResult(ValidatorAttribute.ValidateType.Success, "Scene Loader is ready!");
        }

        public delegate void SceneCallback();

        public enum SceneTransition
        {
            None,
            Fade,
            Animation
        }

        private class SceneEvent
        {
            public string scene;
            public SceneCallback callback;

            public bool callOnce;

            public SceneEvent(Scenes scene, SceneCallback callback, bool callOnce = false)
            {
                this.scene = scene.ToString();
                this.callback = callback;
                this.callOnce = callOnce;
            }
        }

#if UNITY_EDITOR
#endif
    }
}

//Changelog
//v1.0.0 - Base version
//v1.0.1 - Custom events, transition
//v1.0.2 - Fixed scene opening