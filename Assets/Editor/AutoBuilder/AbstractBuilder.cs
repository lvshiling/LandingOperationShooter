using Assets.Editor.ConfigLoader;
using UnityEditor;
using System;
using UnityEngine;
using System.Collections.Generic;

public abstract class AbstractBuilder 
{
    private static string doNotObfuscateFileName = "doNotObfuscate.txt";
    #region global propertiies
    private string projectPath
    {
        get
        {
            return EditorPrefs.GetString("projectPath", "");
        }
        set
        {
            EditorPrefs.SetString("projectPath", value);
        }
    }
    protected string dependencies
    {
        get
        {
            return EditorPrefs.GetString("dependencies", "");
        }
        set
        {
            EditorPrefs.SetString("dependencies", value);
        }
    }

    protected string outputlocation
    {
        get
        {
            return EditorPrefs.GetString("outputlocation", "");
        }
        set
        {
            EditorPrefs.SetString("outputlocation", value);
        }
    }

    protected BuildTarget buildTarget
    {
        get
        {
            switch (EditorPrefs.GetString("BuildTarget", "Android"))
            {
                case "iOS": return BuildTarget.iOS;
                default: return BuildTarget.Android;
            }
        }
        set
        {
            switch (value)
            {
                case BuildTarget.iOS: EditorPrefs.SetString("BuildTarget", "iOS"); break;
                default: EditorPrefs.SetString("BuildTarget", "Android"); break;
            }
        }
    }

    protected bool adsVersion
    {
        get
        {
            return EditorPrefs.GetString("adsVersion", "false") == "true";
        }
        set
        {
            EditorPrefs.SetString("adsVersion", value ? "true" : "false");
        }
    }

    protected bool finalVersion
    {
        get
        {
            return EditorPrefs.GetString("finalVersion", "false") == "true";
        }
        set
        {
            EditorPrefs.SetString("finalVersion", value ? "true" : "false");
        }
    }

    protected AndroidTargetDevice targetDevice
    {
        get
        {
            switch (EditorPrefs.GetString("AndroidTargetDevice", "FAT"))
            {
                case "x86": return AndroidTargetDevice.x86;
                case "ARMv7": return AndroidTargetDevice.ARMv7;
                default: return AndroidTargetDevice.FAT;
            }
        }
        set {
            switch (value)
            {
                case AndroidTargetDevice.x86: EditorPrefs.SetString("AndroidTargetDevice", "x86"); break;
                case AndroidTargetDevice.ARMv7: EditorPrefs.SetString("AndroidTargetDevice", "ARMv7"); break;
                default: EditorPrefs.SetString("AndroidTargetDevice", "FAT"); break;
            }
        }
    }
    protected bool increaseBundleVersionCode
    {
        get
        {
            return EditorPrefs.GetString("increaseBundleVersionCode", "false") == "true";
        }
        set
        {
            EditorPrefs.SetString("increaseBundleVersionCode", value ? "true" : "false");
        }
    }
    protected string archTag
    {
        get
        {
            return EditorPrefs.GetString("archTag", "");
        }
        set
        {
            EditorPrefs.SetString("archTag", value);
        }
    }

    protected bool overrideVersion
    {
        get
        {
            return EditorPrefs.GetString("overrideVersion", "false") == "true";
        }
        set
        {
            EditorPrefs.SetString("overrideVersion", value? "true" : "false");
        }
    }
   
    protected static string BUILD_STATUS_CHECK = "BUILD_STATUS_CHECK";
    protected static string BUILD_STATUS_WAIT_FOR_BUILD = "BUILD_STATUS_WAIT_FOR_BUILD";
    protected static string BUILD_STATUS_READY_TO_BUILD = "BUILD_STATUS_READY_TO_BUILD";
    protected static string BUILD_STATUS_IMMEDIATE_BUILD = "BUILD_STATUS_IMMEDIATE_BUILD";
    protected static string BUILD_STATUS_BUILD = "BUILD_STATUS_BUILD";

    public static string status
    {
        get
        {
            return EditorPrefs.GetString("BUILD_STATUS", BUILD_STATUS_CHECK);
        }
        protected set
        {
            Debug.Log("status = " + value);
            EditorPrefs.SetString("BUILD_STATUS", value);
        }
    }

    #endregion

    #region build section
    public void Build(bool finalVersion, AndroidTargetDevice targetDevice, bool increaseBundleVersionCode, string archTag)
    {
        this.archTag = archTag;
        this.targetDevice = targetDevice;
        this.increaseBundleVersionCode = increaseBundleVersionCode;
        Build(finalVersion);
    }

    virtual public void PrepareForFastBuild(bool finalVersion)
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;
        Init();
        this.finalVersion = finalVersion;
        LoadConfig();
        CheckProject();
        SetupPlatform();
        SaveInfoFileValuesToFile();
    }

    virtual public void Quit()
    {
        if (isBatchMode() && string.IsNullOrEmpty(GetArg("-quit", true)))
        {
            EditorApplication.Exit(0);
        }
    }

    virtual public void FastBuild()
    {
        Init();
#if UNITY_ANDROID
        ConfigSetter.ResetKeystore();   
#endif
        if (status == BUILD_STATUS_IMMEDIATE_BUILD)
        {
            LaunchBuild();
        }
    }

    public void Build(bool finalVersion)
    {
        Init();
        status = BUILD_STATUS_CHECK;
        this.finalVersion = finalVersion;
        // LOAD CONFIG 
        LoadConfig();

        // CHECK PROJECT
        CheckProject();

        // APPLY PLATFORM SETTINGS
        SetupPlatform();

        PreBuildOperations();

        if (status == BUILD_STATUS_IMMEDIATE_BUILD)
        {
            LaunchBuild();
        }
    }

    protected void LoadConfig()
    {
        ConfigLoader.LoadConfig();
        if (ConfigLoader.config.IsLoaded)
        {
            Debug.Log("OK. Config.txt Loaded Successfully");
        }
        else
        {
            Debug.Log("ERROR. Config.txt not found");
            Debug.Log("Build Canceled!");
            ExitWithException();
        }
    }

    protected void CheckProject()
    {
        var hasProjectErrors = false;
        if (!CheckIcons()) hasProjectErrors = true;
        if (!CheckTitles()) hasProjectErrors = true;
        if (!CheckKeys()) hasProjectErrors = true;
        if (!CheckKeystores()) hasProjectErrors = true;

        CheckPackageStructure.Check(dependencies);
        if (CheckPackageStructure.hasErrors)
        {
            hasProjectErrors = true;
            Debug.Log(CheckPackageStructure.errorLog);
        }

        // exit on config error
        if (hasProjectErrors)
        {
            Debug.Log("Build Canceled!");
            ExitWithException();
        }
    }

    protected void SetupPlatform()
    {
        if (finalVersion)
        {
            ConfigSetter.SetFinalVersion();
        }
        else
        {
            ConfigSetter.SetTestVersion();
        }
        SetBundleVersion();
        ApplyPlatformSettings();
        try
        {
            AssetDatabase.SaveAssets();
        }
        catch (Exception e)
        {
            Debug.Log("Can't Save Assets! Exception catched: " + e.Message);
        }
    }

    protected void SaveInfoFileValuesToFile()
    {        
        try
        {
            List<string> stringValues = Assets.Editor.GBNEditor.GBNEditorInfoFileBuilder.ExtractAllStringValues();
            if (stringValues != null)
            {
                System.IO.File.WriteAllLines(GetProjectPath() + "/" + doNotObfuscateFileName, stringValues.ToArray());
                Debug.Log(doNotObfuscateFileName + " file created succesfully!");
            }
        }
        catch (Exception e)
        {
            Debug.Log("Can't create " + doNotObfuscateFileName + " file! Exception catched: " + e.Message);
        }
    }
    
    public void CheckBuilder()
    {
        if (status == BUILD_STATUS_READY_TO_BUILD && isAssetsRefreshed())
        {
            LaunchBuild();
        }
        else if (status == BUILD_STATUS_WAIT_FOR_BUILD && isAssetsRefreshed())
        {
            status = BUILD_STATUS_READY_TO_BUILD;
            LaunchBuild();
        }
    }

    virtual public void BuilderOnScriptsReloaded()
    {

    }
   
    private void LaunchBuild() {
        Debug.Log(this + ".LaunchBuild");
        status = BUILD_STATUS_BUILD;
        LogDefines();
        ConfigLoader.LoadConfig();
        var locationPathName = GetPaltformOutputLocation();
        if (locationPathName == "")
        {
            Debug.Log("ERROR. Non supported platform: " + buildTarget);
            Debug.Log("Build Canceled!");
            ExitWithException();

        }
        Debug.Log("OK. Build Location: " + locationPathName);
        // CHECK SCENES
        var editorScenes = EditorBuildSettings.scenes;
        var buildScenes = new string[editorScenes.Length];
        for (var i = 0; i < buildScenes.Length; i++)
        {
            buildScenes[i] = editorScenes[i].path;
            Debug.Log("OK. Build Scene " + i + ": " + buildScenes[i]);
        }
        if (buildScenes.Length == 0)
        {
            Debug.Log("ERROR. No scenes to build!");
            Debug.Log("Build Canceled!");
            ExitWithException();
        }
        BuildLauncher(buildScenes, locationPathName);
        PostBuildOperations();
        Quit();
    }
#endregion

#region abstract section
    virtual protected void Init()
    {
        projectPath = "";
        outputlocation = "";
    }

    abstract protected bool isAssetsRefreshed();

    abstract protected void PreBuildOperations();

    abstract protected void PostBuildOperations();

    abstract protected void BuildLauncher(string[] buildScenes, string locationPathName);

    abstract protected void LogDefines();

    abstract protected string GetPaltformOutputLocation();

    abstract protected string GetPlatformOutputPath();

    abstract protected string GenerateExtraBundle();

    abstract protected void ApplyPlatformSettings();

    abstract protected bool CheckKeys();

    abstract protected bool CheckKeystores();

    abstract protected bool CheckTitles();

    abstract protected bool CheckIcons();

    abstract protected void SetBundleVersion();
#endregion

#region utils
    protected string GetProjectPath()
    {
        if (projectPath == "")
        {
            var items = Application.dataPath.Split(new string[] { "/" }, StringSplitOptions.None);
            projectPath = string.Join("/", items, 0, items.Length - 1);
        }
        return projectPath;
    }

    protected virtual void ExitWithException()
    {
        var message = "PROJECT STATUS FAILED!";
        if (isBatchMode())
        {
            Debug.LogError(message);
            EditorApplication.Exit(1);
        }
        else
        {
            throw new Exception(message);
        }
    }

    protected string GetDate()
    {
        return DateTime.Now.ToString("yyyy.MM.dd_HH.mm");
    }

    protected bool isBatchMode() {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-batchmode")
            {
                return true;
            }
        }
        return false;
    }

    protected static string GetArg(string name, bool noparams = false)
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name)
            {
                if (noparams)
                {
                    return "true";
                }
                else if (args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }
        }
        return null;
    }
    #endregion
}
