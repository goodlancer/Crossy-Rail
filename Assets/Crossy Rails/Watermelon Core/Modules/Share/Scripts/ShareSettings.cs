using UnityEngine;
using Watermelon.Core;

namespace Watermelon
{
    [SetupTab("Share", texture = "icon_share")]
    [CreateAssetMenu(fileName = "Share Settings", menuName = "Settings/Share Settings")]
    public class ShareSettings : ScriptableObject
    {
        public bool isShareEnabled;

        public string shareMessageAndroid = @"Android test message. https://play.google.com";
        public string shareMessageIOS = @"iOS test message. https://apps.apple.com";
    }
}