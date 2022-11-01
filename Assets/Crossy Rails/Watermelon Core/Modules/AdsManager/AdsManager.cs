#pragma warning disable 0649
#pragma warning disable 0162

#if MODULE_ADMOB
using GoogleMobileAds.Api;
#endif
#if MODULE_UNITYADS
using UnityEngine.Monetization;
using UnityEngine.Advertisements;
#endif

using System;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.Core;

namespace Watermelon
{
    [Define("MODULE_ADMOB")]
    [Define("MODULE_UNITYADS")]
    public class AdsManager : MonoBehaviour
    {
        private static AdsManager instance;

        [SerializeField]
        private AdsData settings;
        public static AdsData Settings
        {
            get { return instance.settings; }
        }

#if UNITY_EDITOR
        private bool showBanner;
        private bool showIntertitial;
        private bool showRewardedVideo;

        private AdvertisingHandler.RewardedVideoCallback dummyCallback;
#endif

        private AdvertisingHandler[] advertisingModules = new AdvertisingHandler[]
        {
#if MODULE_ADMOB
            new AdMobHandler(AdvertisingModules.AdMob), // AdMob module
#endif

#if MODULE_UNITYADS
            new UnityAdsHandler(AdvertisingModules.UnityAds) // Unity Ads module
#endif
        };

        private static Dictionary<AdvertisingModules, AdvertisingHandler> advertisingLink = new Dictionary<AdvertisingModules, AdvertisingHandler>();
        
        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogWarning("[AdsManager]: Module already exists!");

                Destroy(this);

                return;
            }

            if (settings == null)
            {
                Debug.LogWarning("[AdsManager]: Settings don't exist!");

                Destroy(this);

                return;
            }

            instance = this;
        }

        private void Start()
        {
            // Initialize all advertising modules
            advertisingLink = new Dictionary<AdvertisingModules, AdvertisingHandler>();
            for (int i = 0; i < advertisingModules.Length; i++)
            {
                advertisingModules[i].Init(settings);

                advertisingLink.Add(advertisingModules[i].ModuleType, advertisingModules[i]);
            }
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if(showBanner)
            {
                float f = Screen.dpi / 160f;
                float dp = Screen.height / f;
                
                int bannerHeight = (int)((dp > 720f) ? 90f * f : (dp > 400f) ? 50f * f : 32f * f);
                int bannerWidth = (int)(320 * (Screen.dpi / 160));

                Rect bannerRect = new Rect();

                switch(settings.adPosition)
                {
                    case BannerPosition.Bottom:
                        bannerRect = new Rect((Screen.width - bannerWidth) / 2, Screen.height - bannerHeight, bannerWidth, bannerHeight);
                        break;
                    case BannerPosition.Top:
                        bannerRect = new Rect((Screen.width - bannerWidth) / 2, 0, bannerWidth, bannerHeight);
                        break;
                }

                GUI.Box(bannerRect, "BANNER DUMMY");
            }

            if(showIntertitial)
            {
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "INTERSTITIAL DUMMY", GUI.skin.window);

                int buttonHeight = Screen.height / 8;
                int buttonWidth = Screen.width / 2;
                if(GUI.Button(new Rect((Screen.width - buttonWidth) / 2, Screen.height - (buttonHeight * 2), buttonWidth, buttonHeight), "Close"))
                {
                    showIntertitial = false;

                    Time.timeScale = 1;
                }
            }

            if (showRewardedVideo)
            {
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "REWARD VIDEO DUMMY", GUI.skin.window);

                int buttonHeight = Screen.height / 6;
                int buttonWidth = Screen.width / 2;

                GUILayout.BeginArea(new Rect((Screen.width - buttonWidth) / 2, (Screen.height / 2) - (buttonHeight / 2), buttonWidth, buttonHeight));
                if(GUILayout.Button("REWARD", GUILayout.ExpandHeight(true)))
                {
                    if (dummyCallback != null)
                    {
                        dummyCallback.Invoke(true);
                        dummyCallback = null;

                        Time.timeScale = 1;
                    }

                    showRewardedVideo = false;
                }
                if (GUILayout.Button("CLOSE", GUILayout.ExpandHeight(true)))
                {
                    if (dummyCallback != null)
                    {
                        dummyCallback.Invoke(false);
                        dummyCallback = null;

                        Time.timeScale = 1;
                    }

                    showRewardedVideo = false;
                }
                GUILayout.EndArea();
            }
        }
#endif

        public static bool IsModuleActive(AdvertisingModules advertisingModules)
        {
            return advertisingLink.ContainsKey(advertisingModules);
        }

        public static void RequestInterstitial(AdvertisingModules advertisingModules)
        {
            if (!IsModuleActive(advertisingModules))
                return;

            if (advertisingLink[advertisingModules].IsInterstitialLoaded())
                return;

            advertisingLink[advertisingModules].RequestInterstitial();
        }

        public static void RequestRewardBasedVideo(AdvertisingModules advertisingModules)
        {
            if (!IsModuleActive(advertisingModules))
                return;

            if (advertisingLink[advertisingModules].IsRewardedVideoLoaded())
                return;

            advertisingLink[advertisingModules].RequestRewardedVideo();
        }

        public static bool IsInterstitialLoaded(AdvertisingModules advertisingModules)
        {
#if UNITY_EDITOR
            return true;
#endif

            if (!IsModuleActive(advertisingModules))
                return false;

            return advertisingLink[advertisingModules].IsInterstitialLoaded();
        }

        public static bool IsRewardBasedVideoLoaded(AdvertisingModules advertisingModules)
        {
#if UNITY_EDITOR
            return true;
#endif

            if (!IsModuleActive(advertisingModules))
                return false;

            return advertisingLink[advertisingModules].IsRewardedVideoLoaded();
        }

        public static void ShowBanner(AdvertisingModules advertisingModules)
        {
#if UNITY_EDITOR
            instance.showBanner = true;

            return;
#endif

            if (!IsModuleActive(advertisingModules))
                return;

            advertisingLink[advertisingModules].ShowBanner();
        }

        public static void ShowInterstitial(AdvertisingModules advertisingModules)
        {
#if UNITY_EDITOR
            Time.timeScale = 0;

            instance.showIntertitial = true;

            return;
#endif

            if (!IsModuleActive(advertisingModules))
                return;

            if (!advertisingLink[advertisingModules].IsInterstitialLoaded())
                return;

            advertisingLink[advertisingModules].ShowInterstitial();
        }

        public static void ShowRewardBasedVideo(AdvertisingModules advertisingModules, AdvertisingHandler.RewardedVideoCallback callback)
        {
#if UNITY_EDITOR
            Time.timeScale = 0;

            instance.showRewardedVideo = true;

            instance.dummyCallback = callback;

            return;
#endif

            if (!IsModuleActive(advertisingModules))
                return;

            if (!advertisingLink[advertisingModules].IsRewardedVideoLoaded())
                return;

            advertisingLink[advertisingModules].ShowRewardedVideo(callback);
        }

        public static void DestroyBanner(AdvertisingModules advertisingModules)
        {
#if UNITY_EDITOR
            instance.showBanner = false;

            return;
#endif

            if (!IsModuleActive(advertisingModules))
                return;

            advertisingLink[advertisingModules].DestroyBanner();
        }

        public static void HideBanner(AdvertisingModules advertisingModules)
        {
#if UNITY_EDITOR
            instance.showBanner = false;

            return;
#endif
            
            if (!IsModuleActive(advertisingModules))
                return;

            advertisingLink[advertisingModules].HideBanner();
        }
    }
    
    public abstract class AdvertisingHandler
    {
        private AdvertisingModules moduleType;
        public AdvertisingModules ModuleType
        {
            get { return moduleType; }
        }

        protected AdsData adsSettings;
        protected RewardedVideoCallback rewardedVideoCallback;

        public AdvertisingHandler(AdvertisingModules moduleType)
        {
            this.moduleType = moduleType;
        }

        public abstract void Init(AdsData adsSettings);

        public abstract void ShowBanner();
        public abstract void HideBanner();
        public abstract void DestroyBanner();

        public abstract void RequestInterstitial();
        public abstract void ShowInterstitial();
        public abstract bool IsInterstitialLoaded();

        public abstract void RequestRewardedVideo();
        public abstract void ShowRewardedVideo(RewardedVideoCallback callback);
        public abstract bool IsRewardedVideoLoaded();

        public delegate void RewardedVideoCallback(bool hasReward);
    }

#if MODULE_ADMOB
    public class AdMobHandler : AdvertisingHandler
    {
        private BannerView bannerView;
        private InterstitialAd interstitial;
        private RewardBasedVideoAd rewardBasedVideo;

        public AdMobHandler(AdvertisingModules moduleType) : base(moduleType) { }

        public override void Init(AdsData adsSettings)
        {
            this.adsSettings = adsSettings;

            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize(GetAppID());

            // Get singleton reward based video ad reference.
            rewardBasedVideo = RewardBasedVideoAd.Instance;

            // RewardBasedVideoAd is a singleton, so handlers should only be registered once.
            rewardBasedVideo.OnAdLoaded += HandleRewardBasedVideoLoaded;
            rewardBasedVideo.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
            rewardBasedVideo.OnAdOpening += HandleRewardBasedVideoOpened;
            rewardBasedVideo.OnAdStarted += HandleRewardBasedVideoStarted;
            rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
            rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
            rewardBasedVideo.OnAdLeavingApplication += HandleRewardBasedVideoLeftApplication;

            RequestRewardedVideo();

            Debug.Log("[AdsManager]: AdMob inited!");
        }

        public override void DestroyBanner()
        {
            if (bannerView != null)
                bannerView.Destroy();
        }

        public override void HideBanner()
        {
            if (bannerView != null)
                bannerView.Hide();
        }

        public override void RequestInterstitial()
        {
            // Clean up interstitial ad before creating a new one.
            if (interstitial != null)
            {
                interstitial.Destroy();
            }

            // Create an interstitial.
            interstitial = new InterstitialAd(GetInterstitialID());

            // Register for ad events.
            interstitial.OnAdLoaded += HandleInterstitialLoaded;
            interstitial.OnAdFailedToLoad += HandleInterstitialFailedToLoad;
            interstitial.OnAdOpening += HandleInterstitialOpened;
            interstitial.OnAdClosed += HandleInterstitialClosed;
            interstitial.OnAdLeavingApplication += HandleInterstitialLeftApplication;

            // Load an interstitial ad.
            interstitial.LoadAd(CreateAdRequest());
        }

        public override void RequestRewardedVideo()
        {
            rewardBasedVideo.LoadAd(CreateAdRequest(), GetRewardedVideoID());
        }

        public override void ShowBanner()
        {
            // Clean up banner ad before creating a new one.
            if (bannerView != null)
            {
                bannerView.Destroy();
            }

            AdSize adSize = AdSize.Banner;

            switch (adsSettings.adSizeType)
            {
                case AdSizeType.Banner:
                    adSize = AdSize.Banner;
                    break;
                case AdSizeType.MediumRectangle:
                    adSize = AdSize.MediumRectangle;
                    break;
                case AdSizeType.IABBanner:
                    adSize = AdSize.IABBanner;
                    break;
                case AdSizeType.Leaderboard:
                    adSize = AdSize.Leaderboard;
                    break;
                case AdSizeType.SmartBanner:
                    adSize = AdSize.SmartBanner;
                    break;
            }

            AdPosition adPosition = AdPosition.Bottom;
            switch (adsSettings.adPosition)
            {
                case BannerPosition.Bottom:
                    adPosition = AdPosition.Bottom;
                    break;
                case BannerPosition.Top:
                    adPosition = AdPosition.Top;
                    break;
            }

            bannerView = new BannerView(GetBannerID(), adSize, adPosition);

            // Register for ad events.
            bannerView.OnAdLoaded += HandleAdLoaded;
            bannerView.OnAdFailedToLoad += HandleAdFailedToLoad;
            bannerView.OnAdOpening += HandleAdOpened;
            bannerView.OnAdClosed += HandleAdClosed;
            bannerView.OnAdLeavingApplication += HandleAdLeftApplication;

            // Load a banner ad.
            bannerView.LoadAd(CreateAdRequest());
        }

        public override void ShowInterstitial()
        {
            interstitial.Show();
        }

        public override void ShowRewardedVideo(RewardedVideoCallback callback)
        {
            if (rewardedVideoCallback != null)
                rewardedVideoCallback = null;

            rewardedVideoCallback = callback;

            rewardBasedVideo.Show();
        }

        public override bool IsInterstitialLoaded()
        {
            return interstitial != null && interstitial.IsLoaded();
        }

        public override bool IsRewardedVideoLoaded()
        {
            return rewardBasedVideo != null && rewardBasedVideo.IsLoaded();
        }

        public AdRequest CreateAdRequest()
        {
            AdRequest.Builder builder = new AdRequest.Builder();

            if(adsSettings.testMode)
                builder = builder.AddTestDevice("*");

            if(adsSettings.enableGDPR)
            {
                if(!GDPRController.GetGDPRState())
                {
                    builder = builder.AddExtra("npa", "1");
                }
            }
            else
            {
                builder = builder.AddExtra("npa", "1");
            }

            return builder.Build();
        }

        #region Banner callback handlers
        public void HandleAdLoaded(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleAdLoaded event received");
        }

        public void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            Debug.Log("[AdsManager]: HandleFailedToReceiveAd event received with message: " + args.Message);
        }

        public void HandleAdOpened(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleAdOpened event received");
        }

        public void HandleAdClosed(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleAdClosed event received");
        }

        public void HandleAdLeftApplication(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleAdLeftApplication event received");
        }
        #endregion

        #region Interstitial callback handlers
        public void HandleInterstitialLoaded(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleInterstitialLoaded event received");
        }

        public void HandleInterstitialFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            Debug.Log("[AdsManager]: HandleInterstitialFailedToLoad event received with message: " + args.Message);
        }

        public void HandleInterstitialOpened(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleInterstitialOpened event received");
        }

        public void HandleInterstitialClosed(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleInterstitialClosed event received");
        }

        public void HandleInterstitialLeftApplication(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleInterstitialLeftApplication event received");
        }
        #endregion

        #region RewardedVideo callback handlers
        public void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleRewardBasedVideoLoaded event received");
        }

        public void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            if (rewardedVideoCallback != null)
            {
                RewardedVideoCallback videoCallbackTemp = rewardedVideoCallback;

                videoCallbackTemp.Invoke(false);

                rewardedVideoCallback = null;
            }

            Debug.Log("[AdsManager]: HandleRewardBasedVideoFailedToLoad event received with message: " + args.Message);
        }

        public void HandleRewardBasedVideoOpened(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleRewardBasedVideoOpened event received");
        }

        public void HandleRewardBasedVideoStarted(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleRewardBasedVideoStarted event received");
        }

        public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
        {
            if (rewardedVideoCallback != null)
            {
                RewardedVideoCallback videoCallbackTemp = rewardedVideoCallback;

                videoCallbackTemp.Invoke(false);

                rewardedVideoCallback = null;
            }

            Debug.Log("[AdsManager]: HandleRewardBasedVideoClosed event received");
        }

        public void HandleRewardBasedVideoRewarded(object sender, Reward args)
        {
            if (rewardedVideoCallback != null)
            {
                RewardedVideoCallback videoCallbackTemp = rewardedVideoCallback;

                videoCallbackTemp.Invoke(true);

                rewardedVideoCallback = null;
            }

            string type = args.Type;
            double amount = args.Amount;

            Debug.Log("[AdsManager]: HandleRewardBasedVideoRewarded event received for " + amount.ToString() + " " + type);
        }

        public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleRewardBasedVideoLeftApplication event received");
        }
        #endregion

        public string GetAppID()
        {
#if UNITY_ANDROID
            return adsSettings.androidAppID;
#elif UNITY_IOS
            return adsSettings.IOSAppID;
#else
            return "unexpected_platform";
#endif
        }

        public string GetBannerID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.androidBannerID;
#elif UNITY_IOS
            return adsSettings.IOSBannerID;
#else
            return "unexpected_platform";
#endif
        }

        public string GetInterstitialID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.androidInterstitialID;
#elif UNITY_IOS
            return adsSettings.IOSInterstitialID;
#else
            return "unexpected_platform";
#endif
        }

        public string GetRewardedVideoID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.androidRewardedVideoID;
#elif UNITY_IOS
            return adsSettings.IOSRewardedVideoID;
#else
            return "unexpected_platform";
#endif
        }
    }
#endif

#if MODULE_UNITYADS
    public class UnityAdsHandler : AdvertisingHandler, IUnityAdsListener
    {
        public UnityAdsHandler(AdvertisingModules moduleType) : base(moduleType) { }

        public override void Init(AdsData adsSettings)
        {
            this.adsSettings = adsSettings;

            string unityAdsAppID = GetUnityAdsAppID();

            Advertisement.AddListener(this);
            Advertisement.Initialize(unityAdsAppID, adsSettings.testMode);

            if(adsSettings.enableGDPR)
            {
                UnityEngine.Advertisements.MetaData gdprMetaData = new UnityEngine.Advertisements.MetaData("gdpr");
                gdprMetaData.Set("consent", GDPRController.GetGDPRState().ToString());
                Advertisement.SetMetaData(gdprMetaData);
            }

            UnityEngine.Advertisements.BannerPosition adPosition = UnityEngine.Advertisements.BannerPosition.BOTTOM_CENTER;
            switch (adsSettings.adPosition)
            {
                case BannerPosition.Bottom:
                    adPosition = UnityEngine.Advertisements.BannerPosition.BOTTOM_CENTER;
                    break;
                case BannerPosition.Top:
                    adPosition = UnityEngine.Advertisements.BannerPosition.TOP_CENTER;
                    break;
            }

            Advertisement.Banner.SetPosition(adPosition);

            Debug.Log("[AdsManager]: Unity Ads initialized: " + Advertisement.isInitialized);
            Debug.Log("[AdsManager]: Unity Ads is supported: " + Advertisement.isSupported);
            Debug.Log("[AdsManager]: Unity Ads test mode enabled: " + Advertisement.debugMode);
            Debug.Log("[AdsManager]: Unity Ads version: " + Advertisement.version);
        }

        public override void DestroyBanner()
        {
            Advertisement.Banner.Hide(true);
        }

        public override void HideBanner()
        {
            Advertisement.Banner.Hide(false);
        }

        public override void RequestInterstitial()
        {
            // Unity Ads has auto video caching
        }

        public override void RequestRewardedVideo()
        {
            // Unity Ads has auto video caching
        }

        public override void ShowBanner()
        {
            Advertisement.Banner.Show(GetUnityAdsBannerID());
        }

        public override void ShowInterstitial()
        {
            Advertisement.Show(GetUnityAdsInterstitialID());
        }

        public override void ShowRewardedVideo(RewardedVideoCallback callback)
        {
            if (rewardedVideoCallback != null)
                rewardedVideoCallback = null;

            rewardedVideoCallback = callback;

            Advertisement.Show(GetUnityAdsRewardedVideoID());
        }

        public override bool IsInterstitialLoaded()
        {
            return Advertisement.IsReady(GetUnityAdsInterstitialID());
        }

        public override bool IsRewardedVideoLoaded()
        {
            return Advertisement.IsReady(GetUnityAdsRewardedVideoID());
        }

        public string GetUnityAdsAppID()
        {
#if UNITY_ANDROID
            return adsSettings.androidUnityAdsAppID;
#elif UNITY_IOS
            return adsSettings.IOSUnityAdsAppID;
#else
            return "unexpected_platform";
#endif
        }

        public string GetUnityAdsBannerID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.androidUnityAdsBannerID;
#elif UNITY_IOS
            return adsSettings.IOSUnityAdsBannerID;
#else
            return "unexpected_platform";
#endif
        }

        public string GetUnityAdsInterstitialID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.androidUnityAdsInterstitialID;
#elif UNITY_IOS
            return adsSettings.IOSUnityAdsInterstitialID;
#else
            return "unexpected_platform";
#endif
        }

        public string GetUnityAdsRewardedVideoID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.androidUnityAdsRewardedVideoID;
#elif UNITY_IOS
            return adsSettings.IOSUnityAdsRewardedVideoID;
#else
            return "unexpected_platform";
#endif
        }

        public void OnUnityAdsReady(string placementId)
        {
            Debug.Log("[AdsManager]: OnUnityAdsReady - " + placementId);
        }

        public void OnUnityAdsDidError(string message)
        {
            Debug.Log("[AdsManager]: OnUnityAdsDidError - " + message);
        }

        public void OnUnityAdsDidStart(string placementId)
        {
            Debug.Log("[AdsManager]: OnUnityAdsDidStart - " + placementId);
        }

        public void OnUnityAdsDidFinish(string placementId, UnityEngine.Advertisements.ShowResult showResult)
        {
            Debug.Log("[AdsManager]: OnUnityAdsDidFinish - " + placementId + ". Result - " + showResult);

            bool state = showResult == UnityEngine.Advertisements.ShowResult.Finished;
            
            // Reward the player
            if (rewardedVideoCallback != null)
            {
                RewardedVideoCallback videoCallbackTemp = rewardedVideoCallback;

                videoCallbackTemp.Invoke(state);

                rewardedVideoCallback = null;
            }
        }
    }
#endif

    public enum AdvertisingModules
    {
        AdMob = 0,
        UnityAds = 1
    }
}