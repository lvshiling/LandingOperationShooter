using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using AdvertisingPack;

public class GBNHZshow : MonoBehaviour
{
#if FINAL_VERSION
    static bool printDebug = false;
#else
    static bool printDebug = true;
#endif

    static string debugPrefix = "AdsDebug: ";

    #region SINGLETON
    private static GBNHZshow instance;

    public static GBNHZshow Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(GBNHZshow)) as GBNHZshow;

                if (instance == null)
                {
                    GameObject gameObject = new GameObject(typeof(GBNHZshow).Name);
                    gameObject.transform.SetParent(null);
                    DontDestroyOnLoad(gameObject);

                    instance = gameObject.AddComponent(typeof(GBNHZshow)) as GBNHZshow;
                }
                else
                {
                    instance.gameObject.transform.SetParent(null);
                    DontDestroyOnLoad(instance.gameObject);
                }
            }
            return instance;
        }
    }
    #endregion

    [SerializeField]
    private List<GameObject> rewardButtons;
    [Header("Privacy Policy")]
    [SerializeField]
    private GameObject privacyPolicyButton;

    private static float lastAdTime = 0f;

    private static float lastInFocusTime = 0f;
    private static bool isOutOfFocus = false;

    private static float timeOnSceneWithoutInterval = 0f;

    private static Dictionary<string, int> adEventsCounters;

    private static bool delayedShowInvoked = false;

    private static bool isLoaded = false;

    int curSceneIndex = -1;

    private static float nextIntervalAdTime;

    private bool adsLimited = false;

    private bool _useInterval = false;
    private bool useInterval
    {
        get
        {
            return _useInterval;
        }
        set
        {
            _useInterval = value;
        }
    }
    private bool _isGameScene = false;
    private bool isGameScene
    {
        get
        {
            return _isGameScene;
        }
        set
        {
            _isGameScene = value;
        }
    }

    private bool _isBannerActive = false;
    public bool isBannerActive
    {
        get
        {
            return _isBannerActive;
        }
    }

    private float intervalDelay;

    private float pauseBetweenAds;

    private float timeScaleBeforeAds = 1f;
    private float soundVolumeBeforeAds = 1f;

    private float soundVolume
    {
        get
        {
            return AudioListener.volume;
        }
        set
        {
            AudioListener.volume = value;
        }
    }

    private float timeScale
    {
        get
        {
            return Time.timeScale;
        }
        set
        {
            Time.timeScale = value;
        }
    }
    
    private long lastInvokedRewardId = -1;
    private bool showRewardLocked = false;

    public int currentRewardType
    {
        get
        {
            if (lastInvokedRewardId < 0)
            {
                return -1;
            }
            return GetRewardTypeById(lastInvokedRewardId);
        }
    }

    public static int GetRewardTypeById(long id)
    {
        return (int)(id % 100);
    }

    public static long GenerateRewardId(GameObject rewardBtn, int rewardType)
    {
        return System.Math.Abs((System.Math.Abs((long)rewardBtn.transform.GetInstanceID()) * 100 + rewardType));
    }

    public void SetGameSceneFlag(bool value)
    {
        if (isGameScene != value)
        {
            isGameScene = value;

            SetPauseBetweenAds();

            SetBannerState();
        }
    }

    private static int GetAdEventCounter(string evt)
    {
        if (adEventsCounters == null)
        {
            adEventsCounters = new Dictionary<string, int>();
        }

        string key = "Ads" + evt;
        if (adEventsCounters.ContainsKey(key))
        {
            return adEventsCounters[key];
        }
        else
        {
            int val = 0;
            adEventsCounters.Add(key, val);
            return val;
        }
    }

    private static void SetAdEventCounter(string evt, int val)
    {
        if (adEventsCounters == null)
        {
            adEventsCounters = new Dictionary<string, int>();
        }

        string key = "Ads" + evt;
        if (adEventsCounters.ContainsKey(key))
        {
            adEventsCounters[key] = val;
        }
        else
        {
            adEventsCounters.Add(key, val);
        }
    }

    public static void Init()
    {
        if (instance == null)
        {
            instance = FindObjectOfType(typeof(GBNHZshow)) as GBNHZshow;

            if (instance == null)
            {
                GameObject gameObject = new GameObject(typeof(GBNHZshow).Name);
                gameObject.transform.SetParent(null);
                DontDestroyOnLoad(gameObject);

                instance = gameObject.AddComponent(typeof(GBNHZshow)) as GBNHZshow;
            }
            else
            {
                instance.gameObject.transform.SetParent(null);
                DontDestroyOnLoad(instance.gameObject);
            }
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(transform.gameObject);
        }
        else if (this != instance)
        {
            if (rewardButtons == null)
            {
                rewardButtons = new List<GameObject>();
            }
            instance.rewardButtons = rewardButtons;
            instance.privacyPolicyButton = privacyPolicyButton;

            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }

        curSceneIndex = SceneManager.GetActiveScene().buildIndex;

        useInterval = true;
        intervalDelay = -1;
        isGameScene = false;
        adsLimited = !CoolTool.Ads;

        if (rewardButtons == null)
        {
            rewardButtons = new List<GameObject>();
        }

        CoolTool.OnResetAds += OnCooltoolAdsEvent;

        SceneManager.activeSceneChanged += OnSceneWasLoaded;
        SceneManager.sceneUnloaded += OnSceneWasUnloaded;

        GBNHZinit.AddOnLoadListener(AwakeCall);
    }

    private void OnSceneWasUnloaded(Scene oldScene)
    {
        FinalizeTimers();

        if (rewardButtons != null)
        {
            rewardButtons.RemoveAll(item => item == null);
        }
    }

    private void OnSceneWasLoaded(Scene oldScene, Scene newScene)
    {
        if (privacyPolicyButton == null)
        {
            BtnPrivacyPolicy findButton = FindObjectOfType<BtnPrivacyPolicy>();
            if (findButton != null)
            {
                privacyPolicyButton = findButton.gameObject;
            }
        }

        if (curSceneIndex == newScene.buildIndex)
        {
            DisableAllButtons();
            GBNHZinit.AddOnLoadListener(OnAdsLoaded);
            return;
        }

        curSceneIndex = newScene.buildIndex;

        isGameScene = GBNHZinit.IsGameScene(curSceneIndex);

        InitTimers();

        SetBannerState();

        DisableAllButtons();

        if (GBNHZinit.isLoaded)
        {
            OnAdsLoaded(true);
        }
    }

    private void Update()
    {
        CheckIntervalAd();
    }

    private void OnDestroy()
    {
        if (curSceneIndex >= 0)
        {
            FinalizeTimers();
        }
        GBNHZinit.RemoveOnLoadListener(OnAdsLoaded);
        GBNHZinit.RemoveOnLoadListener(AwakeCall);
        CoolTool.OnResetAds -= OnCooltoolAdsEvent;
        SceneManager.activeSceneChanged -= OnSceneWasLoaded;
        SceneManager.sceneUnloaded -= OnSceneWasUnloaded;
    }

    void OnEnable()
    {
        EnableEvents();
    }

    void OnDisable()
    {
        DisableEvents();
    }

    private void OnCooltoolAdsEvent(bool state)
    {
        adsLimited = !state;
        UpdateRewardBtns();
    }

    private void AwakeCall(bool state)
    {
        if (!isLoaded)
        {
            nextIntervalAdTime = GBNHZinit.GetDelay("interval") + Time.unscaledTime;
            isLoaded = true;
        }
        //чекаем игровая это сцена или нет
        isGameScene = GBNHZinit.IsGameScene(curSceneIndex);

        InitTimers();

        SetBannerState();
    }

    private void SetBannerState()
    {
#if ADS_VERSION
        if (isGameScene)
        {
            AdsManager.Banner.Hide();
            if (CoolTool.isTestDevice || printDebug)
            {
                Debug.Log(debugPrefix + "Нижний баннер отключен.");
            }
        }
        else
        {
            AdsManager.Banner.Show();
            if (CoolTool.isTestDevice || printDebug)
            {
                Debug.Log(debugPrefix + "Нижний баннер включен.");
            }
        }
#endif
    }

    private void SetPauseBetweenAds()
    {
        pauseBetweenAds = GBNHZinit.GetDelay("adpausegame");
        if (CoolTool.isTestDevice || printDebug)
        {
            Debug.Log(debugPrefix + "Сцена #" + curSceneIndex + " | " + "Промежуток между показами рекламы: " + pauseBetweenAds + "с.");
        }
    }

    private void InitTimers()
    {
        SetPauseBetweenAds();

        if (useInterval)
        {
            intervalDelay = Mathf.Max(GBNHZinit.GetDelay("interval"), pauseBetweenAds);
            nextIntervalAdTime += timeOnSceneWithoutInterval;

            timeOnSceneWithoutInterval = 0f;

            if (CoolTool.isTestDevice || printDebug)
            {
                Debug.Log(debugPrefix + "Сцена #" + curSceneIndex + " | " + "Следующая интервальная реклама в: " + nextIntervalAdTime + ". Текущее значение таймера: " + Time.unscaledTime + " (Реклама через " + (nextIntervalAdTime - Time.unscaledTime) + "c.)");
                if (adsLimited)
                {
                    Debug.Log(debugPrefix + "Сцена #" + curSceneIndex + " | " + "Интервальная реклама не будет показана, т.к. включено ограничение рекламы.");
                }
            }
        }
        else
        {
            if (Mathf.Approximately(timeOnSceneWithoutInterval, 0f))
            {
                timeOnSceneWithoutInterval = Time.unscaledTime;
            }
            else
            {
                timeOnSceneWithoutInterval = Time.unscaledTime - timeOnSceneWithoutInterval;
            }

            if (CoolTool.isTestDevice || printDebug)
            {
                Debug.Log(debugPrefix + "Сцена #" + curSceneIndex + " | " + "Интервальная реклама не используется. Текущее значение таймера: " + Time.unscaledTime);
            }
        }
    }
    
#if ADS_VERSION
    private void OnRewardedVideoImpressionEvent()
    {
        PauseOn();
#if UNITY_IOS
        OnApplicationFocus(false);   
#endif
    }

    private void OnInterstitialImpressionEvent()
    {
        PauseOn();
#if UNITY_IOS
        OnApplicationFocus(false);
#endif
    }

    private void OnInterstitialFinishedEvent()
    {
        PauseOff();
#if UNITY_IOS
        OnApplicationFocus(true);
#endif
    }

    private void OnRewardedVideoSuccessEvent()
    {
        PauseOff();

        if (lastInvokedRewardId < 0)
        {
            GBNEventManager.TriggerEvent(GBNEvent.REWARD_SHOWN);
        }
        else
        {
            GBNEventManager.TriggerEvent(GBNEvent.REWARD_SHOWN + lastInvokedRewardId);
        }
#if UNITY_IOS
        OnApplicationFocus(true);
#endif
    }

    private void OnRewardedVideoClosedEvent()
    {
        PauseOff();

        if (lastInvokedRewardId < 0)
        {
            GBNEventManager.TriggerEvent(GBNEvent.REWARD_CANCELED);
        }
        else
        {
            GBNEventManager.TriggerEvent(GBNEvent.REWARD_CANCELED + lastInvokedRewardId);
        }
#if UNITY_IOS
        OnApplicationFocus(true);
#endif
    }

    private void OnBannerShow()
    {
        _isBannerActive = true;
    }

    private void OnBannerHide()
    {
        _isBannerActive = false;
    }
#endif

    public static void AddRewardButton(GameObject btn)
    {
        if (Instance.rewardButtons == null)
        {
            Instance.rewardButtons = new List<GameObject>();
        }

        if (!Instance.rewardButtons.Contains(btn))
        {
            bool containsAsChild = false;
            foreach (GameObject obj in Instance.rewardButtons)
            {
                if (btn.transform.IsChildOf(obj.transform))
                {
                    containsAsChild = true;
                    break;
                }
            }
            if (!containsAsChild)
            {
                Instance.rewardButtons.Add(btn);
                Instance.UpdateRewardBtn(btn);
            }
        }
    }

    public static void RemoveRewardButton(GameObject btn)
    {
        if (instance == null)
        {
            return;
        }
        if (instance.rewardButtons != null)
        {
            int index = instance.rewardButtons.IndexOf(btn);
            if (index >= 0)
            {
                instance.rewardButtons.RemoveAt(index);
            }
        }
    }

    private void OnAdsLoaded(bool ok)
    {
        UpdateButtonsState();
    }

    private void PauseOn()
    {
        // save previous params
        soundVolumeBeforeAds = soundVolume;
        timeScaleBeforeAds = timeScale;
        // set pause on ads
        soundVolume = 0;
        timeScale = 0;
    }

    private void PauseOff()
    {
        // restore state before ads
        soundVolume = soundVolumeBeforeAds;
        timeScale = timeScaleBeforeAds;
    }

    public void ShowReward(long rewardId)
    {
#if ADS_VERSION
        if (showRewardLocked)
        {
            return;
        }
        string rewardName = "reward" + GetRewardTypeById(rewardId);
        if (adsLimited && GBNHZinit.IsBlacklistReward(rewardName))
        {
            if (CoolTool.isTestDevice || printDebug)
            {
                Debug.Log(debugPrefix + "Показ заблокирован, т.к. включено ограничение рекламы и ревард [" + rewardName + "] находится в черном списке.");
            }
            return;
        }

        if (!GBNAPI.Network.IsConnected())
        {
            GBNAPI.Dialogs.NoInternetAccessWarning();
            return;
        }
        lastInvokedRewardId = rewardId;

        if (CoolTool.isTestDevice || printDebug)
        {
            Debug.Log(debugPrefix + "Показ ревардной рекламы  [" + rewardName + "].");
        }

        AdsManager.RewardedVideo.Show();

        lastAdTime = Time.unscaledTime;
        if (CoolTool.isTestDevice || printDebug)
        {
            Debug.Log(debugPrefix + "Этот показ учтен в таймере последней рекламы.");
        }
        StartCoroutine(LockRewardShowing(5f));
#endif
    }

    IEnumerator LockRewardShowing(float seconds)
    {
        showRewardLocked = true;
        yield return new WaitForSecondsRealtime(seconds);
        showRewardLocked = false;
    }

    private void ShowInternal(bool logAdTime = true)
    {
#if ADS_VERSION
        AdsManager.Interstitial.Show();
        if (logAdTime)
        {
            lastAdTime = Time.unscaledTime;
            if (CoolTool.isTestDevice || printDebug)
            {
                Debug.Log(debugPrefix + "Этот показ учтен в таймере последней рекламы.");
            }
        }
        else
        {
            if (CoolTool.isTestDevice || printDebug)
            {
                Debug.Log(debugPrefix + "Этот показ НЕ учтен в таймере последней рекламы.");
            }
        }
#endif
    }

    public void Show()
    {
#if ADS_VERSION
        if (adsLimited)
        {
            if (CoolTool.isTestDevice || printDebug)
            {
                Debug.Log(debugPrefix + "Показ заблокирован, т.к. включено ограничение рекламы.");
            }
            return;
        }
        ShowInternal();
#endif
    }

    public void Show(bool logAdTime)
    {
#if ADS_VERSION
        if (adsLimited && logAdTime)
        {
            if (CoolTool.isTestDevice || printDebug)
            {
                Debug.Log(debugPrefix + "Показ заблокирован, т.к. включено ограничение рекламы.");
            }
            return;
        }
        if (CoolTool.isTestDevice || printDebug)
        {
            Debug.Log(debugPrefix + "Вызов показа рекламы");
        }
        ShowInternal(logAdTime);
#endif
    }

    public void ShowFromFirstEvent(string evt)
    {
        if (GBNHZinit.IsAdsEnabled() && GBNHZinit.GetRate(evt) > 0)
        {
            int counter = GetAdEventCounter("Ads" + evt);
            if (counter == 0)
            {
                if (CoolTool.isTestDevice || printDebug)
                {
                    Debug.Log(debugPrefix + "Счетчик рекламы по событию [" + evt + "] был увеличен - показ после первого события.");
                }
                SetAdEventCounter("Ads" + evt, GBNHZinit.GetRate(evt) - 1);
            }
            Show(evt);
        }
    }

    public void Show(string evt)
    {
#if ADS_VERSION
        if (adsLimited && !GBNHZinit.IsWhitelistEvent(evt))
        {
            if (CoolTool.isTestDevice || printDebug)
            {
                Debug.Log(debugPrefix + "Показ заблокирован, т.к. включено ограничение рекламы и событие [" + evt + "] не в белом списке.");
            }
            return;
        }
        if (CoolTool.isTestDevice || printDebug)
        {
            Debug.Log(debugPrefix + "Произошло событие рекламы [" + evt + "].");
        }
        if (GBNHZinit.IsAdsEnabled() && GBNHZinit.GetRate(evt) > 0)
        {
            int counter = GetAdEventCounter("Ads" + evt) + 1;
            SetAdEventCounter("Ads" + evt, counter);

            if (counter % GBNHZinit.GetRate(evt) == 0)
            {
                //достаточно ли прошло времени с последней рекламы
                if (Time.unscaledTime >= lastAdTime + pauseBetweenAds || lastAdTime == 0f)
                {
                    float delay = GBNHZinit.GetDelay(evt);
                    if (CoolTool.isTestDevice || printDebug)
                    {
                        Debug.Log(debugPrefix + "Сработал триггер рекламы [" + evt + "]" + (delay > 0 ? " Реклама будет показана через " + delay + "с." : ""));
                    }
                    if (!delayedShowInvoked)
                    {
                        StartCoroutine(DelayedShow(delay));
                    }
                    else
                    {
                        if (CoolTool.isTestDevice || printDebug)
                        {
                            Debug.Log(debugPrefix + "Реклама по событию [" + evt + "] не была показана т.к уже вызван отложенный показ.");
                        }
                    }
                }
                else
                {
                    if (CoolTool.isTestDevice || printDebug)
                    {
                        Debug.Log(debugPrefix + "Реклама по событию [" + evt + "] не была показана т.к не прошло достаточно времени с последнего показа рекламы. Следующая реклама не раньше, чем в " + (lastAdTime + pauseBetweenAds)
                        + ". Текущее значение таймера: " + Time.unscaledTime + " (Реклама доступна через " + ((lastAdTime + pauseBetweenAds) - Time.unscaledTime) + "c.)");
                    }
                }
            }
            else
            {
                if (CoolTool.isTestDevice || printDebug)
                {
                    Debug.Log(debugPrefix + "Триггер рекламы [" + evt + "] не сработал, т.к. счетчик не достиг нужного значения Rate (" + (counter % GBNHZinit.GetRate(evt)) + "/" + GBNHZinit.GetRate(evt) + ").");
                }
            }
        }
#endif
    }

    private IEnumerator DelayedShow(float delay)
    {
        delayedShowInvoked = true;

        if (delay > 0)
        {
            HoldIntervalAdsForSecondsInternal(delay);
            yield return new WaitForSecondsRealtime(delay);
        }
        ShowInternal();
        delayedShowInvoked = false;
    }

    public void StartIntervalAd()
    {
        if (useInterval)
        {
            return;
        }
        if (CoolTool.isTestDevice || printDebug)
        {
            Debug.Log(debugPrefix + "Сцена #" + curSceneIndex + " | " + "Интервальная реклама включена вручную.");
        }
        FinalizeTimers();
        useInterval = true;
        InitTimers();
    }

    public void PauseIntervalAd()
    {
        if (!useInterval)
        {
            return;
        }
        if (CoolTool.isTestDevice || printDebug)
        {
            Debug.Log(debugPrefix + "Сцена #" + curSceneIndex + " | " + "Интервальная реклама приостановлена вручную.");
        }
        FinalizeTimers();
        useInterval = false;
        InitTimers();
    }

    public void HoldIntervalAdsForSeconds(float time)
    {
#if ADS_VERSION
        if (useInterval)
        {
            float holdTime = Mathf.Clamp(time, 0f, float.MaxValue);
            if (CoolTool.isTestDevice || printDebug)
            {
                Debug.Log(debugPrefix + "Запрос на сдвиг интервальной рекламы на " + holdTime + "c.");
            }
            HoldIntervalAdsForSecondsInternal(holdTime);
        }
#endif
    }

    private void HoldIntervalAdsForSecondsInternal(float holdTime)
    {
#if ADS_VERSION
        if (useInterval)
        {
            float nextIntervalPause = nextIntervalAdTime - Time.unscaledTime;
            if (nextIntervalPause < holdTime)
            {
                nextIntervalAdTime = Time.unscaledTime + holdTime;
                if (CoolTool.isTestDevice || printDebug)
                {
                    Debug.Log(debugPrefix + "Интервальная реклама была сдвинута на " + holdTime + "c. Новое время интервальной рекламы: " + nextIntervalAdTime
                    + ". Текущее значение таймера: " + Time.unscaledTime + " (Реклама через " + (nextIntervalAdTime - Time.unscaledTime) + "c.).");
                }
            }
            else
            {
                if (CoolTool.isTestDevice || printDebug)
                {
                    Debug.Log(debugPrefix + "Здвиг интервальной рекламы не требуется. Время интервальной рекламы: " + nextIntervalAdTime
                    + ". Текущее значение таймера: " + Time.unscaledTime + " (Реклама через " + (nextIntervalAdTime - Time.unscaledTime) + "c.).");
                }
            }
        }
#endif
    }

    void EnableEvents()
    {
#if ADS_VERSION
        AdsManager.Interstitial.OnImpression += OnInterstitialImpressionEvent;
        AdsManager.RewardedVideo.OnImpression += OnRewardedVideoImpressionEvent;

        AdsManager.Interstitial.OnFinished += OnInterstitialFinishedEvent;
        AdsManager.RewardedVideo.OnSuccess += OnRewardedVideoSuccessEvent;
        AdsManager.RewardedVideo.OnClosed += OnRewardedVideoClosedEvent;

        AdsManager.Banner.OnShow += OnBannerShow;
        AdsManager.Banner.OnHide += OnBannerHide;
#endif
    } 

    void DisableEvents()
    {
#if ADS_VERSION
        AdsManager.Interstitial.OnImpression -= OnInterstitialImpressionEvent;
        AdsManager.RewardedVideo.OnImpression -= OnRewardedVideoImpressionEvent;

        AdsManager.Interstitial.OnFinished -= OnInterstitialFinishedEvent;
        AdsManager.RewardedVideo.OnSuccess -= OnRewardedVideoSuccessEvent;
        AdsManager.RewardedVideo.OnClosed -= OnRewardedVideoClosedEvent;

        AdsManager.Banner.OnShow -= OnBannerShow;
        AdsManager.Banner.OnHide -= OnBannerHide;
#endif
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            lastInFocusTime = Time.realtimeSinceStartup;
            isOutOfFocus = true;
            if (CoolTool.isTestDevice || printDebug)
            {
                Debug.Log(debugPrefix + "Фокус потерян.");
            }
        }
        else
        {
            float outOfFocusTime = Time.realtimeSinceStartup - lastInFocusTime;
            lastInFocusTime = Time.realtimeSinceStartup;
            if (lastAdTime > 0f)
            {
                lastAdTime += outOfFocusTime;
            }
            if (useInterval)
            {
                nextIntervalAdTime += outOfFocusTime;
            }
            isOutOfFocus = false;
            if (CoolTool.isTestDevice || printDebug)
            {
                Debug.Log(debugPrefix + "Фокус восстановлен, таймеры скорректированы. Таймеры отодвинуты на " + outOfFocusTime + "c.");
            }
        }
    }

    private void CheckIntervalAd()
    {
#if ADS_VERSION
        if (useInterval && !isOutOfFocus && GBNHZinit.IsAdsEnabled() && intervalDelay > 0)
        {
            if (Time.unscaledTime > nextIntervalAdTime)
            {
                if (nextIntervalAdTime < lastAdTime + pauseBetweenAds)
                {
                    nextIntervalAdTime = lastAdTime + pauseBetweenAds;
                    if (CoolTool.isTestDevice || printDebug)
                    {
                        Debug.Log(debugPrefix + "Интервальная реклама не была показана т.к не прошло достаточно времени с последнего показа рекламы. Интервальная реклама была сдвинута. Новое время интервальной рекламы: " + nextIntervalAdTime
                            + ". Текущее значение таймера: " + Time.unscaledTime + " (Реклама через " + (nextIntervalAdTime - Time.unscaledTime) + "c.).");
                    }
                }
                else
                {
                    if (CoolTool.isTestDevice || printDebug)
                    {
                        Debug.Log(debugPrefix + "Показ рекламы по интервалу " + intervalDelay + " секунд");
                    }
                    nextIntervalAdTime = Time.unscaledTime + intervalDelay;
                    Show();
                }
            }
        }
#endif
    }

    private void FinalizeTimers()
    {
        if (useInterval)
        {
            if (CoolTool.isTestDevice || printDebug)
            {
                Debug.Log(debugPrefix + "Сцена #" + curSceneIndex + " | " + "Сохранено значение таймера для следующей интервальной рекламы: " + nextIntervalAdTime);
            }
        }
        else
        {
            timeOnSceneWithoutInterval = Time.unscaledTime - timeOnSceneWithoutInterval;
            if (CoolTool.isTestDevice || printDebug)
            {
                Debug.Log(debugPrefix + "Сцена #" + curSceneIndex + " | " + " Время на сцене без интервальной рекламы: " + timeOnSceneWithoutInterval);
            }
        }
    }

    private void DisableAllButtons()
    {
        SetActivePrivacyPolicyBtn(false);
        SetActiveRewardBtns(false);
    }

    public void SetActivePrivacyPolicyBtn(bool state)
    {
        if (privacyPolicyButton != null)
        {
            privacyPolicyButton.SetActive(state);
        }
    }

    public void SetActiveRewardBtns(bool state)
    {
        if (rewardButtons != null)
        {
            if (state)
            {
                UpdateRewardBtns();
            }
            else
            {
                foreach (GameObject go in rewardButtons)
                {
                    if (go != null)
                        go.SetActive(state);
                }
            }
        }
    }

    private void UpdateRewardBtns()
    {
        if (rewardButtons != null)
        {
            foreach (GameObject go in rewardButtons)
            {
                UpdateRewardBtn(go);
            }
        }
    }

    private void UpdateRewardBtn(GameObject btnGO)
    {
        if (btnGO != null)
        {
            var gbnhzBtnReward = btnGO.GetComponentInChildren<GBNHZbtnReward>();
            if (gbnhzBtnReward != null)
            {
                string rewardName = "reward" + gbnhzBtnReward.rewardType;
                bool btnEnabled = (GBNHZinit.GetRate(rewardName, true) != 0);
                if (adsLimited && GBNHZinit.IsBlacklistReward(rewardName))
                {
                    btnEnabled = false;
                }
                btnGO.SetActive(GBNHZinit.IsAdsEnabled() && btnEnabled);
            }
            else
            {
                btnGO.SetActive(GBNHZinit.IsAdsEnabled() && !adsLimited);
            }
        }
    }

    private void UpdateButtonsState()
    {
        if (privacyPolicyButton != null)
        {
#if ADS_VERSION && UNITY_ANDROID
            privacyPolicyButton.SetActive(true);
#else
            privacyPolicyButton.SetActive(false);
#endif
        }
        UpdateRewardBtns();
    }
}
