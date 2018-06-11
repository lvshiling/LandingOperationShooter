using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AdvertisingPack;
using UnityEngine.SceneManagement;
using GBNAPI;

namespace AdvertisingPack
{
#if UNITY_EDITOR
    public class TestAdsController : AbstractAdsController
    {
        public class DummyMonoBehaviour : MonoBehaviour
        {

        }

        [RequireComponent(typeof(Canvas))]
        public class TestBannerBehaviour : MonoBehaviour
        {
            public static bool HasBanner = false;

            [RequireComponent(typeof(Image), typeof(AspectRatioFitter), typeof(CanvasGroup))]
            public class TestBanner : MonoBehaviour
            {
                public RectTransform rt;
                public AspectRatioFitter arf;
                public CanvasGroup cg;

                void Awake()
                {
                    transform.SetParent(instance.transform);
                    rt = GetComponent<RectTransform>();
                    arf = GetComponent<AspectRatioFitter>();
                    cg = GetComponent<CanvasGroup>();
                }
            }

            private static bool _bannerEnabled = false;
            private static bool bannerEnabled
            {
                get
                {
                    return _bannerEnabled && HasBanner;
                }
                set
                {
                    _bannerEnabled = value;
                }
            }
            private static TestBannerBehaviour _instance = null;
            private static TestBannerBehaviour instance
            {
                get
                {
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("TestBannerCanvas");
                        DontDestroyOnLoad(go);
                        _instance = go.AddComponent<TestBannerBehaviour>();
                        Canvas canvas = go.GetComponent<Canvas>();
                        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                        canvas.sortingOrder = 200;
                        go.transform.SetAsLastSibling();
                    }
                    return _instance;
                }
            }

            bool isHorizontal = true;

            TestBanner banner;

            public class BannerPosition
            {
                private float _x;
                public float x { get { return _x; } }

                private float _y;
                public float y { get { return _y; } }

                public BannerPosition()
                {
                    string x = "center";
                    string y = "bottom";

                    switch (x)
                    {
                        case "left":
                            _x = 0f;
                            break;
                        default:
                        case "center":
                            _x = 0.5f;
                            break;
                        case "right":
                            _x = 1f;
                            break;
                    }

                    if (y == "top")
                        _y = 1f;
                    else
                        _y = 0f;
                }
            }

            static BannerPosition _banPos;
            public static BannerPosition bannerPosition
            {
                get
                {
                    if (_banPos == null)
                    {
                        _banPos = new BannerPosition();
                    }
                    return _banPos;
                }
            }

            void Start()
            {
                ResizeBanner();
            }

            void Update()
            {
                if (!Application.isPlaying && isHorizontal != Screen.width > Screen.height)
                {
                    ResizeBanner();
                }
            }

            void ResizeBanner()
            {
                if (banner == null)
                {
                    banner = new GameObject("Banner").AddComponent<TestBanner>();
                }

                transform.SetAsLastSibling();

                banner.cg.alpha = bannerEnabled ? 0.5f : 0f;
                banner.cg.interactable = bannerEnabled;
                banner.cg.blocksRaycasts = bannerEnabled;

                banner.arf.aspectRatio = 6.4f;
        
                banner.rt.pivot = bannerPosition.x * Vector2.right + (bannerPosition.y == 1 ? Vector2.up : Vector2.zero);
                isHorizontal = Screen.width > Screen.height;
                if (isHorizontal)
                {
                    banner.arf.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                    banner.rt.anchorMin = bannerPosition.x * Vector2.right + (bannerPosition.y == 1 ? 0.84375f * Vector2.up : Vector2.zero);
                    banner.rt.anchorMax = bannerPosition.x * Vector2.right + (bannerPosition.y == 1 ? Vector2.up : 0.15625f * Vector2.up);
                    banner.rt.offsetMin = Vector2.zero;
                    banner.rt.offsetMax = Vector2.zero;
                    banner.rt.anchoredPosition = Vector2.zero;
                }
                else
                {
                    banner.arf.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
                    banner.rt.anchorMin = (bannerPosition.x == 0 ? 0 : (0.05555f + (bannerPosition.x == 1 ? 0.05555f : 0))) * Vector2.right + (bannerPosition.y == 1 ? Vector2.up : Vector2.zero);
                    banner.rt.anchorMax = (bannerPosition.x == 1 ? 1 : (0.94445f - (bannerPosition.x == 0 ? 0.05555f : 0))) * Vector2.right + (bannerPosition.y == 1 ? Vector2.up : Vector2.zero);
                    banner.rt.offsetMin = Vector2.zero;
                    banner.rt.offsetMax = Vector2.zero;
                    banner.rt.anchoredPosition = Vector2.zero;
                }
            }

            public static void SetBanner(bool state)
            {
                bannerEnabled = state;
                instance.ResizeBanner();
            }
        }

        private static MonoBehaviour CoroutineHelper = null;

        public override void Init()
        {
            TestBannerBehaviour.HasBanner = false;
        }

        protected void CreateCoroutineHelper()
        {
            if (CoroutineHelper == null)
            {
                GameObject go = new GameObject("TestAdsController(CoroutineHelper)");
                CoroutineHelper = go.AddComponent<DummyMonoBehaviour>();
                GameObject.DontDestroyOnLoad(go);
            }
        }

        public override void ShowInterstitial()
        {
            OnInterstitialImpression.Invoke();
#if !FINAL_VERSION
            if (CoroutineHelper == null)
            {
                CreateCoroutineHelper();
            }

            CoroutineHelper.StartCoroutine(WaitForFakeInterstitialAds(2f));
#else
            OnInterstitialFinished.Invoke();
#endif
        }

        private IEnumerator WaitForFakeInterstitialAds(float delay)
        {
            float t = 0f;
            BroadcastAll("OnApplicationFocus", false);
            Debug.LogWarning("Interstitial Started");
            while (t < delay)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            Debug.LogWarning("Interstitial Ends");
            OnInterstitialFinished.Invoke();
            BroadcastAll("OnApplicationFocus", true);
        }

        public override void ShowRewardedVideo()
        {
            OnRewardedVideoImpression.Invoke();
#if !FINAL_VERSION
            if (CoroutineHelper == null)
            {
                CreateCoroutineHelper();
            }

            CoroutineHelper.StartCoroutine(WaitForFakeRewardedVideoAds(2f));
#else
            OnRewardedVideoSuccess.Invoke();
#endif
        }

        private IEnumerator WaitForFakeRewardedVideoAds(float delay)
        {
            float t = 0f;
            BroadcastAll("OnApplicationFocus", false);
            Debug.LogWarning("RewardedVideo Started");
            while (t < delay)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            Debug.LogWarning("RewardedVideo Ends");
            OnRewardedVideoSuccess.Invoke();
            BroadcastAll("OnApplicationFocus", true);
        }

        private void BroadcastAll(string fun, object msg)
        {
            GameObject[] gos = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
            foreach (GameObject go in gos)
            {
                if (go && go.transform.parent == null)
                {
                    go.gameObject.BroadcastMessage(fun, msg, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        public override bool IsInterstitialReady()
        {
            return true;
        }

        public override bool IsRewardedVideoReady()
        {
            return true;
        }

        public override bool IsBannerReady()
        {
            return true;
        }

        public override void ShowBanner()
        {
            TestBannerBehaviour.SetBanner(true);
            OnBannerShow.Invoke();
        }

        public override void HideBanner()
        {
            TestBannerBehaviour.SetBanner(false);
            OnBannerHide.Invoke();
        }

	public override void GenerateReport()
        {
#if ADS_VERSION
            var report = new SDKManagement.SDKReport();

            report.Add(new SDKManagement.SDKInfo("Test Ads", SDKManagement.SDKStatus.TEST, "Ads SDK does not start in the editor."));

            if (OnReportComplete != null)
            {
                OnReportComplete.Invoke(report);
            }
#endif            
        }
    }
#endif
}
