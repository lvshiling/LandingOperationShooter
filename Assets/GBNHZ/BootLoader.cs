using GBNAPI;
using AdvertisingPack;
using AnalyticsPack;
using SDKManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BootLoader : MonoSingleton<BootLoader>
{
    protected override void Awake ()
    {
        base.Awake();

        StartCoroutine(EntryPoint());
    }

    IEnumerator EntryPoint()
    {
#if ADS_VERSION
        yield return StartCoroutine(RunGDPRDialog());
        yield return StartCoroutine(RunDefaultPermissionsCheck()); //default permissions required for the Analytics and Ads SDK (the AdsManager)

        GameObject sdkManager = new GameObject("SDKManager", new System.Type[] { typeof(SDKManager) });
        GameObject analytics = new GameObject("Analytics", new System.Type[] { typeof(Analytics) });

        AdsManager.Init();
#else
        yield return null;
#endif
    }

    IEnumerator RunGDPRDialog()
    {
        // Configure GDPR Window 
        var gdprWindow = new GDPRWindow();

        // OPTIONAL METHODS can set before show
        //gdprWindow.IsGDPRScope = false;
        //gdprWindow.SetShortTitle("Short Title");
        //gdprWindow.SetShortText("Short Text");
        //gdprWindow.SetInfoTitle("Info Title");
        //gdprWindow.SetInfoText("Info TEXT");

        // Show GDPR Window 
        gdprWindow.Show(CompanyInfo.Struct.policy);
        // Wait for User responce
        while (!gdprWindow.ConfirmationStatus)
        {
            yield return null;
        }
    }

    IEnumerator RunDefaultPermissionsCheck()
    {
        string[] permissions = GetDefaultPermissions();
        PermissionManager pm = new PermissionManager();
        pm.Add(permissions);
        pm.RequestStatus();
        while (!pm.isChecked(permissions))
        {
            yield return null;
        }
    }

    private string [] GetDefaultPermissions()
    {
        string sdkAdaptersFilename = "SDKAdapters";

        TextAsset config = Resources.Load<TextAsset>(sdkAdaptersFilename);
        if (config != null)
        {
            JSONObject rootJson = new JSONObject(config.text);
            if (rootJson != null && rootJson.HasField("defaultPermissions"))
            {
                JSONObject defaultPermissions = rootJson.GetField("defaultPermissions");

                if (defaultPermissions.IsArray)
                {
                    string [] permissions = new string[defaultPermissions.Count];
                    for (int i = 0; i < permissions.Length; ++i)
                    {
                        permissions[i] = defaultPermissions[i].str;
                    }
                    return permissions;
                }
                else
                {
                    Debug.Log("Error: Resources / SDKAdapters.txt field 'defaultPermissions' is not array");
                }

            }
            else
            {
                Debug.Log("Error: Resources / SDKAdapters.txt has not field 'defaultPermissions'");
            }
        }
        else
        {
            Debug.Log("Error: Resources / SDKAdapters.txt not found");
        }

        return new string[] { };
    }
}
