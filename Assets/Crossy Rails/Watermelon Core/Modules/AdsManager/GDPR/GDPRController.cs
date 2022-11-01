using UnityEngine;
using UnityEngine.SceneManagement;

namespace Watermelon
{
    public class GDPRController : MonoBehaviour
    {
        [SerializeField]
        private AdsData adsData;

        [Space]
        [SerializeField]
        private GameObject gdprPanel;

        private const string PREFS_NAME = "GDPR";
        private const string PRIVACY_LINK = @"https://www.appodeal.com/home/privacy-policy/";

        private void Awake()
        {
            if(adsData != null)
            {
                if(adsData.enableGDPR)
                {
                    if (!PlayerPrefs.HasKey(PREFS_NAME))
                    {
                        gdprPanel.SetActive(true);
                    }
                    else
                    {
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                    }
                }
                else
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                }
            }
            else
            {
                Debug.LogError("[GDPRController]: Ads Data doesn't selected!");
            }
        }

        public void OpenPrivacyLink()
        {
            Application.OpenURL(PRIVACY_LINK);
        }

        public void SetGDPRState(bool state)
        {
            PlayerPrefs.SetInt(PREFS_NAME, state ? 1 : 0);

            gdprPanel.SetActive(false);

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public static bool GetGDPRState()
        {
            if (PlayerPrefs.HasKey(PREFS_NAME))
            {
                return PlayerPrefs.GetInt(PREFS_NAME) == 1 ? true : false;
            }

            return false;
        }
    }
}