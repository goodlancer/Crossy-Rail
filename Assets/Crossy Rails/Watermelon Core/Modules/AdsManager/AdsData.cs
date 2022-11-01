#if MODULE_ADMOB
using GoogleMobileAds.Api;
#endif
#if MODULE_UNITYADS
using UnityEngine.Monetization;
#endif
using UnityEngine;
using Watermelon.Core;

namespace Watermelon
{
    [SetupTab("Advertising", texture = "icon_ads")]
    [CreateAssetMenu(fileName = "Ads Settings", menuName = "Settings/Ads Settings")]
    public class AdsData : ScriptableObject
    {
        [System.Serializable]
        public class AdsFrequensy
        {
        	[Tooltip("Delay in seconds between interstitial appearings.")]
        	public float showInterstitialDelay = 30f;
        }

        public AdvertisingModules bannerType;
        public AdvertisingModules interstitialType;
        public AdvertisingModules rewardedVideoType;

        public AdsFrequensy adsFrequensy;

        #region UnityAds
        //Unity Ads
        [Header("Application ID")]
        public string androidUnityAdsAppID = "1234567";
        public string IOSUnityAdsAppID = "1234567";

        //Banned ID
        [Header("Banner ID")]
        public string androidUnityAdsBannerID = "banner";
        public string IOSUnityAdsBannerID = "banner";

        //Interstitial ID
        [Header("Interstitial ID")]
        public string androidUnityAdsInterstitialID = "video";
        public string IOSUnityAdsInterstitialID = "video";

        //Rewarder Video ID
        [Header("Rewarded Video ID")]
        public string androidUnityAdsRewardedVideoID = "rewardedVideo";
        public string IOSUnityAdsRewardedVideoID = "rewardedVideo";
        #endregion

        #region AdMob
        //Application ID
        [Header("Application ID")]
        public string androidAppID = "ca-app-pub-3940256099942544~3347511713";
        public string IOSAppID = "ca-app-pub-3940256099942544~1458002511";

        //Banned ID
        [Header("Banner ID")]
        public string androidBannerID = "ca-app-pub-3940256099942544/6300978111";
        public string IOSBannerID = "ca-app-pub-3940256099942544/2934735716";

        //Interstitial ID
        [Header("Interstitial ID")]
        public string androidInterstitialID = "ca-app-pub-3940256099942544/1033173712";
        public string IOSInterstitialID = "ca-app-pub-3940256099942544/4411468910";

        //Rewarder Video ID
        [Header("Rewarded Video ID")]
        public string androidRewardedVideoID = "ca-app-pub-3940256099942544/5224354917";
        public string IOSRewardedVideoID = "ca-app-pub-3940256099942544/1712485313";
        #endregion

        public bool testMode = false;

        public bool enableGDPR = false;
        public string privacyLink = "";

        public AdSizeType adSizeType = AdSizeType.Banner;
        public BannerPosition adPosition = BannerPosition.Bottom;
    }

    public enum AdSizeType
    {
        Banner = 0,
        MediumRectangle = 1,
        IABBanner = 2,
        Leaderboard = 3,
        SmartBanner = 4
    }

    public enum BannerPosition
    {
        Bottom = 0,
        Top = 1,
    }
}