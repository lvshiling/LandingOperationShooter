using SDKManagement;
using UnityEngine;

namespace AnalyticsPack
{
    public class AnalyticsDummy : IAnalytics
    {
        public bool IsInited()
        {
#if UNITY_EDITOR || !FINAL_VERSION
            return true;
#else
            return false;
#endif
        }

        public void Init()
        {
#if UNITY_EDITOR || !FINAL_VERSION
            Debug.Log("Init Dummy Analitycs... Ok!");
#endif
        }

        public void InvokeEvent(string name, string key, string val)
        {
#if UNITY_EDITOR || !FINAL_VERSION
            Debug.Log(string.Concat("<color='blue'>", name, " ", key, " ", val, "</color>"));
#endif
        }

        public void InvokeEvents(Ev e)
        {
#if UNITY_EDITOR || !FINAL_VERSION
            Debug.Log(e);
#endif
        }

        public SDKInfo GetInfo()
        {
#if UNITY_EDITOR || !FINAL_VERSION
            return new SDKInfo("Dummy", SDKStatus.INITED, "OK");
#else
            return new SDKInfo("Dummy", SDKStatus.CANCELED, "Works in Unity Editor only");
#endif
        }
    }
}