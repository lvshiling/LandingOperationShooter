using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Assets.Editor.GBNEditor;
using System.Linq;
using UnityEngine.SceneManagement;
using GBNAPI;

namespace Assets.Editor.ConfigLoader
{
    public class ConfigSetter
    {
        private static string AndroidManifest = "/Plugins/Android/AndroidManifest.xml";

        public static Config config
        {
            get
            {
                return ConfigLoader.config;
            }
        }

        private static bool mobileInput
        {
            get
            {
                return (config.GetParam(Config.MOBILE_INPUT) == "true") ? true : false;
            }
            set
            {
                var oldMI = config.GetParam(Config.MOBILE_INPUT);
                var newMI = value ? "true" : "false";
                if (oldMI != newMI)
                {
                    ConfigLoader.LoadConfigIfNotLoaded();
                    config.SetParam(Config.MOBILE_INPUT, newMI);
                    ConfigLoader.SaveConfig();
                }
            }
        }
        public static bool finalVersion
        {
            get
            {
                return EditorPrefs.GetInt("FINAL_VERSION", 0) == 1;
            }
            private set
            {
                EditorPrefs.SetInt("FINAL_VERSION", value ? 1 : 0);
            }
        }
        private static bool adsVersion
        {
            get
            {
                return EditorPrefs.GetInt("ADS_VERSION", 1) == 1;
            }
            set
            {
                EditorPrefs.SetInt("ADS_VERSION", value ? 1 : 0);
            }
        }
        private static bool hasCrossPlatformInput
        {
            get
            {
                return EditorPrefs.GetInt("HAS_CROSS_PLATFORM_INPUT", 1) == 1;
            }
            set
            {
                EditorPrefs.SetInt("HAS_CROSS_PLATFORM_INPUT", value ? 1 : 0);
            }
        }

        private static bool localeVersion
        {
            get
            {
                return (config.GetParam(Config.LOCALE_VERSION) == "true") ? true : false;
            }
            set
            {
                var locale = config.GetParam(Config.LOCALE_VERSION);
                var newLocale = value ? "true" : "false";
                if (locale != newLocale)
                {
                    ConfigLoader.LoadConfigIfNotLoaded();
                    config.SetParam(Config.LOCALE_VERSION, newLocale);
                    ConfigLoader.SaveConfig();
                }
            }
        }

        public static string encryptionKey = string.Empty;

        private static void SetEncryptionKey()
        {
            encryptionKey = GenerateEncryptionKey();
        }

        private static string GenerateEncryptionKey(int length = 30)
        {
            var key = string.Empty;
            System.Random rnd = new System.Random();

            for (int i = 0; i < length; i++)
            {
                key += rnd.Next(0, 9).ToString();
            }
            return key;
        }

        private static void SetIcons(BuildTargetGroup buildTargetGroup, string assetsIconFilename, int iconSize)
        {
            if (File.Exists(assetsIconFilename))
            {
                AssetDatabase.ImportAsset(assetsIconFilename, ImportAssetOptions.DontDownloadFromCacheServer);

                Texture2D[] defaulticons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown);

                if (defaulticons.Length < 1)
                {
                    defaulticons = new Texture2D[1];
                }

                for (var i = 0; i < defaulticons.Length; i++)
                {
                    defaulticons[i] = AssetDatabase.LoadAssetAtPath(assetsIconFilename, typeof(Texture2D)) as Texture2D;
                }
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, defaulticons);

                PlayerSettings.SetIconsForTargetGroup(buildTargetGroup, new Texture2D[] { });
            }
        }

        internal static string GetUnityAdsKey()
        {
            string id = GBNAPI.SDKInfo.GetKey("sdk_unityadskey");

            if (string.IsNullOrEmpty(id))
                return "";
            else
                return id;
        }

        private static void RemoveTamoco()
        {
            var startTamoco = GameObject.Find("StartTamoco");
            if (startTamoco != null)
            {
                GameObject.DestroyImmediate(startTamoco);
            }
        }
        
        public static bool HasAppLovinKeyInManifest()
        {
            if (File.Exists(Application.dataPath + AndroidManifest) && config.GetAppLovinKey() != "")
            {
                var text = File.ReadAllText(Application.dataPath + AndroidManifest);
                return text.IndexOf(config.GetAppLovinKey()) >= 0;
            }
            return false;
        }

        private static void SetAppLovinKey()
        {
            var appLovinKey = config.GetAppLovinKey();
            var replaceKey = "<meta-data android:name=\"applovin.sdk.key\" android:value=\"" + appLovinKey + "\"/>";

            if (File.Exists(Application.dataPath + AndroidManifest))
            {
                var text = File.ReadAllText(Application.dataPath + AndroidManifest);
                if (text.IndexOf(appLovinKey) < 0)
                {
                    Regex regex = new Regex(@"<meta-data\s+android:name=.applovin\.sdk\.key.\s+android:value=[\W\D][0-9a-zA-Z_\-]+.\s*\/>");
                    var match = regex.Match(text);
                    if (match.Success)
                    {
                        var result = regex.Replace(text, replaceKey);
                        File.WriteAllText(Application.dataPath + AndroidManifest, result);
                    }
                    else
                    {
                        Debug.LogWarning("Can`t find applovin.sdk.key in " + AndroidManifest);
                    }
                }
                else
                {
                    Debug.Log("AppLovin Key is actual!");
                }
            }
            else
            {
                Debug.LogWarning("File " + AndroidManifest + " not found!");
            }
        }

        private static void FixMobileInput()
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            hasCrossPlatformInput = mobileInput;
            // search MobileSingleStickControl in all scenes
            for (var i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                var scene = EditorSceneManager.OpenScene(EditorBuildSettings.scenes[i].path, OpenSceneMode.Single);
                EditorSceneManager.SetActiveScene(scene);
                var hasChanges = false;
                MonoBehaviour mobileSingleStickControlScript = null;
                GameObject mobileJoystickGameObject = null;

                var gameObjects = scene.GetRootGameObjects();

                foreach (var go in gameObjects)
                {
                    var children = go.transform.GetComponentsInChildren<MonoBehaviour>(true);
                    foreach (var t in children)
                    {
                        if (t != null)
                        {
                            if (mobileSingleStickControlScript == null)
                            {
                                if (t.GetType().ToString() == "UnityStandardAssets.CrossPlatformInput.MobileControlRig")
                                {
                                    mobileSingleStickControlScript = t;
                                }
                            }
                            if (mobileJoystickGameObject == null)
                            {
                                if (t.GetType().ToString() == "UnityStandardAssets.CrossPlatformInput.Joystick")
                                {
                                    mobileJoystickGameObject = t.gameObject;
                                }
                            }
                        }
                    }
                }
                if (mobileSingleStickControlScript != null || mobileJoystickGameObject != null)
                {
                    var isEnabledMobileSingleStickControlScript = false;
                    var isEnabledmobileJoystickGameObject = false;
                    SetScriptingDefineSymbols(new string[] { "CROSS_PLATFORM_INPUT", "MOBILE_INPUT" }, new bool[] { false, false });
                    if (mobileSingleStickControlScript != null)
                    {
                        isEnabledMobileSingleStickControlScript = mobileSingleStickControlScript.enabled;
                        mobileSingleStickControlScript.enabled = false;
                    }
                    if (mobileJoystickGameObject != null)
                    {
                        isEnabledmobileJoystickGameObject = mobileJoystickGameObject.activeSelf;
                        mobileJoystickGameObject.SetActive(false);
                        mobileJoystickGameObject.SetActive(isEnabledmobileJoystickGameObject);
                    }
                    if (mobileSingleStickControlScript != null && isEnabledMobileSingleStickControlScript)
                    {
                        mobileSingleStickControlScript.enabled = true;
                    }
                    hasChanges = true;
                    hasCrossPlatformInput = true;
                }
                if (hasChanges)
                {
                    EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                    Debug.Log("<color=green><b>MobileSingleStickControl Fixed in scene " + scene.name + "</b></color>");
                }
            }
            SetScriptingDefineSymbols(new string[] { "CROSS_PLATFORM_INPUT", "MOBILE_INPUT" }, new bool[] { hasCrossPlatformInput, hasCrossPlatformInput });
            if (!hasCrossPlatformInput)
            {
                Debug.Log("<color=yellow><b>MobileSingleStickControl Not found!</b></color>");
            }

            if (EditorBuildSettings.scenes.Length > 0)
            {
                var scene = EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path, OpenSceneMode.Single);
                EditorSceneManager.SetActiveScene(scene);
            }
        }

        public static void SetCompanyInfoIntoResourses(string store)
        {
            //valid store values are
            //"Amazon"
            //"Google"
            //"Apple"

            string account = PlayerSettings.companyName;

            string aName = account.ToLower().Trim().Replace(" ", "");

            if (File.Exists(Config.accountsInfoFile))
            {
                GBNEditorInfoFileBuilder.SetValue("info_key", encryptionKey);

                string text = File.ReadAllText(Config.accountsInfoFile);
                JSONObject jObj = new JSONObject(text);

                bool isValid = false;
                bool hasAccount = false;
                if (jObj != null && jObj.HasField("urls"))
                {
                    jObj = jObj.GetField("urls");

                    string[] fields = { "cooltool", "youtube", "companies" };

                    if (jObj.HasFields(fields))
                    {
                        GBNEditorInfoFileBuilder.SetValue("info_cooltool", Crypt.GBNEncrypt(jObj.GetField("cooltool").str, encryptionKey));
                        GBNEditorInfoFileBuilder.SetValue("info_youtube", Crypt.GBNEncrypt(jObj.GetField("youtube").str, encryptionKey));

                        jObj = jObj.GetField("companies");
                        if (jObj.IsArray)
                        {
                            fields = new string[] { "name", "store", "policy", "email", "url" };
                            isValid = true;
                            for (int i = 0; i < jObj.Count; i++)
                            {
                                if (jObj[i].HasFields(fields))
                                {
                                    string cName = jObj[i].GetField("name").str.ToLower().Trim().Replace(" ", "");
                                    string cStore = jObj[i].GetField("store").str;
                                    if (cName.Equals(aName) && store.Equals(cStore))
                                    {
                                        hasAccount = true;
                                        Dictionary<string, string> jsonToDict = jObj[i].ToDictionary();
                                        foreach (KeyValuePair<string, string> kvp in jsonToDict)
                                        {
                                            GBNEditorInfoFileBuilder.SetValue("info_" + kvp.Key, Crypt.GBNEncrypt(kvp.Value, encryptionKey));
                                        }
                                    }
                                }
                            }
                            if (!hasAccount)
                            {
                                Debug.LogError("File " + Config.accountsInfoFile + " contains no information about \"" + account + "\" account for " + store + " store!");
                            }
                        }
                    }
                }
                if (!isValid)
                {
                    Debug.LogError("File " + Config.accountsInfoFile + " contains wrong information!");
                }
            }
            else
            {
                Debug.LogError("File " + Config.accountsInfoFile + " does not exist!");
            }
        }

        public static void SetGDPRTexts(string store)
        {
            //valid store values are
            //"Amazon"
            //"Google"
            //"Apple"
            /*
            string gdprTexts = "";

            if (store.Equals("Google"))
            {
                gdprTexts = config.GetGDPRKey(); ;
            }
            else if (store.Equals("Apple"))
            {
                gdprTexts = config.GetGDPRKey(); ;
            }
            else if (store.Equals("Amazon"))
            {

            }

            if (!string.IsNullOrEmpty(gdprTexts))
            {
                GBNEditorInfoFileBuilder.SetValue("info_sdk/sdk_gdpr", Crypt.GBNEncrypt(gdprTexts, encryptionKey));
            }
            */
        }

        [MenuItem("Config/Применить настройки Google Play", false, 4)]
        public static void ApplyConfigGPDefault()
        {
            ConfigLoader.LoadConfig();
            ApplyConfigGP("", -1);
            // save all assets
            AssetDatabase.SaveAssets();
        }

        public static void ApplyConfigGP(string bundleVersion = "1.0.0", int bundleVersionCode = 1, AndroidTargetDevice targetDevice = AndroidTargetDevice.FAT)
        {
            // apply title
            if (config.GetParam(Config.gpTitle) != "")
            {
                PlayerSettings.productName = config.GetParam(Config.gpTitle);
            }
            // apply account
            if (config.GetParam(Config.gpAccount) != "")
            {
                PlayerSettings.companyName = config.GetParam(Config.gpAccount);
            }
            // apply bundle
            if (config.GetParam(Config.gpBundle) != "")
            {
#if UNITY_5_6_OR_NEWER
                PlayerSettings.applicationIdentifier = config.GetParam(Config.gpBundle);
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, config.GetParam(Config.gpBundle));
#else
                PlayerSettings.bundleIdentifier = config.GetParam(Config.gpBundle);
#endif
            }
            // apply bundleVersion
            if (bundleVersion != "")
            {
                PlayerSettings.bundleVersion = bundleVersion;
            }
            else
            {
                PlayerSettings.bundleVersion = config.GetGPVersion();
            }
            // apply buildVersion
            if (bundleVersionCode >= 0)
            {
                PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
            }
            else
            {
                PlayerSettings.Android.bundleVersionCode = config.GetGPCode();
            }
            // apply keystore
            ApplyKeystore(config.GetParam(Config.gpAccount), config.GetParam(Config.gpBundle));

            // check icons android or ios
            SetIcons(BuildTargetGroup.Android, Config.iconGP, 512);

            // min sdk version
            PlayerSettings.Android.minSdkVersion = Config.androidSdkVersion;

            // set Android Device Filter 
            PlayerSettings.Android.targetDevice = targetDevice;

            // disable Android TV
            PlayerSettings.Android.androidTVCompatibility = false;

            adsVersion = true;
            ApplyCommonSettings();

            SetBootLoader();

            SetCoolTool();

            // flurry
            SetFlurryKey("Google");

            // unityads
            SetUnityAdsKey("Google");

            SetCompanyInfoIntoResourses("Google");

            SetGDPRTexts("Google");

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("<color=green><b>Настройки Google Play применены</b></color>");
        }

        [System.Obsolete("SDK was removed!", true)]
        public static void SetAndroidProximitySDKInclude(bool setTo = true)
        {
            PluginImporter[] androidPlugins = PluginImporter.GetAllImporters();
            PluginImporter proximity = null;
            foreach (PluginImporter pi in androidPlugins)
            {
                if (pi.assetPath.Contains("proximity_sdk-release"))
                {
                    proximity = pi;
                    break;
                }
            }
            if (proximity != null)
            {
                proximity.SetCompatibleWithPlatform(BuildTarget.Android, setTo);
            }
        }

        public static void ResetKeystore()
        {
            string bundle = "";
#if UNITY_5_6_OR_NEWER
            bundle = PlayerSettings.applicationIdentifier;
#else
            bundle = PlayerSettings.bundleIdentifier;
#endif
            ApplyKeystore(PlayerSettings.companyName, bundle);
        }

        private static void ApplyKeystore(string account, string bundle)
        {
            string aliasName = ConfigLoader.GetAliasFromBundle(bundle);
            var keystoreData = ConfigLoader.GetKeyStore(aliasName);
            if (keystoreData.Length <= 0)
            {
                Debug.LogError("Keystore " + aliasName + " not found!");
                return;
            }
            // check alias from keystoreData
            if (IsAlias(keystoreData[3]))
            {
                aliasName = keystoreData[3];
            }
            else
            {
                Debug.Log("AliasName not found in KeystoreData, used aliasName from bundle");
            }

            if (!keystoreData.Contains(account))
            {
                Debug.LogWarning("<color=red> (!) Account name doesn't match alias and keystore! Please check if this is necessary! </color>");
            }

            PlayerSettings.Android.keystoreName = Config.keystoresFolder + "/" + keystoreData[2];
            PlayerSettings.Android.keystorePass = keystoreData[1];
            PlayerSettings.Android.keyaliasName = aliasName;
            PlayerSettings.Android.keyaliasPass = keystoreData[1];
        }

        private static bool IsAlias(string aliasNameForTest)
        {
            Regex regex = new Regex(@"\W");
            return !string.IsNullOrEmpty(aliasNameForTest) && !regex.IsMatch(aliasNameForTest);
        }

        public static void ApplyConfigGPLocal(string bundleVersion = "1.0.0", int bundleVersionCode = 1, AndroidTargetDevice targetDevice = AndroidTargetDevice.FAT)
        {
            // apply title
            if (config.GetParam(Config.gpTitle) != "")
            {
                PlayerSettings.productName = config.GetParam(Config.gpTitle);
            }
            // apply account
            if (config.GetParam(Config.gpAccount) != "")
            {
                PlayerSettings.companyName = config.GetParam(Config.gpAccount);
            }
            // apply bundle
            if (config.GetParam(Config.gpBundle) != "")
            {
#if UNITY_5_6_OR_NEWER
                PlayerSettings.applicationIdentifier = config.GetParam(Config.gpBundle);
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, config.GetParam(Config.gpBundle));
#else
                PlayerSettings.bundleIdentifier = config.GetParam(Config.gpBundle);
#endif
            }
            // apply bundleVersion
            if (bundleVersion != "")
            {
                PlayerSettings.bundleVersion = bundleVersion;
            }
            else
            {
                PlayerSettings.bundleVersion = config.GetGPVersion();
            }
            // apply buildVersion
            if (bundleVersionCode >= 0)
            {
                PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
            }
            else
            {
                PlayerSettings.Android.bundleVersionCode = config.GetGPCode();
            }

            // check icons android or ios
            SetIcons(BuildTargetGroup.Android, Config.iconGP, 512);

            // min sdk version
            PlayerSettings.Android.minSdkVersion = Config.androidSdkVersion;

            // set Android Device Filter 
            PlayerSettings.Android.targetDevice = targetDevice;

            // disable Android TV
            PlayerSettings.Android.androidTVCompatibility = false;

            adsVersion = true;
            ApplyCommonSettings();

            if (finalVersion)
            {
                // apply keystore
                ApplyKeystore(config.GetParam(Config.gpAccount), config.GetParam(Config.gpBundle));
            }
            else
            {
                // do not apply keystore
                PlayerSettings.Android.keystoreName = "";
                PlayerSettings.Android.keystorePass = "";
                PlayerSettings.Android.keyaliasName = "";
                PlayerSettings.Android.keyaliasPass = "";
            }

            SetBootLoader();

            SetCoolTool();

            // flurry
            SetFlurryKey("Google");

            // unityads
            SetUnityAdsKey("Google");

            SetCompanyInfoIntoResourses("Google");

            SetGDPRTexts("Google");

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("<color=green><b>Настройки Google Play применены</b></color>");
        }

        private static void ApplyCommonSettings()
        {
            QualitySettings.masterTextureLimit = 0; //(0-fullres, 1-halfres, 2-qurtres, 3-eightres)

            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;

            PatchAutomator.RemoveOldFiles();

            if (!GBNEditorAccounts.Git.account.IsEmpty() && !Environment.GetCommandLineArgs().Contains("-batchmode")) //здесь обновляем только в эдитор-режиме
            {
                try
                {
                    ConfigLoader.UpdateAccountsInfoFile();
#if UNITY_ANDROID
                    ConfigLoader.UpdateKeyStores();
#endif
                }
                catch
                {
                    Debug.Log("Can't update accounts_info file and keystores from GIT!");
                }
            }

            GBNEditorInfoFileBuilder.Clear();

            SetEncryptionKey();

            SetCloudInfo();

            SetupUnityLogs();

            // scenes build order
            SetScenesBuildOrder();

            FixMobileInput();

            // scripting define symbols
            SetScriptingDefineSymbols(new string[] { "ADS_VERSION", "FINAL_VERSION", "LOCALE_VERSION", "CROSS_PLATFORM_INPUT", "MOBILE_INPUT" },
                new bool[] { adsVersion, finalVersion, localeVersion, hasCrossPlatformInput, hasCrossPlatformInput });

            // GBNHZ
            SetBalanceUrlKey();

            SetExecutionOrder();

            RemoveSdkManager();

            RemoveBootLoader();

            //remove all legacy gameobjects
            RemoveCuebiq();
            RemoveFlurry();
            RemoveGameCenterInit();
            RemoveHola();
            RemoveHuq();
            RemoveOneAudience();
            RemoveTamoco();
            RemoveTutela();
        }
        private static void SetScriptingDefineSymbols(string[] defineNames, bool[] enable)
        {
            SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defineNames, enable);
            SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, defineNames, enable);
        }

        private static void SetExecutionOrder()
        {
            var monoScripts = MonoImporter.GetAllRuntimeMonoScripts();
            foreach (var monoScript in monoScripts)
            {
                if (monoScript.GetClass() != null && monoScript.GetClass() == typeof(GBNHZinit))
                {
                    MonoImporter.SetExecutionOrder(monoScript, -500);
                }
            }
        }

        public static void SetCloudInfo()
        {
            GBNEditorInfoFileBuilder.SetValue("info_sdk/sdk_cloudurl", Crypt.GBNEncrypt(ConfigLoader.cloudConnectorWebServiceUrl, encryptionKey));
            GBNEditorInfoFileBuilder.SetValue("info_sdk/sdk_cloudpwd", Crypt.GBNEncrypt(ConfigLoader.cloudConnectorServicePassword, encryptionKey));
        }

        private static void SetupUnityLogs()
        {
            // disable Unity Debug.Log in final version
            PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
            PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
            PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.Full);
            PlayerSettings.SetStackTraceLogType(LogType.Log, finalVersion ? StackTraceLogType.None : StackTraceLogType.ScriptOnly);
            PlayerSettings.SetStackTraceLogType(LogType.Warning, finalVersion ? StackTraceLogType.None : StackTraceLogType.ScriptOnly);
        }

        [MenuItem("Config/Применить настройки Amazon", false, 4)]
        public static void ApplyConfigAmazonDefault()
        {
            ConfigLoader.LoadConfig();
            ApplyConfigAmazon("", -1);
            // save all assets
            AssetDatabase.SaveAssets();
        }

        public static void ApplyConfigAmazon(string bundleVersion = "1.0.0", int bundleVersionCode = 1, AndroidTargetDevice targetDevice = AndroidTargetDevice.FAT)
        {
            // apply title
            if (config.GetParam(Config.amTitle) != "")
            {
                PlayerSettings.productName = config.GetParam(Config.amTitle);
            }
            // apply account
            if (config.GetParam(Config.amAccount) != "")
            {
                PlayerSettings.companyName = config.GetParam(Config.amAccount);
            }
            // apply bundle
            if (config.GetParam(Config.amBundle) != "")
            {
#if UNITY_5_6_OR_NEWER
                PlayerSettings.applicationIdentifier = config.GetParam(Config.amBundle);
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, config.GetParam(Config.amBundle));
#else
                PlayerSettings.bundleIdentifier = config.GetParam(Config.amBundle);
#endif
            }
            // apply bundleVersion
            if (bundleVersion != "")
            {
                PlayerSettings.bundleVersion = bundleVersion;
            }
            else
            {
                PlayerSettings.bundleVersion = config.GetGPVersion();
            }
            // apply buildVersion
            if (bundleVersionCode >= 0)
            {
                PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
            }
            else
            {
                PlayerSettings.Android.bundleVersionCode = config.GetGPCode();
            }

            // apply keystore
            ApplyKeystore(config.GetParam(Config.amAccount), config.GetParam(Config.amBundle));

            // check icons android or ios
            SetIcons(BuildTargetGroup.Android, Config.iconAM, 512);

            // min sdk version
            PlayerSettings.Android.minSdkVersion = Config.androidSdkVersion;
            // set Android Device Filter 
            PlayerSettings.Android.targetDevice = targetDevice;

            // disable Android TV
            PlayerSettings.Android.androidTVCompatibility = false;

            adsVersion = false;
            ApplyCommonSettings();

            SetBootLoader();

            SetCompanyInfoIntoResourses("Amazon");

            SetGDPRTexts("Amazon");

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Debug.Log("<color=green><b>Настройки Amazon применены</b></color>");
        }

        private static void SetAppId(string appID)
        {
            if (!string.IsNullOrEmpty(appID))
            {
                Debug.Log(appID);
                GBNEditorInfoFileBuilder.SetValue("info_sdk/sdk_appleappid", Crypt.GBNEncrypt(appID, encryptionKey));
            }
        }

        [MenuItem("Config/Применить настройки AppStore Free", false, 4)]
        public static void ApplyConfigASFreeDefault()
        {
            ConfigLoader.LoadConfig();
            ApplyConfigASFree("", "");
            // save all assets
            AssetDatabase.SaveAssets();
        }
        public static void ApplyConfigASFree(string bundleVersion, string buildVersion)
        {
            // apply title
            if (config.GetParam(Config.asTitleFree) != "")
            {
                PlayerSettings.productName = config.GetParam(Config.asTitleFree);
            }
            // apply account
            if (config.GetParam(Config.asAccountFree) != "")
            {
                PlayerSettings.companyName = config.GetParam(Config.asAccountFree);
            }
            // apply bundle
            if (config.GetParam(Config.asBundleFree) != "")
            {
#if UNITY_5_6_OR_NEWER
                PlayerSettings.applicationIdentifier = config.GetParam(Config.asBundleFree);
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, config.GetParam(Config.asBundleFree));
#else
                PlayerSettings.bundleIdentifier = config.GetParam(Config.asBundleFree);
#endif
            }
            // apply bundleVersion
            if (bundleVersion != "")
            {
                PlayerSettings.bundleVersion = bundleVersion;
            }
            else
            {
                PlayerSettings.bundleVersion = config.GetASVersion(true);
            }
            // apply buildVersion
            if (buildVersion != "")
            {
                PlayerSettings.iOS.buildNumber = buildVersion;
            }
            else
            {
                PlayerSettings.iOS.buildNumber = config.GetASVersion(true);
            }
            // apply Team ID
            var data = ConfigLoader.GetAppStoreAccount();
            if (data.Length >= 3)
            {
                PlayerSettings.iOS.appleDeveloperTeamID = data[1];
            }
            else
            {
                throw new Exception("App Store TeamID not found");
            }

            // check icons android or ios
            SetIcons(BuildTargetGroup.iOS, Config.iconASFree, 1024);

            // set scripting backend=IL2CPP
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);

            // set architecture = Universal
            // ARMv7 = 0
            // ARM64 = 1
            // Universal = 2
            PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 2);

            // Set target minimum iOS Version
            PlayerSettings.iOS.targetOSVersionString = Config.TargetIOSVersionString;

            adsVersion = true;
            ApplyCommonSettings();

            SetBootLoader();

            SetCoolTool();

            // flurry
            SetFlurryKey("Apple");

            // appodeal
            SetUnityAdsKey("Apple");

            // Init GameCenter
            SetGameCenterInit();

            SetAppId(config.GetParam(Config.asAppIDFree));

            SetCompanyInfoIntoResourses("Apple");

            SetGDPRTexts("Apple");

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Debug.Log("<color=green><b>Настройки App Store Free применены</b></color>");
        }

        [MenuItem("Config/Локализация/Включить локализацию")]
        public static void SetLocaleVersion()
        {
            localeVersion = true;
            SetScriptingDefineSymbols(new string[] { "LOCALE_VERSION" }, new bool[] { localeVersion });
        }

        [MenuItem("Config/Локализация/Выключить локализацию")]
        public static void SetNonLocaleVersion()
        {
            localeVersion = false;
            SetScriptingDefineSymbols(new string[] { "LOCALE_VERSION" }, new bool[] { localeVersion });
        }

        [MenuItem("Config/Mobile Input/Включить Mobile Input")]
        public static void SetMobileInput()
        {
            mobileInput = true;
            hasCrossPlatformInput = true;
            SetScriptingDefineSymbols(new string[] { "CROSS_PLATFORM_INPUT", "MOBILE_INPUT" }, new bool[] { hasCrossPlatformInput, hasCrossPlatformInput });
        }

        [MenuItem("Config/Mobile Input/Выключить Mobile Input")]
        public static void SetNonMobileInput()
        {
            mobileInput = false;
            hasCrossPlatformInput = false;
            SetScriptingDefineSymbols(new string[] { "CROSS_PLATFORM_INPUT", "MOBILE_INPUT" }, new bool[] { hasCrossPlatformInput, hasCrossPlatformInput });
        }

        [MenuItem("Config/Версия/Переключить на финальную версию")]
        public static void SetFinalVersion()
        {
            finalVersion = true;
            SetScriptingDefineSymbols(new string[] { "FINAL_VERSION" }, new bool[] { finalVersion });
        }

        [MenuItem("Config/Версия/Переключить на тестовую версию")]
        public static void SetTestVersion()
        {
            finalVersion = false;
            SetScriptingDefineSymbols(new string[] { "FINAL_VERSION" }, new bool[] { finalVersion });
        }

        public static void ApplyScriptingDefineSymbols(BuildTargetGroup buildTargetGroup, bool adsVersion, bool finalVersion)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            string adsFind = adsVersion ? "!ADS_VERSION" : "ADS_VERSION";
            string adsReplace = adsVersion ? "ADS_VERSION" : "!ADS_VERSION";
            string finalFind = finalVersion ? "!FINAL_VERSION" : "FINAL_VERSION";
            string finalReplace = finalVersion ? "FINAL_VERSION" : "!FINAL_VERSION";

            bool adsFound = false;
            bool finalFound = false;
            var list = new List<string>();
            foreach (var define in defines)
            {
                var newDefine = define;
                if (define == adsFind)
                {
                    newDefine = adsReplace;
                }
                else if (define == finalFind)
                {
                    newDefine = finalReplace;
                }
                if (newDefine == adsReplace)
                {
                    adsFound = true;
                }
                if (newDefine == finalReplace)
                {
                    finalFound = true;
                }
                list.Add(newDefine);
            }
            if (!adsFound)
            {
                list.Add(adsReplace);
            }
            if (!finalFound)
            {
                list.Add(finalReplace);
            }
            var newDefines = string.Join(";", list.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefines);
        }

        public static void SetScriptingDefineSymbolsForGroup(BuildTargetGroup buildTargetGroup, string[] defineNames, bool[] enable)
        {
            var defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
            for (var i = defines.Count - 1; i >= 0; i--)
            {
                if (defines[i].Contains("!"))
                {
                    defines.RemoveAt(i);
                }
            }
            for (var i = 0; i < defineNames.Length && i < enable.Length; i++)
            {
                var defineName = defineNames[i];
                if (enable[i])
                {
                    if (!defines.Contains(defineName))
                    {
                        defines.Add(defineName);
                    }
                }
                else
                {
                    if (defines.Contains(defineName))
                    {
                        while (defines.Contains(defineName))
                        {
                            defines.Remove(defineName);
                        }
                    }
                }
            }
            string definesString = string.Join(";", defines.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, definesString);
        }

        public static bool GetScriptingDefineSymbol(BuildTargetGroup buildTargetGroup, string defineName)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            if (defines.IndexOf("!" + defineName) >= 0)
            {
                return false;
            }
            if (defines.IndexOf(defineName) >= 0)
            {
                return true;
            }
            return false;
        }

        private static void SetCrossPlatformMobileInput(BuildTargetGroup buildTargetGroup, string defineName, bool enable)
        {
            var defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
            if (enable)
            {
                if (defines.Contains(defineName))
                {
                    return;
                }
                defines.Add(defineName);
            }
            else
            {
                if (!defines.Contains(defineName))
                {
                    return;
                }
                while (defines.Contains(defineName))
                {
                    defines.Remove(defineName);
                }
            }
            string definesString = string.Join(";", defines.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, definesString);

        }
        #region SDKManager
        private static void RemoveSdkManager()
        {
            var sdkManager = GameObject.Find("SDKManager");
            if (sdkManager != null)
            {
                GameObject.DestroyImmediate(sdkManager);
            }
        }
        #endregion
        #region BootLoader
        private static void RemoveBootLoader()
        {
            var bootLoader = GameObject.Find("BootLoader");
            if (bootLoader != null)
            {
                GameObject.DestroyImmediate(bootLoader);
            }
        }

        private static void SetBootLoader()
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            // set BootLoader on all scenes
            for (var i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                var scene = EditorSceneManager.OpenScene(EditorBuildSettings.scenes[i].path, OpenSceneMode.Single);
                EditorSceneManager.SetActiveScene(scene);

                RemoveBootLoader();

                if (config.GetParam(Config.additiveScenes).IndexOf(EditorBuildSettings.scenes[i].path) < 0)
                {
                    var bootLoader = new GameObject("BootLoader", typeof(BootLoader));
                }

                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }
            // set active scene 0
            if (EditorBuildSettings.scenes.Length > 0)
            {
                var scene = EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path, OpenSceneMode.Single);
                EditorSceneManager.SetActiveScene(scene);
            }
        }

        public static bool HasBootLoaderGameObject()
        {
            return GameObject.Find("BootLoader") != null;
        }
        #endregion
        #region CoolTool

        private static void RemoveCoolTool()
        {
            var coolTool = GameObject.Find("CoolTool");
            if (coolTool != null)
            {
                GameObject.DestroyImmediate(coolTool);
            }
            coolTool = GameObject.Find("CoolToolPrefab");
            if (coolTool != null)
            {
                GameObject.DestroyImmediate(coolTool);
            }
        }

        private static void SetCoolTool()
        {
            RemoveCoolTool();
            GameObject coolTool = new GameObject("CoolTool", typeof(CoolTool));

            var cooltoolKey = config.GetCoolToolKey();

            if (!string.IsNullOrEmpty(cooltoolKey))
            {
                GBNEditorInfoFileBuilder.SetValue("info_sdk/sdk_cooltool", Crypt.GBNEncrypt(cooltoolKey, encryptionKey));
            }
        }

        public static bool HasCoolToolGameObject()
        {
            return GameObject.Find("CoolTool") != null;
        }

        public static string GetCurrentCoolToolKey()
        {
            string current = GBNAPI.SDKInfo.GetKey("sdk_cooltool");
            if (string.IsNullOrEmpty(current))
            {
                return "";
            }
            else
            {
                return current;
            }
        }
        #endregion
        #region Hola
        private static void RemoveHola()
        {
            var hola = GameObject.Find("Hola");
            if (hola != null)
            {
                GameObject.DestroyImmediate(hola);
            }
        }

        public static bool HasHolaUI()
        {
            GameObject ui1 = FindObjectWithDisabled("HolaSettingsButton");
            GameObject ui2 = FindObjectWithDisabled("HolaSettingsBlock");

            //return ((ui1 != null && ui2 == null) || (ui1 == null && ui2 != null));
            return (ui1 != null) ^ (ui2 != null);
        }
        #endregion

        private static GameObject CustomFindInChild(GameObject parentGO, string nameToFind)
        {
            Transform transform = parentGO.transform;
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                var child = transform.GetChild(i);
                if (child.gameObject.name == nameToFind)
                {
                    return child.gameObject;
                }
                GameObject result = CustomFindInChild(child.gameObject, nameToFind);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private static GameObject FindObjectWithDisabled(string nameToFind)
        {
            List<GameObject> rootObjects = new List<GameObject>();
            Scene scene = SceneManager.GetActiveScene();
            scene.GetRootGameObjects(rootObjects);

            GameObject result = null;

            for (int i = 0; i < rootObjects.Count; ++i)
            {
                if (rootObjects[i].name == nameToFind)
                {
                    result = rootObjects[i];
                    break;
                }
                else
                {
                    result = CustomFindInChild(rootObjects[i], nameToFind);
                    if (result != null)
                    {
                        break;
                    }
                }
            }
            return result;
        }
        #region Huq
        private static void RemoveHuq()
        {
            var huq = GameObject.Find("Huq");
            if (huq != null)
            {
                GameObject.DestroyImmediate(huq);
            }
        }

        private static void SetHuqKey()
        {
            var huqKey = config.GetHuqKey();

            if (!string.IsNullOrEmpty(huqKey))
            {
                GBNEditorInfoFileBuilder.SetValue("info_sdk/sdk_huq", Crypt.GBNEncrypt(huqKey, encryptionKey));
            }
        }

        public static string GetHuqKey()
        {
            string key = GBNAPI.SDKInfo.GetKey("sdk_huq");
            if (!string.IsNullOrEmpty(key))
            {
                return key;
            }

            return "";
        }
#endregion
        #region Tutela
        private static void RemoveTutela()
        {
            var tutela = GameObject.Find("Tutela");
            if (tutela != null)
            {
                GameObject.DestroyImmediate(tutela);
            }
        }

        private static void SetTutelaKey()
        {
            var tutelaKey = config.GetTutelaKey();

            if (!string.IsNullOrEmpty(tutelaKey))
            {
                GBNEditorInfoFileBuilder.SetValue("info_sdk/sdk_tutela", Crypt.GBNEncrypt(tutelaKey, encryptionKey));
            }
        }

        public static string GetTutelaKey()
        {
            string key = GBNAPI.SDKInfo.GetKey("sdk_tutela");
            if (!string.IsNullOrEmpty(key))
            {
                return key;
            }

            return "";
        }
        #endregion
        #region Cuebiq
        private static void RemoveCuebiq()
        {
            var cuebiq = GameObject.Find("Cuebiq");
            if (cuebiq != null)
            {
                GameObject.DestroyImmediate(cuebiq);
            }
        }

        private static void SetCuebiqKey()
        {
            var cuebiqKey = config.GetCuebiqKey();

            if (!string.IsNullOrEmpty(cuebiqKey))
            {
                GBNEditorInfoFileBuilder.SetValue("info_sdk/sdk_cuebiq", Crypt.GBNEncrypt(cuebiqKey, encryptionKey));
            }
        }

        public static string GetCuebiqKey()
        {
            string key = GBNAPI.SDKInfo.GetKey("sdk_cuebiq");
            if (!string.IsNullOrEmpty(key))
            {
                return key;
            }

            return "";
        }
        #endregion
        #region OneAudience
        private static void RemoveOneAudience()
        {
            var oneAudience = GameObject.Find("OneAudience");
            if (oneAudience != null)
            {
                GameObject.DestroyImmediate(oneAudience);
            }
        }

        private static void SetOneAudienceKey(string store)
        {
            //valid store values are
            //"Amazon"
            //"Google"
            //"Apple"

            string key = "";

            if (store.Equals("Google"))
            {
                key = config.GetParam(Config.gpOneAudience);
            }
            else if (store.Equals("Apple"))
            {
                key = config.GetParam(Config.asOneAudience);
            }
            else if (store.Equals("Amazon"))
            {

            }

            if (!string.IsNullOrEmpty(key))
            {
                GBNEditorInfoFileBuilder.SetValue("info_sdk/sdk_oneaudience", Crypt.GBNEncrypt(key, encryptionKey));
            }
        }

        public static string GetOneOudienceKey()
        {
            string key = GBNAPI.SDKInfo.GetKey("sdk_oneaudience");
            if (!string.IsNullOrEmpty(key))
            {
                return key;
            }

            return "";
        }
        #endregion
        #region MobKnow
        private static void SetMobKnowKey()
        {
            var mobknowKey = config.GetParam(Config.gpMobKnow);//"bd680c26c362e66f02d14584b360d772";//com.gamingfever.deadcityzombiesafari

            if (!string.IsNullOrEmpty(mobknowKey))
            {
                GBNEditorInfoFileBuilder.SetValue("info_sdk/sdk_mobknow", Crypt.GBNEncrypt(mobknowKey, encryptionKey));
            }
        }

        public static string GetMobKnowKey()
        {
            string key = GBNAPI.SDKInfo.GetKey("sdk_mobknow");
            if (!string.IsNullOrEmpty(key))
            {
                return key;
            }

            return "";
        }
        #endregion
        #region Flurry
        public static bool HasFlurryPrefab()
        {
            return (AssetDatabase.LoadAssetAtPath("Assets/Analytics/Analytics.prefab", typeof(GameObject)) as GameObject) != null;
        }

        public static string GetCurrentFlurryKey()
        {
            string current = GBNAPI.SDKInfo.GetKey("sdk_flurrykey");
            if (string.IsNullOrEmpty(current))
            {
                return "";
            }
            else
            {
                return current;
            }
        }

        private static void RemoveFlurry()
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            // remove flurry from all scenes
            for (var i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                var scene = EditorSceneManager.OpenScene(EditorBuildSettings.scenes[i].path, OpenSceneMode.Single);
                EditorSceneManager.SetActiveScene(scene);
                // find and reomove instance of flurry
                var gbnFlurry = GameObject.Find("Analytics");
                if (gbnFlurry != null)
                {
                    GameObject.DestroyImmediate(gbnFlurry);
                }
                gbnFlurry = GameObject.Find("GBNFlurry");
                if (gbnFlurry != null)
                {
                    GameObject.DestroyImmediate(gbnFlurry);
                }
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }
            // set active scene 0
            if (EditorBuildSettings.scenes.Length > 0)
            {
                var scene = EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path, OpenSceneMode.Single);
                EditorSceneManager.SetActiveScene(scene);
            }
        }

        private static void SetFlurryKey(string store)
        {
            //valid store values are
            //"Amazon"
            //"Google"
            //"Apple"

            string key = "";

            if (store.Equals("Google"))
            {
                key = config.GetParam(Config.gpFlurry);
            }
            else if (store.Equals("Apple"))
            {
                key = config.GetParam(Config.asFlurry);
            }
            else if (store.Equals("Amazon"))
            {

            }

            if (!string.IsNullOrEmpty(key))
            {
                GBNEditorInfoFileBuilder.SetValue("info_sdk/sdk_flurrykey", Crypt.GBNEncrypt(key, encryptionKey));
            }
        }
        #endregion

        public static bool HasGbnHzInitPrefab()
        {
            return (AssetDatabase.LoadAssetAtPath("Assets/GBNHZ/GBNHZinit.prefab", typeof(GameObject)) as GameObject) != null;
        }
        public static bool HasGbnHzInitGameObject()
        {
            return GameObject.Find("GBNHZinit") != null;
        }

        public static string GetBalanceUrlKey()
        {
            string cloudAdsId = SDKInfo.GetKey("sdk_cloudadsid");
            if (!string.IsNullOrEmpty(cloudAdsId))
            {
                return cloudAdsId;
            }
            else
            {
                return "";
            }
        }

        private static void SetUnityAdsKey(string store)
        {
            //valid store values are
            //"Amazon"
            //"Google"
            //"Apple"

            string key = "";

            if (store.Equals("Google"))
            {
                key = config.GetParam(Config.gpUnityAdsKey);
            }
            else if (store.Equals("Apple"))
            {
                key = config.GetParam(Config.asUnityAdsKey);
            }
            else if (store.Equals("Amazon"))
            {

            }

            if (!string.IsNullOrEmpty(key))
            {
                GBNEditorInfoFileBuilder.SetValue("info_sdk/sdk_unityadskey", Crypt.GBNEncrypt(key, encryptionKey));
            }
        }

        private static void SetBalanceUrlKey()
        {
            string balanceUrlKey = config.GetParam(Config.balanceUrlKey);
            if (!string.IsNullOrEmpty(balanceUrlKey))
            {
                GBNEditorInfoFileBuilder.SetValue("info_sdk/sdk_cloudadsid", Crypt.GBNEncrypt(balanceUrlKey, encryptionKey));
            }
            else
            {
                Debug.LogWarning("Balance URL Key in Config.txt is empty! cloudadsid was not setted!");
            }
        }

        private static void SetGameCenterInit()
        {
            var gameCenterInit = GameObject.Find("GameCenterInit");
            if (gameCenterInit == null)
            {
                gameCenterInit = new GameObject("GameCenterInit");
                gameCenterInit.AddComponent<GameCenterInit>();
            }
        }

        private static void RemoveGameCenterInit()
        {
            var gameCenterInit = GameObject.Find("GameCenterInit");
            if (gameCenterInit != null)
            {
                GameObject.DestroyImmediate(gameCenterInit);
            }
        }

        private static void SetScenesBuildOrder()
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            RemoveMissingScenes();
            if (EditorBuildSettings.scenes.Length > 0)
            {
                var scene = EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path, OpenSceneMode.Single);
                EditorSceneManager.SetActiveScene(scene);
            }
            else
            {
                Debug.LogError("No scenes added in Build Settings");
            }
        }
        public static void RemoveMissingScenes()
        {
            // check scene pathes
            var scenePathes = new List<string>();
            for (var i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                if (File.Exists(EditorBuildSettings.scenes[i].path))
                {
                    scenePathes.Add(EditorBuildSettings.scenes[i].path);
                }
                else
                {
                    Debug.Log("Found missing scene: " + EditorBuildSettings.scenes[i].path);
                }
            }
            // remove missing scenes
            if (scenePathes.Count < EditorBuildSettings.scenes.Length)
            {
                EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[scenePathes.Count];
                for (var i = 0; i < scenePathes.Count; i++)
                {
                    scenes[i] = new EditorBuildSettingsScene(scenePathes[i], true);
                }
                EditorBuildSettings.scenes = scenes;
                Debug.Log("Missing scenes was removed from Build Settings");
                //AssetDatabase.SaveAssets();
            }
        }
    }
}
