#pragma warning disable 0649
#pragma warning disable 0414

using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Watermelon
{
    public class ShareController : MonoBehaviour
    {
        private static ShareController instance;

        [SerializeField]
        private ShareSettings shareData;
        public static ShareSettings Settings
        {
            get { return instance.shareData; }
        }

        private bool isProcessing = false;
        private bool isFocus = false;

        private void Awake()
        {
            instance = this;
        }

        public static void ShareMessage()
        {
#if UNITY_ANDROID
            if (!instance.isProcessing)
                instance.StartCoroutine(instance.ShareScreenshot(instance.shareData.shareMessageAndroid));
#elif UNITY_IOS
            if (!instance.isProcessing)
                instance.StartCoroutine(instance.CallSocialShareRoutine(instance.shareData.shareMessageIOS));
#else
            Debug.Log("[Share Controller]: No sharing set up for this platform.");
#endif
        }

        IEnumerator ShareScreenshot(string content)
        {
            isProcessing = true;

            yield return new WaitForEndOfFrame();

#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
            intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
            AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
        
            intentObject.Call<AndroidJavaObject>("setType", "text/plain");

            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), content);
            AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Share");
            currentActivity.Call("startActivity", chooser);

            yield return new WaitForSecondsRealtime(1);

            yield return new WaitUntil(() => isFocus);
#endif

            isProcessing = false;
        }

#if UNITY_IOS && !UNITY_EDITOR
        public struct ConfigStruct
        {
            public string title;
            public string message;
        }

        [DllImport("__Internal")] private static extern void showAlertMessage(ref ConfigStruct conf);
        public struct SocialSharingStruct
        {
            public string text;
            public string url;
            public string image;
            public string subject;
        }

        [DllImport("__Internal")] private static extern void showSocialSharing(ref SocialSharingStruct conf);
        public void CallSocialShare(string title, string message)
        {
            ConfigStruct conf = new ConfigStruct();
            conf.title = title;
            conf.message = message;
            showAlertMessage(ref conf);
            isProcessing = false;
        }

        public static void CallSocialShareAdvanced(string defaultTxt, string subject, string url, string img)
        {
            SocialSharingStruct conf = new SocialSharingStruct();
            conf.text = defaultTxt;
            conf.url = url;
            conf.image = img;
            conf.subject = subject;
            showSocialSharing(ref conf);
        }
#endif

        IEnumerator CallSocialShareRoutine(string content)
        {
            isProcessing = true;

            yield return new WaitForSeconds(1f);

#if UNITY_IOS && !UNITY_EDITOR
            CallSocialShareAdvanced(content, "", "", "");
#else
            isProcessing = false;
#endif
        }

        private void OnApplicationFocus(bool focus)
        {
            isFocus = focus;
        }
    }
}