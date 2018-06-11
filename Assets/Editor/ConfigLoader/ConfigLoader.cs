using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Editor.GBNEditor;
using Ionic.Zip;
using System.Linq;

namespace Assets.Editor.ConfigLoader
{
    public class ConfigLoader
    {
        public static string cloudConnectorWebServiceUrl = "https://script.google.com/macros/s/AKfycbzF6WJBGvpK7I6dchtFzRjsPWRdsSCJt-u1Hrbz-a0OYCjOfMdL/exec";
        public static string cloudConnectorServicePassword = "hv9OrrBp";

        private static string configFileName = "Config.txt";
        public static string baseUrl = "https://jira.gbn.company/rest/api/latest/";
        public static string gitApiUrl = "https://gitlab.gbn.company/";
        public static string issueId
        {
            get
            {
                if (!config.IsLoaded)
                {
                    if (!config.Load(configFileName))
                    {
                        return "";
                    }
                    else
                    {
                        Debug.Log("<color=green><b>Файл Asset/" + configFileName + " загружен!</b></color>");
                    }
                }
                return config.GetParam(Config.epicId);
            }
            private set
            {
                config.ChangeEpicID(configFileName, value);
            }
        }
        private static string httpBase
        {
            get
            {
                return string.Format("Basic {0}", GBNEditorAccounts.Jira.account.privateToken);
            }
        }
        private static bool isLoading = false;
        private static bool isConnectionError = false;

        private static string _projectPath = "";
        public static string projectPath
        {
            get
            {
                if (string.IsNullOrEmpty(_projectPath))
                {
                    var items = Application.dataPath.Split(new string[] { "/" }, StringSplitOptions.None);
                    _projectPath = string.Join("/", items, 0, items.Length - 1);
                    //_projectPathWin = string.Join("\\", items, 0, items.Length - 1);
                }
                return _projectPath;
            }
        }

        private static UnityWebRequest www;

        private static string lastUrl;

        private static Config _config = null;


        public static Config config
        {
            get
            {
                if (_config == null)
                {
                    _config = new Config();

                }
                /*
                if (EditorApplication.update != null)
                {
                    EditorApplication.update -= AutoUpdateConfigFromJira;
                }
                EditorApplication.update += AutoUpdateConfigFromJira;
                */
                return _config;
            }
        }

        [MenuItem("Config/Загрузить|Обновить accounts_info.txt", false, 3)]
        public static void UpdateAccountsInfoFile()
        {
            string downloadPath = projectPath + "/gitTemp.zip";
            string gitGroup = "unityprojects";
            string gitProject = "AccountsInfoFile";
            string keysPath = "/Assets/Editor";
            string keysDirName = "ConfigLoader";
            if (GBNEditorAccounts.Git.account.IsEmpty())
            {
                Debug.Log("<color=red><b>Необходимо ввести Git Token!</b></color>");
                JiraAuthWindow.ShowWindow();
                return;
            }

            string oldFile = "";
            string newFile = "";

            if (File.Exists(Config.accountsInfoFile))
            {
                oldFile = File.ReadAllText(Config.accountsInfoFile);
            }

            string token = GBNEditorAccounts.Git.account.privateToken;
            string downloadArchiveMessage = GBNEditorNetwork.FileGetRequest(gitApiUrl + "/" + gitGroup + "/" + gitProject + "/repository/archive.zip",
                                                                                "?ref=master" + "&private_token=" + token, downloadPath);
            bool error = false;

            if (downloadArchiveMessage != string.Empty)
            {
                //OK
            }
            else
            {
                Debug.Log("accounts_info.txt archive download failed");
                error = true;
            }

            using (ZipFile archive = ZipFile.Read(downloadPath))
            {
                string newEntryFirstDir = keysDirName;

                List<ZipEntry> zipEntriesList = archive.Entries.ToList();
                for (int i = 0; i < zipEntriesList.Count; i++)
                {
                    ZipEntry entry = zipEntriesList[i];

                    string oldEntryFirstDir = entry.FileName.Substring(0, entry.FileName.IndexOf('/'));

                    entry.FileName = entry.FileName.Replace(oldEntryFirstDir, newEntryFirstDir);
                }

                archive.Save();
            }

            using (ZipFile archive = ZipFile.Read(downloadPath))
            {
                try
                {
                    archive.ExtractAll(projectPath + keysPath + "/", ExtractExistingFileAction.OverwriteSilently);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Exception on extracting archive: " + ex.ToString());
                    error = true;
                }
            }

            try
            {
                File.Delete(downloadPath);
            }
            catch (Exception)
            {
                Debug.LogError("Exception on deleting archive");
            }

            if (!error)
            {
                string res = "<b>accounts_info.txt updated successfully!</b>";
                if (File.Exists(Config.accountsInfoFile) && !string.IsNullOrEmpty(oldFile))
                {
                    newFile = File.ReadAllText(Config.accountsInfoFile);
                    if (!oldFile.Equals(newFile))
                    {
                        res += " (new != old)";
                    }
                    else
                    {
                        res += " (new == old)";
                    }
                }
                Debug.Log(res);
            }
        }

        [MenuItem("Config/Загрузить|Обновить KeyStores", false, 3)]
        public static void UpdateKeyStores()
        {
            string downloadPath = projectPath + "/gitTemp.zip";
            string gitGroup = "unityprojects";
            string gitProject = "KeyStores";
            string keysPath = "";
            string keysDirName = "Keys";
            if (GBNEditorAccounts.Git.account.IsEmpty())
            {
                Debug.Log("<color=red><b>Необходимо ввести Git Token!</b></color>");
                JiraAuthWindow.ShowWindow();
                return;
            }
            string token = GBNEditorAccounts.Git.account.privateToken;
            string downloadArchiveMessage = GBNEditorNetwork.FileGetRequest(gitApiUrl + "/" + gitGroup + "/" + gitProject + "/repository/archive.zip",
                                                                                "?ref=master" + "&private_token=" + token, downloadPath);
            bool error = false;

            if (downloadArchiveMessage != string.Empty)
            {
                //OK
            }
            else
            {
                Debug.Log("KeyStores archive download failed");
                error = true;
            }

            using (ZipFile archive = ZipFile.Read(downloadPath))
            {
                string newEntryFirstDir = keysDirName;

                List<ZipEntry> zipEntriesList = archive.Entries.ToList();
                for (int i = 0; i < zipEntriesList.Count; i++)
                {
                    ZipEntry entry = zipEntriesList[i];

                    string oldEntryFirstDir = entry.FileName.Substring(0, entry.FileName.IndexOf('/'));

                    entry.FileName = entry.FileName.Replace(oldEntryFirstDir, newEntryFirstDir);
                }

                archive.Save();
            }

            using (ZipFile archive = ZipFile.Read(downloadPath))
            {
                if (Directory.Exists(projectPath + keysPath + "/" + keysDirName))
                {
                    try
                    {
                        Directory.Delete(projectPath + keysPath + "/" + keysDirName, true);
                    }
                    catch (Exception)
                    {
                        Debug.Log("Exception on delete folder " + projectPath + keysPath + "/" + keysDirName);
                    }
                }
                try
                {
                    archive.ExtractAll(projectPath + keysPath + "/");
                }
                catch (Exception ex)
                {
                    Debug.LogError("Exception on extracting archive: " + ex.ToString());
                    error = true;
                }
            }

            try
            {
                File.Delete(downloadPath);
            }
            catch (Exception)
            {
                Debug.LogError("Exception on deleting archive");
            }

            if (!error)
            {
                Debug.Log("<b>KeyStores updated successfully!</b>");
            }
        }

        private static string GetConfigCsVersion()
        {
            var configPath = "Assets/Editor/ConfigLoader/Config.cs";
            if (File.Exists(configPath))
            {
                var lines = File.ReadAllLines(configPath);
                for (var i = 0; i < lines.Length; i++)
                {
                    if (lines[i].IndexOf("public static string version") >= 0)
                    {
                        var first = lines[i].IndexOf("\"");
                        var last = lines[i].IndexOf("\"", first + 1);
                        if (last > first)
                        {
                            var version = lines[i].Substring(first + 1, last - first - 1);
                            return version;
                        }
                    }
                }
            }
            return Config.version;
        }

        public static void AutoUpdateConfigFromJira()
        {
            if (isAuthorized() && !isLoading && !isConnectionError)
            {
                if (GetConfigCsVersion() != config.GetConfigTxtVersion())
                {
                    Debug.Log("Autoupdate " + configFileName + " to version " + GetConfigCsVersion());
                    Load();
                }
            }
        }

        private static bool isAuthorized()
        {
            var authorized = true;
            if (issueId == "" || !GBNEditorAccounts.Jira.Check())
            {
                authorized = false;
            }
            return authorized;
        }

        [MenuItem("Config/Загрузить Config из Jira", false, 2)]
        private static void LoadJiraEpic()
        {
            if (isAuthorized())
            {
                Load();
            }
            else
            {
                Debug.LogWarning("Перед загрузкой необходимо авторизоваться в JIRA и задать Epic ID!");
                JiraAuthWindow.ShowWindow();
            }
        }

        [MenuItem("Config/Загрузить Config из проекта", false, 2)]
        public static void LoadConfig()
        {
            if (!GBNEditorAccounts.Git.account.IsEmpty() && Environment.GetCommandLineArgs().Contains("-batchmode"))  //здесь обновляем только в батч-режиме
            {
                try
                {
                    UpdateAccountsInfoFile();
#if UNITY_ANDROID
                    UpdateKeyStores();
#endif
                }
                catch
                {
                    Debug.Log("Can't update accounts_info file and keystores from GIT!");
                }
            }

            if (!config.Load(configFileName))
            {
                Debug.LogError("Файл Asset/" + configFileName + " не найден!");
            }
            else
            {
                Debug.Log("<color=green><b>Файл Asset/" + configFileName + " загружен!</b></color>");
            }
        }

        public static void LoadConfigIfNotLoaded()
        {
            if (!config.IsLoaded)
            {
                LoadConfig();
            }
        }

        public static void ReloadConfig()
        {
            LoadConfig();
            SaveConfig();
        }
        
        internal static void SaveAdditiveScenes(string additiveScenes)
        {
            if (config.GetParam(Config.additiveScenes) != additiveScenes)
            {
                LoadConfigIfNotLoaded();
                config.SetParam(Config.additiveScenes, additiveScenes, true);
                SaveConfig();
            }
        }

        public static bool IsCorrectBundle(string bundle)
        {
            if (!string.IsNullOrEmpty(bundle))
            {
                Regex regex = new Regex(@"[a-z]\w+\.[a-z]\w+\.[a-z]\w+");
                Match match = regex.Match(bundle);
                return match.Success && match.Value == bundle;
            }
            return false;
        }
        public static bool IsCorrectTitle(string title)
        {
            if (!string.IsNullOrEmpty(title))
            {
                Regex regex = new Regex(@"[A-Za-z0-9 -]+");
                Match match = regex.Match(title);
                return match.Success && match.Value == title;
            }
            return false;
        }

        public static bool IsCorrectIntId(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                Regex regex = new Regex(@"[0-9]+");
                Match match = regex.Match(id);
                return match.Success && match.Value == id;
            }
            return false;
        }

        public static bool IsValidUnityAdsKey(string key)
        {
            //1541679
            if (!string.IsNullOrEmpty(key))
            {
                Regex regex = new Regex(@"[0-9]{7,}");
                Match match = regex.Match(key);
                return match.Success && match.Value == key;
            }
            return false;
        }

        public static string GetKeyStoreFilepath(string bundle)
        {
            string aliasName = GetAliasFromBundle(bundle);
            var keystoreData = GetKeyStore(aliasName);
            if (keystoreData.Length <= 0)
            {
                Debug.LogError("Keystore " + aliasName + " not found!");
                return "";
            }

            if (keystoreData.Length < 3)
            {
                Debug.LogError("Invalid Keystore List format for entry: " + aliasName + " not found!");
                return "";
            }
            return Config.keystoresFolder + "/" + keystoreData[2];
        }

        public static string GetAliasFromBundle(string bundle)
        {
            var data = bundle.Split(new string[] { "." }, 3, StringSplitOptions.None);
            if (data.Length < 3)
            {
                Debug.LogError("Invalid bundle format: " + bundle);
                return "";
            }
            return data[1];
        }

        public static string[] GetKeyStore(string alias)
        {
            alias = alias.ToLower().Replace(" ", "");
            if (File.Exists(Config.keystoresFolder + "/" + Config.keystoresList))
            {
                var lines = File.ReadAllLines(Config.keystoresFolder + "/" + Config.keystoresList);
                for (var i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].ToLower().Replace(" ", "");
                    if (line.IndexOf(alias) >= 0)
                    {
                        return lines[i].Split(new string[] { ";" }, StringSplitOptions.None);
                    }
                }
            }
            else
            {
                Debug.LogError("File " + Config.keystoresFolder + "/" + Config.keystoresList + " does not exist!");
            }
            return new string[] { };
        }

        public static string[] GetAppStoreAccount()
        {
            string account = config.GetParam(Config.asAccountFree);

            if (File.Exists(Config.accountsInfoFile))
            {
                string text = File.ReadAllText(Config.accountsInfoFile);
                JSONObject jObj = new JSONObject(text);
                string[] fields = { "account", "distribution", "development" };
                bool isValid = false;
                if (jObj != null && jObj.HasField("stores"))
                {
                    jObj = jObj.GetField("stores");
                    if (jObj.HasField("apple"))
                    {
                        jObj = jObj.GetField("apple");
                        if (jObj.HasField("teams"))
                        {
                            jObj = jObj.GetField("teams");
                            if (jObj.IsArray)
                            {
                                isValid = true;
                                for (int i = 0; i < jObj.Count; i++)
                                {
                                    if (jObj[i].HasFields(fields))
                                    {
                                        string checkAccount = jObj[i].GetField(fields[0]).str;
                                        if (checkAccount.Equals(account))
                                        {
                                            return new string[] { checkAccount, jObj[i].GetField(fields[1]).str, jObj[i].GetField(fields[2]).str };
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (!isValid)
                {
                    Debug.LogError("File " + Config.accountsInfoFile + " contains wrong AppStore config information!");
                }
            }
            else
            {
                Debug.LogError("File " + Config.accountsInfoFile + " does not exist! Can't parse AppStore config!");
            }

            return new string[] { };
        }

        internal static string CheckAccountsInfoFile()
        {
            //valid store values are
            //"Amazon"
            //"Google"
            //"Apple"

            bool gpAccountFound = false;
            bool amAccountFound = false;
            bool asAccountFreeFound = false;
            string gpAccount = config.GetParam(Config.gpAccount).ToLower().Trim().Replace(" ", "");
            string amAccount = config.GetParam(Config.amAccount).ToLower().Trim().Replace(" ", "");
            string asAccountFree = config.GetParam(Config.asAccountFree).ToLower().Trim().Replace(" ", "");

            string res = "OK";

            if (File.Exists(Config.accountsInfoFile))
            {
                string text = File.ReadAllText(Config.accountsInfoFile);
                JSONObject jObj = new JSONObject(text);
                string[] fields = { "name", "store", "policy", "email", "url" };
                bool isValid = false;
                if (jObj != null && jObj.IsObject)
                {
                    if (jObj.HasField("urls"))
                    {
                        jObj = jObj.GetField("urls");
                        if (jObj.HasField("companies"))
                        {
                            jObj = jObj.GetField("companies");
                            if (jObj.IsArray)
                            {
                                isValid = true;
                                for (int i = 0; i < jObj.Count; i++)
                                {
                                    if (jObj[i].HasFields(fields))
                                    {
                                        string cName = jObj[i].GetField("name").str.ToLower().Trim().Replace(" ", "");
                                        string cStore = jObj[i].GetField("store").str;
                                        if (!gpAccountFound && cName.Equals(gpAccount) && cStore.Equals("Google"))
                                        {
                                            gpAccountFound = true;
                                            continue;
                                        }
                                        if (!amAccountFound && cName.Equals(amAccount) && cStore.Equals("Amazon"))
                                        {
                                            amAccountFound = true;
                                            continue;
                                        }
                                        if (!asAccountFreeFound && cName.Equals(asAccountFree) && cStore.Equals("Apple"))
                                        {
                                            asAccountFreeFound = true;
                                            continue;
                                        }
                                    }
                                }
                                if (!gpAccountFound || !amAccountFound || !asAccountFreeFound)
                                {
                                    res = "ERROR\nFile " + Config.accountsInfoFile + " contains no information about\n";
                                    if (!gpAccountFound)
                                    {
                                        string accName = config.GetParam(Config.gpAccount);
                                        res += "" + (string.IsNullOrEmpty(accName) ? "EMPTY_NAME" : accName) + " for Google ";
                                    }
                                    if (!amAccountFound)
                                    {
                                        string accName = config.GetParam(Config.amAccount);
                                        res += "" + (string.IsNullOrEmpty(accName) ? "EMPTY_NAME" : accName) + " for Amazon ";
                                    }
                                    if (!asAccountFreeFound)
                                    {
                                        string accName = config.GetParam(Config.asAccountFree);
                                        res += "" + (string.IsNullOrEmpty(accName) ? "EMPTY_NAME" : accName) + " for Apple ";
                                    }
                                }
                            }
                        }
                    }
                    if (!isValid)
                    {
                        res = "ERROR\nFile " + Config.accountsInfoFile + " contains data in wrong format!";
                    }
                }
                else
                {
                    res = "ERROR\nFile " + Config.accountsInfoFile + " is not json formatted!";
                }
            }
            else
            {
                res = "ERROR\nFile " + Config.accountsInfoFile + " does not exist!";
            }

            return res;
        }

        internal static string CheckAppStoreTeamsInfo()
        {
            bool accountFound = false;
            bool accountValid = false;

            string asAccountFree = config.GetParam(Config.asAccountFree);

            string res = "OK";

            if (File.Exists(Config.accountsInfoFile))
            {
                string text = File.ReadAllText(Config.accountsInfoFile);
                JSONObject jObj = new JSONObject(text);
                string[] fields = { "account", "distribution", "development" };
                bool isValid = false;
                if (jObj != null && jObj.IsObject)
                {
                    if (jObj.HasField("stores"))
                    {
                        jObj = jObj.GetField("stores");
                        if (jObj.HasField("apple"))
                        {
                            jObj = jObj.GetField("apple");
                            if (jObj.HasField("teams"))
                            {
                                jObj = jObj.GetField("teams");
                                if (jObj.IsArray)
                                {
                                    isValid = true;
                                    for (int i = 0; i < jObj.Count; i++)
                                    {
                                        if (jObj[i].HasFields(fields))
                                        {
                                            string account = jObj[i].GetField(fields[0]).str;
                                            if (asAccountFree.Equals(account))
                                            {
                                                accountFound = true;
                                                if (!string.IsNullOrEmpty(jObj[i].GetField(fields[1]).str) && !string.IsNullOrEmpty(jObj[i].GetField(fields[2]).str))
                                                {
                                                    accountValid = true;
                                                }
                                                break;
                                            }
                                        }
                                    }
                                    if (!accountFound)
                                    {
                                        string accName = config.GetParam(Config.asAccountFree);
                                        res = "ERROR\nFile " + Config.accountsInfoFile + " contains no information about " + (string.IsNullOrEmpty(accName) ? "EMPTY_NAME" : accName) + " team!";
                                    }
                                    else if (!accountValid)
                                    {
                                        string accName = config.GetParam(Config.asAccountFree);
                                        res = "ERROR\nFile " + Config.accountsInfoFile + " contains invalid information about " + (string.IsNullOrEmpty(accName) ? "EMPTY_NAME" : accName) + " team!";
                                    }
                                }
                            }
                        }
                    }
                    if (!isValid)
                    {
                        res = "ERROR\nFile " + Config.accountsInfoFile + " contains data in wrong format!";
                    }
                }
                else
                {
                    res = "ERROR\nFile " + Config.accountsInfoFile + " is not json formatted!";
                }
            }
            else
            {
                res = "ERROR\nFile " + Config.accountsInfoFile + " does not exist!";
            }

            return res;
        }

        private static string RecursiveFindPath(string startPath, string fileName)
        {
            var files = Directory.GetFiles(startPath, fileName, SearchOption.AllDirectories);
            if (files.Length > 0) return files[0];

            return "";
        }

        public static void SaveIssueId(string jiraIssueId)
        {
            if (jiraIssueId == "")
            {
                Debug.LogError("Jira Epic ID is empty!");
                return;
            }
            jiraIssueId = AddPrefix(jiraIssueId);
            if (!MatchJiraEpicIdFormat(jiraIssueId))
            {
                Debug.LogError("Wrong format of Jira Epic ID: " + jiraIssueId);
                return;
            }
            issueId = jiraIssueId;
            Debug.Log("<color=green><b>Epic ID = " + issueId + " Saved!</b></color>");
        }

        public static void SaveJiraAuth()
        {
            LoadJiraEpic();
        }

        private static string AddPrefix(string jiraIssueId)
        {
            int num;
            if (int.TryParse(jiraIssueId, out num)) return "G3D-" + jiraIssueId;
            return jiraIssueId;
        }

        private static bool MatchJiraEpicIdFormat(string jiraIssueId)
        {
            if (jiraIssueId.Length < 4)
                return false;
            var left = jiraIssueId.Substring(0, 4);
            var right = jiraIssueId.Substring(4);
            if (left != "G3D-")
                return false;
            int num;
            if (!int.TryParse(right, out num))
                return false;
            return true;
        }

        private static void Load()
        {
            isLoading = true;
            isConnectionError = false;
            LoadConfig();
            lastUrl = baseUrl + "issue/" + issueId;
            GetIssue(lastUrl);
            EditorApplication.update += EditorUpdate;
        }

        private static void GetIssue(string url)
        {
            www = UnityWebRequest.Get(url);
            www.SetRequestHeader("Authorization", httpBase);
            www.Send();
        }

        private static void EditorUpdate()
        {
            if (!www.isDone)
            {
                return;
            }

            isLoading = false;

            if (EditorApplication.update != null)
                EditorApplication.update -= EditorUpdate;

#if UNITY_2017_1_OR_NEWER
            if (www.isNetworkError)
#else
            if (www.isError)
#endif
            {
                Debug.LogError("ConfigLoader.GetIssue Error: " + www.error);
            }
            else
            {
                if (www.responseCode == 200)
                {
                    Debug.Log("<color=green><b>Jira Issue " + issueId + " loaded successfully!</b></color>");
                    ParseConfig(www.downloadHandler.text);
                    SaveConfig();
                }
                else if (www.responseCode == 401)
                {
                    Debug.LogError("Error 401: Unauthorized");
                    Debug.Log("Jira response: " + www.downloadHandler.text);
                    isConnectionError = true;
                }
                else if (www.responseCode == 404)
                {
                    Debug.LogError("Error 404: Jira Issue " + issueId + " not found");
                    Debug.Log("Jira response: " + www.downloadHandler.text);
                    isConnectionError = true;
                }
                else
                {
                    Debug.LogError("Request failed (status:" + www.responseCode + ")");
                    Debug.Log("Jira response: " + www.downloadHandler.text);
                    isConnectionError = true;
                }
            }
        }

        public static void SaveConfig()
        {
            config.Save(configFileName);
        }

        private static void ParseConfig(string text)
        {
            config.Parse(text, issueId);
        }
    }
}
