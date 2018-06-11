using UnityEngine;
using System.Collections;
using GBNAPI;

public class BtnUrl : MonoBehaviour
{
    [SerializeField]
    private bool isMoreGames;

    private int appID;

    private void Awake()
    {
        CoolTool.OnResetMoregames += OnReset;
    }

    private void OnEnable()
    {
        OnReset(CoolTool.Moregames);
    }

    private void OnReset(bool state)
    {
        gameObject.SetActive(state);
    }

    private void OnDestroy()
    {
        CoolTool.OnResetMoregames -= OnReset;
    }

    public void Start()
    {
#if UNITY_IOS
        if (!int.TryParse(SDKInfo.GetKey("sdk_appleappid"), out appID))
        {
            Debug.LogError("AppID is empty!");
        }
#endif
        CompanyInfo.Parse();
    }

    public void OpenGameURL()
    {
        string url = "";

        if (!isMoreGames)
        {
#if UNITY_IOS
            //Apple
            url = "https://itunes.apple.com/app/id" + appID.ToString();
#else
            if (CompanyInfo.Struct.store.Equals("Amazon"))
            {
                //Amazon
                url = "https://www.amazon.com/gp/mas/dl/android?p=" + CompanyInfo.bundleIdentifier;
            }
            else
            {
                //Google
                url = "market://details?id=" + CompanyInfo.bundleIdentifier;
            }
#endif
        }
        else
        {
#if UNITY_IOS
            url = CompanyInfo.Struct.moregames;
            if (string.IsNullOrEmpty(url))
                url = CompanyInfo.Struct.url;
#else
            url = CompanyInfo.Struct.url;
#endif
        }
        Application.OpenURL(url);
    }
}
