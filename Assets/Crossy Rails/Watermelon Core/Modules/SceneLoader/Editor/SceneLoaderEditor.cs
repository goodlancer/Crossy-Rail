using UnityEngine;
using UnityEditor;
using Watermelon.Core;
using UnityEngine.SceneManagement;
using System;

namespace Watermelon
{
    [CustomEditor(typeof(SceneLoader))]
    public class SceneLoaderEditor : Editor
    {
        [InitializeOnLoadMethod]
        static void CheckInitScene()
        {
            string initScenePath = "Assets/" + ApplicationConsts.PROJECT_FOLDER + "/Game/Scenes/Init.unity";
            string sceneName = "Init";

            EditorApplication.playModeStateChanged += (PlayModeStateChange playModeState) =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    if (!EditorApplication.isPlaying)
                    {
                        bool hasInitScene = false;

                        for (int i = 0; i < SceneManager.sceneCount; i++)
                        {
                            if (SceneManager.GetSceneAt(i).name == sceneName)
                                hasInitScene = true;
                        }

                        if (!hasInitScene)
                        {
                            try
                            {
                                Scenes tempScene = (Scenes)Enum.Parse(typeof(Scenes), SceneManager.GetActiveScene().name);

                                SceneLoader.CurrentScene = tempScene;
                            }
                            catch (ArgumentException)
                            {
                                Debug.Log("[SceneLoader] Unknown scene!");
                            }

                            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(initScenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);
                            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
                        }
                    }
                }
                else
                {
                    if (!UnityEditor.EditorApplication.isPlaying)
                    {
                        if (SceneManager.sceneCount > 1)
                            UnityEditor.SceneManagement.EditorSceneManager.CloseScene(SceneManager.GetSceneByName(sceneName), true);
                    }
                }
            };
        }
    }
}
