using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AnalyticsPack;

public class AnalyticsWizardReward : MonoBehaviour {

    public int rewardType = 1;

    void Start()
    {
        if (GetComponent<Button>())//Нажатие на ревард
        {
            GetComponent<Button>().onClick.AddListener(SetAnalyticsReward);
        }

        if (GetComponent<GBNHZbtnReward>())//показ рекламы
        {
            GetComponent<GBNHZbtnReward>().onShow.AddListener(SetAnalyticsCompleteReward);
        }
    }

    void SetAnalyticsReward()
    {
        AnalyticsWizard.Instance.Reward(rewardType);
    }

    void SetAnalyticsCompleteReward()
    {
        AnalyticsWizard.Instance.CompleteReward(rewardType);
    }
}
