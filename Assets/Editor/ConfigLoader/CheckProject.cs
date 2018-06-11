using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Assets.Editor.ConfigLoader
{
    public class CheckProjectWindow : EditorWindow
    {
        private enum AppliedPlatform
        {
            UNDEFINED,
            Google,
            Apple,
            Amazon
        }

        private class SdkStatus
        {
            public enum Status
            {
                UNDEFINED,
                NA,
                OK,
                WARNING,
                ERROR
            }

            public readonly string name;
            public readonly string parameter;
            public Status gpStatus { get; private set; }
            public Status asStatus { get; private set; }
            public Status amStatus { get; private set; }

            public string comments { get; private set; }

            public bool hasProblems
            {
                get
                {
                    return gpStatus == Status.ERROR || asStatus == Status.ERROR || amStatus == Status.ERROR || !string.IsNullOrEmpty(comments);
                }
            }

            public SdkStatus(string sdkName, string sdkParameter)
            {
                name = sdkName;
                parameter = sdkParameter;
                gpStatus = Status.UNDEFINED;
                asStatus = Status.UNDEFINED;
                amStatus = Status.UNDEFINED;
                comments = "";
            }

            public void SetGpStatus(Status status)
            {
                gpStatus = status;
            }

            public void SetAsStatus(Status status)
            {
                asStatus = status;
            }

            public void SetAmStatus(Status status)
            {
                amStatus = status;
            }

            public SdkStatus(string sdkName, string sdkParameter, Status gpInfo, Status asInfo, Status amInfo)
            {
                name = sdkName;
                parameter = sdkParameter;
                gpStatus = gpInfo;
                asStatus = asInfo;
                amStatus = amInfo;
                comments = "";
            }

            public void AddComment(string comment)
            {
                if (comments.Length != 0)
                {
                    comments += "\n";
                }
                comments += comment;
            }
        }

        private static List<SdkStatus> paramsList = null;

        private static AppliedPlatform appliedPlatform
        {
            get
            {
                try
                {
                    return (AppliedPlatform)Enum.Parse(typeof(AppliedPlatform), GBNAPI.CompanyInfo.Struct.store, true);
                }
                catch
                {
                    return AppliedPlatform.UNDEFINED;
                }
            }
        }

        private static Dictionary<string, string> configStatuses = new Dictionary<string, string>();

        private static Config config = null;
        private static Texture2D icon_gp;
        private static Texture2D icon_amazon;
        private static Texture2D icon_ios_free;

        private static EditorBuildSettingsScene[] scenes;

        private static string configStatus = "UNKNOWN";
        private static string configTxtVersion = "UNKNOWN";

        private static string iconsFolder = "UNKNOWN";
        private static string iconIosFree = "UNKNOWN";
        private static string iconGp = "UNKNOWN";
        private static string iconAmazon = "UNKNOWN";

        private static string keystoresFolder = "UNKNOWN";
        private static string keystoresList = "UNKNOWN";
        private static string amazonKeystore = "UNKNOWN";
        private static string googlePlayKeystore = "UNKNOWN";

        private static string gbnhzPrefab = "UNKNOWN";
        private static string gbnhzGameObject = "UNKNOWN";
        private static string gbnhzBalanceKey = "UNKNOWN";

        private static string structureProjectStatus = "UNKNOWN";

        private static string androidAdsVersion = "UNKNOWN";
        private static string androidFinalVersion = "UNKNOWN";
        private static string androidLocaleVersion = "UNKNOWN";
        private static string iosAdsVersion = "UNKNOWN";
        private static string iosFinalVersion = "UNKNOWN";
        private static string iosLocaleVersion = "UNKNOWN";

        private static string accountsInfoFile = "UNKNOWN";

        private static string appstoreTeams = "UNKNOWN";

        private static List<string> requiredKeys = new List<string>(new string[] { "GP Version / Code", "AS Version" });
        private static List<string> bundles = new List<string>(new string[] { Config.gpBundle, Config.amBundle, Config.asBundleFree });
        private static List<string> titles = new List<string>(new string[] { Config.gpTitle, Config.amTitle, Config.asTitleFree });
        private static List<string> intId = new List<string>(new string[] { Config.asAppIDFree });

        private static string[] optionalKeys = new string[] { "Additive Scenes", "GitLab Url", "GameStorage Url", "ADS_VERSION", "FINAL_VERSION", "LOCALE_VERSION", "Balance URL Key" };
        private static string[] obsoleteKeys = new string[] 
        {
            Config.gpTamocoApiId,
            Config.gpTamocoApiSecret,
            Config.asTamocoApiId,
            Config.asTamocoApiSecret,
            Config.gpFyberAppId,
            Config.gpFyberSecToken,
            Config.asFyberAppId,
            Config.asFyberSecToken,
            Config.gpAppodealKey,
            Config.asAppodealKey,
            Config.gpMobKnow,
            Config.gpOneAudience,
            Config.asOneAudience,
        };

        [MenuItem("Config/Проверка проекта", false, 0)]
        public static void ShowWindow()
        {
            GetWindow<CheckProjectWindow>();
            CheckProject();
        }

        private static void LoadConfig()
        {
            ConfigLoader.LoadConfig();
            config = ConfigLoader.config;
            GBNAPI.SDKInfo.Parse();
            GBNAPI.CompanyInfo.Parse();
            paramsList = new List<SdkStatus>();
        }

        private static void CheckProject()
        {
            LoadConfig();

            CheckConfig();

            CheckScenes();
            CheckIcons();
            CheckBtnUrlCs();
            CheckDefines();

            CheckUnityAds();

            CheckGbnHzInit();

            CheckKeystores();

            CheckFlurry();

            CheckAppStoreTeams();

            CheckCoolTool();

            CheckBootLoader();
        }

        private static void CheckConfig()
        {
            if (config == null)
            {
                LoadConfig();
            }
            configStatus = (config.IsLoaded) ? "OK" : "Config.txt Not Loaded!";
            configTxtVersion = config.GetConfigTxtVersion();
            configStatuses.Clear();

            foreach (var param in config.data)
            {
                if (obsoleteKeys.Contains(param.Key))
                {
                    continue;
                }
                // check and show for empty params (not for additive scenes)
                if (string.IsNullOrEmpty(config.data[param.Key]) && param.Key != Config.additiveScenes)
                {
                    configStatuses.Add(param.Key, "Is Empty");
                }
                else if ((param.Key == Config.gpUnityAdsKey || param.Key == Config.asUnityAdsKey) && !ConfigLoader.IsValidUnityAdsKey(param.Value))
                {
                    configStatuses.Add(param.Key, "Wrong Format");
                }
                // check and show wrong format params
                else if (ContainsWrongCharacters(param.Value))
                {
                    configStatuses.Add(param.Key, "Wrong Format");
                }
                // show requred fields statuses anyway
                else if (requiredKeys.Contains(param.Key))
                {
                    configStatuses.Add(param.Key, param.Value);
                }
                // check bundles for correct format
                else if (bundles.Contains(param.Key) && !ConfigLoader.IsCorrectBundle(param.Value))
                {
                    configStatuses.Add(param.Key, "Wrong Format");
                }
                // check titles for correct format
                else if (titles.Contains(param.Key) && !ConfigLoader.IsCorrectTitle(param.Value))
                {
                    configStatuses.Add(param.Key, "Wrong Format");
                }
                // check int id for correct format
                else if (intId.Contains(param.Key) && !ConfigLoader.IsCorrectIntId(param.Value))
                {
                    configStatuses.Add(param.Key, "Wrong Format");
                }
            }
        }

        private static void CheckUnityAds()
        {
            CheckPackageStructure.Check();
            structureProjectStatus = CheckPackageStructure.hasErrors ? "ERROR (Look at Console)" : "OK";
            if (config.IsLoaded)
            {
                string sdkName = "UnityAds";

                bool gpFail = string.IsNullOrEmpty(config.GetParam(Config.gpUnityAdsKey));
                bool asFail = string.IsNullOrEmpty(config.GetParam(Config.asUnityAdsKey));

                SdkStatus status = new SdkStatus(sdkName, "app ID",
                    gpFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK,
                    asFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK,
                    SdkStatus.Status.NA
                );
                if (gpFail)
                {
                    status.AddComment("\"GP UnityAds Key\" in Config.txt is empty!");
                }
                if (appliedPlatform == AppliedPlatform.Google && (string.IsNullOrEmpty(ConfigSetter.GetUnityAdsKey()) || !ConfigSetter.GetUnityAdsKey().Equals(config.GetParam(Config.gpUnityAdsKey))))
                {
                    status.AddComment("GP setted UnityAds Key is WRONG or EMPTY!");
                }
                if (asFail)
                {
                    status.AddComment("\"AS UnityAds Key\" in Config.txt is empty!");
                }
                if (appliedPlatform == AppliedPlatform.Apple && (string.IsNullOrEmpty(ConfigSetter.GetUnityAdsKey()) || !ConfigSetter.GetUnityAdsKey().Equals(config.GetParam(Config.asUnityAdsKey))))
                {
                    status.AddComment("AS setted UnityAds Key is WRONG or EMPTY!");
                }
                paramsList.Add(status);
            }
        }

        private static void CheckAppLovinKey()
        {
            if (config.IsLoaded)
            {
                string sdkName = "Applovin";

                bool gpFail = string.IsNullOrEmpty(config.GetAppLovinKey());

                SdkStatus status = new SdkStatus(sdkName, "key",
                    gpFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK,
                    SdkStatus.Status.NA,
                    SdkStatus.Status.NA
                );
                if (gpFail)
                {
                    status.AddComment("GP Applovin key is EMPTY!");
                }
                if (appliedPlatform == AppliedPlatform.Google && !ConfigSetter.HasAppLovinKeyInManifest())
                {
                    status.AddComment("GP setted Applovin key is WRONG or EMPTY!");
                }
                paramsList.Add(status);
            }
        }

        private static void CheckHuq()
        {
            if (config.IsLoaded)
            {
                string sdkName = "Huq";

                bool gpFail;
                SdkStatus status;

                gpFail = string.IsNullOrEmpty(config.GetHuqKey());

                status = new SdkStatus(sdkName, "key",
                   gpFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK,
                   SdkStatus.Status.NA,
                   SdkStatus.Status.NA
                );
                if (gpFail)
                {
                    status.AddComment("GP Huq key is EMPTY!");
                }
                if (appliedPlatform == AppliedPlatform.Google && (string.IsNullOrEmpty(ConfigSetter.GetHuqKey()) || !ConfigSetter.GetHuqKey().Equals(config.GetHuqKey())))
                {
                    status.AddComment("GP setted Huq key is WRONG or EMPTY!");
                }
                paramsList.Add(status);
            }
        }

        private static void CheckTutela()
        {
            if (config.IsLoaded)
            {
                string sdkName = "Tutela";

                bool gpFail;
                SdkStatus status;

                gpFail = string.IsNullOrEmpty(config.GetTutelaKey());

                status = new SdkStatus(sdkName, "key",
                   gpFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK,
                   SdkStatus.Status.NA,
                   SdkStatus.Status.NA
                );
                if (gpFail)
                {
                    status.AddComment("GP Tutela key is EMPTY!");
                }
                if (appliedPlatform == AppliedPlatform.Google && (string.IsNullOrEmpty(ConfigSetter.GetTutelaKey()) || !ConfigSetter.GetTutelaKey().Equals(config.GetTutelaKey())))
                {
                    status.AddComment("GP setted Tutela key is WRONG or EMPTY!");
                }
                paramsList.Add(status);
            }
        }

        private static void CheckCuebiq()
        {
            if (config.IsLoaded)
            {
                string sdkName = "Cuebiq";

                bool gpFail;
                SdkStatus status;

                gpFail = string.IsNullOrEmpty(config.GetCuebiqKey());

                status = new SdkStatus(sdkName, "key",
                   gpFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK,
                   SdkStatus.Status.NA,
                   SdkStatus.Status.NA
                );
                if (gpFail)
                {
                    status.AddComment("GP Cuebiq key is EMPTY!");
                }
                if (appliedPlatform == AppliedPlatform.Google && (string.IsNullOrEmpty(ConfigSetter.GetCuebiqKey()) || !ConfigSetter.GetCuebiqKey().Equals(config.GetCuebiqKey())))
                {
                    status.AddComment("GP setted Cuebiq key is WRONG or EMPTY!");
                }
                paramsList.Add(status);
            }
        }

        private static void CheckHola()
        {
            if (config.IsLoaded)
            {
                string sdkName = "Hola";

                bool gpFail;
                SdkStatus status;

                gpFail = !(ConfigSetter.HasHolaUI() || appliedPlatform != AppliedPlatform.Google);

                status = new SdkStatus(sdkName, "UI",
                    gpFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK,
                    SdkStatus.Status.NA,
                    SdkStatus.Status.NA
                );
                if ((gpFail) && (appliedPlatform == AppliedPlatform.Google))
                {
                    status.AddComment("HolaSettingsBlock/HolaSettingsButton GameObject not found\nor HolaSettingsBlock and HolaSettingsButton GameObjects are BOTH on the scene");
                }
                paramsList.Add(status);
            }
        }

        private static void CheckBtnUrlCs()
        {
            accountsInfoFile = ConfigLoader.CheckAccountsInfoFile();
        }

        private static void CheckAppStoreTeams()
        {
            appstoreTeams = ConfigLoader.CheckAppStoreTeamsInfo();
        }

        private static void CheckFlurry()
        {
            string sdkName = "Flurry";

            bool gpFail = !ConfigSetter.HasFlurryPrefab();
            bool asFail = !ConfigSetter.HasFlurryPrefab();
            
            SdkStatus status = new SdkStatus(sdkName, "prefab",
                gpFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK,
                asFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK,
                SdkStatus.Status.NA
            );
            if (gpFail || asFail)
            {
                status.AddComment("Assets/Analytics/Analytics.prefab not found");
            }
            paramsList.Add(status);

            gpFail = string.IsNullOrEmpty(config.GetParam(Config.gpFlurry));
            asFail = string.IsNullOrEmpty(config.GetParam(Config.asFlurry));

            status = new SdkStatus(sdkName, "key",
                gpFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK,
                asFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK,
                SdkStatus.Status.NA
            );
            if (gpFail)
            {
                status.AddComment("GP Flurry Key is Empty");
            }
			bool isKeyEmpty = string.IsNullOrEmpty (GBNAPI.SDKInfo.GetKey ("sdk_flurrykey"));
			bool isKeyWrong = string.IsNullOrEmpty(GBNAPI.SDKInfo.GetKey("sdk_flurrykey")) || !GBNAPI.SDKInfo.GetKey ("sdk_flurrykey").Equals (config.GetParam (Config.gpFlurry));
			if (appliedPlatform == AppliedPlatform.Google && (isKeyEmpty || isKeyWrong))
            {
                status.AddComment("GP setted Flurry key is WRONG or EMPTY!");
            }
            if (asFail)
            {
                status.AddComment("AS Flurry Key is Empty");
            }
			isKeyWrong = string.IsNullOrEmpty(GBNAPI.SDKInfo.GetKey("sdk_flurrykey")) || !GBNAPI.SDKInfo.GetKey ("sdk_flurrykey").Equals (config.GetParam (Config.asFlurry));
			if (appliedPlatform == AppliedPlatform.Apple && (isKeyEmpty || isKeyWrong))
            {
                status.AddComment("AS setted Flurry key is WRONG or EMPTY!");
            }
            paramsList.Add(status);
        }

        private static void CheckOneAudience()
        {
            string sdkName = "OneAudience";

            bool gpFail;
            bool asFail;
            SdkStatus status;

            gpFail = string.IsNullOrEmpty(config.GetParam(Config.gpOneAudience));
            asFail = string.IsNullOrEmpty(config.GetParam(Config.asOneAudience));

            status = new SdkStatus(sdkName, "key",
                gpFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK,
                asFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK,
                SdkStatus.Status.NA
            );
            if (gpFail)
            {
                status.AddComment("GP OneAudience Key is Empty");
            }
            if (appliedPlatform == AppliedPlatform.Google && (string.IsNullOrEmpty(ConfigSetter.GetOneOudienceKey()) || !ConfigSetter.GetOneOudienceKey().Equals(config.GetParam(Config.gpOneAudience))))
            {
                status.AddComment("GP setted OneAudience Key is WRONG or EMPTY!");
            }
            if (asFail)
            {
                status.AddComment("AS OneAudience Key is Empty");
            }
            if (appliedPlatform == AppliedPlatform.Apple && (string.IsNullOrEmpty(ConfigSetter.GetOneOudienceKey()) || !ConfigSetter.GetOneOudienceKey().Equals(config.GetParam(Config.asOneAudience))))
            {
                status.AddComment("AS setted OneAudience Key is WRONG or EMPTY!");
            }
            paramsList.Add(status);
        }

        private static void CheckMobKnow()
        {
            if (config.IsLoaded)
            {
                string sdkName = "MobKnow";

                bool gpFail;
                SdkStatus status;
               
                gpFail = string.IsNullOrEmpty(config.GetParam(Config.gpMobKnow));

                status = new SdkStatus(sdkName, "key",
                   gpFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK,
                   SdkStatus.Status.NA,
                   SdkStatus.Status.NA
                );
                if (gpFail)
                {
                    status.AddComment("GP MobKnow key is EMPTY!");
                }
                if (appliedPlatform == AppliedPlatform.Google && (string.IsNullOrEmpty(ConfigSetter.GetMobKnowKey()) || !ConfigSetter.GetMobKnowKey().Equals(config.GetParam(Config.gpMobKnow))))
                {
                    status.AddComment("GP setted MobKnow key is WRONG or EMPTY!");
                }
                paramsList.Add(status);
            }
        }

        private static void CheckCoolTool()
        {
            string sdkName = "CoolTool";

            bool goFail = !ConfigSetter.HasCoolToolGameObject();

            SdkStatus status = new SdkStatus(sdkName, "gameObject",
                goFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK,
                goFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK,
                SdkStatus.Status.NA
            );
            if (goFail)
            {
                status.AddComment("CoolTool gameObject not found");
            }
            paramsList.Add(status);

            bool keyFail = string.IsNullOrEmpty(config.GetCoolToolKey());

            status = new SdkStatus(sdkName, "key",
                keyFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK,
                keyFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK,
                SdkStatus.Status.NA
            );
            if (keyFail)
            {
                status.AddComment("CoolTool key is Empty");
            }
            bool isKeyEmpty = string.IsNullOrEmpty(GBNAPI.SDKInfo.GetKey("sdk_cooltool"));
            bool isKeyWrong = isKeyEmpty || !GBNAPI.SDKInfo.GetKey("sdk_cooltool").Equals(config.GetCoolToolKey());
            if ((appliedPlatform == AppliedPlatform.Google || appliedPlatform == AppliedPlatform.Apple) && (isKeyEmpty || isKeyWrong))
            {
                status.AddComment("CoolTool key is WRONG or EMPTY!");
            }
            paramsList.Add(status);
        }

        private static void CheckBootLoader()
        {
            string sdkName = "BootLoader";

            bool goFail = !ConfigSetter.HasBootLoaderGameObject();

            SdkStatus status = new SdkStatus(sdkName, "gameObject",
                goFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK,
                goFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK,
                goFail ? SdkStatus.Status.ERROR : SdkStatus.Status.OK
            );
            if (goFail)
            {
                status.AddComment("BootLoader gameObject not found");
            }
            paramsList.Add(status);
        }

        private static void CheckGbnHzInit()
        {
            gbnhzPrefab = ConfigSetter.HasGbnHzInitPrefab() ? "OK" : "Assets/GBNHZ/GBNHZinit.prefab not found";
            gbnhzGameObject = ConfigSetter.HasGbnHzInitGameObject() ? "OK" : "GBNHZinit not found";
            gbnhzBalanceKey = ConfigSetter.GetBalanceUrlKey() == "" ? "Key is Empty" : (ConfigSetter.GetBalanceUrlKey() == config.GetParam(Config.balanceUrlKey) ? "OK" : "Wrong Key");
        }

        private static void CheckDefines()
        {
            androidAdsVersion = ConfigSetter.GetScriptingDefineSymbol(BuildTargetGroup.Android, "ADS_VERSION") ? "TRUE" : "FALSE";
            androidFinalVersion = ConfigSetter.GetScriptingDefineSymbol(BuildTargetGroup.Android, "FINAL_VERSION") ? "TRUE" : "FALSE";
            androidLocaleVersion = ConfigSetter.GetScriptingDefineSymbol(BuildTargetGroup.Android, "LOCALE_VERSION") ? "TRUE" : "FALSE";
            iosAdsVersion = ConfigSetter.GetScriptingDefineSymbol(BuildTargetGroup.iOS, "ADS_VERSION") ? "TRUE" : "FALSE";
            iosFinalVersion = ConfigSetter.GetScriptingDefineSymbol(BuildTargetGroup.iOS, "FINAL_VERSION") ? "TRUE" : "FALSE";
            iosLocaleVersion = ConfigSetter.GetScriptingDefineSymbol(BuildTargetGroup.iOS, "LOCALE_VERSION") ? "TRUE" : "FALSE";
        }

        private static void CheckScenes()
        {
            ConfigSetter.RemoveMissingScenes();
            scenes = EditorBuildSettings.scenes;
        }

        private static string CheckFolder(string foldername)
        {
            return Directory.Exists(Application.dataPath + "/" + foldername) ? "OK" : "\"" + foldername + "\" does not exist!";
        }

        private static string CheckFile(string filepath)
        {
            return File.Exists(Application.dataPath + "/" + filepath) ? "OK" : "\"" + filepath + "\" does not exist!";
        }

        private static string CheckIcon(string filepath)
        {
            return File.Exists(filepath) ? "OK" : "\"" + filepath + "\" does not exist!";
        }

        private static void CheckKeystores()
        {
            if (config != null && config.IsLoaded)
            {
                keystoresFolder = Directory.Exists(Config.keystoresFolder) ? "OK" : "\"" + Config.keystoresFolder + "\" does not exist!";
                keystoresList = File.Exists(Config.keystoresFolder + "/" + Config.keystoresList) ? "OK" : "\"" + Config.keystoresFolder + "/" + Config.keystoresList + "\" does not exist!";
                var amazonKeystoreFilepath = ConfigLoader.GetKeyStoreFilepath(config.GetParam(Config.amBundle));
                amazonKeystore = File.Exists(amazonKeystoreFilepath) ? "OK" : "\"" + amazonKeystoreFilepath + "\" does not exist!";
                var googlePlayKeystoreFilepath = ConfigLoader.GetKeyStoreFilepath(config.GetParam(Config.gpBundle));
                googlePlayKeystore = File.Exists(googlePlayKeystoreFilepath) ? "OK" : "\"" + googlePlayKeystoreFilepath + "\" does not exist!";
            }
        }

        private static void CheckIcons()
        {
            iconsFolder = CheckFolder("Icons");
            iconGp = CheckIcon(Config.iconGP);
            iconAmazon = CheckIcon(Config.iconAM);
            iconIosFree = CheckIcon(Config.iconASFree);

            icon_gp = new Texture2D(512, 512, TextureFormat.ARGB32, true);
            if (File.Exists(Config.iconGP))
            {
                var image = File.ReadAllBytes(Config.iconGP);
                icon_gp.LoadImage(image);
            }

            icon_amazon = new Texture2D(512, 512, TextureFormat.ARGB32, true);
            if (File.Exists(Config.iconAM))
            {
                var image = File.ReadAllBytes(Config.iconAM);
                icon_amazon.LoadImage(image);
            }

            icon_ios_free = new Texture2D(512, 512, TextureFormat.ARGB32, true);
            if (File.Exists(Config.iconASFree))
            {
                var image = File.ReadAllBytes(Config.iconASFree);
                icon_ios_free.LoadImage(image);
            }
        }

        private static bool ContainsWrongCharacters(string value)
        {

            for (var i = 0; i < value.Length; i++)
            {
                int code = (int)value[i];
                if (code > 127)
                {
                    Debug.LogWarning("Wrong Characters: " + value);
                    return true;
                }

            }
            return false;
        }

        Vector2 scroll_pos;

        void ShowSdkPlatformStatusString(SdkStatus paramString)
        {
            if (!paramString.hasProblems && string.IsNullOrEmpty(paramString.comments))
            {
                return;
            }
            GUIStyle guiStyle = new GUIStyle { fontStyle = FontStyle.Bold };
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(paramString.parameter, GUIStyle.none);
            //gp
            EditorGUILayout.LabelField(SdkStatusToColoredString(paramString.gpStatus), guiStyle);
            //as
            EditorGUILayout.LabelField(SdkStatusToColoredString(paramString.asStatus), guiStyle);
            //am
            EditorGUILayout.LabelField(SdkStatusToColoredString(paramString.amStatus), guiStyle);
            GUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(paramString.comments))
            {
                string[] lines = paramString.comments.Split('\n');
                foreach (string line in lines)
                {
                    EditorGUILayout.LabelField("<color=" + "red" + ">" + line + "</color>", GUIStyle.none);
                }
            }
        }

        string SdkStatusToColoredString(SdkStatus.Status encodedString)
        {
            string res = "";
            if (encodedString == SdkStatus.Status.NA)
            {
                res = "<color=" + "gray" + ">NA</color>";
                return res;
            }

            switch (encodedString)
            {
                case SdkStatus.Status.OK:
                    res = "<color=" + "green" + ">"+ "OK" + "</color>";
                    break;
                case SdkStatus.Status.ERROR:
                    res = "<color=" + "red" + ">" + "ERROR" + "</color>";
                    break;
                case SdkStatus.Status.WARNING:
                    res = "<color=" + "yellow" + ">" + "WARNING" + "</color>";
                    break;
                default:
                    res = "<color=" + "black" + ">" + "UNDEFINED" + "</color>";
                    break;
            }
            return res;
        }

        string DefineSymbolStatusToColoredString(string status)
        {
            string color = (status == "TRUE") ? "green" : ((status == "FALSE") ? "yellow" : "red");
            return "<color=" + color + ">" + status + "</color>";
        }

        void ShowDefineSymbols()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scripting Define Symbol\\Platform", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Android", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("iOS", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            var guiStyle = new GUIStyle { fontStyle = FontStyle.Bold };

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ADS_VERSION", GUIStyle.none);
            EditorGUILayout.LabelField(DefineSymbolStatusToColoredString(androidAdsVersion), guiStyle);
            EditorGUILayout.LabelField(DefineSymbolStatusToColoredString(iosAdsVersion), guiStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("FINAL_VERSION", GUIStyle.none);
            EditorGUILayout.LabelField(DefineSymbolStatusToColoredString(androidFinalVersion), guiStyle);
            EditorGUILayout.LabelField(DefineSymbolStatusToColoredString(iosFinalVersion), guiStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("LOCALE_VERSION", GUIStyle.none);
            EditorGUILayout.LabelField(DefineSymbolStatusToColoredString(androidLocaleVersion), guiStyle);
            EditorGUILayout.LabelField(DefineSymbolStatusToColoredString(iosLocaleVersion), guiStyle);
            GUILayout.EndHorizontal();
        }

        void OnGUI()
        {
            scroll_pos = GUILayout.BeginScrollView(scroll_pos, GUILayout.Width(position.width), GUILayout.Height(position.height));

            if (config != null)
            {
                GUILayout.Label(config.GetParam(Config.epicId) + " " + config.GetParam(Config.epicName), EditorStyles.boldLabel);
            }
            GUILayout.Label("Config.txt version: " + configTxtVersion, EditorStyles.boldLabel);
            EditorGUILayout.Separator();

            ShowMessage("Applied platform:", appliedPlatform.ToString(), appliedPlatform != AppliedPlatform.UNDEFINED);
            ShowField("Company Info validation:", GBNAPI.CompanyInfo.Struct.IsValid() ? "OK" : "ERROR", true);
            EditorGUILayout.Separator();

            ShowField("Config статус:", configStatus);

            foreach (var status in configStatuses)
            {
                if (status.Value == "Is Empty" || status.Value == "Wrong Format")
                {
                    ShowField(status.Key + ":", status.Value);
                }
                else
                {
                    ShowMessage(status.Key + ":", "" + status.Value, true);
                }

            }

            EditorGUILayout.Separator();

            GUILayout.Label("GBNHZinit", EditorStyles.boldLabel);
            ShowField("Prefab:", gbnhzPrefab);
            ShowField("GameObject:", gbnhzGameObject, true);
            ShowField("Balance URL Key:", gbnhzBalanceKey, true);
            EditorGUILayout.Separator();

            GUILayout.Label("UnityAds", EditorStyles.boldLabel);
            ShowMessage("Ads Version:", GBNHZinit.version, true);
            ShowField("UnityAds package:", structureProjectStatus);
            EditorGUILayout.Separator();

            if (paramsList != null)
            {
                EditorGUILayout.Space();
                if (paramsList.FindAll(x => x.hasProblems).Count > 0)
                {
                    string currentSdk = "";

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("SDK\\Platform", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Google", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Apple", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Amazon", EditorStyles.boldLabel);
                    GUILayout.EndHorizontal();

                    for (int i = 0; i < paramsList.Count; ++i)
                    {
                        if (!currentSdk.Equals(paramsList[i].name) && paramsList.FindAll(x => x.name.Equals(paramsList[i].name) && x.hasProblems).Count > 0)
                        {
                            currentSdk = paramsList[i].name;
                            GUILayout.Label(paramsList[i].name, EditorStyles.boldLabel);
                        }
                        ShowSdkPlatformStatusString(paramsList[i]);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("<color=" + "green" + ">" + "All SDKs settings is OK for applied platform" + "</color>", new GUIStyle { fontStyle = FontStyle.Bold });
                }
                EditorGUILayout.Space();
            }

            GUILayout.Label("Keystores", EditorStyles.boldLabel);
            ShowField("Папка Keys:", keystoresFolder);
            ShowField("Список ключей:", keystoresList, true);
            ShowField("Amazon Keystore:", amazonKeystore, true);
            ShowField("Google Keystore:", googlePlayKeystore, true);

            EditorGUILayout.Separator();
            GUILayout.Label("Структура проекта", EditorStyles.boldLabel);

            if (scenes != null && config != null)
            {
                ShowMessage("Build Scenes:", "" + scenes.Length, scenes.Length > 0);
                var isScenesAdditive = new bool[scenes.Length];
                var additiveScenes = config.GetParam(Config.additiveScenes);
                for (var i = 0; i < scenes.Length; i++)
                {
                    isScenesAdditive[i] = (additiveScenes.IndexOf(scenes[i].path) >= 0);

                    var guiStyle = new GUIStyle { fontStyle = FontStyle.Bold };
                    GUILayout.BeginHorizontal();
                    var color = (true) ? "green" : "red";
                    EditorGUILayout.LabelField("<color=" + color + ">" + scenes[i].path + "</color>", guiStyle);
                    isScenesAdditive[i] = EditorGUILayout.Toggle("Is Additive", isScenesAdditive[i]);
                    if (isScenesAdditive[i])
                    {
                        additiveScenes = AddAdditiveScene(additiveScenes, scenes[i].path);
                    }
                    else
                    {
                        additiveScenes = RemoveAdditiveScene(additiveScenes, scenes[i].path);
                    }
                    GUILayout.EndHorizontal();


                }
                ConfigLoader.SaveAdditiveScenes(additiveScenes);

            }
            EditorGUILayout.Separator();

            GUILayout.Label("Иконки", EditorStyles.boldLabel);
            ShowField("Папка Icons:", iconsFolder, true);
            ShowField("Иконка Google:", iconGp, true);
            ShowField("Иконка Amazon:", iconAmazon, true);
            ShowField("Иконка iOS Free: ", iconIosFree, true);
            GUILayout.BeginHorizontal();
            var sz = 64;
            if (icon_gp != null)
            {
                GUILayout.Label(icon_gp, GUILayout.MaxWidth(sz), GUILayout.MaxHeight(sz));
            }
            if (icon_amazon != null)
            {
                GUILayout.Label(icon_amazon, GUILayout.MaxWidth(sz), GUILayout.MaxHeight(sz));
            }
            if (icon_ios_free != null)
            {
                GUILayout.Label(icon_ios_free, GUILayout.MaxWidth(sz), GUILayout.MaxHeight(sz));
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();

            GUILayout.Label("Accounts Info", EditorStyles.boldLabel);
            ShowMultistringField("accounts_info.txt URLs", accountsInfoFile);

            EditorGUILayout.Separator();
            ShowMultistringField("accounts_info.txt AppStore Teams", appstoreTeams);

            EditorGUILayout.Separator();

            ShowDefineSymbols();

            EditorGUILayout.Separator();

            if (GUILayout.Button("Обновить"))
            {
                CheckProject();
            }

            GUILayout.EndScrollView();
        }

        private string AddAdditiveScene(String scenes, String scene)
        {
            if (scenes.IndexOf(scene) < 0)
            {
                if (scenes == "")
                {
                    scenes = scene;
                }
                else
                {
                    scenes = scenes + "," + scene;
                }
                // Debug.Log("ADD SCENE " + scene + ", ADDITIVE SCENES: " + scenes);
            }
            return scenes;
        }

        private string RemoveAdditiveScene(String scenes, String scene)
        {
            if (scenes.IndexOf(scene) >= 0)
            {
                var oldScenes = scenes.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                var newScenes = new List<string>();
                foreach (var sc in oldScenes)
                {
                    if (sc != scene)
                    {
                        newScenes.Add(sc);
                    }
                }
                scenes = string.Join(",", newScenes.ToArray());
                //Debug.Log("REMOVE SCENE " + scene + ", ADDITIVE SCENES: " + scenes);
            }
            return scenes;
        }

        private void ShowMultistringField(string labelText, string statusText, bool showOnError = false)
        {
            string[] listOfStrings = statusText.Split(new char[] { '\n' });

            if (listOfStrings.Length > 0)
            {
                ShowField(labelText, listOfStrings[0]);
            }
            if (listOfStrings.Length > 1)
            {
                for (int i = 1; i < listOfStrings.Length; ++i)
                {
                    EditorGUILayout.LabelField("<color=" + "red" + ">" + listOfStrings[i] + "</color>", GUIStyle.none);
                }
            }
        }

        private void ShowField(string labelText, string statusText, bool showOnError = false)
        {
            bool optional = isOptional(labelText);

            if (showOnError && (statusText == "OK" || statusText == "TRUE" || optional))
            {
                return;
            }

            GUIStyle guiStyle = new GUIStyle { fontStyle = FontStyle.Bold };
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(labelText, GUIStyle.none);
            string errorColor = optional ? "yellow" : "red";
            string color = (statusText == "OK" || statusText == "TRUE") ? "green" : errorColor;
            EditorGUILayout.LabelField("<color=" + color + ">" + statusText + "</color>", guiStyle);
            GUILayout.EndHorizontal();
        }

        private void ShowMessage(string labelText, string messageText, bool success)
        {
            var guiStyle = new GUIStyle { fontStyle = FontStyle.Bold };

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(labelText, GUIStyle.none);
            var color = (success) ? "green" : "red";
            EditorGUILayout.LabelField("<color=" + color + ">" + messageText + "</color>", guiStyle);
            GUILayout.EndHorizontal();
        }

        private bool isOptional(string labelText)
        {
            foreach (var key in optionalKeys)
            {
                if (labelText.IndexOf(key) >= 0) return true;
            }
            return false;
        }

        private bool isRequired(string labelText)
        {
            foreach (var key in requiredKeys)
            {
                if (labelText.IndexOf(key) >= 0) return true;
            }
            return false;
        }
    }
}
