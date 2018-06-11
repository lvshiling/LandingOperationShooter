using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static partial class PatchAutomator {

    private static string filesToRemoveTxtName = "filesToRemove.txt";

    private static string ScriptDirectory
    {
        get
        {
            string[] path = Directory.GetFiles(Application.dataPath, (typeof(PatchAutomator)).Name + ".cs", SearchOption.AllDirectories);
            if (path.Length > 0)
            {
                string[] dirs = path[0].Split(new char[] { '/', '\\' }, StringSplitOptions.None);
                return string.Join("/", dirs, 0, dirs.Length - 1);
            }
            else
            {
                return Application.dataPath;
            }
        }
    }

    [MenuItem("Patch Automator/Remove Old Files")]
    public static void RemoveOldFiles()
    {
        string filePath = ScriptDirectory + "/" + filesToRemoveTxtName;
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; ++i)
            {
                if (File.Exists(Application.dataPath + lines[i]))
                {
                    File.Delete(Application.dataPath + lines[i]);
                    Debug.Log(Application.dataPath + lines[i] + " file removed");
                }
                else if (Directory.Exists(Application.dataPath + lines[i]))
                {
                    Directory.Delete(Application.dataPath + lines[i], true);
                    Debug.Log(Application.dataPath + lines[i] + " directory removed");
                }
            }
            Debug.Log("Removing old files complete");
        }
    }
}
