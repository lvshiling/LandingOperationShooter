using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckGlVersion : MonoBehaviour {
    private static CheckGlVersion _instance;

    public static CheckGlVersion Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CheckGlVersion>();
            }
            return _instance;
        }
    }

  
    public string GetVersion()
    {
       return SystemInfo.graphicsDeviceVersion;

    }


    public bool CheckEffectPossible()
    {
        string version = GetVersion();
        Debug.Log(version);
#if UNITY_ANDROID
        if (version.Contains("3.0") || version.Contains("3.2"))
        {
            return true;
        }
        else
        {
            return false;
        }
#elif UNITY_IOS
         if(version.Contains("3.0") || version.Contains("3.1"))
        {
            return true;
        }
        else
        {
            return false;
        }
#endif

    }
}
