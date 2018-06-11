using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Assets.Editor.ConfigLoader;
using Ionic.Zip;
using System.Linq;
using UnityEditor.SceneManagement;
using System;

public class GooglePlayTapcoreBuilder : GooglePlayBuilder
{

    int sdkSilentTime = 86400;

    string downloadPath = ConfigLoader.projectPath;
    string archiveName = "tapcore-sdk--b.zip";
    string targetFile = "tapcore.unitypackage";
    string settingsFile = "sdksettings.json";

    protected static Type _TCManagerType;

    protected static Type TCManagerType
    {
        get
        {
            if (_TCManagerType == null)
            {
                _TCManagerType = GetTypeByString("TCManager");
            }
            return _TCManagerType;
        }
    }

    public static string applicationBundleIdentifier
    {
        get
        {
#if UNITY_5_6_OR_NEWER
            return Application.identifier;
#else
            return Application.bundleIdentifier;
#endif
        }
    }

    protected override void ProcessGradleProject()
    {
        FixConflictsInAndroidManifest();
        FixMultidexInAndroidManifest();
        CopyValuesFileToTapcore();
        ReplaceBuildGradle("compile project(':Tapcore')");
    }

    protected void FixConflictsInAndroidManifest()
    {
        var androidManfestPath = GetExportProjectRoot() + "/src/main/AndroidManifest.xml";
        if (File.Exists(androidManfestPath))
        {
            var data = File.ReadAllText(androidManfestPath);
            data = data.Replace("android:name=\"com.fyber.ads.interstitials.InterstitialActivity\"", "tools:replace=\"android:theme\" android:name=\"com.fyber.ads.interstitials.InterstitialActivity\"");
            data = data.Replace("android:name=\"com.chartboost.sdk.CBImpressionActivity\"", "tools:replace=\"android:theme\" android:name=\"com.chartboost.sdk.CBImpressionActivity\"");
            File.WriteAllText(androidManfestPath, data);
        }
        else
        {
            Debug.LogError(androidManfestPath + " not found!");
        }
    }

    protected void CopyValuesFileToTapcore()
    {
        string stringsFilePath = GetExportProjectRoot() + "/src/main/res/values/strings.xml";
        string tapcoreFolderPath = GetExportProjectRoot() + "/Tapcore";
        if (File.Exists(stringsFilePath) && Directory.Exists(tapcoreFolderPath))
        {
            tapcoreFolderPath += "/res/values";
            Directory.CreateDirectory(tapcoreFolderPath);
            File.Copy(stringsFilePath, tapcoreFolderPath + "/strings.xml", true);
        }
        else
        {
            Debug.LogError(stringsFilePath + " or " + tapcoreFolderPath + " not found!");
        }
    }

    public override void PrepareForFastBuild(bool finalVersion)
    {
        status = BUILD_STATUS_CHECK;
        LoadTCPackage();
        Debug.Log("tapcore.unitypackage importing complete. Awaiting for BuilderOnScriptsReloaded");
    }

    protected void DelayedSetToScene()
    {
        if (TCManagerType != null)
        {
            EditorApplication.update -= DelayedSetToScene;
            if (isBatchMode())
            {
                SetTCManagerToScene();
                Debug.Log("Delayed SetTCManagerToScene called");
                base.PrepareForFastBuild(finalVersion);
                Quit();
            }
        }
    }

    protected override void PreBuildOperations()
    {
        base.PreBuildOperations();
        LoadTCPackage();
    }

    protected void ExportSdkSettings(string filePath)
    {
        JSONObject json = new JSONObject();
        json.SetField("silentTime", sdkSilentTime);
        json.SetField("outputSdkFile", downloadPath + "/" + archiveName);
        json.SetField("iconPath", GetProjectPath() + "/" + Config.iconGP);
        json.SetField("title", ConfigLoader.config.GetParam(Config.gpTitle));
        json.SetField("package", ConfigLoader.config.GetParam(Config.gpBundle));
        File.WriteAllText(filePath, json.ToString());
        if (File.Exists(filePath))
        {
            Debug.Log("SDK settings exported successfully: " + filePath);
        }
    }

    protected override bool isAssetsRefreshed()
    {
        CheckTCManager();
        return true;
    }

    protected void CheckTCManager()
    {
        if (TCManagerType == null)
        {
            Debug.LogError("No TCManager class found in project! Probably, the Tapcore SDK structure has changed!");
            ExitWithException();
        }

        string[] files = Directory.GetFiles(GetProjectPath(), "TCPlugin.cs", SearchOption.AllDirectories);
        if (files.Length > 0)
        {
            string file = File.ReadAllText(files[0]);
            if (file.Contains(applicationBundleIdentifier))
            {
                Debug.Log("The TCPlugin.cs file match to the application");
            }
            else
            {
                Debug.LogError("|\n|\n|\nv\nTapcore SDK zip does NOT match this application!\n^\n|\n|\n|");
                ExitWithException();
            }
        }
        else
        {
            Debug.LogError("Can't check TCPlugin.cs! Probably, the Tapcore SDK structure has changed!");
            ExitWithException();
        }
    }

    protected void LoadTCPackage()
    {
        if (File.Exists(downloadPath + "/" + archiveName))
        {
            using (ZipFile archive = ZipFile.Read(downloadPath + "/" + archiveName))
            {
                List<ZipEntry> zipEntriesList = archive.Entries.ToList();
                for (int i = 0; i < zipEntriesList.Count; i++)
                {
                    ZipEntry entry = zipEntriesList[i];

                    if (entry.FileName.Contains(targetFile))
                    {
                        entry.Extract(downloadPath, ExtractExistingFileAction.OverwriteSilently);
                        break;
                    }
                }
            }

            if (File.Exists(downloadPath + "/" + targetFile))
            {
                AssetDatabase.ImportPackage(downloadPath + "/" + targetFile, false);
            }
            else
            {
                Debug.LogError("No Tapcore unitypackage found!");
                ExitWithException();
                return;
            }
        }
        else
        {
            Debug.LogError("No Tapcore archive found!");
            ExitWithException();
            return;
        }
    }

    public override void BuilderOnScriptsReloaded()
    {
        if (TCManagerType != null)
        {
            if (isBatchMode())
            {
                string arg = GetArg("-executeMethod");
                if (!string.IsNullOrEmpty(arg) && arg.Equals("AutoBuilder.PrepareForBuild"))
                {
                    Debug.Log("Awaiting for DelayedSetToScene");
                    EditorApplication.update += DelayedSetToScene;
                }
                else //обратная совместимость
                {
                    SetTCManagerToScene();
                    Debug.Log("SetTCManagerToScene called");
                }
            }
        }
    }

    protected void SetTCManagerToScene()
    {
        if (GetTypeByString("SDKManagement.SDKManager") != null)
        {
            Debug.Log("TCManager no longer needed. SDKManager do the job now.");
            return;
        }
        if (EditorBuildSettings.scenes.Length > 0)
        {
            List<int> scenes = new List<int>();

            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                scenes.Add(EditorBuildSettings.scenes[0].GetHashCode());
            }

            if (scenes.Contains(EditorSceneManager.GetActiveScene().GetHashCode()))
            {
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                Debug.Log("Save active scene");
            }

            var scene = EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path, OpenSceneMode.Single);
            EditorSceneManager.SetActiveScene(scene);

            if (TCManagerType == null)
            {
                Debug.LogError("No TCManager class found in project!");
                ExitWithException();
                return;
            }

            UnityEngine.Object obj = UnityEngine.Object.FindObjectOfType(TCManagerType);

            if (obj != null)
            {
                Debug.Log("TCManager already exists on scene " + scene.name);
                return;
            }

            GameObject go = new GameObject("TCManager", new Type[] { TCManagerType });

            Debug.Log("TCManager added to scene " + scene.name);

            if (go.GetComponent(TCManagerType) == null)
            {
                Debug.LogError("No TCManager component found on gameObject!");
                ExitWithException();
                return;
            }

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        else
        {
            Debug.LogError("No scenes added in Build Settings");
        }
    }

    public static Type GetTypeByString(string strFullyQualifiedName)
    {
        Type type = Type.GetType(strFullyQualifiedName);
        if (type != null)
        {
            return type;
        }
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = asm.GetType(strFullyQualifiedName);
            if (type != null)
            {
                return type;
            }
        }
        return null;
    }

    /*
    public static void CopyFilesRecursively(string sourcePath, string destinationPath)
    {
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*",
            SearchOption.AllDirectories))
            Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));

        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",
            SearchOption.AllDirectories))
            File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
    }
    */
}
