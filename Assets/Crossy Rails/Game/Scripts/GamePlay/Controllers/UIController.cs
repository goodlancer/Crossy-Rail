using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    public GameObject menuPanel;
    public GameObject gamePanel;
    public GameObject goldTutorialPanel;
    public GameObject minesTutorialPanel;
    public GameObject levelCompletePanel;
    public GameObject levelCompleteParticle;
    public GameObject settingsButton;
    public GameObject shareButton;
    public GameObject reloadButton;
    public GameObject skipButton;
    public GameObject devPanel;

    [Space(5)]
    public Text levelText1;
    public Text levelText2;


    private void Awake()
    {
        instance = this;

        if (!AudioController.Settings.isAudioEnabled && !AudioController.Settings.isMusicEnabled)
        {
            settingsButton.SetActive(false);
        }

        shareButton.SetActive(ShareController.Settings.isShareEnabled);
    }

    public void UpdateLevelText(int levelNumber)
    {
        levelText1.text = "LEVEL " + levelNumber;
        levelText2.text = "LEVEL " + levelNumber;
    }

    public void ShowMenu()
    {
        menuPanel.SetActive(true);
        reloadButton.SetActive(false);
        skipButton.SetActive(false);
    }

    public void HideMenu()
    {
        menuPanel.SetActive(false);
        goldTutorialPanel.SetActive(false);
        minesTutorialPanel.SetActive(false);
        reloadButton.SetActive(true);
        skipButton.SetActive(true);
    }

    public void ShowLevelComplete()
    {
        levelCompletePanel.SetActive(true);
        levelCompleteParticle.SetActive(true);
        reloadButton.SetActive(false);
        skipButton.SetActive(false);
    }

    public void HideLevelComplete()
    {
        levelCompletePanel.SetActive(false);
    }

    public void ShowGoldTutorialPanel()
    {
        minesTutorialPanel.SetActive(false);
        goldTutorialPanel.SetActive(true);
    }

    public void ShowMinesTutorialPanel()
    {
        if (!goldTutorialPanel.activeSelf)
        {
            minesTutorialPanel.SetActive(true);
        }
    }

    public void ReloadButton()
    {
        GameController.ReloadLevel();
        AudioController.PlaySound(AudioController.Settings.sounds.buttonClip, AudioController.AudioType.Sound, 1f);
    }

    public void SkipLevelButton()
    {
        GameController.SkipLevel();
        AudioController.PlaySound(AudioController.Settings.sounds.buttonClip, AudioController.AudioType.Sound, 1f);
    }


    public void NoAdsButton()
    {
        AudioController.PlaySound(AudioController.Settings.sounds.buttonClip, AudioController.AudioType.Sound, 1f);
    }

    public void ShareButton()
    {
        AudioController.PlaySound(AudioController.Settings.sounds.buttonClip, AudioController.AudioType.Sound, 1f);
        ShareController.ShareMessage();
    }


    #region Developement

    public void ResetProgressButton()
    {
        GameSettingsPrefs.Set("current level", 1);
        GameController.LoadLevelDev(1);

        HideMenu();
    }

    public void PrevLevelButton()
    {
        int currentLevelNumber = 0;
        currentLevelNumber = GameSettingsPrefs.Get<int>("current level");

        currentLevelNumber--;

        if (currentLevelNumber <= 0)
        {
            currentLevelNumber = LevelsDatabase.LevelsCount;
        }

        GameSettingsPrefs.Set("current level", currentLevelNumber);
        GameController.LoadLevelDev(currentLevelNumber);

        HideMenu();
    }

    public void NextLevelButton()
    {
        int currentLevelNumber = 0;
        currentLevelNumber = GameSettingsPrefs.Get<int>("current level");
        currentLevelNumber++;

        if (currentLevelNumber > LevelsDatabase.LevelsCount)
        {
            currentLevelNumber = 1;
        }

        GameSettingsPrefs.Set("current level", currentLevelNumber);
        GameController.LoadLevelDev(currentLevelNumber);

        HideMenu();
    }

    public void HideDevPanelButton()
    {
        devPanel.SetActive(false);
    }

    #endregion
}