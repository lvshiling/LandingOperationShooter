using Assets.Editor.ConfigLoader;
using System.IO;
using UnityEditor;
using UnityEngine;
using System;

public class AmazonBuilder : AbstractBuilder
{
    #region android properties
    private string apkName
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
    #endregion

    #region override section
    override protected void Init()
    {
        base.Init();
        Debug.Log("AmazonBuilder");
        buildTarget = BuildTarget.Android;
        adsVersion = false;
        dependencies = "Android";
        apkName = "";
        increaseBundleVersionCode = false;
    }

    override protected void BuildLauncher(string[] buildScenes, string locationPathName)
    {
        // BUILD
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = buildScenes;
        buildPlayerOptions.locationPathName = locationPathName;
        buildPlayerOptions.target = buildTarget;
        buildPlayerOptions.options = BuildOptions.None;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Internal;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

    override protected bool CheckIcons()
    {
        if (!File.Exists(Config.iconAM))
        {
            Debug.Log("ERROR: Icon " + Config.iconAM + " does not exist!");
            return false;
        }
        return true;
    }

    override protected bool CheckTitles()
    {
        var res = true;
        // bundle
        if (!ConfigLoader.IsCorrectBundle(ConfigLoader.config.GetParam(Config.amBundle)))
        {
            res = false;
            Debug.Log("ERROR: Amazon Bundle is Empty or Wrong Format");
        }
        // account
        if (ConfigLoader.config.GetParam(Config.amAccount) == "")
        {
            res = false;
            Debug.Log("ERROR: Amazon Account is Empty");
        }
        // title
        if (!ConfigLoader.IsCorrectTitle(ConfigLoader.config.GetParam(Config.amTitle)))
        {
            res = false;
            Debug.Log("ERROR: Amazon Title is Empty or Wrong format");
        }
        return res;
    }

    override protected bool CheckKeys()
    {
        return true;
    }

    override protected void ApplyPlatformSettings()
    {
        ConfigSetter.ApplyConfigAmazon(PlayerSettings.bundleVersion, PlayerSettings.Android.bundleVersionCode);
    }

    protected override bool isAssetsRefreshed()
    {
        return true;
    }

    public override void PrepareForFastBuild(bool finalVersion)
    {
        base.PrepareForFastBuild(finalVersion);
        CheckPackageStructure.RemoveWithDependencies("Amazon");
        ReplaceAndroidManifest();
        Quit();
    }

    public override void FastBuild()
    {
        base.FastBuild();
        status = BUILD_STATUS_READY_TO_BUILD;
    }

    protected override void PreBuildOperations()
    {
        CheckPackageStructure.RemoveWithDependencies("Amazon");
        ReplaceAndroidManifest();
        status = BUILD_STATUS_READY_TO_BUILD;
    }

    override protected void PostBuildOperations()
    {

    }

    override protected string GetPlatformOutputPath()
    {
        return "Builds/Amazon";
    }

    override protected string GetPaltformOutputLocation()
    {
        if (outputlocation == "")
        {
            outputlocation = GetPlatformOutputPath() + "/" + GenerateExtraBundle();
        }
        return outputlocation;
    }

    override protected string GenerateExtraBundle()
    {
        if (apkName == "")
        {
            apkName = ConfigLoader.config.GetParam(Config.amBundle) + "_" + PlayerSettings.bundleVersion + "_" + PlayerSettings.Android.bundleVersionCode + "_" + GetDate() + ".apk";
        }
        return apkName;

    }

    protected override void LogDefines()
    {
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
        Debug.Log("Defines symbols: " + defines);
    }

    protected override bool CheckKeystores()
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

    protected override void SetBundleVersion()
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
    #endregion

    #region private section
    private void ReplaceAndroidManifest()
    {
        var androidManifest = "Assets/Plugins/Android/AndroidManifest.xml";
        var amazonManifest = "Assets/Plugins/Android/AndroidManifestAmazon.xml";

        if (File.Exists(androidManifest) && File.Exists(amazonManifest))
        {
            var data = File.ReadAllText(amazonManifest);
            File.WriteAllText(androidManifest, data);
        }
    }
    #endregion
}
