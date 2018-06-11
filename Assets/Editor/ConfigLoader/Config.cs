using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.ConfigLoader
{
    public class Config
    {
        public static string version = "4.0.3";

        public static AndroidSdkVersions androidSdkVersion = AndroidSdkVersions.AndroidApiLevel18;
        public static string TargetIOSVersionString = "8.1";


        public const string LOCALE_VERSION = "Locale Version";
        public const string MOBILE_INPUT = "Mobile Input";

        private readonly string fieldGpBundle = "customfield_11100";
        private readonly string fieldAsBundleFree = "customfield_11101";
        private readonly string fieldAmazonBundle = "customfield_13101";
        private readonly string fieldGpAccount = "customfield_11000";
        private readonly string fieldAsAccountFree = "customfield_13901";

        private readonly string fieldGpVersionCode = "customfield_13009";
        private readonly string fieldAsVersion = "customfield_13010";

        private readonly string fieldGpTitle = "customfield_12000";
        private readonly string fieldAsTitleFree = "customfield_12001";
        private readonly string fieldAmTitle = "customfield_16400";
        private readonly string fieldEpicName = "customfield_10182";
        private readonly string fieldGpFlurry = "customfield_12002";
        private readonly string fieldAsFlurry = "customfield_12003";
        private readonly string fieldGpOneAudience = "customfield_12301";
        private readonly string fieldAsOneAudience = "customfield_12618";
        private readonly string fieldBalanceUrl = "customfield_12701";

        private readonly string fieldGitLabUrl = "customfield_13201";
        private readonly string fieldGameStorageUrl = "customfield_12908";

        private readonly string fieldAppIDFree = "customfield_13007";
        // ==================
        private readonly string fieldGpFyberAppId = "customfield_15213";
        private readonly string fieldGpFyberSecToken = "customfield_15214";
        private readonly string fieldAsFyberAppId = "customfield_15211";
        private readonly string fieldAsFyberSecToken = "customfield_15212";

        private readonly string fieldGpTamocoApiId = "customfield_15217";
        private readonly string fieldGpTamocoApiSecret = "customfield_15218";
        private readonly string fieldAsTamocoApiId = "customfield_15215";
        private readonly string fieldAsTamocoApiSecret = "customfield_15216";

        private readonly string fieldGpAppodealKey = "customfield_16202";
        private readonly string fieldAsAppodealKey = "customfield_16203";

        private readonly string fieldGpUnityAdsKey = "customfield_17300";
        private readonly string fieldAsUnityAdsKey = "customfield_17301";

        private readonly string fieldGpMobKnow = "customfield_17200";
        // ==================
        private readonly string[] KEY_SEPARATOR = new string[] { ";" };
        private readonly string[] LINE_SEPARATOR = new string[] { "\n" };

        public static string epicName = "Epic Name";
        public static string epicId = "Epic ID";

        public static string gpTitle = "GP Title";
        public static string gpBundle = "GP Bundle";
        public static string gpAccount = "GP Account";
        public static string gpVersionCode = "GP Version / Code";
        public static string gpFlurry = "GP Flurry";
        public static string gpOneAudience = "GP OneAudience";
        public static string gpFyberAppId = "GP Fyber App Id";
        public static string gpFyberSecToken = "GP Fyber Sec Token";
        public static string gpTamocoApiId = "GP Tamoco Api Id";
        public static string gpTamocoApiSecret = "GP Tamoco Api Secret";
        public static string gpAppodealKey = "GP Appodeal Key";

        public static string gpUnityAdsKey = "GP UnityAds Key";

        public static string gpMobKnow = "GP MobKnow";

        public static string amTitle = "Amazon Title";
        public static string amBundle = "Amazon Bundle";
        public static string amAccount = "Amazon Account";

        public static string asTitleFree = "AS Title Free";
        public static string asBundleFree = "AS Bundle Free";

        public static string asVersion = "AS Version";

        public static string asAppIDFree = "App ID Free";

        public static string asFyberAppId = "AS Fyber App Id";
        public static string asFyberSecToken = "AS Fyber Sec Token";
        public static string asTamocoApiId = "AS Tamoco Api Id";
        public static string asTamocoApiSecret = "AS Tamoco Api Secret";
        public static string asAppodealKey = "AS Appodeal Key";

        public static string asUnityAdsKey = "AS UnityAds Key";

        public static string asAccountFree = "AS Account Free";

        public static string asFlurry = "AS Flurry";
        public static string asOneAudience = "AS OneAudience";

        public static string balanceUrlKey = "Balance URL Key";

        public static string buildScenes = "Build Scenes";
        public static string additiveScenes = "Additive Scenes";

        public static string gitLabUrl = "GitLab Url";
        public static string gameStorageUrl = "GameStorage Url";

        private static string[] _iconASFree = new string[] { "Assets/Icons/icon_ios_free_1024.png", "Assets/Icons/icon_ios_free.png" };
        private static string[] _iconGP = new string[] { "Assets/Icons/icon_gp_am_512.png" };
        private static string[] _iconAM = new string[] { "Assets/Icons/icon_am_512.png", "Assets/Icons/icon_gp_am_512.png" };

        private static Dictionary<string, Dictionary<string, string>> SDKsConfig;

        public static string iconASFree
        {
            get
            {
                foreach (var icon in _iconASFree)
                {
                    if (File.Exists(icon)) return icon;
                }
                return _iconASFree[0];
            }
        }
        public static string iconGP
        {
            get
            {
                foreach (var icon in _iconGP)
                {
                    if (File.Exists(icon)) return icon;
                }
                return _iconGP[0];
            }
        }
        public static string iconAM
        {
            get
            {
                foreach (var icon in _iconAM)
                {
                    if (File.Exists(icon)) return icon;
                }
                return _iconAM[0];
            }
        }

        public static string keystoresFolder = "Keys";
        public static string keystoresList = "KeystoresJenkins.txt";

        public static string accountsInfoFile = "Assets/Editor/ConfigLoader/accounts_info.txt";

        private static JSONObject accountsInfoSdkPart = null;

        private bool needReplaceAllValues = false;

        private bool isLoaded = false;

        public bool IsLoaded
        {
            get { return isLoaded; }
        }

        public Dictionary<string, string> data = new Dictionary<string, string>();

        public void Parse(string jsonString, string jiraEpicId)
        {
            SDKsConfig = null;
            accountsInfoSdkPart = null;

            Debug.Log(typeof(Config).Assembly.FullName);

            if (!LoadCoolToolConfig())
            {
                Debug.LogException(new Exception("No CoolTool config found! Check " + accountsInfoFile + " file!"));
            }

            if (!LoadAppLovinConfig())
            {
                Debug.LogException(new Exception("No AppLovin config found! Check " + accountsInfoFile + " file!"));
            }

            if (!LoadHuqConfig())
            {
                Debug.LogException(new Exception("No Huq config found! Check " + accountsInfoFile + " file!"));
            }

            if (!LoadTutelaConfig())
            {
                Debug.LogException(new Exception("No Tutela config found! Check " + accountsInfoFile + " file!"));
            }

            if (!LoadCuebiqConfig())
            {
                Debug.LogException(new Exception("No Cuebiq config found! Check " + accountsInfoFile + " file!"));
            }
            /*
            if (!LoadGDPRConfig())
            {
                Debug.LogError(new Exception("No GDPR config found! Check " + accountsInfoFile + " file!"));
            }
            */
            // get fields of epic issue
            var json = new JSONObject(jsonString);
            json = json.GetField("fields");
            // check type
            var issuetype = json.GetField("issuetype");
            //var type = issuetype.GetField("name").ToString();
            var type = "";
            issuetype.GetField(ref type, "name");
            if (type != "Epic")
            {
                Debug.LogError("Wrong Jira issue " + jiraEpicId + " Issue Type: " + type);
                return;
            }
            // set version
            needReplaceAllValues = true;
            SetParam("version", version);
            SetParam("Ads Version", GBNHZinit.version);
            // parse Epic
            needReplaceAllValues = (GetParam(epicId) != "" && GetParam(epicId) != jiraEpicId);
            SetParam(epicId, jiraEpicId);
            ParseParam(json, fieldEpicName, epicName);
            // parse Google Play params
            ParseParam(json, fieldGpTitle, gpTitle);
            ParseParam(json, fieldGpBundle, gpBundle);
            ParseParamValue(json, fieldGpAccount, gpAccount);
            ParseParam(json, fieldGpFlurry, gpFlurry);
            ParseParam(json, fieldGpUnityAdsKey, gpUnityAdsKey);
            // parse Amazon params
            ParseParam(json, fieldGpTitle, amTitle);
            ParseParam(json, fieldAmTitle, amTitle);
            ParseParam(json, fieldAmazonBundle, amBundle);
            SetParam(amAccount, "Amazing Games");
            // parse AppStore params
            ParseParam(json, fieldAsTitleFree, asTitleFree);
            ParseParam(json, fieldAsBundleFree, asBundleFree);
            ParseParam(json, fieldAppIDFree, asAppIDFree);
            ParseParam(json, fieldAsFlurry, asFlurry);
            ParseParamValue(json, fieldAsAccountFree, asAccountFree);
            ParseParam(json, fieldAsUnityAdsKey, asUnityAdsKey);
            // parse versions
            ParseParam(json, fieldGpVersionCode, gpVersionCode);
            ParseParam(json, fieldAsVersion, asVersion);
            // parse balance URL
            ParseBalanceUrlKey(json, fieldBalanceUrl, balanceUrlKey);
            // parse GitLab URL
            ParseParam(json, fieldGitLabUrl, gitLabUrl);
            // parse GameStorage URL
            ParseParam(json, fieldGameStorageUrl, gameStorageUrl);
            // Set Additive Scenes
            SetParam(additiveScenes, GetParam(additiveScenes));
            if (GetParam(LOCALE_VERSION) == "")
            {
                SetParam(LOCALE_VERSION, "false");
            }
            else
            {
                SetParam(LOCALE_VERSION, GetParam(LOCALE_VERSION));
            }
            if (GetParam(MOBILE_INPUT) == "")
            {
                SetParam(MOBILE_INPUT, "false");
            }
            else
            {
                SetParam(MOBILE_INPUT, GetParam(MOBILE_INPUT));
            }
            //SaveEpicJson(jsonString);
            isLoaded = true;
        }

        private static JSONObject GetSdkConfig()
        {
            if (accountsInfoSdkPart == null)
            {
                if (File.Exists(Config.accountsInfoFile))
                {
                    string text = File.ReadAllText(Config.accountsInfoFile);
                    JSONObject jObj = new JSONObject(text);
                    if (jObj != null && jObj.HasField("sdk"))
                    {
                        accountsInfoSdkPart = jObj.GetField("sdk");
                    }
                    else
                    {
                        Debug.LogError("File " + Config.accountsInfoFile + " has no SDKs information!");
                    }
                }
                else
                {
                    Debug.LogError("File " + Config.accountsInfoFile + " not found!");
                }
            }

            return accountsInfoSdkPart;
        }

        private bool LoadSkdConfig(string sdkName)
        {
            if (SDKsConfig == null)
            {
                SDKsConfig = new Dictionary<string, Dictionary<string, string>>();
            }

            Dictionary<string, string> sdkConfig = new Dictionary<string, string>();

            bool done = false;

            JSONObject jObj = GetSdkConfig();

            if (jObj != null)
            {
                string[] fields = { "name", "key" };
                bool isValid = false;
                if (jObj.HasField(sdkName))
                {
                    jObj = jObj.GetField(sdkName);
                    if (jObj.IsArray)
                    {
                        isValid = true;
                        for (int i = 0; i < jObj.Count; i++)
                        {
                            if (jObj[i].HasFields(fields))
                            {
                                string name = jObj[i].GetField(fields[0]).str;
                                string key = jObj[i].GetField(fields[1]).str;
                                if (key != null)
                                {
                                    key = TrimAndReplace(key);
                                }
                                else
                                {
                                    key = jObj[i].GetField(fields[1]).ToString();
                                }
                                sdkConfig.Add(name, key);
                            }
                        }
                        SDKsConfig.Add(sdkName, sdkConfig);
                        done = true;
                    }
                }
                if (!isValid)
                {
                    Debug.LogError("File " + Config.accountsInfoFile + " contains wrong " + sdkName + "-SDK information!");
                }
            }

            return done;
        }

        private bool LoadCoolToolConfig()
        {
            return LoadSkdConfig("cooltool");
        }

        private bool LoadAppLovinConfig()
        {
            return LoadSkdConfig("applovin");
        }

        private bool LoadHuqConfig()
        {
            return LoadSkdConfig("huq");
        }

        private bool LoadTutelaConfig()
        {
            return LoadSkdConfig("tutela");
        }

        private bool LoadCuebiqConfig()
        {
            return LoadSkdConfig("cuebiq");
        }
        /*
        private bool LoadGDPRConfig()
        {
            return LoadSkdConfig("gdpr");
        }
        */
        private string GetSdkKey(string sdkName)
        {
            if (SDKsConfig != null && SDKsConfig.ContainsKey(sdkName))
            {
                if (SDKsConfig[sdkName].ContainsKey(GetParam(gpAccount)))
                {
                    return SDKsConfig[sdkName][GetParam(gpAccount)];
                }
                else if (SDKsConfig[sdkName].ContainsKey("Default"))
                {
                    return SDKsConfig[sdkName]["Default"];
                }
            }
            return "";
        }

        public string GetCoolToolKey()
        {
            return GetSdkKey("cooltool");
        }

        public string GetAppLovinKey()
        {
            return GetSdkKey("applovin");
        }

        public string GetHuqKey()
        {
            return GetSdkKey("huq");
        }

        public string GetTutelaKey()
        {
            return GetSdkKey("tutela");
        }

        public string GetCuebiqKey()
        {
            return GetSdkKey("cuebiq");
        }
        /*
        public string GetGDPRKey()
        {
            return GetSdkKey("gdpr");
        }
        */

        private void SaveEpicJson(string json)
        {
            File.WriteAllText(Application.dataPath + "/" + GetParam(epicId) + ".json", json);
            AssetDatabase.Refresh();
            Debug.Log("Epic " + GetParam(epicId) + ".json saved!");
        }

        public void SetParam(string dataKey, string dataValue, bool forceReplace = false)
        {
            if (data.ContainsKey(dataKey))
            {
                if (forceReplace || needReplaceAllValues || !string.IsNullOrEmpty(dataValue))
                {
                    data[dataKey] = dataValue;
                }
                else if (data[dataKey] != "")
                {
                    Debug.Log("Used previous value of \"" + dataKey + "\"=" + data[dataKey]);
                }
            }
            else
            {
                data.Add(dataKey, dataValue);
            }
        }

        private void ParseParam(JSONObject json, string jsonField, string dataKey)
        {
            var value = "";
            json.GetField(ref value, jsonField);
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogWarning(dataKey + " is Empty!");
            }
            else
            {
                var trimmed = TrimAndReplace(value);
                if (trimmed.Length != value.Length)
                {
                    Debug.LogWarning("Trimmed extra chars '" + value + "'");
                }
                value = trimmed;
            }
            SetParam(dataKey, value);
        }

        private string TrimAndReplace(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
            return value.Trim(new char[] { ' ' }).Replace("\t", "").Replace("\\t", "").Replace("\r", "").Replace("\\r", "").Replace("\n", "").Replace("\\n", "");

        }

        private void ParseBalanceUrlKey(JSONObject json, string jsonField, string dataKey)
        {
            var value = "";
            json.GetField(ref value, jsonField);
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogWarning(dataKey + " is Empty!");
            }
            else
            {
                // parse key from url
                var keyFound = false;
                if (value.IndexOf("docs.google.com") >= 0)
                {
                    var terms = value.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    var max = 30;
                    foreach (var term in terms)
                    {
                        if (term.Length > max)
                        {
                            max = term.Length;
                            value = term;
                            keyFound = true;
                        }
                    }
                }
                if (!keyFound)
                {
                    Debug.LogWarning(dataKey + " Wrong format: " + value);
                    value = "";
                }
            }
            SetParam(dataKey, value);
        }

        private void ParseParamValue(JSONObject json, string jsonField, string dataKey)
        {
            var value = "";
            if (json.HasField(jsonField))
            {
                var jsonTmp = json.GetField(jsonField);
                jsonTmp.GetField(ref value, "value");
                if (string.IsNullOrEmpty(value))
                {
                    Debug.LogWarning(dataKey + " is Empty!");
                }
            }
            else
            {
                Debug.LogWarning(dataKey + " is Empty!");
            }
            SetParam(dataKey, value);
        }

        public void Save(string filename)
        {
            if (isLoaded)
            {
                var content = "";
                foreach (var param in data)
                {
                    content += string.Format("{0}{1}{2}{3}", param.Key, KEY_SEPARATOR[0], param.Value, LINE_SEPARATOR[0]);
                }
                File.WriteAllText(Application.dataPath + "/" + filename, content);
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogWarning("Config.txt does not saved!");
            }

        }

        public void ChangeEpicID(string filename, string jiraIssueId)
        {
            if (!File.Exists(Application.dataPath + "/" + filename))
            {
                File.WriteAllText(Application.dataPath + "/" + filename, "");
                isLoaded = true;
                SetParam("version", version, true);
                SetParam("Ads Version", GBNHZinit.version, true);

                SetParam(epicId, jiraIssueId, true);

                ClearEpicIDSensetiveFields(filename);

                SetParam(additiveScenes, "", true);
                SetParam(LOCALE_VERSION, "false", true);
                SetParam(MOBILE_INPUT, "false", true);
                Save(filename);
            }
            else
            {
                if (!isLoaded)
                    Load(filename);
                if (!GetParam(epicId).Equals(jiraIssueId))
                {
                    SetParam(epicId, jiraIssueId, true);
                    ClearEpicIDSensetiveFields(filename);
                    Save(filename);
                }
            }
        }

        private void ClearEpicIDSensetiveFields(string filename)
        {
            SetParam(epicName, "", true);

            SetParam(gpTitle, "", true);
            SetParam(gpBundle, "", true);
            SetParam(gpAccount, "", true);
            SetParam(gpFlurry, "", true);
            SetParam(gpOneAudience, "", true);
            SetParam(gpAppodealKey, "", true);
            SetParam(gpMobKnow, "", true);

            SetParam(amTitle, "", true);
            SetParam(amBundle, "", true);
            SetParam(amAccount, "", true);

            SetParam(asTitleFree, "", true);
            SetParam(asBundleFree, "", true);
            SetParam(asAppIDFree, "", true);
            SetParam(asFlurry, "", true);
            SetParam(asAccountFree, "", true);
            SetParam(asOneAudience, "", true);
            SetParam(asAppodealKey, "", true);

            SetParam(gpVersionCode, "", true);
            SetParam(asVersion, "", true);

            SetParam(balanceUrlKey, "", true);

            SetParam(gitLabUrl, "", true);

            SetParam(gameStorageUrl, "", true);
        }

        public bool Load(string filename)
        {
            SDKsConfig = null;
            accountsInfoSdkPart = null;

            if (!LoadCoolToolConfig())
            {
                Debug.LogException(new Exception("No CoolTool config found! Check " + accountsInfoFile + " file!"));
            }
            if (!LoadAppLovinConfig())
            {
                Debug.LogException(new Exception("No AppLovin config found! Check " + accountsInfoFile + " file!"));
            }
            if (!LoadHuqConfig())
            {
                Debug.LogException(new Exception("No Huq config found! Check " + accountsInfoFile + " file!"));
            }
            if (!LoadTutelaConfig())
            {
                Debug.LogException(new Exception("No Tutela config found! Check " + accountsInfoFile + " file!"));
            }
            if (!LoadCuebiqConfig())
            {
                Debug.LogException(new Exception("No Cuebiq config found! Check " + accountsInfoFile + " file!"));
            }
            /*
            if (!LoadGDPRConfig())
            {
                Debug.LogError(new Exception("No GDPR config found! Check " + accountsInfoFile + " file!"));
            }
            */
            if (File.Exists(Application.dataPath + "/" + filename))
            {
                var content = File.ReadAllText(Application.dataPath + "/" + filename);
                var lines = content.Split(LINE_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var pair = line.Split(KEY_SEPARATOR, StringSplitOptions.None);
                    var value = TrimAndReplace(pair[1]);
                    SetParam(pair[0], value);
                }
                isLoaded = true;
                return true;
            }
            return false;
        }
        
        public string GetConfigTxtVersion()
        {
            if (File.Exists(Application.dataPath + "/Config.txt"))
            {
                var content = File.ReadAllText(Application.dataPath + "/Config.txt");
                var lines = content.Split(LINE_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var pair = line.Split(KEY_SEPARATOR, StringSplitOptions.None);
                    var value = TrimAndReplace(pair[1]);
                    if (pair[0] == "version") return value;
                }
            }
            Debug.Log("GetConfigTxtVersion ERROR EMPTY");
            return "";
        }

        public string GetVersion()
        {
            if (data.ContainsKey("version"))
            {
                return data["version"];
            }
            return version;
        }

        public string GetGPVersion()
        {
            if (CheckGPVersionCode())
            {
                // match Version \d+\.\d+\.\d+
                // first match
                Regex regex = new Regex(@"\d+\.\d+\.\d+");
                Match match = regex.Match(data[gpVersionCode]);
                if (match.Success)
                {
                    return match.Value;
                }
                else
                {
                    Debug.LogWarning("Jira Field GP Version / Code wrong format or empty! Current Value=\"" + data[gpVersionCode] + "\" Required format \"1.0.0 / 1\". Used 1.0.0");
                }
            }
            else
            {
                Debug.LogWarning("Jira Field GP Version / Code wrong format or empty! Required format \"1.0.0 / 1\". Used 1.0.0");
            }
            return "1.0.0";
        }

        public int GetGPCode()
        {
            if (CheckGPVersionCode())
            {
                // match Code \d+(\.\d+\.\d+)* 
                // second match
                Regex regex = new Regex(@"\d+(\.\d+\.\d+)*");
                MatchCollection matches = regex.Matches(data[gpVersionCode]);
                if (matches.Count >= 2)
                {
                    return int.Parse(matches[1].Value);
                }
                else
                {
                    Debug.Log("Jira Field GP Version / Code contains versionName only. Used 1");
                }
            }
            else
            {
                Debug.LogWarning("Jira Field GP Version / Code wrong format or empty! Required format \"1.0.0 / 1\". Used 1");
            }
            return 1;
        }

        public bool CheckGPVersionCode()
        {
            if (data.ContainsKey(gpVersionCode))
            {
                //  check GP^ \d+\.\d+\.\d+(\D*\/\D*\d+)*
                Regex regex = new Regex(@"\d+\.\d+\.\d+(\D*\/\D*\d+)*");
                return regex.IsMatch(data[gpVersionCode]);
            }
            return false;
        }


        public string GetASVersion(bool freeVersion)
        {
            if (CheckASVersion())
            {
                // match Version \d+\.\d+\.\d+
                // first or second match
                Regex regex = new Regex(@"\d+\.\d+\.\d+");
                MatchCollection matches = regex.Matches(data[asVersion]);
                if (matches.Count >= 2)
                {
                    return freeVersion ? matches[0].Value : matches[1].Value;
                }
                else if (matches.Count >= 1)
                {
                    return matches[0].Value;
                }
            }
            else
            {
                Debug.LogWarning("Jira Field AS Version wrong format or empty! Required format \"1.0.0 / 1.0.0\". Used 1.0.0");
            }

            return "1.0.0";
        }

        public bool CheckASVersion()
        {
            if (data.ContainsKey(gpVersionCode))
            {
                //  \d+\.\d+\.\d+(\D+\/\D+\d+\.\d+\.\d+)* check AS
                Regex regex = new Regex(@"\d+\.\d+\.\d+(\D+\/\D+\d+\.\d+\.\d+)*");
                return regex.IsMatch(data[asVersion]);
            }
            return false;
        }

        public string GetParam(string key)
        {
            if (data.ContainsKey(key))
            {
                return data[key];
            }
            return "";
        }
    }
}
