using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class AdsManagerExampleScript : MonoBehaviour
    {
        private Vector2 scrollView;

        public Text text;

        private void Awake()
        {
            Application.logMessageReceived += Log;
        }

        private void Log(string condition, string stackTrace, LogType type)
        {
            text.text += condition + "\n";
        }

        private void OnGUI()
        {
            Rect tempRect = new Rect(0, 0, Screen.width, Screen.height / 2);
            GUI.BeginGroup(tempRect);

            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width), GUILayout.Height(tempRect.height / 3), GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Show Banner", GUILayout.ExpandHeight(true)))
            {
                AdsManager.ShowBanner(AdsManager.Settings.bannerType);
            }
            if (GUILayout.Button("Destroy Banner", GUILayout.ExpandHeight(true)))
            {
                AdsManager.DestroyBanner(AdsManager.Settings.bannerType);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width), GUILayout.Height(tempRect.height / 3), GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Request Interstitial", GUILayout.ExpandHeight(true)))
            {
                AdsManager.RequestInterstitial(AdsManager.Settings.interstitialType);
            }
            if (GUILayout.Button("Show Interstitial", GUILayout.ExpandHeight(true)))
            {
                AdsManager.ShowInterstitial(AdsManager.Settings.interstitialType);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width), GUILayout.Height(tempRect.height / 3), GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Request Video", GUILayout.ExpandHeight(true)))
            {
                AdsManager.RequestRewardBasedVideo(AdsManager.Settings.rewardedVideoType);
            }
            if (GUILayout.Button("Show Video", GUILayout.ExpandHeight(true)))
            {
                AdsManager.ShowRewardBasedVideo(AdsManager.Settings.rewardedVideoType, (state) =>
                {
                    if(state)
                    {
                        Debug.Log("You get REWARD!");
                    }
                    else
                    {
                        Debug.Log("NO REWARD!");
                    }
                });
            }
            GUILayout.EndHorizontal();
            GUI.EndGroup();
        }
    }
}