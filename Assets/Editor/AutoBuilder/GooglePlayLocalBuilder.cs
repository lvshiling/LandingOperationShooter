using Assets.Editor.ConfigLoader;
using System.IO;
using UnityEditor;
using UnityEngine;
using System;

public class GooglePlayLocalBuilder : GooglePlayBuilder
{
    override protected void Init()
    {
        base.Init();
        isExport = false;
    }

    override protected void ApplyPlatformSettings()
    {
        ConfigSetter.ApplyConfigGPLocal(PlayerSettings.bundleVersion, PlayerSettings.Android.bundleVersionCode, targetDevice);
    }

    public override void FastBuild()
    {
        try
        {
            Directory.CreateDirectory(GetPlatformOutputPath());
        }
        catch
        {
            Debug.Log("ERROR. Can't create directory: " + GetPlatformOutputPath());
            Debug.Log("Build would be failed!");
            ExitWithException();
        }
        base.FastBuild();
        status = BUILD_STATUS_WAIT_FOR_BUILD;
        EditorPrefs.SetInt("WAIT_ITERATIONS", 0);
    }

    override protected void PreBuildOperations()
    {
        base.PreBuildOperations();
        try
        {
            Directory.CreateDirectory(GetPlatformOutputPath());
        }
        catch
        {
            Debug.Log("ERROR. Can't create directory: "+ GetPlatformOutputPath());
            Debug.Log("Build would be failed!");
            ExitWithException();
        }
    }
}
