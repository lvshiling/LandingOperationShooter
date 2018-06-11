using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Assets.Editor.ConfigLoader
{
    public class CheckPackageStructure
    {
        #region public

        public static string debugLog { get; private set; }
        public static string errorLog { get; private set; }

        public static bool hasErrors { get; private set; }

        [MenuItem("Patch Automator/Check Package/Check GBN Ads package structure")]
        public static void Check()
        {
            ClearLog();
            Load();
            if (isLoaded)
            {
                CheckFilesToRemove();
                CheckDirsToRemove();
                CheckDirsToExist("");
                CheckFilesToExist("");
                CheckFilesForText("");
            }
            ShowLog();
        }

        public static void Check(string dependencies)
        {
            ClearLog();
            Load();
            if (isLoaded)
            {
                CheckFilesToRemove();
                CheckDirsToRemove();
                CheckDirsToExist(dependencies);
                CheckFilesToExist(dependencies);
                CheckFilesForText(dependencies);
            }

            //ShowLog();
        }

        public static void RemoveWithDependencies(string dependencies)
        {
            ClearLog();
            Load();
            if (isLoaded)
            {
                RemoveFilesWithDependencies(dependencies);
                RemoveDirsWithDependencies(dependencies);
                AssetDatabase.Refresh();
            }
            ShowLog();
        }

        [MenuItem("Patch Automator/Check Package/Clean GBN Ads package")]
        public static void Clean()
        {
            ClearLog();
            Load();
            if (isLoaded)
            {
                RemoveFiles();
                RemoveDirs();
                AssetDatabase.Refresh();
            }
            ShowLog();
        }

        public static void Update()
        {
            // TODO update fyber.unitypackage from GitLab
            throw new NotImplementedException();
        }

        #endregion

        #region private
        private static string packageStructure = "Assets/Editor/ConfigLoader/PackageStructure.txt";
        private static bool isLoaded = false;
        private static List<FileEntry> filesToRemove = null;
        private static List<FileEntry> filesToExist = null;
        private static List<FileEntry> filesToContainsText = null;
        private static List<FileEntry> dirsToRemove = null;
        private static List<FileEntry> dirsToExist = null;


        private static void Load()
        {
            if (File.Exists(packageStructure))
            {
                filesToRemove = new List<FileEntry>();
                filesToExist = new List<FileEntry>();
                filesToContainsText = new List<FileEntry>();
                dirsToRemove = new List<FileEntry>();
                dirsToExist = new List<FileEntry>();

                var lines = File.ReadAllLines(packageStructure);
                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line) || line.IndexOf("#") == 0) continue;
                    var parts = line.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        switch (parts[0])
                        {
                            case "D-":
                                if (parts.Length >= 3)
                                {
                                    dirsToRemove.Add(new FileEntry(parts[1], "", parts[2]));
                                }
                                else
                                {
                                    dirsToRemove.Add(new FileEntry(parts[1]));
                                }
                                break;
                            case "D+":
                                if (parts.Length >= 3)
                                {
                                    dirsToExist.Add(new FileEntry(parts[1], "", parts[2]));
                                }
                                else
                                {
                                    dirsToExist.Add(new FileEntry(parts[1]));
                                }
                                break;
                            case "F-":
                                if (parts.Length >= 3)
                                {
                                    filesToRemove.Add(new FileEntry(parts[1], "", parts[2]));
                                }
                                else
                                {
                                    filesToRemove.Add(new FileEntry(parts[1]));
                                }
                                break;
                            case "F+":
                                if (parts.Length >= 3)
                                {
                                    filesToExist.Add(new FileEntry(parts[1], "", parts[2]));
                                }
                                else
                                {
                                    filesToExist.Add(new FileEntry(parts[1]));
                                }
                                break;
                            case "F?":
                                if (parts.Length >= 4)
                                {
                                    filesToContainsText.Add(new FileEntry(parts[1], parts[2], parts[3]));
                                }
                                else if (parts.Length >= 3)
                                {
                                    filesToContainsText.Add(new FileEntry(parts[1], parts[2]));
                                }
                                else
                                {
                                    filesToExist.Add(new FileEntry(parts[1]));
                                }
                                break;
                            default: break;
                        }
                    }
                }
                isLoaded = true;
            }
            else
            {
                Debug.LogError("Could not load " + packageStructure);
                isLoaded = false;
                hasErrors = true;
            }
        }
        private static void ClearLog()
        {
            debugLog = "";
            errorLog = "";
            hasErrors = false;
        }

        private static void ShowLog()
        {
            /*
            if (string.IsNullOrEmpty(debugLog))
            {
                Debug.LogWarning("Fyber Debug Log is empty");
            }
            else
            {
                Debug.Log(debugLog);
            }
            */
            if (string.IsNullOrEmpty(errorLog))
            {
                //Debug.Log("Fyber Log Contains No Errors");
            }
            else
            {
                Debug.LogError(errorLog);
            }
        }

        private static void CheckFilesToRemove()
        {
            var count = 0;
            foreach (var file in filesToRemove)
            {
                if (file.Exists() && !file.hasDependenies())
                {
                    errorLog += "ERROR. File should not exists: " + file.FileName + "\n";
                    debugLog += "ERROR. File should not exists: " + file.FileName + "\n";
                    count++;
                    hasErrors = true;
                }
                else
                {
                    debugLog += "OK. File not exists: " + file.FileName + "\n";
                }
            }
            if (count > 0)
            {
                errorLog += "Need to remove: " + count + " files!\n";
            }
        }

        private static void CheckDirsToRemove()
        {
            var count = 0;
            foreach (var dir in dirsToRemove)
            {
                if (dir.Exists() && !dir.hasDependenies())
                {
                    errorLog += "ERROR. Directory should not exists: " + dir.FileName + "\n";
                    debugLog += "ERROR. Directory should not exists: " + dir.FileName + "\n";
                    count++;
                    hasErrors = true;
                }
                else
                {
                    debugLog += "OK. Directory not exists: " + dir.FileName + "\n";
                }
            }
            if (count > 0)
            {
                errorLog += "Need to remove: " + count + " directories!\n";
            }
        }

        private static void CheckDirsToExist(string dependencies)
        {
            var count = 0;
            foreach (var dir in dirsToExist)
            {
                if (!dir.CheckDependencies(dependencies))
                {
                    continue;
                }
                if (!dir.Exists())
                {
                    errorLog += "ERROR. Directory should exists: " + dir.FileName + "\n";
                    debugLog += "ERROR. Directory should exists: " + dir.FileName + "\n";
                    count++;
                    hasErrors = true;
                }
                else
                {
                    debugLog += "OK. Directory exists: " + dir.FileName + "\n";
                }
            }
            if (count > 0)
            {
                errorLog += "Need to add: " + count + " directories!\n";
            }
        }
        private static void CheckFilesToExist(string dependencies)
        {
            var count = 0;
            foreach (var file in filesToExist)
            {
                if (!file.CheckDependencies(dependencies))
                {
                    continue;
                }

                if (!file.Exists())
                {
                    errorLog += "ERROR. File should exists: " + file.FileName + "\n";
                    debugLog += "ERROR. File should exists: " + file.FileName + "\n";
                    count++;
                    hasErrors = true;
                }
                else
                {
                    debugLog += "OK. File exists: " + file.FileName + "\n";
                }
            }
            if (count > 0)
            {
                errorLog += "Need to add: " + count + " files!\n";
            }
        }
        private static void CheckFilesForText(string dependencies)
        {
            var count = 0;
            foreach (var file in filesToContainsText)
            {
                if (!file.CheckDependencies(dependencies))
                {
                    continue;
                }
                if (!file.isContainsText())
                {
                    errorLog += "ERROR. File should exists: " + file.FileName + " and contains text: " + file.Text + "\n";
                    debugLog += "ERROR. File should exists: " + file.FileName + " and contains text: " + file.Text + "\n";
                    count++;
                    hasErrors = true;
                }
                else
                {
                    debugLog += "OK. File exists: " + file.FileName + " and contains text: " + file.Text + "\n";
                }
            }
            if (count > 0)
            {
                errorLog += "Need to add or update: " + count + " files!\n";
            }
        }
        private static void RemoveFiles()
        {
            var count = 0;
            foreach (var file in filesToRemove)
            {
                if (file.Exists() && !file.hasDependenies())
                {
                    if (file.FileName.IndexOf("Assets/") == 0)
                    {
                        AssetDatabase.DeleteAsset(file.FileName);
                    }
                    debugLog += "Removed File: " + file.FileName + "\n";
                    count++;
                }
            }
            debugLog += "Removed " + count + " files!\n";
        }

        private static void RemoveDirs()
        {
            var count = 0;
            foreach (var dir in dirsToRemove)
            {
                if (dir.Exists() && !dir.hasDependenies())
                {
                    if (dir.FileName.IndexOf("Assets/") == 0)
                    {
                        AssetDatabase.DeleteAsset(dir.FileName);
                    }
                    debugLog += "Removed Directory: " + dir.FileName + "\n";
                    count++;
                }
            }
            debugLog += "Removed " + count + " directories!\n";
        }

        private static void RemoveDirsWithDependencies(string dependencies)
        {
            var count = 0;
            foreach (var dir in dirsToRemove)
            {
                if (dir.Exists() && dir.CheckAllDependencies(dependencies))
                {
                    if (dir.FileName.IndexOf("Assets/") == 0)
                    {
                        AssetDatabase.DeleteAsset(dir.FileName);
                    }
                    debugLog += "Removed Directory: " + dir.FileName + "\n";
                    count++;
                }
            }
            debugLog += "Removed " + count + " directories!\n";
        }
        private static void RemoveFilesWithDependencies(string dependencies)
        {
            var count = 0;
            foreach (var file in filesToRemove)
            {
                if (file.Exists() && file.CheckAllDependencies(dependencies))
                {
                    if (file.FileName.IndexOf("Assets/") == 0)
                    {
                        AssetDatabase.DeleteAsset(file.FileName);
                    }
                    debugLog += "Removed File: " + file.FileName + "\n";
                    count++;
                }
            }
            debugLog += "Removed " + count + " files!\n";
        }

        private class FileEntry
        {
            public string FileName { get; private set; }
            public string Text { get; private set; }
            public string[] Dependencies;
            private const string IOS = "ios";
            private const string ANDROID = "android";

            public FileEntry(string filename)
            {
                FileName = filename;
                Text = "";
                Dependencies = new string[] { };
            }
            public FileEntry(string filename, string text)
            {
                FileName = filename;
                Text = text;
                Dependencies = new string[] { };
            }
            public FileEntry(string filename, string text, string dependencies)
            {
                FileName = filename;
                Text = text;
                Dependencies = dependencies.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            }

            public bool Exists()
            {
                return File.Exists(FileName) || Directory.Exists(FileName);
            }
            public bool isContainsText()
            {
                if (File.Exists(FileName))
                {
                    if (string.IsNullOrEmpty(Text))
                    {
                        return true;
                    }
                    var data = File.ReadAllText(FileName);
                    return data.IndexOf(Text) >= 0;
                }
                return false;
            }

            public bool CheckDependencies(string dependencies)
            {
                if (dependencies == "" || Dependencies.Length == 0)
                {
                    return true;
                }
                var count = 0;
                foreach (var dep in Dependencies)
                {
                    if (dependencies.Contains(dep))
                    {
                        count++;
                    }
                }
                return count == Dependencies.Length;
            }

            public bool CheckAllDependencies(string dependencies)
            {
                if (dependencies == "" || Dependencies.Length == 0)
                {
                    return false;
                }
                var count = 0;
                foreach (var dep in Dependencies)
                {
                    if (dependencies.Contains(dep))
                    {
                        count++;
                    }
                }
                return count == Dependencies.Length;
            }

            internal bool hasDependenies()
            {
                return Dependencies.Length > 0;
            }
        }
        #endregion
    }

}
