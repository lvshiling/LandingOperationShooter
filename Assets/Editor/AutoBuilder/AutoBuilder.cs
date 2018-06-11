using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using Assets.Editor.GBNEditor;

[InitializeOnLoad]
public class AutoBuilder : AssetPostprocessor
{
    private const string BUILD_TYPE_GOOGLE_PLAY = "BUILD_TYPE_GOOGLE_PLAY";
    private const string BUILD_TYPE_GOOGLE_PLAY_TAPCORE = "BUILD_TYPE_GOOGLE_PLAY_TAPCORE";
    private const string BUILD_TYPE_GOOGLE_PLAY_LOCAL = "BUILD_TYPE_GOOGLE_PLAY_LOCAL";
    private const string BUILD_TYPE_AMAZON = "BUILD_TYPE_AMAZON";
    private const string BUILD_TYPE_APP_STORE_FREE = "BUILD_TYPE_APP_STORE_FREE";

    private const string BUILD_TYPE_SCREENSHOT = "BUILD_TYPE_SCREENSHOT";

    private static Action onPostprocessAllAssets = null;
    private static Action onScriptsReloaded = null;

    private static string BuildType
    {
        get
        {
            return EditorPrefs.GetString("BUILD_TYPE", BUILD_TYPE_GOOGLE_PLAY);
        }
        set
        {
            EditorPrefs.SetString("BUILD_TYPE", value);
        }
    }
    private static AbstractBuilder builder = null;
    private static AbstractBuilder Builder
    {
        get
        {
            switch (BuildType)
            {
                case BUILD_TYPE_GOOGLE_PLAY:
                    builder = new GooglePlayBuilder();
                    break;
                case BUILD_TYPE_GOOGLE_PLAY_TAPCORE:
                    builder = new GooglePlayTapcoreBuilder();
                    break;
                case BUILD_TYPE_GOOGLE_PLAY_LOCAL:
                    builder = new GooglePlayLocalBuilder();
                    break;
                case BUILD_TYPE_AMAZON:
                    builder = new AmazonBuilder();
                    break;
                case BUILD_TYPE_APP_STORE_FREE:
                    builder = new IosFreeBuilder();
                    break;
                case BUILD_TYPE_SCREENSHOT:
                    builder = new ScreenshotBuilder();
                    break;
                default:
                    builder = new GooglePlayBuilder();
                    break;
            }
            return builder;
        }
    }
    [InitializeOnLoadMethod]
    static void OnLoadMethod()
    {
        EditorApplication.update -= CheckBuilder;
        EditorApplication.update += CheckBuilder;
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        CheckBuilder();
        if (onPostprocessAllAssets != null)
        {
            onPostprocessAllAssets.Invoke();
        }
    }

    [DidReloadScripts]
    static void OnScriptsReloaded()
    {
        Builder.BuilderOnScriptsReloaded();
        if (onScriptsReloaded != null)
        {
            onScriptsReloaded.Invoke();
        }
    }

    public static void CheckBuilder()
    {
        Builder.CheckBuilder();
    }
    [MenuItem("AutoBuilder/Local Build GP Final")]
    public static void LocalBuildGP()
    {
        RunFirstAfterLaunch();
        BuildType = BUILD_TYPE_GOOGLE_PLAY_LOCAL;
        Builder.Build(true);
    }
    [MenuItem("AutoBuilder/Local Build GP Test")]
    public static void LocalBuildGPTest()
    {
        RunFirstAfterLaunch();
        BuildType = BUILD_TYPE_GOOGLE_PLAY_LOCAL;
        Builder.Build(false);
    }
    [MenuItem("AutoBuilder/Build GP")]
    public static void BuildGP()
    {
        RunFirstAfterLaunch();
        BuildType = BUILD_TYPE_GOOGLE_PLAY;
        Builder.Build(true, AndroidTargetDevice.FAT, false, "");
    }

    public static void PrepareForBuild()
    {
        string buildTypeParam = GetArg("-buildtype");
        if (string.IsNullOrEmpty(buildTypeParam))
        {
            Debug.LogError("This build commang requires the -buildtype parameter (gp/am/as)!");
            EditorApplication.Exit(1);
        }
        RunFirstAfterLaunch();
        switch (buildTypeParam)
        {
            case "gp":
                BuildType = BUILD_TYPE_GOOGLE_PLAY;
                break;
            case "gptapcore":
                BuildType = BUILD_TYPE_GOOGLE_PLAY_TAPCORE;
                break;
            case "am":
                BuildType = BUILD_TYPE_AMAZON;
                break;
            case "as":
                BuildType = BUILD_TYPE_APP_STORE_FREE;
                break;
            default:
                Debug.LogError("The -buildtype parameter is invalid!");
                EditorApplication.Exit(1);
                break;
        }
        Builder.PrepareForFastBuild(true);
    }
    public static void FastBuild()
    {
        string buildTypeParam = GetArg("-buildtype");
        if (string.IsNullOrEmpty(buildTypeParam))
        {
            Debug.LogError("This build commang requires the -buildtype parameter (gp/am/as)!");
            EditorApplication.Exit(1);
        }
        switch (buildTypeParam)
        {
            case "gp":
                BuildType = BUILD_TYPE_GOOGLE_PLAY;
                break;
            case "gptapcore":
                BuildType = BUILD_TYPE_GOOGLE_PLAY_TAPCORE;
                break;
            case "am":
                BuildType = BUILD_TYPE_AMAZON;
                break;
            case "as":
                BuildType = BUILD_TYPE_APP_STORE_FREE;
                break;
            default:
                Debug.LogError("The -buildtype parameter is invalid!");
                EditorApplication.Exit(1);
                break;
        }
        Builder.FastBuild();
    }

    public static void BuildGPTapcore()
    {
        RunFirstAfterLaunch();
        BuildType = BUILD_TYPE_GOOGLE_PLAY_TAPCORE;
        Builder.Build(true, AndroidTargetDevice.FAT, false, "");
    }
    public static void BuildGPTapcorePrebuild()
    {
        RunFirstAfterLaunch();
        BuildType = BUILD_TYPE_GOOGLE_PLAY_TAPCORE;
        Builder.PrepareForFastBuild(true);
    }
    [MenuItem("AutoBuilder/Stop")]
    public static void StopBuilding()
    {
        EditorApplication.update -= CheckBuilder;
        if (EditorPrefs.HasKey("BUILD_STATUS"))
        {
            EditorPrefs.DeleteKey("BUILD_STATUS");
        }
        if (EditorPrefs.HasKey("WAIT_ITERATIONS"))
        {
            EditorPrefs.DeleteKey("WAIT_ITERATIONS");
        }
        builder = null;
        BuildType = "";
    }
    public static void BuildGPArchX86()
    {
        RunFirstAfterLaunch();
        BuildType = BUILD_TYPE_GOOGLE_PLAY;
        Builder.Build(true, AndroidTargetDevice.x86, true, "_x86");
    }
    public static void BuildGPArchArm()
    {
        RunFirstAfterLaunch();
        BuildType = BUILD_TYPE_GOOGLE_PLAY;
        Builder.Build(true, AndroidTargetDevice.ARMv7, false, "_arm");
    }
    public static void BuildAM()
    {
        RunFirstAfterLaunch();
        BuildType = BUILD_TYPE_AMAZON;
        Builder.Build(true);
    }
    public static void BuildScreenshot()
    {
        RunFirstAfterLaunch();
        BuildType = BUILD_TYPE_SCREENSHOT;
        Builder.Build(true);
    }
    public static void BuildASFreeXcode8()
    {
        RunFirstAfterLaunch();
        BuildType = BUILD_TYPE_APP_STORE_FREE;
        if (WaitForImportPackage())
        {
            onPostprocessAllAssets += delegate
            {
                onPostprocessAllAssets = null;
                Builder.Build(true);
            };
        }
        else
        {
            Builder.Build(true);
        }
    }

    protected static void RunFirstAfterLaunch()
    {
        if (!string.IsNullOrEmpty(GetArg("-batchmode", true)))
        {
            //in batchmode
            GBNEditorAccounts.Git.account.Clear();
            string gitToken = GetArg("-gittoken");
            if (!string.IsNullOrEmpty(gitToken))
            {
                if (GBNEditorAccounts.Git.Login(gitToken))
                {
                    Debug.Log("GIT Login success!");
                }
                else
                {
                    Debug.Log("GIT Login failed! Check command line args");
                }
            }
        }
        else
        {
            //in editor
        }
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

    protected static bool WaitForImportPackage()
    {
        return !string.IsNullOrEmpty(GetArg("-importPackage"));
    }

    /*
    private static void RemoveDefine(string define)
    {
        string scriptingDefine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
        string[] scriptingDefines = scriptingDefine.Split(';');
        System.Collections.Generic.List<string> listDefines = new System.Collections.Generic.List<string>(scriptingDefines);
        if (listDefines.Contains(define))
        {
            listDefines.Remove(define);
        }
        string newDefines = string.Join(";", listDefines.ToArray());
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, newDefines);
    }

    private static void AddDefine(string define)
    {
        string scriptingDefine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
        string[] scriptingDefines = scriptingDefine.Split(';');
        System.Collections.Generic.List<string> listDefines = new System.Collections.Generic.List<string>(scriptingDefines);
        if (!listDefines.Contains(define))
        {
            listDefines.Add(define);
        }
        string newDefines = string.Join(";", listDefines.ToArray());
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, newDefines);
    }
    */
}
