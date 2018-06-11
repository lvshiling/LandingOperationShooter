using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Text;
using System.Text.RegularExpressions;
using GBNAPI;
using System.Collections.Generic;

//Cooltool Version: 3.0.1

public class CoolTool : MonoBehaviour
{
    //Default flags states (will be used when the CoolTool server is unavailable)
    private const bool CrosspromoDefaultState = false;
    private const bool YoutubeDefaultState =
#if ADS_VERSION
    false;
#else
    true;
#endif
    private const bool CheatsDefaultState = false;
    private const bool AdsDefaultState = false;
    private const bool MoregamesDefaultState =
#if ADS_VERSION
    false;
#else
    true;
#endif
    private const bool TestDeviceDefaultState = false;
    //

    public static Action<bool> OnResetCheats, OnResetYoutube, OnResetCrosspromo, OnResetAds, OnResetMoregames;

    public static bool cheats
    {
        get
        {
            if (mainParams != null)
            {
                return mainParams.cheats == 1;
            }
            return CheatsDefaultState;
        }
    }

    public static bool Youtube
    {
        get
        {
            if (mainParams != null)
            {
                return mainParams.youtube == 1;
            }
            return YoutubeDefaultState;
        }
    }

    public static bool Crosspromo
    {
        get
        {
            if (mainParams != null)
            {
                return mainParams.crosspromo == 1;
            }
            return CrosspromoDefaultState;
        }
    }

    public static bool Ads
    {
        get
        {
            if (mainParams != null)
            {
                return mainParams.ads == 1;
            }
            return AdsDefaultState;
        }
    }

    public static bool Moregames
    {
        get
        {
            if (mainParams != null)
            {
                return mainParams.moregames == 1;
            }
            return MoregamesDefaultState;
        }
    }
    public static bool isTestDevice
    {
        get
        {
            if (mainParams != null)
            {
                return mainParams.testdevice == 1;
            }
            return TestDeviceDefaultState;
        }
    }
    [System.Obsolete("isYoutubeStatus is deprecated, please use CoolTool.Youtube instead.")]
    public bool isYoutubeStatus
    {
        get
        {
            return Youtube;
        }
    }

    [System.Obsolete("isCrosspromoStatus is deprecated, please use CoolTool.Crosspromo instead.")]
    public bool isCrosspromoStatus
    {
        get
        {
            return Crosspromo;
        }
    }


    private static CoolTool instance = null;

    private string url;

    private string prefsKey = "__cachedservervalue";

    private string cachedJson
    {
        get
        {
            if (PlayerPrefs.HasKey(prefsKey))
            {
#if !FINAL_VERSION || UNITY_EDITOR
                Debug.Log("<color=yellow>Параметры CoolTool взяты из кеша последнего успешного запроса!</color>");
#endif
                return PlayerPrefs.GetString(prefsKey);
            }
            return ""; //JsonUtility.ToJson(new MainParams());
        }
        set
        {
            PlayerPrefs.SetString(prefsKey, value);
        }
    }

    private bool isSyncronized = false;

    private bool isDuplicateObject = false;

    private bool isRequestCompleted = false;
    public static bool IsRequestCompleted
    {
        get
        {
            if (instance != null)
            {
                return instance.isRequestCompleted;
            }
            else
            {
                return false;
            }
        }
    }

    private string _cryptKey = "";
    private string cryptKey
    {
        get
        {
            if (string.IsNullOrEmpty(_cryptKey))
            {
                _cryptKey = SDKInfo.GetKey("sdk_cooltool");
            }
            return _cryptKey;
        }
    }

    public static string sdkJson
    {
        get; private set;    
    }

    private static MainParams mainParams = null;

    [Serializable]
    public class MainParams
    {
        public MainParams()
        {
            crosspromo = CrosspromoDefaultState ? 1 : 0;
            youtube = YoutubeDefaultState ? 1: 0;
            cheats = CheatsDefaultState ? 1 : 0;
            ads = AdsDefaultState ? 1 : 0;
            moregames = MoregamesDefaultState ? 1 : 0;
            platform = "NONE";
            bundle = "NONE";
            protocol = "NONE";
            testdevice = TestDeviceDefaultState ? 1 : 0;
        }

        public int crosspromo;
        public int youtube;
        public int cheats;
        public int ads;
        public int moregames;
        public string platform;
        public string bundle;
        public string protocol;
        public int testdevice;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            isDuplicateObject = true;
        }

        if (mainParams == null)
        {
            mainParams = new MainParams();
        }

        if (isDuplicateObject)
        {
            return;
        }

        url = GetServerUrl();
    }

    IEnumerator Start()
    {
        InitialEvent(false);

        if (isDuplicateObject)
        {
            SetParams();
            Destroy(gameObject);
            yield break;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
#if !FINAL_VERSION || UNITY_EDITOR
            Debug.Log("<color=yellow>Не обращаемся к серверу CoolTool - нет интернета</color>");
#endif
            Parse(cachedJson);
            SetParams();
            StartCoroutine(WaitForServerResponse());
            yield break;
        }
#if !UNITY_ANDROID && !UNITY_IOS
#if !FINAL_VERSION || UNITY_EDITOR
        Debug.Log("<color=red>Платформа не поддерживается! CoolTool доступен только в Android и iOS версиях</color>");
#endif
        SetParams();
        yield break;
#endif
#if !ADS_VERSION
#if !FINAL_VERSION || UNITY_EDITOR
        Debug.Log("<color=green>Не обращаемся к серверу CoolTool в Amazon версии</color>");
#endif
        SetParams();
        yield break;
#endif
#if UNITY_EDITOR
        if (!isCorrectBundle(GetBundleIdentifier()))
        {
            Debug.Log("<color=red><b>E:</b> Некорректный бандл! " + GetBundleIdentifier() + "</color>");
            SetParams();
            yield break;
        }
        if (System.IO.File.Exists(Application.dataPath + "/Config.txt"))
        {
            string[] KEY_SEPARATOR = new string[] { ";" };
            string[] LINE_SEPARATOR = new string[] { "\n" };
            string content = System.IO.File.ReadAllText(Application.dataPath + "/Config.txt");
            string[] lines = content.Split(LINE_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var pair = line.Split(KEY_SEPARATOR, StringSplitOptions.None);
                var value = pair[1].Trim(new char[] { ' ' }).Replace("\t", "").Replace("\\t", "").Replace("\r", "").Replace("\\r", "").Replace("\n", "").Replace("\\n", "");
#if UNITY_ANDROID
            if (pair[0] == "GP Bundle")
#elif UNITY_IOS
            if (pair[0] == "AS Bundle Free")
#endif
                {
                    if (value != GetBundleIdentifier() || !isCorrectBundle(value))
                    {
                        Debug.Log("<color=red><b>E:</b> Бандл в настройках не соответствует бандлу платформы в Config.txt или бандл в Config.txt некорректного формата.</color> \nБандл в настроках: " + GetBundleIdentifier() + ", Бандл в Config.txt: " + value + ".");
                        SetParams();
                        yield break;
                    }
                }
            }
        }
#endif
        yield return StartCoroutine(ServerRequest());
        SetParams();
        StartCoroutine(WaitForServerResponse());
    }

    private IEnumerator ServerRequest()
    {
        isRequestCompleted = false;
        WWW www = new WWW(url);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
#if !FINAL_VERSION || UNITY_EDITOR
            Debug.Log("<color=red>Ошибка сервера CoolTool: " + www.error + "</color>");
#endif
            Parse(cachedJson);
        }
        else
        {
            //Debug.Log(www.text);
            string json = Crypt.AESDecrypt(www.text, cryptKey);
            //Debug.Log(json);
            if (string.IsNullOrEmpty(json))
            {
#if !FINAL_VERSION || UNITY_EDITOR
                Debug.Log("<color=red>Ошибка CoolTool: не удалось расшифровать ответ!</color>");
#endif
                Parse(cachedJson);
            }
            else if (json.Contains("Not valid data"))
            {
#if !FINAL_VERSION || UNITY_EDITOR
                Debug.Log("<color=red>Ошибка CoolTool: сервер не смог расшифровать запрос или переданы некорректные данные!</color>");
#endif
                Parse(cachedJson);
            }
            else
            {
                isSyncronized = true;
                cachedJson = json;
                Parse(json);
            }
        }
        www.Dispose();
    }

    private IEnumerator WaitForServerResponse()
    {
        while (!isSyncronized)
        {
            yield return StartCoroutine(ContiniousDialing());
            if (isSyncronized)
            {
                SetParams();
            }
        }
    }

    private void Parse(string _json)
    {
        if (string.IsNullOrEmpty(_json))
        {
            mainParams = new MainParams();
        }
        else
        {
            try
            {
                mainParams = JsonUtility.FromJson<MainParams>(_json);
                ParseSdkList(_json);
            }
            catch
            {
                mainParams = new MainParams();
            }
        }
        isRequestCompleted = true;
    }

    private void ParseSdkList(string _json)
    {
        JSONObject obj = new JSONObject(_json);
        if (obj.HasField("sdk"))
        {
            JSONObject sdk = obj.GetField("sdk");
            if (sdk.IsArray && sdk.list.Count > 0)
            {
                sdkJson = sdk.ToString();
            }
            else
            {
                //SDK is not Array
            }
        }
        else
        {
            //SDK field not found in json
        }
    }

    private void SetParams()
    {
#if !FINAL_VERSION || UNITY_EDITOR
        Debug.Log("<color=yellow>CoolTool: crosspromo=" + (Crosspromo ? 1 : 0) + ", youtube=" + (Youtube ? 1 : 0) + ", cheats=" + (cheats ? 1 : 0) + ", ads=" + (Ads ? 1 : 0) + ", moregames=" + (Moregames ? 1 : 0) + ", testdevice=" + (isTestDevice ? 1 : 0) + "</color>");
#endif
        if (OnResetCheats != null)
        {
            OnResetCheats.Invoke(cheats);
        }
        if (OnResetYoutube != null)
        {
            OnResetYoutube.Invoke(Youtube);
        }
        if (OnResetCrosspromo != null)
        {
            OnResetCrosspromo.Invoke(Crosspromo);
        }
        if (OnResetAds != null)
        {
            OnResetAds.Invoke(Ads);
        }
        if (OnResetMoregames != null)
        {
            OnResetMoregames.Invoke(Moregames);
        }
    }

    IEnumerator ContiniousDialing()
    {
        string serverUrl = GetAddress(url) + "/";

        float interval = 5f;

        WWW ping = null;

        while (true)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                yield return new WaitForSecondsRealtime(interval);
            }
            else
            {
                ping = new WWW(serverUrl);
                yield return ping;
                if (!string.IsNullOrEmpty(ping.error))
                {
                    ping.Dispose();
                    yield return new WaitForSecondsRealtime(interval);
                }
                else
                {
                    break;
                }
            }
        }
        yield return StartCoroutine(ServerRequest());
    }

    private string GetBundleIdentifier()
    {
#if UNITY_5_6_OR_NEWER
        return Application.identifier;
#else
        return Application.bundleIdentifier;
#endif
    }

    private string GetServerUrl()
    {
        string _url = GetAddress(CompanyInfo.Struct.cooltool);
        string protocol = "v3";
        string platform = "GP";
#if UNITY_ANDROID
        platform = "GP";
#elif UNITY_IOS
        platform = "AS";
#endif
        // Generate request Json
        string json = "{\"bundle\": \"" + GetBundleIdentifier() 
            + "\", \"platform\": \"" + platform 
            + "\", \"device_id\": \"" + GetDeviceUniqueIdentifier() 
            + "\"}";
        string cryptedJson = Crypt.AESEncrypt(json, cryptKey);
        if (string.IsNullOrEmpty(cryptedJson))
        {
            cryptedJson = "{\"error\":\"AESEncrypt fail\"}";
        }
        //Debug.Log(string.Format("{0}/{1}/{2}", _url, protocol, WWW.EscapeURL(cryptedJson)));
        return string.Format("{0}/{1}/{2}", _url, protocol, WWW.EscapeURL(cryptedJson));
    }

    public static string GetDeviceUniqueIdentifier()
    {
#if UNITY_IOS && !UNITY_EDITOR
        string id = UnityEngine.iOS.Device.advertisingIdentifier;
#else
        string id = SystemInfo.deviceUniqueIdentifier;
#endif
        return id;
    }

    void InitialEvent(bool state)
    {
        if (OnResetYoutube != null)
        {
            OnResetYoutube.Invoke(state);
        }
        if (OnResetCrosspromo != null)
        {
            OnResetCrosspromo.Invoke(state);
        }
        if (OnResetMoregames != null)
        {
            OnResetMoregames.Invoke(state);
        }
    }

    bool isCorrectBundle(string bundle)
    {
        if (!string.IsNullOrEmpty(bundle))
        {
            Regex regex = new Regex(@"[a-z]\w+\.[a-z]\w+\.[a-z]\w+");
            Match match = regex.Match(bundle);
            return match.Success && match.Value == bundle;
        }
        return false;
    }

    private string GetAddress(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return "";
        }
        return new Uri(url).GetLeftPart(UriPartial.Authority);
    }

    private string GetDomainName(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return "";
        }
        return new Uri(url).Host;
    }

    /*
    public static int GetStableHashCode(this string str)
    {
        unchecked
        {
            int hash1 = 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1 || str[i + 1] == '\0')
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }
    */
}
