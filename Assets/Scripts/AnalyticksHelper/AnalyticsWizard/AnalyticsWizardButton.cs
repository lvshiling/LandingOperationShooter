using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using AnalyticsPack;

public class AnalyticsWizardButton : MonoBehaviour {

    [SerializeField]
    public enum EventsAnalytics
    {
        TapPlayGame = 1,
        TapMoreGames,
        TapRateGame,
        TapCrosspromo,
        TapYouTube,
        TapMainMenu,
        TapRestart,
        TapResume,
        TapNextLevel,
        TapPrivacyPolicy,
        TapShop,
        TapPause,
        TapLevels,
        
    }

    public EventsAnalytics eventsAnalytics;

    void Start () {
        if (GetComponent<Button>())
        {
            GetComponent<Button>().onClick.AddListener(SetAnalytics);
        }
	}
	
	void SetAnalytics()
    {
        switch (eventsAnalytics)
        {
            case EventsAnalytics.TapCrosspromo:
                AnalyticsWizard.Instance.TapCrosspromo();
                break;

            case EventsAnalytics.TapLevels:
                AnalyticsWizard.Instance.TapLevels();
                break;


            case EventsAnalytics.TapMainMenu:
                AnalyticsWizard.Instance.TapMainMenu();
                break;

            case EventsAnalytics.TapMoreGames:
                AnalyticsWizard.Instance.TapMoreGames();
                break;

            case EventsAnalytics.TapNextLevel:
                AnalyticsWizard.Instance.TapNextLevel();
                break;

            case EventsAnalytics.TapPlayGame:
                AnalyticsWizard.Instance.TapPlayGame();
                break;

            case EventsAnalytics.TapPrivacyPolicy:
                AnalyticsWizard.Instance.TapPrivacyPolicy();
                break;

            case EventsAnalytics.TapRateGame:
                AnalyticsWizard.Instance.TapRateGame();
                break;

            case EventsAnalytics.TapRestart:
                AnalyticsWizard.Instance.TapRestart();
                break;

            case EventsAnalytics.TapResume:
                AnalyticsWizard.Instance.TapResume();
                break;

            case EventsAnalytics.TapShop:
                AnalyticsWizard.Instance.TapShop();
                break;

            case EventsAnalytics.TapPause:
                AnalyticsWizard.Instance.TapPause();
                break;

            case EventsAnalytics.TapYouTube:
                AnalyticsWizard.Instance.TapYouTube();
                break;    
        }
    }
}
