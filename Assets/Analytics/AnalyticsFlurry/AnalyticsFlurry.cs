using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Linq;
using System.Text;
using SDKManagement;
#if ADS_VERSION
using AnalyticsPack;
#endif
using Debug = UnityEngine.Debug;

namespace AnalyticsPack
{
    public class AnalyticsFlurry : IAnalytics
    {
        private string _androidGooglePlayApiKey = string.Empty;
        private string _iosFreeApiKey = string.Empty;

        Dictionary<string, string> defEvents;

        public bool IsInited()
        {
#if FINAL_VERSION && ADS_VERSION
#if UNITY_ANDROID && !UNITY_EDITOR
        return FlurryAndroid.IsSessionActive();
#endif
#if UNITY_IOS && !UNITY_EDITOR
        return FlurryIOS.ActiveSessionExists();
#endif
#endif
            return false;
        }

        public void Init()
        {
#if FINAL_VERSION && ADS_VERSION
            IAnalyticsFlurry service = Flurry.Instance;

            _androidGooglePlayApiKey = "";
            _iosFreeApiKey = "";

#if !ANALYTICS_TEST
#if UNITY_ANDROID
            _androidGooglePlayApiKey = GBNAPI.SDKInfo.GetKey("sdk_flurrykey");
#elif UNITY_IOS
            _iosFreeApiKey = GBNAPI.SDKInfo.GetKey("sdk_flurrykey");
#endif

            if (string.IsNullOrEmpty(_iosFreeApiKey) && string.IsNullOrEmpty(_androidGooglePlayApiKey))
            {
                return;
            }
#endif

            service.SetLogLevel(LogLevel.All);
#if !UNITY_EDITOR
#if UNITY_ANDROID
			FlurryAndroid.SetCaptureUncaughtExceptions(false);
#endif
#endif
            service.StartSession(_iosFreeApiKey, _androidGooglePlayApiKey);
#if UNITY_ANDROID
            FlurryAndroid.SetLogEnabled(true);
#endif
#if UNITY_IOS
			FlurryIOS.SetDebugLogEnabled(true);
#endif
            //Debug.Log("Init Flurry Analitycs... Ok!");
#endif
        }

        public void InvokeEvent(string name, string key, string val)
        {
#if FINAL_VERSION && ADS_VERSION
            Dictionary<string, string> pairs = new Dictionary<string, string>();
            pairs.Add(key, val);
            Flurry.Instance.LogEvent(name, pairs);
#endif
           
            Debug.Log("Flurry Event: " + name + ", key=" + key + ", val=" + val);
            
		}

        public void InvokeEvents(Ev e)
        {
#if FINAL_VERSION && ADS_VERSION
            Flurry.Instance.LogEvent(e.name, e.PackAllParams());
#endif
        }

        public SDKInfo GetInfo()
        {
            SDKInfo info = new SDKInfo("Flurry", "Key: " + GBNAPI.SDKInfo.GetKey("sdk_flurrykey"));

#if UNITY_EDITOR
            info.status = SDKStatus.TEST;
#else
#if UNITY_ANDROID
            var status = FlurryAndroid.IsSessionActive();
            info.status = status ? SDKStatus.INITED : SDKStatus.FAILED;
            if (status)
            {
                var version = FlurryAndroid.GetAgentVersion();
                var sessionId = FlurryAndroid.GetSessionId();
                info.message += "\nVersion: "+version +" sessionId: "+ sessionId;
            }
#elif UNITY_IOS
            info.status = FlurryIOS.ActiveSessionExists() ? SDKStatus.INITED : SDKStatus.FAILED;
#endif
#endif
            return info;
        }
    }
}
