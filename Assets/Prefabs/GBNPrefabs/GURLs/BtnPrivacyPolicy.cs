using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtnPrivacyPolicy : MonoBehaviour {

    /*
    private void Start()
    {
        gameObject.SetActive(false);
        GBNHZinit.AddOnLoadListener(OnAdsLoaded);
    }

    private void OnAdsLoaded(bool ok)
    {
#if ADS_VERSION && UNITY_ANDROID
        gameObject.SetActive(true);
#else
        gameObject.SetActive(false);
#endif
    }

    private void OnDestroy()
    {
        GBNHZinit.RemoveOnLoadListener(OnAdsLoaded);
    }
    */

    public void OpenPolicy()
    {
        string url = GBNAPI.CompanyInfo.Struct.policy;
        Application.OpenURL(url);
    }
}
