using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GBNAPI;

public class BtnRateApp : MonoBehaviour {

    [SerializeField]
    private bool isIosRateDialogTrigger = false;

    [SerializeField]
    private PopupRateApp popupRateApp;

    private string rateUrl;
    private string rateEmail;
    private int appID;

    private static bool? state = null;
    private static float timer = 120f;

    private void Start()
    {
#if UNITY_IOS
        if (!int.TryParse(SDKInfo.GetKey("sdk_appleappid"), out appID))
        {
            Debug.LogError("AppID is empty!");
        }
#endif
        rateUrl = GetStoreUrl();

        rateEmail = GetCompanyEmail();

        Dialogs.RateApp.Init(rateUrl);

#if UNITY_IOS
        //hide on iOS
        if (isIosRateDialogTrigger)
        {
            LayoutElement layoutElement = gameObject.GetComponent<LayoutElement>();
            if (layoutElement != null)
            {
                layoutElement.ignoreLayout = true;
            }
            else
            {
                layoutElement = gameObject.AddComponent<LayoutElement>();
                layoutElement.ignoreLayout = true;
            }
            UIBehaviour[] uiBehaviours = GetComponents<UIBehaviour>();
            if (uiBehaviours != null && uiBehaviours.Length > 0)
            {
                foreach (UIBehaviour g in uiBehaviours)
                {
                    g.enabled = false;
                }
            }
            layoutElement.enabled = true;
            for (int i = 0; i < transform.childCount; ++i)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
#endif
    }

    private void OnEnable()
    {
#if UNITY_IOS
        if (isIosRateDialogTrigger)
        {
            if (state == null)
            {
                state = false;
            }
            else
            {
                if (state == false && Time.realtimeSinceStartup >= timer)
                {

                    if (Application.internetReachability != NetworkReachability.NotReachable)
                    {
                        Debug.Log("Show rate app popup");
                        Dialogs.RateApp.Show();
                        state = true;
                    }
                }
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
#endif
    }

    private string GetStoreUrl()
    {
        string url = "";

#if UNITY_IOS
        url = string.Format("https://itunes.apple.com/app/id{0}?action=write-review", WWW.EscapeURL(appID.ToString()));
#elif UNITY_ANDROID
        if (CompanyInfo.Struct.store.Equals("Amazon"))
        {
            url = string.Format("http://www.amazon.com/gp/mas/dl/android?p={0}", WWW.EscapeURL(CompanyInfo.bundleIdentifier));
        }
        else
        {
            url = string.Format("https://play.google.com/store/apps/details?id={0}", WWW.EscapeURL(CompanyInfo.bundleIdentifier));
        }
#endif

        return url;
    }

    private string GetCompanyEmail()
    {
        string email = CompanyInfo.Struct.email;

#if !ADS_VERSION
        email = "";
#endif

        return email;
    }

    public void OpenRateAppPopup()
    {
        if (string.IsNullOrEmpty(rateUrl))
        {
            Start();
        }

        if (popupRateApp != null)
        {
#if UNITY_ANDROID
            popupRateApp.Show(rateUrl, rateEmail);
#else
            //не используется
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                Dialogs.RateApp.Show();
            }
            else
            {
                Dialogs.NoInternetAccessWarning();
            }
#endif
        }
        else
        {
            Debug.LogError("popupRateApp is not defined!");
        }
    }    
}
