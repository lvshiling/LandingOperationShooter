using Assets.Editor.ConfigLoader;
using System.IO;
using UnityEditor;
using UnityEngine;
using System;

public class GooglePlayBuilder : AbstractBuilder
{
    #region android properties
    protected string apkName
    {
        get
        {
            return EditorPrefs.GetString("apkName", "");
        }
        set
        {
            EditorPrefs.SetString("apkName", value);
        }
    }

    protected bool isExport
    {
        get
        {
            return EditorPrefs.GetInt("isExport", 0) == 1;
        }
        set
        {
            EditorPrefs.SetInt("isExport", value ? 1 : 0);
        }
    }
    // Wait For Fyber refresh assets completion
    protected int MAX_WAIT_ITERATIONS = 1000;
    #endregion

    #region override section
    override protected void Init()
    {
        base.Init();
        buildTarget = BuildTarget.Android;
        adsVersion = true;
        dependencies = "Android;ADS_VERSION";
        apkName = "";
        isExport = true;
    }

    override protected void ExitWithException()
    {
        EditorPrefs.SetInt("WAIT_ITERATIONS", 0);
        base.ExitWithException();
    }

    override protected void SetBundleVersion()
    {
        overrideVersion = GetArg("-overrideVersion") == "true" ? true : false;
        Debug.Log("OK. overrideVersion: " + overrideVersion);
        if (overrideVersion)
        {
            PlayerSettings.bundleVersion = GetArg("-bundleVersion");
            PlayerSettings.Android.bundleVersionCode = int.Parse(GetArg("-bundleVersionCode"));
        }
        else
        {
            PlayerSettings.bundleVersion = ConfigLoader.config.GetGPVersion();
            PlayerSettings.Android.bundleVersionCode = ConfigLoader.config.GetGPCode();
            if (PlayerSettings.Android.bundleVersionCode <= 0)
            {
                PlayerSettings.Android.bundleVersionCode = 1;
            }
        }
        if (increaseBundleVersionCode)
        {
            PlayerSettings.Android.bundleVersionCode++;
        }
        Debug.Log("OK. bundleVersion: " + PlayerSettings.bundleVersion);
        Debug.Log("OK. bundleVersionCode: " + PlayerSettings.Android.bundleVersionCode);
    }

    override protected void LogDefines()
    {
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
        Debug.Log("Defines symbols: " + defines);
    }

    public override void FastBuild()
    {
        base.FastBuild();
        status = BUILD_STATUS_WAIT_FOR_BUILD;
        EditorPrefs.SetInt("WAIT_ITERATIONS", 0);
    }

    protected override void PreBuildOperations()
    {
        status = BUILD_STATUS_WAIT_FOR_BUILD;
        EditorPrefs.SetInt("WAIT_ITERATIONS", 0);
    }
    
    override protected void BuildLauncher(string[] buildScenes, string locationPathName)
    {
        // BUILD
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = buildScenes;
        buildPlayerOptions.locationPathName = locationPathName;
        buildPlayerOptions.target = buildTarget;
        if (isExport)
        {
            // Export project
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
            EditorUserBuildSettings.development = false;
            buildPlayerOptions.options = BuildOptions.AcceptExternalModificationsToPlayer;
        }
        else
        {
            // Build APK
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            if (finalVersion)
            {
                buildPlayerOptions.options = BuildOptions.None;
                EditorUserBuildSettings.development = false;
            }
            else
            {
                buildPlayerOptions.options = BuildOptions.Development;
                EditorUserBuildSettings.development = true;
            }
        }
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

    override protected void PostBuildOperations()
    {
        if (isExport)
        {
            ProcessGradleProject();
        }
        SaveOutputLocation(GetPlatformOutputPath() + "/apkname.txt");
        SaveApkSettings(GetPlatformOutputPath() + "/apksettings.json");
    }

    override protected bool isAssetsRefreshed()
    {
        return true;
    }

    override protected bool CheckIcons()
    {
        if (!File.Exists(Config.iconGP))
        {
            Debug.Log("ERROR: Icon " + Config.iconGP + " does not exist!");
            return false;
        }
        return true;
    }

    override protected bool CheckKeystores()
    {
        if (!Directory.Exists(Config.keystoresFolder))
        {
            Debug.Log("ERROR: \"" + Config.keystoresFolder + "\" does not exist!");
            return false;
        }
        if (!File.Exists(Config.keystoresFolder + "/" + Config.keystoresList))
        {
            Debug.Log("ERROR: \"" + Config.keystoresFolder + "/" + Config.keystoresList + "\" does not exist!");
            return false;
        }
        return true;
    }

    override protected bool CheckTitles()
    {
        var res = true;
        // bundle
        if (!ConfigLoader.IsCorrectBundle(ConfigLoader.config.GetParam(Config.gpBundle)))
        {
            res = false;
            Debug.Log("ERROR: GooglePlay Bundle is Empty or Wrong Format");
        }
        // account
        if (ConfigLoader.config.GetParam(Config.gpAccount) == "")
        {
            res = false;
            Debug.Log("ERROR: GooglePlay Account is Empty");
        }
        // title
        if (!ConfigLoader.IsCorrectTitle(ConfigLoader.config.GetParam(Config.gpTitle)))
        {
            res = false;
            Debug.Log("ERROR: GooglePlay Title is Empty or Wrong format");
        }
        return res;
    }

    override protected bool CheckKeys()
    {
        var res = true;
        // flurry
        if (ConfigLoader.config.GetParam(Config.gpFlurry) == "")
        {
            res = false;
            Debug.Log("ERROR: GooglePlay Flurry Key is Empty");
        }
        // check unityads key
        if (!ConfigLoader.IsValidUnityAdsKey(ConfigLoader.config.GetParam(Config.gpUnityAdsKey)))
        {
            res = false;
            Debug.Log("ERROR: GooglePlay UnityAds Key is Empty or Wrong Format");
        }
        return res;
    }

    override protected void ApplyPlatformSettings()
    {
        ConfigSetter.ApplyConfigGP(PlayerSettings.bundleVersion, PlayerSettings.Android.bundleVersionCode, targetDevice);
    }

    override protected string GetPlatformOutputPath()
    {
        return "Builds/GooglePlay";
    }

    override protected string GetPaltformOutputLocation()
    {
        if (outputlocation == "")
        {
            outputlocation = GetPlatformOutputPath() + "/" + GenerateExtraBundle();
        }
        if (isExport)
        {
            return GetPlatformOutputPath() + "/ExportProject";
        }
        return outputlocation;
    }

    override protected string GenerateExtraBundle()
    {
        if (apkName == "")
        {
            apkName = ConfigLoader.config.GetParam(Config.gpBundle) + "_" + PlayerSettings.bundleVersion + "_" + PlayerSettings.Android.bundleVersionCode + "_AdsVersion-" + Config.version + "_" + GetDate() + archTag + (finalVersion ? "" : "_test") + ".apk";
            //GetPlatformPackedOutputLocation() зависит от формата apkName!
        }
        return apkName;
    }
    #endregion

    #region private section
    protected void SaveOutputLocation(string filePath)
    {
        if (isExport)
        {
            var names = new string[3];
            var projectName = ConfigLoader.config.GetParam(Config.gpTitle);
            var gradleRoot = GetPaltformOutputLocation() + "/" + projectName;
            names[0] = "APK_NAME=" + GenerateExtraBundle();
            names[1] = "GRADLE_ROOT=\"" + gradleRoot + "\"";
            names[2] = "GRADLE_APK=\"" + gradleRoot + "/build/outputs/apk/" + projectName + "-release.apk\"";
            File.WriteAllLines(filePath, names);
        }
        else
        {
            var names = new string[1];
            names[0] = "APK_NAME=" + GenerateExtraBundle();
            File.WriteAllLines(filePath, names);
        }
    }

    protected void SaveApkSettings(string filePath)
    {
        JSONObject json = new JSONObject();
        json.SetField("inputApkPath", GetProjectPath() + "/" + GetPlatformOutputPath() + "/" + GenerateExtraBundle());
        json.SetField("outputApkPath", GetProjectPath() + "/" + GetPlatformPackedOutputLocation());
        json.SetField("keystorePath", GetProjectPath() + "/" + PlayerSettings.Android.keystoreName);
        json.SetField("keystorePass", PlayerSettings.Android.keystorePass);
        json.SetField("keyaliasName", PlayerSettings.Android.keyaliasName);

        json.SetField("iconPath", GetProjectPath() + "/" + Config.iconGP);
        json.SetField("title", ConfigLoader.config.GetParam(Config.gpTitle));
        json.SetField("package", ConfigLoader.config.GetParam(Config.gpBundle));
        json.SetField("annotation", ConfigLoader.config.GetParam(Config.gpTitle) + " Annotation");
        json.SetField("text", ConfigLoader.config.GetParam(Config.gpTitle) + " Description");

        File.WriteAllText(filePath, json.ToString());
    }

    protected virtual void ProcessGradleProject()
    {
        FixMultidexInAndroidManifest();
        ReplaceBuildGradle();
    }

    protected void FixMultidexInAndroidManifest()
    {
        var androidManfestPath = GetExportProjectRoot() + "/src/main/AndroidManifest.xml";
        if (File.Exists(androidManfestPath))
        {
            var data = File.ReadAllText(androidManfestPath);
            data = data.Replace("application android:name=\"com.cuebiq.cuebiqsdk.unity.CuebiqSDKApplication\"", "application android:name=\"android.support.multidex.MultiDexApplication\"");
            File.WriteAllText(androidManfestPath, data);
        }
        else
        {
            Debug.LogError(androidManfestPath + " not found!");
        }
    }

    protected void ReplaceBuildGradle(string additionalDependenciesLines = "")
    {
        var newBuildGradle = GetExportProjectRoot() + "/build.gradle";
        if (File.Exists(newBuildGradle))
        {
            var data = File.ReadAllText(newBuildGradle);
            data = PatchGradleClassPath(data);
            data = PatchBuildToolsVersion(data, GetBuildToolsVersion()); // "25.0.2" or "25.0.3"
            data = data.Replace("//%EXCLUDE_NON_PLATFORM_ARCHITECTURE%", ExcludeNonPlatformArchitecture());
            data = data.Replace("//%ADDITIONAL_DEPENDENCIES%", additionalDependenciesLines);
            if (Directory.Exists(GetExportProjectRoot()))
            {
                File.WriteAllText(newBuildGradle, data);
            }
            else
            {
                Debug.LogError("Directory " + GetExportProjectRoot() + " not found!");
            }
        }
        else
        {
            Debug.LogError(newBuildGradle + " not found!");
        }
    }

    protected string PatchGradleClassPath(string buildGradle, string desiredVersion = "2.3.3")
    {
        string replaceKey = "com.android.tools.build:gradle:" + desiredVersion;
        var regex = new System.Text.RegularExpressions.Regex(@"com\.android\.tools\.build:gradle:\d+\.\d+\.\d+");
        var match = regex.Match(buildGradle);
        if (match.Success)
        {
            buildGradle = regex.Replace(buildGradle, replaceKey);
        }
        else
        {
            Debug.LogWarning("Can`t find classpath com.android.tools.build:gradle in build.gradle");
        }
        return buildGradle;
    }

    protected string PatchBuildToolsVersion(string buildGradle, string desiredVersion = "25.0.3")
    {
        string replaceKey = "buildToolsVersion '" + desiredVersion + "'";
        var regex = new System.Text.RegularExpressions.Regex(@"buildToolsVersion '\d+\.\d+\.\d+'");
        var match = regex.Match(buildGradle);
        if (match.Success)
        {
            buildGradle = regex.Replace(buildGradle, replaceKey);
        }
        else
        {
            Debug.LogWarning("Can`t find buildToolsVersion in build.gradle");
        }
        return buildGradle;
    }

    protected string ExcludeNonPlatformArchitecture()
    {
        if (targetDevice == AndroidTargetDevice.ARMv7)
        {
            return "exclude \"lib/x86/*.so\"";
        }
        if (targetDevice == AndroidTargetDevice.x86)
        {
            return "exclude \"lib/armeabi-v7a/*.so\"";
        }
        return "";
    }

    protected string GetBuildToolsVersion()
    {
        var platformToolsRoot = EditorPrefs.GetString("AndroidSdkRoot") + "/build-tools/";
        if (Directory.Exists(platformToolsRoot + "25.0.3"))
        {
            return "25.0.3";
        }
        return "25.0.2";
    }

    protected string GetExportProjectRoot()
    {
        return GetPaltformOutputLocation() + "/" + ConfigLoader.config.GetParam(Config.gpTitle);
    }


    protected string GetPlatformPackedOutputLocation()
    {
        var apk = GenerateExtraBundle();
        //patch bundleVersionCode to +1
        var apkSplit = apk.Split('_');
        if (apkSplit.Length >= 3)
        {
            int bundleVersionCode;
            if (int.TryParse(apkSplit[2], out bundleVersionCode))
            {
                bundleVersionCode += 1;
                apkSplit[2] = bundleVersionCode.ToString();
                apk = "";
                for (int i = 0; i < apkSplit.Length; i++)
                {
                    if (i != 0)
                        apk += "_";
                    apk += apkSplit[i];
                }
            }
        }
        //
        var packedApk = apk.Substring(0, apk.Length - 4) + "_tapcored.apk";
        return GetPlatformOutputPath() + "/" + packedApk;
    }
    #endregion
}
