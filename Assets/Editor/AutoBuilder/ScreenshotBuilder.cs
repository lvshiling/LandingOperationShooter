using Assets.Editor.ConfigLoader;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ScreenshotBuilder : AmazonBuilder
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

    override protected void Init()
    {
        base.Init();
        Debug.Log("Screenshot Version");
    }

    protected override void ApplyPlatformSettings()
    {
        base.ApplyPlatformSettings();
        ConfigSetter.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, new string[] { "SCREENSHOT_VERSION" }, new bool[] { true });
    }

    public override void PrepareForFastBuild(bool finalVersion)
    {
        base.PrepareForFastBuild(finalVersion);
    }

    protected override void PreBuildOperations()
    {
        base.PreBuildOperations();
    }

    override protected string GetPlatformOutputPath()
    {
        return "Builds/Screenshot";
    }

    override protected string GenerateExtraBundle()
    {
        if (apkName == "")
        {
            apkName = ConfigLoader.config.GetParam(Config.amBundle) + "_" + PlayerSettings.bundleVersion + "_" + PlayerSettings.Android.bundleVersionCode + "_SCREENSHOT_VERSION_DO_NOT_RELEASE_" + GetDate() + ".apk";
        }
        return apkName;
    }


}
