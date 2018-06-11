using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AdvertisingPack;
using SDKManagement;

namespace AdvertisingPack
{
    public abstract class AbstractAdsController : ISDKReporter
    {
        public abstract void Init();

        public abstract bool IsInterstitialReady();

        public abstract void ShowInterstitial();

        public Action OnInterstitialImpression;
        public Action OnInterstitialFinished;

        public abstract bool IsRewardedVideoReady();

        public abstract void ShowRewardedVideo();

        public Action OnRewardedVideoImpression;
        public Action OnRewardedVideoSuccess;
        public Action OnRewardedVideoClosed;

        public abstract bool IsBannerReady();

        public abstract void ShowBanner();

        public abstract void HideBanner();

        public Action OnBannerShow;
        public Action OnBannerHide;

	    #region ISDKReporter
        public abstract void GenerateReport();
        public Action<SDKReport> OnReportComplete { get; set; }
        public virtual bool hasButton
        {
            get
            {
                return false;
            }
        }
        public virtual void ButtonClick()
        {
           
        }
        public virtual string buttonLabel
        {
            get
            {
                return "";
            }
        }
        #endregion

        protected static bool IsTablet()
        {
            float screenSize = GetPhysicalDisplaySize();

            if (screenSize >= 7.0f)
            {
                return true;
            }

            return false;
        }

        private static float GetPhysicalDisplaySize()
        {
            float inchWidth = 0;
            float inchHeight = 0;
#if UNITY_ANDROID
            try
            {
                AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject metrics = new AndroidJavaObject("android.util.DisplayMetrics");
                activity.Call<AndroidJavaObject>("getWindowManager").Call<AndroidJavaObject>("getDefaultDisplay").Call("getMetrics", metrics);

                inchWidth = Screen.width / metrics.Get<float>("xdpi");
                inchHeight = Screen.height / metrics.Get<float>("ydpi");
            }
            catch
            {
                inchWidth = Screen.width / Screen.dpi;
                inchHeight = Screen.height / Screen.dpi;
            }
#endif
#if UNITY_IOS
            inchWidth = Screen.width / Screen.dpi;
            inchHeight = Screen.height / Screen.dpi;
#endif
            return (float)Math.Round(Math.Sqrt((inchWidth * inchWidth) + (inchHeight * inchHeight)), 1);
        }

    }
}
