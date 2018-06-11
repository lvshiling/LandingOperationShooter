using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

using UnityEngine;

namespace AnalyticsPack
{
    public class AnalyticsGeneralParams : MonoBehaviour
    {
        private bool isInited = false;

        public Action OnInit;

        public void Init()
        {
#if ADS_VERSION
            StartCoroutine(WaitForCoolToolTestDeviceCheck());
            StartCoroutine(WaitDeviceAndLocation());
#endif
        }

        private void InitCompleted()
        {
            if (OnInit != null)
            {
                OnInit.Invoke();
            }
        }

        private IEnumerator WaitDeviceAndLocation()
        {
            while (!isInited)
            {
                if (!isLocationInited)
                {
                    InitLocation();
                }
                if (isDeviceChecked && isLocationInited)
                {
                    InitCompleted();
                }
                yield return null;
            }
            yield break;
        }

        #region EventParams

        private string GetCountry()
        {
            return country;
        }

        private string GetLanguage()
        {
            return Application.systemLanguage.ToString();
        }

        private string GetLongitude()
        {
            return longitude.ToString(Analytics.LOCATION_FORMAT);
        }

        private string GetLatitude()
        {
            return latitude.ToString(Analytics.LOCATION_FORMAT);
        }

        public string GetTimeGMT()
        {
            string s = DateTime.Now.ToUniversalTime().ToString();
            s = s.Replace(':', '-');
            return s;
        }

        public string GetDeviceUniqueIdentifier()
        {
#if UNITY_IOS && !UNITY_EDITOR
            string id = UnityEngine.iOS.Device.advertisingIdentifier;
#else
            string id = SystemInfo.deviceUniqueIdentifier;
#endif
            return id;
        }

        public bool isTestDevice
        {
            get
            {
                return testDevice == "1";
            }
        }

        private string GetTestDevice()
        {
            return testDevice;
        }

        public Dictionary<string, string> GetGeneralParams()
        {
            Dictionary<string, string> events = new Dictionary<string, string>();

            events.Add("device_id", GetDeviceUniqueIdentifier());
            events.Add("timestamp", GetTimeGMT());
            events.Add("language", GetLanguage());
            events.Add("test_device", GetTestDevice());
            events.Add("country_code", GetCountry());
            events.Add("latitude", GetLatitude());
            events.Add("longitude", GetLongitude());

            return events;
        }

        #endregion EventParams

        #region TestDevice

        private string testDevice = "0";

        private bool isDeviceChecked = false;

        private IEnumerator WaitForCoolToolTestDeviceCheck()
        {
            while (!isDeviceChecked && !CoolTool.IsRequestCompleted)
            {
                yield return null;
            }
            isDeviceChecked = true;
            testDevice = CoolTool.isTestDevice ? "1" : "0";
        }

        #endregion TestDevice

        #region CountryTool

        private bool isLocationInited = false;

        private string country = "";

        private double latitude = 0.0;
        private double longitude = 0.0;

        public void InitLocation()
        {
            if (isLocationInited)
                return;
#if UNITY_EDITOR
            isLocationInited = true;
            //Debug.Log("InitLocation call on device only");
            country = "RU";
            return;
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass unityActivityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activityContext = unityActivityClass.GetStatic<AndroidJavaObject>("currentActivity");
                using (AndroidJavaClass countryTool = new AndroidJavaClass("com.countrytool.CountryTool"))
                {
                    countryTool.CallStatic("init", activityContext);
                    country = countryTool.CallStatic<string>("getCountry");
                    string countryProvider = countryTool.CallStatic<string>("getCountryProvider");
                    latitude = countryTool.CallStatic<double>("getLatitude");
                    longitude = countryTool.CallStatic<double>("getLongitude");
                    //Debug.Log("Country: " + country + " countryProvider: " + countryProvider + " latitude: " + latitude + " longitude: " + longitude);
                }
            }
			isLocationInited = true;

#elif UNITY_IOS
            InitLocationIOS();
			if (IsInitedIOS()) {
				country = GetCountryIOS();
				latitude = GetLatitudeIOS();
				longitude = GetLongitudeIOS();
				//Debug.Log("Country: " + country + " latitude: " + latitude + " longitude: " + longitude);
				isLocationInited = true;
			}
#endif
        }

#if UNITY_IOS
        [DllImport("__Internal")]
		private static extern void InitLocationIOS();

        [DllImport("__Internal")]
		private static extern string GetCountryIOS();

        [DllImport("__Internal")]
		private static extern double GetLatitudeIOS();

        [DllImport("__Internal")]
		private static extern double GetLongitudeIOS();

		[DllImport("__Internal")]
		private static extern bool IsInitedIOS();
#endif

        #endregion CountryTool
    }
}