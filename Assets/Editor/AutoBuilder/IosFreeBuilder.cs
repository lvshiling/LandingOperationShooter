using Assets.Editor.ConfigLoader;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

public class IosFreeBuilder : AbstractBuilder
{
    #region override section
    override protected void Init()
    {
        base.Init();
        Debug.Log("IosFreeBuilder");
        buildTarget = BuildTarget.iOS;
        adsVersion = true;
        dependencies = "iOS;ADS_VERSION";
        increaseBundleVersionCode = false;
    }

    override protected bool CheckIcons()
    {
        if (!File.Exists(Config.iconASFree))
        {
            Debug.Log("ERROR: Icon " + Config.iconASFree + " does not exist!");
            return false;
        }
        return true;
    }

    override protected bool CheckTitles()
    {
        var res = true;
        // bundle
        if (!ConfigLoader.IsCorrectBundle(ConfigLoader.config.GetParam(Config.asBundleFree)))
        {
            res = false;
            Debug.Log("ERROR: AppStore Free Bundle is Empty or Wrong Format");
        }
        // account
        if (ConfigLoader.config.GetParam(Config.asAccountFree) == "")
        {
            res = false;
            Debug.Log("ERROR: AppStore Account is Empty");
        }
        // title
        if (!ConfigLoader.IsCorrectTitle(ConfigLoader.config.GetParam(Config.asTitleFree)))
        {
            res = false;
            Debug.Log("ERROR: AppStore Title is Empty or Wrong format");
        }
        // app id
        if (!ConfigLoader.IsCorrectIntId(ConfigLoader.config.GetParam(Config.asAppIDFree)))
        {
            res = false;
            Debug.Log("ERROR: AppStore App ID is Empty or Wrong format");
        }
        return res;
    }

    override protected bool CheckKeys()
    {
        var res = true;
        // flurry
        if (ConfigLoader.config.GetParam(Config.asFlurry) == "")
        {
            res = false;
            Debug.Log("ERROR: AppStore Free Flurry Key is Empty");
        }
        // check unityads key
        if (!ConfigLoader.IsValidUnityAdsKey(ConfigLoader.config.GetParam(Config.asUnityAdsKey)))
        {
            res = false;
            Debug.Log("ERROR: AppStore Free Appodeal Key is Empty or Wrong Format");
        }
        return res;
    }

    override protected void SetBundleVersion()
    {
        overrideVersion = GetArg("-overrideVersion") == "true" ? true : false;
        Debug.Log("OK. overrideVersion: " + overrideVersion);
        if (overrideVersion)
        {
            PlayerSettings.bundleVersion = GetArg("-bundleVersion");
            PlayerSettings.iOS.buildNumber = GetArg("-buildVersion");
        }
        else
        {
            PlayerSettings.bundleVersion = ConfigLoader.config.GetASVersion(true);
            PlayerSettings.iOS.buildNumber = ConfigLoader.config.GetASVersion(true);
        }
        Debug.Log("OK. bundleVersion: " + PlayerSettings.bundleVersion);
        Debug.Log("OK. buildVersion: " + PlayerSettings.iOS.buildNumber);
    }

    override protected void ApplyPlatformSettings()
    {
        ConfigSetter.ApplyConfigASFree(PlayerSettings.bundleVersion, PlayerSettings.iOS.buildNumber);
        ConfigSetter.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, new string[] { "XCODE_8" }, new bool[] { true });
    }

    override protected void LogDefines()
    {
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
        Debug.Log("Defines symbols: " + defines);
    }

    protected override bool isAssetsRefreshed()
    {
        return true;
    }

    public override void PrepareForFastBuild(bool finalVersion)
    {
        base.PrepareForFastBuild(finalVersion);
        CheckPackageStructure.RemoveWithDependencies("AppStoreFree");
        PlayerSettings.iOS.appleEnableAutomaticSigning = false;
        Quit();
    }

    public override void FastBuild()
    {
        status = BUILD_STATUS_READY_TO_BUILD;
        base.FastBuild();
    }

    protected override void PreBuildOperations()
    {
        CheckPackageStructure.RemoveWithDependencies("AppStoreFree");
        PlayerSettings.iOS.appleEnableAutomaticSigning = false;
        status = BUILD_STATUS_READY_TO_BUILD;
    }

    override protected void PostBuildOperations()
    {
        // edit Info.plist
        EditInfoPlist();
        // edit Unity-iPhone.xcodeproj/project.pbxproj

        AddAppstoreIcon();

        EditXcodeProject();
#if UNITY_IOS
        ModifyXcodeProject();//верифицирует проект.
#endif
        var data = ConfigLoader.GetAppStoreAccount();
        var teamId = "";
        var devTeamId = "";
        if (data.Length >= 3)
        {
            teamId = data[1];
            devTeamId = data[2];
        }
        else
        {
            throw new Exception("App Store TeamID not found");
        }

        CreateExportOptions(GetPaltformOutputLocation() + "/developmentExportOptions.plist", "development", teamId, ConfigLoader.config.GetParam(Config.asBundleFree), ConfigLoader.config.GetParam(Config.asBundleFree) + " Development");
        CreateExportOptions(GetPaltformOutputLocation() + "/distributionExportOptions.plist", "app-store", teamId, ConfigLoader.config.GetParam(Config.asBundleFree), ConfigLoader.config.GetParam(Config.asBundleFree) + " Distribution");
        //"${BUNDLE_ID} Development"
        //"${BUNDLE_ID} Distribution"

        GenerateIpaNames(GetPaltformOutputLocation() + "/ipa.names", teamId, devTeamId);
    }

    void AddAppstoreIcon()
    {
        bool iconNameAdded = false;

        if (File.Exists(Config.iconASFree))
        {
            string contentsJsonPath = GetPaltformOutputLocation() + "/Unity-iPhone/Images.xcassets/AppIcon.appiconset/Contents.json";

            string iconName = "Icon-Store.png";

            if (File.Exists(GetPaltformOutputLocation() + "/Unity-iPhone/Images.xcassets/AppIcon.appiconset/" + iconName))
            {
                Debug.Log("\"Icon-Store.png\" (\"ios-marketing\") icon is already exists!");
                return;
            }

            if (File.Exists(contentsJsonPath))
            {
                JSONObject contentsJson = new JSONObject(File.ReadAllText(contentsJsonPath));
                if (contentsJson != null && contentsJson.IsObject && contentsJson.HasField("images"))
                {
                    JSONObject imagesArray = contentsJson.GetField("images");
                    if (imagesArray.IsArray)
                    {
                        for (int i = 0; i < imagesArray.Count; ++i)
                        {
                            if (imagesArray[i].HasField("idiom") && imagesArray[i].GetField("idiom").str.Equals("ios-marketing"))
                            {
                                if (!imagesArray[i].HasField("filename"))
                                {
                                    File.Copy(Config.iconASFree, GetPaltformOutputLocation() + "/Unity-iPhone/Images.xcassets/AppIcon.appiconset/" + iconName, true);
                                    imagesArray[i].AddField("filename", iconName);
                                    File.WriteAllText(contentsJsonPath, contentsJson.ToString());
                                    iconNameAdded = true;
                                    break;
                                }
                            }
                        }
                        if (!iconNameAdded)
                        {
                            /*
                                "size" : "1024x1024",
                                "idiom" : "ios-marketing",
                                "filename" : "Icon-1024.png",
                                "scale" : "1x"
                             */
                            File.Copy(Config.iconASFree, GetPaltformOutputLocation() + "/Unity-iPhone/Images.xcassets/AppIcon.appiconset/" + iconName, true);
                            JSONObject iosMarketingField = new JSONObject();
                            iosMarketingField.AddField("size", "1024x1024");
                            iosMarketingField.AddField("idiom", "ios-marketing");
                            iosMarketingField.AddField("filename", iconName);
                            iosMarketingField.AddField("scale", "1x");
                            imagesArray.Add(iosMarketingField);
                            File.WriteAllText(contentsJsonPath, contentsJson.ToString());
                            iconNameAdded = true;
                        }
                    }
                }
            }
        }
        if (iconNameAdded)
        {
            Debug.Log("OK. \"ios-marketing\" icon is added succesfully!");
        }
        else
        {
            Debug.Log("\"ios-marketing\" icon was not fixed!");
        }
    }

#if UNITY_IOS
    protected static PlistDocument MergePlist(PlistDocument main, PlistDocument addon)
    {
        IDictionary<string, PlistElement> plistDict = main.root.values;
        IDictionary<string, PlistElement> addonDict = addon.root.values;

        foreach (string key in addonDict.Keys)
        {
            if (plistDict.ContainsKey(key))
            {
                PlistElement oldValue = null;
                plistDict.TryGetValue(key, out oldValue);

                PlistElement newValue = null;
                addonDict.TryGetValue(key, out newValue);

                if (oldValue != null && newValue != null)
                {
                    if (oldValue.GetType() == newValue.GetType())
                    {
                        if (oldValue.GetType() == typeof(PlistElementString) || oldValue.GetType() == typeof(PlistElementInteger) || oldValue.GetType() == typeof(PlistElementBoolean))
                        {
                            plistDict[key] = newValue;
                        }
                        else if (oldValue.GetType() == typeof(PlistElementArray))
                        {
                            List<PlistElement> oldArray = oldValue.AsArray().values;
                            List<PlistElement> newArray = newValue.AsArray().values;

                            foreach (PlistElement e in newArray)
                            {
                                if (e.GetType() == typeof(PlistElementString))
                                {
                                    if (oldArray.FindIndex(x => x.AsString() == e.AsString()) < 0)
                                    {
                                        oldArray.Add(e);
                                    }
                                }
                                else if (e.GetType() == typeof(PlistElementInteger))
                                {
                                    if (oldArray.FindIndex(x => x.AsInteger() == e.AsInteger()) < 0)
                                    {
                                        oldArray.Add(e);
                                    }
                                }
                                else if (e.GetType() == typeof(PlistElementBoolean))
                                {
                                    if (oldArray.FindIndex(x => x.AsBoolean() == e.AsBoolean()) < 0)
                                    {
                                        oldArray.Add(e);
                                    }
                                }
                            }
                        }
                        else if (oldValue.GetType() == typeof(PlistElementDict))
                        {
                            IDictionary<string, PlistElement> oldDict = oldValue.AsDict().values;
                            IDictionary<string, PlistElement> newDict = newValue.AsDict().values;

                            foreach (string k in newDict.Keys)
                            {
                                if (!oldDict.ContainsKey(k))
                                {
                                    PlistElement val = null;
                                    newDict.TryGetValue(key, out val);
                                    oldDict.Add(k, val);
                                }
                                else
                                {
                                    PlistElement val = null;
                                    newDict.TryGetValue(key, out val);
                                    oldDict[k] = val;
                                    //не мержит вложенные PlistElement, просто заменяет (критично, если в Dict-е есть Array, но не уверен, что таке встречается).
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                PlistElement value = null;
                if (addonDict.TryGetValue(key, out value))
                {
                    plistDict.Add(key, value);
                }
            }
        }

        return main;
    }

    protected void OneAudiencePlistAddon()
    {
        if (!string.IsNullOrEmpty(GBNAPI.SDKInfo.GetKey("sdk_oneaudience")))
        {
            string infoPlistPath = GetPaltformOutputLocation() + "/Info.plist";
            string oaSchemesPath = ConfigLoader.projectPath + "/Assets/Plugins/iOS/OneAudience/iOS_Schemes.txt";
            if (File.Exists(infoPlistPath) && File.Exists(oaSchemesPath))
            {
                File.SetAttributes(infoPlistPath, FileAttributes.Normal);

                PlistDocument plist = new PlistDocument();
                plist.ReadFromFile(infoPlistPath);

                PlistDocument addon = new PlistDocument();
                addon.ReadFromString("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<plist version=\"1.0\">\n<dict>\n" + File.ReadAllText(oaSchemesPath) + "\n</dict>\n</plist>");

                plist = MergePlist(plist, addon);

                plist.WriteToFile(infoPlistPath);
            }
        }
    }

    protected void BackgroundModesPlistAddod(
        bool audio,
        bool bluetoothCentral,
        bool bluetoothPeripheral,
        bool externalAccessory,
        bool fetch,
        bool location,
        bool newsstandContent,
        bool remoteNotification,
        bool voip
    )
    {
        if (!(audio || bluetoothCentral || bluetoothPeripheral || externalAccessory || fetch || location || newsstandContent || remoteNotification || voip))
        {
            return;
        }
        string infoPlistPath = GetPaltformOutputLocation() + "/Info.plist";
        if (File.Exists(infoPlistPath))
        {
            File.SetAttributes(infoPlistPath, FileAttributes.Normal);

            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(infoPlistPath);

            string addonString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<plist version=\"1.0\">\n<dict>\n" + "<key>UIBackgroundModes</key>\n<array>\n";

            if (audio)
                addonString += "<string>audio</string>\n";
            if (bluetoothCentral)
                addonString += "<string>bluetooth-central</string>";
            if (bluetoothPeripheral)
                addonString += "<string>bluetooth-peripheral</string>\n";
            if (externalAccessory)
                addonString += "<string>external-accessory</string>\n";
            if (fetch)
                addonString += "<string>fetch</string>\n";
            if (location)
                addonString += "<string>location</string>\n";
            if (newsstandContent)
                addonString += "<string>newsstand-content</string>\n";
            if (remoteNotification)
                addonString += "<string>remote-notification</string>\n";
            if (voip)
                addonString += "<string>voip</string>\n";

            addonString += "</array>" + "\n</dict>\n</plist>";

            PlistDocument addon = new PlistDocument();
            addon.ReadFromString(addonString);

            plist = MergePlist(plist, addon);

            plist.WriteToFile(infoPlistPath);
        }
    }

    protected void CalendarUsagePlistAddod()
    {
        string infoPlistPath = GetPaltformOutputLocation() + "/Info.plist";
        if (File.Exists(infoPlistPath))
        {
            File.SetAttributes(infoPlistPath, FileAttributes.Normal);

            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(infoPlistPath);

            PlistDocument addon = new PlistDocument();
            addon.ReadFromString("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<plist version=\"1.0\">\n<dict>\n" +
                "<key>NSCalendarsUsageDescription</key>\n<string>Your calendar data will help us to display more relevant content or ads for you.</string>" +
                "\n</dict>\n</plist>");

            plist = MergePlist(plist, addon);

            plist.WriteToFile(infoPlistPath);
        }
    }

    protected void LocationWhenInUseUsagePlistAddod()
    {
        string infoPlistPath = GetPaltformOutputLocation() + "/Info.plist";
        if (File.Exists(infoPlistPath))
        {
            File.SetAttributes(infoPlistPath, FileAttributes.Normal);

            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(infoPlistPath);

            PlistDocument addon = new PlistDocument();
            addon.ReadFromString("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<plist version=\"1.0\">\n<dict>\n" +
                "<key>NSLocationWhenInUseUsageDescription</key>\n<string>Your location data will help us to display more relevant content or ads for you.</string>" +
                "\n</dict>\n</plist>");

            plist = MergePlist(plist, addon);

            plist.WriteToFile(infoPlistPath);
        }
    }

    protected void BluetoothPeripheralUsagePlistAddod()
    {
        string infoPlistPath = GetPaltformOutputLocation() + "/Info.plist";
        if (File.Exists(infoPlistPath))
        {
            File.SetAttributes(infoPlistPath, FileAttributes.Normal);

            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(infoPlistPath);

            PlistDocument addon = new PlistDocument();
            addon.ReadFromString("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<plist version=\"1.0\">\n<dict>\n" +
                "<key>NSBluetoothPeripheralUsageDescription</key>\n<string>Your bluetooth data will help us to display more relevant content or ads for you.</string>" +
                "\n</dict>\n</plist>");

            plist = MergePlist(plist, addon);

            plist.WriteToFile(infoPlistPath);
        }
    }

    protected void PhotoLibraryUsagePlistAddod()
    {
        string infoPlistPath = GetPaltformOutputLocation() + "/Info.plist";
        if (File.Exists(infoPlistPath))
        {
            File.SetAttributes(infoPlistPath, FileAttributes.Normal);

            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(infoPlistPath);

            PlistDocument addon = new PlistDocument();
            addon.ReadFromString("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<plist version=\"1.0\">\n<dict>\n" +
                "<key>NSPhotoLibraryUsageDescription</key>\n<string>Advertising</string>" +
                "\n</dict>\n</plist>");

            plist = MergePlist(plist, addon);

            plist.WriteToFile(infoPlistPath);
        }
    }

    protected void AdColonyUrlSchemesPlistAddod()
    {
        string infoPlistPath = GetPaltformOutputLocation() + "/Info.plist";
        if (File.Exists(infoPlistPath))
        {
            File.SetAttributes(infoPlistPath, FileAttributes.Normal);

            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(infoPlistPath);

            PlistDocument addon = new PlistDocument();
            addon.ReadFromString("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<plist version=\"1.0\">\n<dict>\n" +
                "<key>LSApplicationQueriesSchemes</key>\n<array>\n<string>fb</string>\n<string>instagram</string>\n<string>tumblr</string>\n<string>twitter</string>\n</array>" +
                "\n</dict>\n</plist>");

            plist = MergePlist(plist, addon);

            plist.WriteToFile(infoPlistPath);
        }
    }

    protected void TapjoyPreCachingPlistAddod() //don't work with current MergePlist()
    {
        string infoPlistPath = GetPaltformOutputLocation() + "/Info.plist";
        if (File.Exists(infoPlistPath))
        {
            File.SetAttributes(infoPlistPath, FileAttributes.Normal);

            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(infoPlistPath);

            PlistDocument addon = new PlistDocument();
            addon.ReadFromString("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<plist version=\"1.0\">\n<dict>\n" +
                "<key>NSAppTransportSecurity</key>\n<dict>\n<key>NSExceptionDomains</key>\n<dict>\n<key>localhost</key>\n<dict>\n<key>NSExceptionAllowsInsecureHTTPLoads</key>\n<true/>\n</dict>\n</dict>\n</dict>" +
                "\n</dict>\n</plist>");

            plist = MergePlist(plist, addon);

            plist.WriteToFile(infoPlistPath);
        }
    }

    protected void ModifyXcodeProject()
    {
        string filePath = GetPaltformOutputLocation() + "/Unity-iPhone.xcodeproj/project.pbxproj";

        if (File.Exists(filePath))
        {
            File.SetAttributes(filePath, FileAttributes.Normal);

            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(filePath);

            string target = pbxProject.TargetGuidByName("Unity-iPhone");

            //other stuff if needed

            File.WriteAllText(filePath, pbxProject.WriteToString());
        }
        else
        {
            Debug.Log("PBXProject not found at path "+filePath);
        }
    }
#endif
    override protected string GenerateExtraBundle()
    {
        return ConfigLoader.config.GetParam(Config.asBundleFree) + "_" + PlayerSettings.bundleVersion + "_AdsVersion-" + Config.version + "_" + GetDate();
    }

    override protected string GetPaltformOutputLocation()
    {
        return "Builds/AppStoreFree";
    }

    override protected void BuildLauncher(string[] buildScenes, string locationPathName)
    {
        // BUILD
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = buildScenes;
        buildPlayerOptions.locationPathName = locationPathName;
        buildPlayerOptions.target = buildTarget;
        buildPlayerOptions.options = BuildOptions.None;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

    protected override string GetPlatformOutputPath()
    {
        return GetPaltformOutputLocation();
    }

    protected override bool CheckKeystores()
    {
        return true;
    }
#endregion

#region private section
    private void GenerateIpaNames(string filePath, string teamId, string devTeamId)
    {
        var names = new string[5];
        var extraBundle = GenerateExtraBundle();
        names[0] = "DEVELOPMENT_IPA_NAME=" + extraBundle + "_dev.ipa";
        names[1] = "DISTRIBUTION_IPA_NAME=" + extraBundle + "_dist.ipa";
        names[2] = "TEAM_ID=" + teamId;
        names[3] = "DEV_TEAM_ID=" + devTeamId;
        names[4] = "BUNDLE_ID=" + ConfigLoader.config.GetParam(Config.asBundleFree);

        File.WriteAllLines(filePath, names);
    }

    private void CreateExportOptions(string filePath, string method, string teamId, string bundleId = "", string profileName = "")
    {
        string template = 
            "<!DOCTYPE plist PUBLIC \" -//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\"><plist version = \"1.0\">" + "\n" +
            "<dict>" + "\n";

        template +=
            "<key>method</key><string>METHOD_NAME</string>" + "\n" +
            "<key>teamID</key><string>TEAM_ID</string>" + "\n";

        if (!string.IsNullOrEmpty(bundleId) && !string.IsNullOrEmpty(profileName))
        {
            template +=
                "<key>provisioningProfiles</key>" + "\n" +
                "<dict>" + "\n" +
                "<key>BUNDLE_ID</key>" + "\n" +
                "<string>PROFILE_NAME</string>" + "\n" +
                "</dict>" + "\n";
        }

        template +=
            "</dict>" + "\n" +
            "</plist>";

        template = template.Replace("METHOD_NAME", method);
        template = template.Replace("TEAM_ID", teamId);
        template = template.Replace("BUNDLE_ID", bundleId);
        template = template.Replace("PROFILE_NAME", profileName);

        File.WriteAllText(filePath, template);

    }

    private void EditXcodeProject()
    {
        var filePath = GetPaltformOutputLocation() + "/Unity-iPhone.xcodeproj/project.pbxproj";
        if (File.Exists(filePath))
        {
            File.SetAttributes(filePath, FileAttributes.Normal);
            var lines = File.ReadAllLines(filePath);
            var list = new List<string>();
            for (var i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Replace("ENABLE_BITCODE = YES;", "ENABLE_BITCODE = NO;");
                if (lines[i].IndexOf("LaunchScreen") >= 0) continue;
                if (lines[i].IndexOf("$(OTHER_LDFLAGS)") >= 0) continue;
                if (lines[i].IndexOf("-all_load") >= 0) continue;
                if (lines[i].IndexOf("SystemCapabilities = {") >= 0)
                {
                    list.Add(lines[i]);
                    list.Add("com.apple.Push = {");
                    list.Add("enabled = 1;");
                    list.Add("};");
                    continue;
                }
                list.Add(lines[i]);
            }
            File.WriteAllLines(filePath, list.ToArray());
            Debug.Log("OK. Xcode project \"" + filePath + "\" fixed!");
        }
        else
        {
            Debug.Log("Error: Xcode project file " + filePath + " does not exist!");
            ExitWithException();
        }
    }

    private void EditInfoPlist()
    {
        var filePath = GetPaltformOutputLocation() + "/Info.plist";
        if (File.Exists(filePath))
        {
            File.SetAttributes(filePath, FileAttributes.Normal);
            var lines = File.ReadAllLines(GetPaltformOutputLocation() + "/Info.plist");
            lines = RemoveLaunchScreens(lines);
            lines = InjectFragment(lines);
            lines = InjectGamekit(lines);
            File.WriteAllLines(filePath, lines);
#if UNITY_IOS
            //OneAudiencePlistAddon();
            //CalendarUsagePlistAddod();
            //LocationWhenInUseUsagePlistAddod();//отключено из за реджектов
            //BackgroundModesPlistAddod(false, false, false, false, false, false, false, true, false);
#endif
            Debug.Log("OK. \"" + filePath + "\" fixed!");
        }
        else
        {
            Debug.Log("Error: File " + filePath + " does not exist!");
            ExitWithException();
        }

    }

    private string[] RemoveLaunchScreens(string[] lines)
    {
        var launchscreens = new string[] {
            "<key>UILaunchStoryboardName~ipad</key>",
            "<string>LaunchScreen-iPad</string>",
            "<key>UILaunchStoryboardName~iphone</key>",
            "<string>LaunchScreen-iPhone</string>",
            "<key>UILaunchStoryboardName~ipod</key>"
        };

        for (var i = 0; i < lines.Length; i++)
        {
            for (var j = 0; j < launchscreens.Length; j++)
            {
                lines[i] = lines[i].Replace(launchscreens[j], "");
            }
        }
        return lines;
    }

    private string[] InjectFragment(string[] lines)
    {
        var fragment1 = "<key>NSAppTransportSecurity</key><dict><key>NSAllowsArbitraryLoads</key><true/></dict>";
        //var fragment2 = "<key>LSApplicationQueriesSchemes</key><array><string>fb</string><string>instagram</string><string>tumblr</string><string>twitter</string></array>";

        var list = new List<string>();
        for (var i = 0; i < lines.Length; i++)
        {
            if (i < lines.Length - 1 && lines[i].IndexOf("</dict>") >= 0 && lines[i + 1].IndexOf("</plist>") >= 0)
            {
                list.Add(fragment1);
                // list.Add(fragment2);
            }
            list.Add(lines[i]);
        }
        return list.ToArray();
    }

    private string[] InjectGamekit(string[] lines)
    {
        var gamekit = new string[] {
            "<key>UIRequiredDeviceCapabilities</key>",
            "<array>",
            "<string>gamekit</string>"
        };
        bool isGamekitFounded = false;
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].IndexOf(gamekit[2]) >= 0)
            {
                isGamekitFounded = true;
                break;
            }
        }
        if (isGamekitFounded) return lines;

        var list = new List<string>();
        for (var i = 0; i < lines.Length; i++)
        {
            list.Add(lines[i]);
            if (i > 2 && lines[i].IndexOf(gamekit[1]) >= 0 && lines[i - 1].IndexOf(gamekit[0]) >= 0)
            {
                list.Add(gamekit[2]);
            }
        }
        return list.ToArray();
    }
#endregion
}
