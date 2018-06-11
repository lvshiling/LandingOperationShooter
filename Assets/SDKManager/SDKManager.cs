using GBNAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDKManagement
{
    public class SDKManager : MonoBehaviour, ISDKReporter
    {
        /// <summary>
        /// Список активных SDK
        /// </summary>
        Dictionary<string, ISDKAdapter> activeSdk;
        /// <summary>
        /// Список неактивных SDK
        /// </summary>
        Dictionary<string, string> inactiveSdk;
        /// <summary>
        /// Список не поддерживаемых SDK
        /// </summary>
        Dictionary<string, string> unsupportedSdk;
        /// <summary>
        /// Список SDK для которых не найдены адаптеры
        /// </summary>
        Dictionary<string, string> failedSdk;
        /// Очередь инициализации
        /// </summary>
        List<string> queueSdk;
        /// <summary>
        /// Словарь адаптеров: "имя - класс" 
        /// </summary>
        Dictionary<string, SDKAdapterConfig> supportedSdkAdapters;
        string sdkAdaptersFilename = "SDKAdapters";

        [System.Serializable]
        private struct SDKAdapterConfig
        {
            public string name;
            public string type;
            public string[] countries;
            public string[] androidPermissions;

            public override string ToString()
            {
                return string.Format("{0} type:{1}, countries:{2} androidPermissions:{3}", name, type, countries == null ? 0 : countries.Length, androidPermissions == null ? 0 : androidPermissions.Length);
            }
        }
        // если нужно добавить новый сдк для тестов, которого нет в CoolTool
        //string testJson = "[\"twinedata\", \"alphabet\"]";
        string testJson = "[]";

        private bool isInited = false;
        public bool IsInited
        {
            get { return isInited; }
            private set { isInited = value; }
        }

        public static SDKManager Instance { get; private set; }

        private bool isBusy = false;
        private string processingSdkName = "";
        private ISDKAdapter processingSdk;
#if UNITY_EDITOR
        private const float sdkPingTime = 1.0f;
#else
        private const float sdkPingTime = 3.0f;
#endif
        private const int sdkPingMax = 10;
        private int sdkPingCounter = 0;

        private PermissionManager pm;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else {
                Destroy(gameObject);
                return;
            }

            // init after GDPR Window
            Init();
            InitFromJson(testJson);
            StartCoroutine(CheckCoolTool());
        }

        private IEnumerator CheckCoolTool()
        {
            // Wait for CoolTool response
            while (string.IsNullOrEmpty(CoolTool.sdkJson)) {
                yield return new WaitForSecondsRealtime(sdkPingTime);
            }
            InitFromJson(CoolTool.sdkJson);
           
            StartCoroutine(InitSDK());
        }

        private void Init()
        {
            if (!IsInited)
            {
                supportedSdkAdapters = new Dictionary<string, SDKAdapterConfig>();
                pm = new PermissionManager();
                LoadAdaptersJson();
                pm.RequestStatus();
                inactiveSdk = new Dictionary<string, string>();
                foreach (var sdk in supportedSdkAdapters.Keys)
                {
                    inactiveSdk.Add(sdk, "SDK is inactive on server");
                }
                activeSdk = new Dictionary<string, ISDKAdapter>();
                queueSdk = new List<string>();
                unsupportedSdk = new Dictionary<string, string>();
                failedSdk = new Dictionary<string, string>();
                IsInited = true;
            }
        }

        /// <summary>
        /// Init SDKs from CoolTool SDK List
        /// </summary>
        /// <param name="sdkJson">List of SDK to initialization</param>
        private void InitFromJson(string sdkJson)
        {
            if (IsInited)
            {
                var newSdkList = Parse(sdkJson);
                foreach (var sdk in newSdkList)
                {
                    if (supportedSdkAdapters.ContainsKey(sdk))
                    {
                        // supported sdk
                        if (inactiveSdk.ContainsKey(sdk))
                        {
                            // remove not ininted sdk from notInitedSdk list
                            inactiveSdk.Remove(sdk);
                            // add sdk to queue if not yet
                            if (!queueSdk.Contains(sdk) && !activeSdk.ContainsKey(sdk))
                            {
                                //Debug.Log("[SDK] " + sdk + " added to initialization queue");
                                queueSdk.Add(sdk);
                            }
                        }
                    }
                    else if (!unsupportedSdk.ContainsKey(sdk))
                    {
                        // add unsupported sdk to list
                        unsupportedSdk.Add(sdk, "SDK is not supported");
                        //Debug.Log("[SDK] " + sdk + " is unsupported");
                    }
                }
            }
            
        } 

        private void LoadAdaptersJson() {
            //Debug.Log("LoadAdaptersConfig");
            TextAsset config = Resources.Load<TextAsset>(sdkAdaptersFilename);
            if (config != null)
            {
                JSONObject rootJson = new JSONObject(config.text);
                if (rootJson != null && rootJson.HasField("adapters"))
                {
                    JSONObject adapters = rootJson.GetField("adapters");

                    if (adapters.IsArray)
                    {
                        foreach (JSONObject adapter in adapters.list)
                        {
                            SDKAdapterConfig adapterConfig = JsonUtility.FromJson<SDKAdapterConfig>(adapter.ToString());
                            if (!supportedSdkAdapters.ContainsKey(adapterConfig.name))
                            {
                                supportedSdkAdapters.Add(adapterConfig.name, adapterConfig);
                                //Debug.Log("[ADAPTER] " + adapterConfig.ToString());
                                pm.Add(adapterConfig.androidPermissions);
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("Error: Resources / SDKAdapters.txt field 'adapters' is not array");
                    }
                    
                }
                else
                {
                    Debug.Log("Error: Resources / SDKAdapters.txt has not field 'adapters'");
                }
            } 
            else
            {
                Debug.Log("Error: Resources / SDKAdapters.txt not found");
            }
        }

        private List<string> Parse(string sdkJson)
        {
            List<string> sdkList = new List<string>();
            //Debug.Log(sdkJson);

            JSONObject json = new JSONObject(sdkJson);

            if (json.IsArray)
            {
                foreach (var obj in json.list)
                {
                    sdkList.Add(obj.str);
                }
            }
            return sdkList;
        }

        private IEnumerator InitSDK()
        {
            if (!isBusy)
            {
                isBusy = true;

                while (queueSdk.Count > 0) {
                    processingSdkName = queueSdk[0];
                    queueSdk.RemoveAt(0);

                    var permissions = supportedSdkAdapters[processingSdkName].androidPermissions;
                    if (pm.isChecked(permissions)) {
                        // пермишены проверены
                        if (pm.isGranted(permissions)) {
                            // все пермишены даны, проверяем страну
                            if (pm.isCountryAllowed(supportedSdkAdapters[processingSdkName].countries))
                            {
                                processingSdk = InstantiateSDKAdapter(processingSdkName);

                                if (processingSdk != null)
                                {
                                    // GameObject создан
                                    processingSdk.Init(processingSdkName);
                                    sdkPingCounter = 0;
                                    while (sdkPingCounter < sdkPingMax && processingSdk.info.status == SDKStatus.CANCELED)
                                    {
                                        sdkPingCounter++;
                                        //Debug.Log("Ping: " + processingSdkName + " sdkPingCounter: " + sdkPingCounter);
                                        yield return new WaitForSecondsRealtime(sdkPingTime);
                                    }
                                    //Debug.Log(processingSdk.info);

                                    yield return new WaitForSecondsRealtime(sdkPingTime);
                                }
                                else
                                {
                                    // ошибка создания GameObject
                                    // TODO not found sdk
                                    //Debug.Log("[SDK] " + processingSdkName + " status: SDK adapter not found");
                                    if (!failedSdk.ContainsKey(processingSdkName))
                                    {
                                        failedSdk.Add(processingSdkName, "Class '" + supportedSdkAdapters[processingSdkName] + "' not found");
                                    }
                                    yield return new WaitForSecondsRealtime(sdkPingTime);
                                }
                            }
                            else
                            {
                                // страна не подходит, не инициализируем
                                if (!unsupportedSdk.ContainsKey(processingSdkName))
                                {
                                    unsupportedSdk.Add(processingSdkName, "SDK Country not matched!");
                                }
                                yield return new WaitForSecondsRealtime(sdkPingTime);
                            }
                            
                          
                        }
                        else
                        {
                            // пермишены не даны, не инициализируем
                            if (!unsupportedSdk.ContainsKey(processingSdkName))
                            {
                                unsupportedSdk.Add(processingSdkName, "Permissions DENIED by User");
                            }
                            yield return new WaitForSecondsRealtime(sdkPingTime);
                        }
                    }
                    else
                    {
                        // ожидаем пермишенов, ставим обратно в очередь
                        queueSdk.Add(processingSdkName);
                        yield return new WaitForSecondsRealtime(sdkPingTime);
                    }
                }
                isBusy = false;
            }
        }
        
        private ISDKAdapter InstantiateSDKAdapter(string sdk)
        {
            ISDKAdapter sdkAdapter = null;
            Type type = null;
            if (supportedSdkAdapters.ContainsKey(sdk)) {
                type = Type.GetType(supportedSdkAdapters[sdk].type);
            }
            if (type != null)
            {
                GameObject go = new GameObject(sdk);
                go.transform.SetParent(transform);
                sdkAdapter = go.AddComponent(type) as ISDKAdapter;
            }
            if (sdkAdapter != null)
            {
                activeSdk.Add(sdk, sdkAdapter);
            }
            return sdkAdapter;
        }

        #region ISDKReproter
        public Action<SDKReport> OnReportComplete
        {
            get;
            set;
        }
        public void GenerateReport()
        {
            SDKReport report = new SDKReport(); 
            if (IsInited)
            {
                // active
                foreach (var sdk in activeSdk.Keys)
                {
                    report.Add(activeSdk[sdk].info);
                }
                // failed == adapter class not found
                foreach (var sdk in failedSdk)
                {
                    report.Add(new SDKInfo(sdk.Key, SDKStatus.FAILED, sdk.Value));
                }
                // unsupported == not implemented
                foreach (var sdk in unsupportedSdk)
                {
                    report.Add(new SDKInfo(sdk.Key, SDKStatus.CANCELED, sdk.Value));
                }
                // inactive == turned off on server
                foreach (var sdk in inactiveSdk)
                {
                    report.Add(new SDKInfo(sdk.Key, SDKStatus.CANCELED, sdk.Value));
                }
                // waiting == in queue
                foreach (var sdk in queueSdk)
                {
                    report.Add(new SDKInfo(sdk, SDKStatus.WAITING, "Wait for Init SDK"));
                }
            }
            if (OnReportComplete != null)
            {
                OnReportComplete.Invoke(report);
            }
        }
        public bool hasButton
        {
            get
            {
                return false;
            }
        }
        public void ButtonClick()
        {

        }
        public string buttonLabel
        {
            get
            {
                return "";
            }
        }
        #endregion
    }

}
