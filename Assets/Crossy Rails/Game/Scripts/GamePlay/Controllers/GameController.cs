using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;
using Watermelon.Core;

public class GameController : MonoBehaviour
{
    private static GameController instance;

    private UIController uiController;
    private Level currentLevel;

    private float lastAddsTime = 0f;
    private int currentLevelNumber;

    public static int CurrentLevelNumber
    {
        get { return instance.currentLevelNumber; }
    }

    private void Awake()
    {
        instance = this;
        uiController = UIController.instance;

        AudioController.PlayRandomMusic();

        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        StartGame();
    }

    private void StartGame()
    {
        currentLevelNumber = GetCurrentLevelNumber();
        currentLevel = LevelsDatabase.GetLevel(currentLevelNumber);
        LevelController.Load(currentLevel);

        uiController.UpdateLevelText(currentLevelNumber);

        AdsManager.RequestInterstitial(AdsManager.Settings.interstitialType);
        AdsManager.ShowBanner(AdsManager.Settings.bannerType);
    }

    private int GetCurrentLevelNumber()
    {
        int levelNumber = GameSettingsPrefs.Get<int>("current level");
        return Mathf.Clamp(levelNumber, 1, LevelsDatabase.LevelsCount);
    }

    private void IncreaseCurrentLevel()
    {
        if (LevelsDatabase.LevelsCount > instance.currentLevelNumber)
        {
            GameSettingsPrefs.Set("current level", instance.currentLevelNumber + 1);
        }
        else
        {
            GameSettingsPrefs.Set("current level", 1);
        }
    }

    public static void OnLevelComplete()
    {
        instance.IncreaseCurrentLevel();

        UIController.instance.ShowMenu();

        // showing interstitial
        if ((instance.lastAddsTime + AdsManager.Settings.adsFrequensy.showInterstitialDelay) < Time.realtimeSinceStartup)
        {
            AdsManager.ShowInterstitial(AdsManager.Settings.interstitialType);

            instance.lastAddsTime = Time.realtimeSinceStartup;
        }

        instance.StartGame();
    }

    public static void OnTapPerformed()
    {
        UIController.instance.HideMenu();
    }

    private IEnumerator DelayedCall(Action callback, float delay)
    {
        yield return new WaitForSeconds(delay);

        callback?.Invoke();
    }

    public static void ReloadLevel()
    {
        UIController.instance.ShowMenu();
        instance.StartGame();
    }

    public static void SkipLevel()
    {
        AdsManager.ShowRewardBasedVideo(AdsManager.Settings.rewardedVideoType, (hasReward) =>
        {
            if (hasReward)
            {
                // successful result               
                UIController.instance.ShowMenu();
                instance.IncreaseCurrentLevel();
                instance.StartGame();
            }
        });

    }

    private int levelNumberDev;

    public static void LoadLevelDev(int number)
    {
        instance.levelNumberDev = number;
        instance.LoadLevelDev();
    }

    public void LoadLevelDev()
    {
        currentLevelNumber = levelNumberDev;

        currentLevel = LevelsDatabase.GetLevel(currentLevelNumber);
        LevelController.Load(currentLevel);

        uiController.UpdateLevelText(currentLevelNumber);
    }
}