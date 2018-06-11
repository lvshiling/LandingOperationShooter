using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GBNAPI;
using AdvertisingPack;
using System;

public class GBNHZinit : MonoBehaviour
{
    public static string version = "1.0.0";
    //обновлены Flurry и OneAudience
    
    [System.Serializable]
    private struct FieldAds
    {
        public string name;
        public int rate;
        public float delay;
        public float delayBetweenLastAd;
    }

    [System.Serializable]
    private struct ScenesInterval
    {
        public bool haveInterval;
        public bool isGameScene;
    }

    [SerializeField]
    private FieldAds[] adFields = new FieldAds[3] {
        new FieldAds { name = "adpausegame",  rate = 0, delay = 45f,delayBetweenLastAd = 0f },
        new FieldAds { name = "interval",  rate = 0, delay = 120f,delayBetweenLastAd = 0f },
        new FieldAds { name = "reward0",    rate = 1, delay = 0f ,delayBetweenLastAd = 0f},
    };

    [SerializeField]
    private string[] eventsWhitelist = new string[] {

    };

    private static List<string> _eventsWhitelist;

    [SerializeField]
    private string[] rewardBlacklist = new string[] {

    };

    private static List<string> _rewardBlacklist;

    [SerializeField]
    private ScenesInterval[] sceneIndexesWithInterval;

    private static Dictionary<string, int> dataScenes = new Dictionary<string, int>();

    private static Dictionary<string, FieldAds> dataAds = new Dictionary<string, FieldAds>
    {
        
        // Значения по умолчанию                       
        // rate - частота показа рекламы. 0 - выкл, 1 - каждый первый вызов, 2 - каждый второй и т.д.
        // delay - задержка показа рекламы после вызова функции Show();
        
        // interval не требует вызова, достаточно задать значение полю delay большее ноля
        //{ "adpausemenu",    new FieldAds { name = "adpausemenu", rate = 0, delay = 30f } },     // реклама будет показываться не чаще, чем через этот промежуток в сценах без интервальной рекламы
        { "adpausegame",    new FieldAds { name = "adpausegame", rate = 0, delay = 45f } },     // реклама будет показываться не чаще, чем через этот промежуток в сценах с интервальной рекламой
        { "interval",       new FieldAds { name = "interval",   rate = 0, delay = 120f } },     // вызовы рекламмы через равный промежуток времени
        //{ "startgame",      new FieldAds { name = "startgame",  rate = 0, delay = 0f } },      // при старте игры
        //{ "pause",          new FieldAds { name = "pause",      rate = 3, delay = 0f } },      // при  нажатии паузы
        //{ "lose",           new FieldAds { name = "lose",       rate = 0, delay = 0f } },      // при проигрыше
        //{ "win",            new FieldAds { name = "win",        rate = 0, delay = 0f } },      // при выигрыше
        //{ "shop",           new FieldAds { name = "shop",       rate = 0, delay = 0f } },      // при входе в магазин
        { "reward0",        new FieldAds { name = "reward0",    rate = 1, delay = 0f } }      // включение реварда типа 0
        //{ "upgrade",        new FieldAds { name = "upgrade",    rate = 3, delay = 3.5f } },    // покупка апгрейда
        //{ "buyloc",         new FieldAds { name = "buyloc",     rate = 0, delay = 0f } },      // покупка локации
        //{ "buyitem",        new FieldAds { name = "buyitem",    rate = 0, delay = 0f } },      // покупка предмета/персонажа и пр.
        //{ "reward2",        new FieldAds { name = "reward2",    rate = 0, delay = 0f } },      // включение реварда типа 1
        //{ "reward3",        new FieldAds { name = "reward3",    rate = 0, delay = 0f } },      // включение реварда типа 2
        //{ "entermap",       new FieldAds { name = "entermap",   rate = 0, delay = 0f } },      // при входе в карту
        //{ "levelup",        new FieldAds { name = "levelup",    rate = 0, delay = 0f } },      // при левелапе
        //{ "missiontrigger1", new FieldAds { name = "missiontrigger1", rate = 0, delay = 0f } },// прочий триггер1
        //{ "missiontrigger2", new FieldAds { name = "missiontrigger2", rate = 0, delay = 0f } },// прочий триггер2
        //{ "missiontrigger3", new FieldAds { name = "missiontrigger3", rate = 0, delay = 0f } } // прочий триггер3
    }; 
    
    private static bool _isLoaded = false;
    public static bool isLoaded
    {
        get
        {
            return _isLoaded;
        }
    }

    private static bool isSuccess = false;

    [SerializeField]
    private bool testAds;
    
    private string cloudAdsId = "";
    public static string CloudBalanceId
    {
        get;
        protected set;
    }

    [SerializeField]
    private bool useCloudAds = true;

    [SerializeField]
    public GameObject globalManagerPrefab;

    static System.Action<bool> cbOnLoad;
    public static void AddOnLoadListener(System.Action<bool> callback)
    {
        if (_isLoaded == true)
        {
            callback(isSuccess);
        }
        cbOnLoad += callback;
    }
    public static void RemoveOnLoadListener(System.Action<bool> callback)
    {
        cbOnLoad -= callback;
    }

    private static bool CheckValid(string name, bool silent = false)
    {
        if (dataAds.ContainsKey(name) == false)
        {
            if (!silent)
            {
                Debug.Log("<color=red>В таблице рекламы НЕТ события с именем \"" + name + "\"</color>");
            }
            return false;
        }
        return true;
    }

    private void UpdateAdsData(FieldAds[] data)
    {
        var list = adFields.ToList();
        if (data != null)
        {
            foreach (FieldAds d in data)
            {
                dataAds[d.name] = d;
                int index = list.FindIndex(x => x.name == d.name);
                if (index >= 0)
                {
                    list[index] = d;
                }
                else
                {
                    list.Add(d);
                }
            }
            adFields = list.ToArray();
        }
    }

    private void Awake()
    {
        if (!isLoaded)
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            cloudAdsId = SDKInfo.GetKey("sdk_cloudadsid");

            InitGlobalManager();

            GBNHZshow.Init();

            //вгоняем рекламу из инспектора в словарь
            dataAds.Clear();
            for (int i = 0; i < adFields.Length; i++)
            {
                dataAds[adFields[i].name] = adFields[i];
            }

            //выставляем в статик-словарь параметры интервальной рекламы
            for (int j = 0; j < sceneIndexesWithInterval.Length; j++)
            {
                dataScenes.Add("sceneInterval" + j, sceneIndexesWithInterval[j].haveInterval ? 1 : 0);
                dataScenes.Add("sceneIsGame" + j, sceneIndexesWithInterval[j].isGameScene ? 1 : 0);
            }

            CloudBalanceId = cloudAdsId;

            GBNCloud.Init();

            if (useCloudAds && cloudAdsId.Length > 0)
            {
                GBNCloud.FetchBook(cloudAdsId, AdsDataLoaded);
            }
            else
            {
                AdsDataLoaded("", false);
            }

            _eventsWhitelist = new List<string>(eventsWhitelist);
            _rewardBlacklist = new List<string>(rewardBlacklist);
        }
    }

    private void InitGlobalManager()
    {
        var globalManager = GameObject.Find("GBNGlobalManager");
        if (globalManager == null)
        {
            globalManager = Instantiate(globalManagerPrefab);
        }
    }

    private void OnDestroy()
    {
        GBNCloud.Uninit();
        cbOnLoad = null; //не очень хорошо - защищает от nullref у нестатик подписок, но удалит и статик подписку, если она была..
    }

    private bool CheckRequaredFields()
    {
        bool changed = false;
        if (!CheckValid("adpausegame"))
        {
            dataAds.Add("adpausegame", new FieldAds { name = "adpausegame", rate = 0, delay = 45f });
            changed = true;
            Debug.Log("<color=green>Добавлено стандартное значение для события \"" + "adpausegame" + "\"</color>");
        }
        if (!CheckValid("interval"))
        {
            dataAds.Add("interval", new FieldAds { name = "interval", rate = 0, delay = 120f });
            changed = true;
            Debug.Log("<color=green>Добавлено стандартное значение для события \"" + "interval" + "\"</color>");
        }
        return changed;
    }
    
    private void AdsDataLoaded(string id, bool success)
    {
        if (success)
        {
            if (id == cloudAdsId)
            {
                // ads
                if (GBNCloud.HasSheet(cloudAdsId, "ads"))
                {
                    FieldAds[] data = GBNCloud.ReadSheet<FieldAds>(cloudAdsId, "ads");
                    CheckRequaredFields();
                    UpdateAdsData(data);                  
                    Debug.Log("<b><color=blue>Таблица рекламы загружена</color></b>");
                }
                else
                {
                    if (CheckRequaredFields())
                    {
                        UpdateAdsData(dataAds.Values.ToArray());
                    }
                    Debug.LogWarning("Лист \"ads\" не найден. Использованы локальные параметры");
                }

                // TODO: Локализацию вынести отсюда. 

#if LOCALE_VERSION
                if (GBNCloud.HasSheet(cloudAdsId, "localization"))
                {
                    GBNLocalization.Instance.Init(GBNCloud.ReadSheetTable<string>(id, "localization", ""));
                }
                else
                {
                    Debug.LogWarning("Лист \"localization\" не найден. GBNLocalization не инициализирован");
                }
#endif
            }
        }
        else
        {
            if (CheckRequaredFields())
            {
                UpdateAdsData(dataAds.Values.ToArray());
            }
            Debug.Log("<b><color=blue>Используется локальная таблица рекламы (облако отключено!)</color></b>");
        }
        _isLoaded = true;
        if (cbOnLoad != null)
        {
            cbOnLoad(isSuccess);
        }
        GBNEventManager.TriggerEvent(GBNEvent.CLOUD_LOADED);
    }

#region PUBLIC
    public static bool IsWhitelistEvent(string evt)
    {
        if (_eventsWhitelist != null)
        {
            if (_eventsWhitelist.Contains(evt))
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsWhitelistReward(string rwd)
    {
        return !IsBlacklistReward(rwd);
    }

    public static bool IsBlacklistReward(string rwd)
    {
        if (_rewardBlacklist != null)
        {
            if (_rewardBlacklist.Contains(rwd))
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsAdsEnabled()
    {
#if ADS_VERSION
        return true;
#else
        return false;

#endif
    }
    
    public static bool IsPrivacyPolicyEnabled()
    {
#if ADS_VERSION && UNITY_ANDROID
        if (string.IsNullOrEmpty(CompanyInfo.Struct.policy))
        {
            return false;
        }
        return true;
#else
        return false;
#endif
    }

    [Obsolete("GetPrivacyPolicyUrl() is deprecated, please use CompanyInfo.Struct.policy instead.")]
    public static string GetPrivacyPolicyUrl()
    {
        return CompanyInfo.Struct.policy;
    }

    public static int GetRate(string name, bool silent = false)
    {
        return CheckValid(name, silent) ? dataAds[name].rate : 0;
    }

    public static float GetDelay(string name, bool silent = false)
    {
        return CheckValid(name, silent) ? dataAds[name].delay : 0f;
    }

    public static float GetDelayBetweenLastAd(string name, bool silent = false)
    {
        return CheckValid(name, silent) ? dataAds[name].delayBetweenLastAd : 0f;
    }

    public static bool IsGameScene(int sceneBuildIndex)
    {
        if (dataScenes != null && dataScenes.ContainsKey("sceneIsGame" + sceneBuildIndex))
        {
            return dataScenes["sceneIsGame" + sceneBuildIndex] == 1;
        }
        return false;
    }

    [Obsolete("IsSceneWithInterval(int sceneBuildIndex) is deprecated, all scenes should have interval AD.")]
    public static bool IsSceneWithInterval(int sceneBuildIndex)
    {
        if (dataScenes != null && dataScenes.ContainsKey("sceneInterval" + sceneBuildIndex))
        {
            return dataScenes["sceneInterval" + sceneBuildIndex] == 1;
        }
        return false;
    }

    public static string applicationBundleIdentifier
    {
        get
        {
            return CompanyInfo.bundleIdentifier;
        }
    }

#endregion
}
