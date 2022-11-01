using UnityEngine;
using UnityEditor;
using Watermelon.Core;
using System.Collections.Generic;

namespace Watermelon
{
    [CustomEditor(typeof(AdsData))]
    public class AdsDataEditor : Editor
    {
        private SerializedProperty bannerTypeProperty;
        private SerializedProperty interstitialTypeProperty;
        private SerializedProperty rewardedVideoTypeProperty;

        private SerializedProperty adSizeTypeProperty;
        private SerializedProperty adPositionProperty;
        
        private SerializedProperty testModeProperty;
        private SerializedProperty enableGDPRProperty;
        private SerializedProperty privacyProperty;

        // AdMob
        private SerializedProperty androidAppIDProperty;
        private SerializedProperty IOSAppIDProperty;

        private SerializedProperty androidBannerIDProperty;
        private SerializedProperty IOSBannerIDProperty;

        private SerializedProperty androidInterstitialIDProperty;
        private SerializedProperty IOSInterstitialIDProperty;

        private SerializedProperty androidRewardedVideoIDProperty;
        private SerializedProperty IOSRewardedVideoIDProperty;

        // Unity Ads
        private SerializedProperty androidUnityAdsAppIDProperty;
        private SerializedProperty IOSUnityAdsAppIDProperty;

        private SerializedProperty androidUnityAdsBannerIDProperty;
        private SerializedProperty IOSUnityAdsBannerIDProperty;

        private SerializedProperty androidUnityAdsInterstitialIDProperty;
        private SerializedProperty IOSUnityAdsInterstitialIDProperty;

        private SerializedProperty androidUnityAdsRewardedVideoIDProperty;
        private SerializedProperty IOSUnityAdsRewardedVideoIDProperty;

        private IEnumerable<SerializedProperty> adsFrequensyProperties;

        private bool isAdMobDefineEnabled = false;
        private bool isUnityAdsDefineEnabled = false;

        private void OnEnable()
        {
            bannerTypeProperty = serializedObject.FindProperty("bannerType");
            interstitialTypeProperty = serializedObject.FindProperty("interstitialType");
            rewardedVideoTypeProperty = serializedObject.FindProperty("rewardedVideoType");

            adSizeTypeProperty = serializedObject.FindProperty("adSizeType");
            adPositionProperty = serializedObject.FindProperty("adPosition");
            
            testModeProperty = serializedObject.FindProperty("testMode");
            enableGDPRProperty = serializedObject.FindProperty("enableGDPR");
            privacyProperty = serializedObject.FindProperty("privacyLink");

            // AdMob
            androidAppIDProperty = serializedObject.FindProperty("androidAppID");
            IOSAppIDProperty = serializedObject.FindProperty("IOSAppID");

            androidBannerIDProperty = serializedObject.FindProperty("androidBannerID");
            IOSBannerIDProperty = serializedObject.FindProperty("IOSBannerID");

            androidInterstitialIDProperty = serializedObject.FindProperty("androidInterstitialID");
            IOSInterstitialIDProperty = serializedObject.FindProperty("IOSInterstitialID");

            androidRewardedVideoIDProperty = serializedObject.FindProperty("androidRewardedVideoID");
            IOSRewardedVideoIDProperty = serializedObject.FindProperty("IOSRewardedVideoID");

            // Unity Ads
            androidUnityAdsAppIDProperty = serializedObject.FindProperty("androidUnityAdsAppID");
            IOSUnityAdsAppIDProperty = serializedObject.FindProperty("IOSUnityAdsAppID");

            androidUnityAdsBannerIDProperty = serializedObject.FindProperty("androidUnityAdsBannerID");
            IOSUnityAdsBannerIDProperty = serializedObject.FindProperty("IOSUnityAdsBannerID");

            androidUnityAdsInterstitialIDProperty = serializedObject.FindProperty("androidUnityAdsInterstitialID");
            IOSUnityAdsInterstitialIDProperty = serializedObject.FindProperty("IOSUnityAdsInterstitialID");

            androidUnityAdsRewardedVideoIDProperty = serializedObject.FindProperty("androidUnityAdsRewardedVideoID");
            IOSUnityAdsRewardedVideoIDProperty = serializedObject.FindProperty("IOSUnityAdsRewardedVideoID");

            adsFrequensyProperties = serializedObject.FindProperty("adsFrequensy").GetChildren();

            isAdMobDefineEnabled = DefineManager.HasDefine("MODULE_ADMOB");
            isUnityAdsDefineEnabled = DefineManager.HasDefine("MODULE_UNITYADS");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            EditorGUILayoutCustom.Header("ADVERTISING");

            EditorGUILayout.PropertyField(bannerTypeProperty);
            EditorGUILayout.PropertyField(interstitialTypeProperty);
            EditorGUILayout.PropertyField(rewardedVideoTypeProperty);

            GUILayout.Space(12);

            EditorGUILayout.PropertyField(adSizeTypeProperty);
            EditorGUILayout.PropertyField(adPositionProperty);

            GUILayout.Space(12);

            EditorGUILayout.PropertyField(testModeProperty);

            GUILayout.Space(12);

            EditorGUILayout.PropertyField(enableGDPRProperty);
            EditorGUILayout.PropertyField(privacyProperty);

            GUILayout.Space(12);

            foreach (SerializedProperty prop in adsFrequensyProperties)
            {
                EditorGUILayout.PropertyField(prop);
            }

            EditorGUILayout.EndVertical();

            GUILayout.Space(8);

            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            EditorGUILayoutCustom.Header("ADMOB");

            if(!isAdMobDefineEnabled)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField(EditorGUIUtility.IconContent("console.warnicon"), EditorStylesExtended.padding00, GUILayout.Width(16), GUILayout.Height(16));
                EditorGUILayout.LabelField("AdMob define isn't enabled!");
                if (GUILayout.Button("Enable", EditorStyles.miniButton))
                {
                    DefineManager.EnableDefine("MODULE_ADMOB");
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.PropertyField(androidAppIDProperty);
            EditorGUILayout.PropertyField(IOSAppIDProperty);
            EditorGUILayout.PropertyField(androidBannerIDProperty);
            EditorGUILayout.PropertyField(IOSBannerIDProperty);
            EditorGUILayout.PropertyField(androidInterstitialIDProperty);
            EditorGUILayout.PropertyField(IOSInterstitialIDProperty);
            EditorGUILayout.PropertyField(androidRewardedVideoIDProperty);
            EditorGUILayout.PropertyField(IOSRewardedVideoIDProperty);
            
            GUILayout.Space(8);

            if(GUILayout.Button("AdMob Dashboard", EditorStylesExtended.button_01))
            {
                Application.OpenURL(@"https://apps.admob.com/v2/home");
            }

            if (GUILayout.Button("AdMob Quick Start Guide", EditorStylesExtended.button_01))
            {
                Application.OpenURL(@"https://developers.google.com/admob/unity/start");
            }

            EditorGUILayout.EndVertical();

            GUILayout.Space(8);

            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            EditorGUILayoutCustom.Header("UNITY ADS");

            if (!isUnityAdsDefineEnabled)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField(EditorGUIUtility.IconContent("console.warnicon"), EditorStylesExtended.padding00, GUILayout.Width(16), GUILayout.Height(16));
                EditorGUILayout.LabelField("UnityAds define isn't enabled!");
                if (GUILayout.Button("Enable", EditorStyles.miniButton))
                {
                    DefineManager.EnableDefine("MODULE_UNITYADS");
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.PropertyField(androidUnityAdsAppIDProperty);
            EditorGUILayout.PropertyField(IOSUnityAdsAppIDProperty);
            EditorGUILayout.PropertyField(androidUnityAdsBannerIDProperty);
            EditorGUILayout.PropertyField(IOSUnityAdsBannerIDProperty);
            EditorGUILayout.PropertyField(androidUnityAdsInterstitialIDProperty);
            EditorGUILayout.PropertyField(IOSUnityAdsInterstitialIDProperty);
            EditorGUILayout.PropertyField(androidUnityAdsRewardedVideoIDProperty);
            EditorGUILayout.PropertyField(IOSUnityAdsRewardedVideoIDProperty);

            GUILayout.Space(8);

            if (GUILayout.Button("Unity Ads Dashboard", EditorStylesExtended.button_01))
            {
                Application.OpenURL(@"https://operate.dashboard.unity3d.com");
            }

            if (GUILayout.Button("Unity Ads Quick Start Guide", EditorStylesExtended.button_01))
            {
                Application.OpenURL(@"https://unityads.unity3d.com/help/monetization/getting-started");
            }

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}