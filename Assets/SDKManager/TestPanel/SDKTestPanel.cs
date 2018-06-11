using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SDKManagement;
using System;
using AdvertisingPack;
using AnalyticsPack;

public class SDKTestPanel : MonoBehaviour {
    
    public List<SDKInfoDrawer> infoDrawers;

    public Toggle SdkToggle;
    public Toggle AnalyticsToggle;
    public Toggle AdsToggle;

    public Button customButton;

    private ISDKReporter sdkReporter = null;

    void OnEnable() {
        Refresh();
    }

    public void Refresh() {
        DrawReport(null);
        GenerateReport();
    }

    private void DrawReport(SDKReport report)
    {
        var min = 0;
        if (report != null)
        {
            min = Mathf.Min(infoDrawers.Count, report.infos.Count);
        }
        for (int i = 0; i < infoDrawers.Count; i++)
        {
            infoDrawers[i].gameObject.SetActive(i < min);
        }
        if (report != null)
        {
            for (int i = 0; i < min; i++)
            {
                infoDrawers[i].SetInfo(report.infos[i], i % 2 == 0);
            }
        }
    }

    private void GenerateReport()
    {
        ClearReporter();
        if (SdkToggle != null && SdkToggle.isOn && SDKManager.Instance != null)
        {
            sdkReporter = SDKManager.Instance;
        }
        if (AnalyticsToggle != null && AnalyticsToggle.isOn && AnalyticsSystem.Instance != null)
        {
            sdkReporter = AnalyticsSystem.Instance;
        }
        if (AdsToggle != null && AdsToggle.isOn && AdsManager.adsController != null)
        {
            sdkReporter = AdsManager.adsController;
        }
        if (sdkReporter != null)
        {
            sdkReporter.OnReportComplete += DrawReport;
            sdkReporter.GenerateReport();
            if (customButton != null)
            {
                customButton.gameObject.SetActive(sdkReporter.hasButton);
                var text = customButton.gameObject.GetComponentInChildren<Text>();
                if (text != null)
                {
                    text.text = sdkReporter.buttonLabel;
                }
            }
        }
    }

    public void customButtonClick()
    {
        if (sdkReporter != null) {
            sdkReporter.ButtonClick();
        }
    }

    private void ClearReporter() {
        if (sdkReporter != null)
        {
            sdkReporter.OnReportComplete -= DrawReport;
            sdkReporter = null;
        }
        if (customButton != null)
        {
            customButton.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        ClearReporter();
    }

}
