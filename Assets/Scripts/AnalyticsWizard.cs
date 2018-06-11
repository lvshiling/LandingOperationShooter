using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnalyticsPack;

public class AnalyticsWizard : MonoBehaviour
{
    public const int DEFAULT_VALUE = -1;
    private static AnalyticsWizard _instance;
    public static AnalyticsWizard Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AnalyticsWizard>();
                if (_instance == null)
                {
                    _instance = new GameObject("_AUTO_AnalyticsWizard").AddComponent<AnalyticsWizard>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        AdvertisingPack.AdsManager.Interstitial.OnImpression += Impression;
    }
    private void OnDestroy()
    {
        AdvertisingPack.AdsManager.Interstitial.OnImpression -= Impression;
    }

    public void SetMenu(Menu menu)
    {
        Analytics.Instance.CurrentMenu = menu;
    }

    public void TapPlayGame()
    {
        Analytics.Instance.events.main.TapPlayGame(Analytics.Instance.CurrentMenu);
    }

    public void TapMoreGames()
    {
        Analytics.Instance.events.main.TapMoreGames(Analytics.Instance.CurrentMenu);
    }

    public void TapRateGame()
    {
        Analytics.Instance.events.main.TapRateGame(Analytics.Instance.CurrentMenu);
    }

    public void TapCrosspromo()
    {
        Analytics.Instance.events.main.TapCrosspromo(Analytics.Instance.CurrentMenu);
    }

    public void TapYouTube()
    {
        Analytics.Instance.events.main.TapYouTube(Analytics.Instance.CurrentMenu);
    }

    public void TapMainMenu()
    {
        Analytics.Instance.events.main.TapMainMenu(Analytics.Instance.CurrentMenu);
    }

    public void TapRestart()
    {
        Analytics.Instance.events.main.TapRestart(Analytics.Instance.CurrentMenu);
    }

    public void TapResume()
    {
        Analytics.Instance.events.main.TapResume(Analytics.Instance.CurrentMenu);
    }

    public void TapNextLevel()
    {
        Analytics.Instance.events.main.TapNextLevel(Analytics.Instance.CurrentMenu);
    }

    public void TapPrivacyPolicy()
    {
        Analytics.Instance.events.main.TapPrivacyPolicy(Analytics.Instance.CurrentMenu);
    }

    public void TapShop()
    {
        Analytics.Instance.events.main.TapShop(Analytics.Instance.CurrentMenu);
    }

    public void TapLevels()
    {
        Analytics.Instance.events.main.TapLevels(Analytics.Instance.CurrentMenu);
    }

    public void TapPause()
    {
        Analytics.Instance.events.main.TapPause(Analytics.Instance.CurrentMenu);
    }

    public void Reward(int rewardType)
    {
        Analytics.Instance.events.main.Reward(Analytics.Instance.CurrentMenu, rewardType,-1);
    }

    public void CompleteReward(int rewardType)
    {
        Analytics.Instance.events.main.CompleteReward(Analytics.Instance.CurrentMenu, rewardType,-1);
    }
    
    public void BuyItem(int itemType, int itemIndex)
    {
        Analytics.Instance.events.main.BuyItem(itemType, itemIndex);
    }

    public void BuyLocation(int locationIndex)
    {
        Analytics.Instance.events.main.BuyLocation(locationIndex);
    }
    
    public void BuyUpgrade(int upgradeType, int upgradeIndex, float upgradeLevel)
    {
        Analytics.Instance.events.main.BuyUpgrade(upgradeType, upgradeIndex, upgradeLevel);
    }
    
    public void StartLevel(int levelType, int levelIndex = DEFAULT_VALUE)
    {
        Analytics.Instance.events.main.StartLevel(levelType, levelIndex);
    }

    public void CompleteTutorial(int tutorId)
    {
        Analytics.Instance.events.main.CompleteTutorial(tutorId);
    }

    public void WinLevel(int levelIndex = DEFAULT_VALUE)
    {
        Analytics.Instance.events.main.WinLevel(levelIndex);
    }

    public void LoseLevel(int levelIndex = DEFAULT_VALUE)
    {
        Analytics.Instance.events.main.Loselevel(levelIndex);
    }
    
    public void Impression()
    {
        Analytics.Instance.events.main.Impression(Analytics.Instance.CurrentMenu);
    }
    
    public void Settings()
    {
        Analytics.Instance.events.main.Settings(Analytics.Instance.CurrentMenu);
    }

    public void NoMoney()
    {
        Analytics.Instance.events.main.NoMoney(Analytics.Instance.CurrentMenu);        
    }
}
