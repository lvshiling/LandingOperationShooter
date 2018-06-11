using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AdvertisingPack;

namespace AdvertisingPack
{
    public class AdsManager
    {
#if FINAL_VERSION
        static bool printDebug = false;
#else
        static bool printDebug = true;
#endif
        static bool allowInitOnRequest = false;

        public static AbstractAdsController adsController { get; private set; }

        static bool isInited = false;

        public static void Init()
        {
#if ADS_VERSION
            if (isInited)
            {
                return;
            }

            if (printDebug)
            {
                Debug.Log("AdsManager.Init()");
            }
#if !UNITY_EDITOR
            adsController = new UnityAdsController(); //можно заменить на любой класс, наследуемый от AbstractAdsController
#else
            adsController = new TestAdsController();
#endif
            adsController.Init();

            adsController.OnInterstitialImpression += OnInterstitialImpressionInvoked;
            adsController.OnInterstitialFinished += OnInterstitialFinishedInvoked;

            adsController.OnRewardedVideoImpression += OnRewardedVideoImpressionInvoked;
            adsController.OnRewardedVideoSuccess += OnRewardedVideoSuccessInvoked;
            adsController.OnRewardedVideoClosed += OnRewardedVideoClosedInvoked;

            adsController.OnBannerShow += OnBannerShowInvoked;
            adsController.OnBannerHide += OnBannerHideInvoked;

            isInited = true;
#endif
        }

        private static void OnInterstitialImpressionInvoked()
        {
            Interstitial.OnImpression.Invoke();
        }

        private static void OnInterstitialFinishedInvoked()
        {
            Interstitial.OnFinished.Invoke();
        }

        private static void OnRewardedVideoImpressionInvoked()
        {
            RewardedVideo.OnImpression.Invoke();
        }

        private static void OnRewardedVideoSuccessInvoked()
        {
            RewardedVideo.OnSuccess.Invoke();
        }

        private static void OnRewardedVideoClosedInvoked()
        {
            RewardedVideo.OnClosed.Invoke();
        }

        private static void OnBannerShowInvoked()
        {
            Banner.OnShow.Invoke();
        }

        private static void OnBannerHideInvoked()
        {
            Banner.OnHide.Invoke();
        }

        public class Interstitial
        {
            public static bool IsReady
            {
                get
                {
                    if (isInited && adsController != null)
                    {
                        return adsController.IsInterstitialReady();
                    }
                    return false;
                }
            }

            public static Action OnImpression = () => {
                if (printDebug)
                {
                    Debug.Log("Interstitial.OnImpression");
                }
            };

            public static Action OnFinished = () =>
            {
                if (printDebug)
                {
                    Debug.Log("Interstitial.OnFinished");
                }
            };

            public static void Show()
            {
                if (!isInited)
                {
                    if (allowInitOnRequest)
                    {
                        Init();
                    }
                    else
                    {
                        if (printDebug)
                        {
                            Debug.Log("AdsManager is not initialized!");
                        }
                        return;
                    }
                }

                if (adsController != null)
                {
                    adsController.ShowInterstitial();
                }
            }
        }
        public class RewardedVideo
        {
            public static bool IsReady
            {
                get
                {
                    if (isInited && adsController != null)
                    {
                        return adsController.IsRewardedVideoReady();
                    }
                    return false;
                }
            }

            public static Action OnImpression = () => {
                if (printDebug)
                {
                    Debug.Log("RewardedVideo.OnImpression");
                }
            };

            public static Action OnSuccess = () => {
                if (printDebug)
                {
                    Debug.Log("RewardedVideo.OnSuccess");
                }
            };

            public static Action OnClosed = () => {
                if (printDebug)
                {
                    Debug.Log("RewardedVideo.OnClosed");
                }
            };

            public static void Show()
            {
                if (!isInited)
                {
                    if (allowInitOnRequest)
                    {
                        Init();
                    }
                    else
                    {
                        if (printDebug)
                        {
                            Debug.Log("AdsManager is not initialized!");
                        }
                        return;
                    }
                }

                if (adsController != null)
                {
                    adsController.ShowRewardedVideo();
                }
            }
        }
        public class Banner
        {
            public static bool IsReady
            {
                get
                {
                    if (isInited && adsController != null)
                    {
                        return adsController.IsBannerReady();
                    }
                    return false;
                }
            }

            public static Action OnShow = () => {
                if (printDebug)
                {
                    Debug.Log("Banner.OnShow");
                }
            };

            public static Action OnHide = () =>
            {
                if (printDebug)
                {
                    Debug.Log("Banner.OnHide");
                }
            };

            public static void Show()
            {
                if (!isInited)
                {
                    if (allowInitOnRequest)
                    {
                        Init();
                    }
                    else
                    {
                        if (printDebug)
                        {
                            Debug.Log("AdsManager is not initialized!");
                        }
                        return;
                    }
                }

                if (adsController != null)
                {
                    adsController.ShowBanner();
                }
            }

            public static void Hide()
            {
                if (isInited && adsController != null)
                {
                    adsController.HideBanner();
                }
            }
        }
    }
}
