using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CloudConnector : MonoBehaviour
{
    // -- Complete the following fields. --
    private string webServiceUrl
    {
        get
        {
			return GBNAPI.SDKInfo.GetKey("sdk_cloudurl");
        }
    }
    [HideInInspector]
    public string spreadsheetId = "1upjpXgV2rffV_gN-AhETK6uS4uypGV9IWna9aPSPUsI"; // If this is a fixed value could also be setup on the webservice to save POST request size.
    private string servicePassword
    {
        get
        {
			return GBNAPI.SDKInfo.GetKey("sdk_cloudpwd");
        }
    }

    private float timeOutLimit = 10f;
    public bool usePOST = true;
    // --
    
    private static CloudConnector _Instance;
    public static CloudConnector Instance
    {
        get
        {
            return _Instance ?? (_Instance = new GameObject("CloudConnector").AddComponent<CloudConnector>());
        }
    }

    private UnityWebRequest www;

    private void OnDestroy()
    {
        _Instance = null;
    }
	
    public void CreateRequest(Dictionary<string, string> form)
    {
        form.Add("ssid", spreadsheetId);
        form.Add("pass", servicePassword);
		
        if (usePOST)
        {
            CloudConnectorCore.UpdateStatus("Establishing Connection at URL " + webServiceUrl);
            www = UnityWebRequest.Post(webServiceUrl, form);
        }
        else // Use GET.
        {
            string urlParams = "?";
            foreach (KeyValuePair<string, string> item in form)
            {
                urlParams += item.Key + "=" + item.Value + "&";
            }
            CloudConnectorCore.UpdateStatus("Establishing Connection at URL " + webServiceUrl + urlParams);
            www = UnityWebRequest.Get(webServiceUrl + urlParams);
        }
		
        StartCoroutine(ExecuteRequest(form));
    }
	
    IEnumerator ExecuteRequest(Dictionary<string, string> postData)
    {
        www.Send();
		
        float startTime = Time.realtimeSinceStartup;
        float elapsedTime = 0.0f;

        while (!www.isDone)
        {
            elapsedTime = Time.realtimeSinceStartup - startTime;
            if (elapsedTime >= timeOutLimit)
            {
                CloudConnectorCore.ProcessResponse("TIME_OUT", elapsedTime);
                break;
            }
			
            yield return null;
        }

        elapsedTime = Time.realtimeSinceStartup - startTime;

#if UNITY_2017_1_OR_NEWER
        if (www.isNetworkError || www.isHttpError)
#else
        if (www.isError)
#endif
        {
            CloudConnectorCore.ProcessResponse(CloudConnectorCore.MSG_CONN_ERR + "Connection error after " + elapsedTime.ToString() + " seconds: " + www.error, elapsedTime);
            yield break;
        }	
		
        CloudConnectorCore.ProcessResponse(www.downloadHandler.text, elapsedTime);
    }
   
}


	
