using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GBNAPI;
using System;
#if ADS_VERSION
using UnityEngine.Advertisements;
#endif

namespace AdvertisingPack
{
    public class UnityAdsController : AbstractAdsController
    {
#if FINAL_VERSION
        private const bool printDebug = false;
#else
        private const bool printDebug = true;
#endif
        private const string rewardedPlacementId = "rewardedVideo";

        string key = "";

        public override void Init()
        {
#if ADS_VERSION
            key = SDKInfo.GetKey("sdk_unityadskey");

            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            if (printDebug)
            {
                Debug.Log(this.GetType() + " - Init()");
            }

#if UNITY_ANDROID || UNITY_IOS

            Advertisement.Initialize(key, printDebug);
#else
            if (printDebug)
            {
                Debug.Log(this.GetType() + " - unsupported platform!");
            }
#endif
#endif
        }

        public override bool IsInterstitialReady()
        {
#if ADS_VERSION
            return Advertisement.IsReady();
#else
            return false;
#endif
        }

        public override bool IsRewardedVideoReady()
        {
#if ADS_VERSION
            return Advertisement.IsReady(rewardedPlacementId);
#else
            return false;
#endif
        }

        public override bool IsBannerReady()
        {
            return false;
        }

        public override void ShowInterstitial()
        {
#if ADS_VERSION
            if (IsInterstitialReady())
            {
                if (printDebug)
                {
                    Debug.Log(this.GetType() + " - Show Interstitial");
                }

                OnInterstitialImpression();

                var options = new ShowOptions
                {
                    resultCallback = result => OnInterstitialFinished()
                };

                Advertisement.Show(options);
            }
            else
            {
                if (printDebug)
                {
                    Debug.Log(this.GetType() + " - Interstitial is not ready!");
                }
            }
#endif
        }

        public override void ShowRewardedVideo()
        {
#if ADS_VERSION
            if (IsRewardedVideoReady())
            {
                if (printDebug)
                {
                    Debug.Log(this.GetType() + " - Show RewardedVideo");
                }

                OnRewardedVideoImpression();

                var options = new ShowOptions
                {

                    resultCallback = (result) =>
                    {
                        if (result == ShowResult.Finished)
                        {
                            OnRewardedVideoSuccess();
                        }
                        else
                        {
                            OnRewardedVideoClosed();
                        }
                    }
                };
                Advertisement.Show(rewardedPlacementId, options);
            }
            else
            {
                if (printDebug)
                {
                    Debug.Log(this.GetType() + " - RewardedVideo is not ready!");
                }
            }
#endif
        }

        public override void ShowBanner()
        {
#if ADS_VERSION
#endif
        }

        public override void HideBanner()
        {
#if ADS_VERSION
#endif
        }

#region ISDKReporter
        public override void GenerateReport()
        {
#if ADS_VERSION
            var report = new SDKManagement.SDKReport(false);
            try
            {
                report.Add(new SDKManagement.SDKInfo("UnityAds", SDKManagement.SDKStatus.INITED, "SDK Version: " + Advertisement.version));
                report.Add(new SDKManagement.SDKInfo(
                    "UnityAds",
                    Advertisement.isInitialized ? SDKManagement.SDKStatus.INITED :
                    (Advertisement.isSupported ? SDKManagement.SDKStatus.FAILED : SDKManagement.SDKStatus.WAITING),
                    Advertisement.isInitialized ? "Initialize: OK" :
                    (Advertisement.isSupported ? "Initialize: FAIL" : "Waiting initialization")
                    ));

                var placementState = Advertisement.GetPlacementState();
                report.Add(new SDKManagement.SDKInfo(
                    "UnityAds",
                    placementState == PlacementState.Ready ? SDKManagement.SDKStatus.INITED :
                    (placementState == PlacementState.Waiting ? SDKManagement.SDKStatus.WAITING :
                    SDKManagement.SDKStatus.FAILED),
                    placementState == PlacementState.Ready ? "Interstitial: READY" :
                    (placementState == PlacementState.Waiting ? "Interstitial: WAITING" :
                    "Interstitial: NOT Available")
                    ));

                placementState = Advertisement.GetPlacementState(rewardedPlacementId);
                report.Add(new SDKManagement.SDKInfo(
                    "UnityAds RewardedVideo",
                    placementState == PlacementState.Ready ? SDKManagement.SDKStatus.INITED :
                    (placementState == PlacementState.Waiting ? SDKManagement.SDKStatus.WAITING :
                    SDKManagement.SDKStatus.FAILED),
                    placementState == PlacementState.Ready ? "Rewarded Video: READY" :
                    (placementState == PlacementState.Waiting ? "Rewarded Video: WAITING" :
                    "Rewarded Video: NOT Available")
                    ));
            }
            catch (Exception e)
            {
                report.Add(new SDKManagement.SDKInfo("UnityAds", SDKManagement.SDKStatus.FAILED, "Initialization FAILED: " + e.Message));
            }

            report.Add(new SDKManagement.SDKInfo("UnityAds", SDKManagement.SDKStatus.INITED, "SDK Key: " + key));

            if (OnReportComplete != null)
            {
                OnReportComplete.Invoke(report);
            }
#endif
        }
#endregion
    }
}
