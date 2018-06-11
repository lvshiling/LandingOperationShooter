using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Assets.Editor.GBNEditor;

namespace Assets.Editor.GBNEditor
{
    public static class GBNEditorAccounts
    {
        public static class Git
        {
            public static string apiUrl = "https://gitlab.gbn.company";

            private static string prefsKey = "gitUserAccount";
            private static AccountInfo _account;
            public static AccountInfo account
            {
                get
                {
                    if (_account == null)
                    {
                        _account = new AccountInfo(prefsKey);
                    }
                    return _account;
                }
                set
                {
                    if (value != null)
                    {
                        _account = value;
                    }
                    if (_account != null)
                    {
                        _account.SaveAccountToPrefs();
                    }
                }
            }

            public static bool Login(string token)
            {
                account.privateToken = token;
                if (Check())
                {
                    account.SaveAccountToPrefs();
                    return true;
                }
                else
                {
                    account.LoadAccountFromPrefs();
                    return false;
                }
            }

            public static bool Check()
            {
                bool authFailed = true;

                string urlForApi = apiUrl + "/api/v3";
                try
                {
                    string token = "private_token=" + account.privateToken;
                    string userInfoResponse = GBNEditorNetwork.GetRequest(urlForApi + "/user", "?" + token);

                    if (ResponseIsValid(userInfoResponse))
                    {
                        JSONObject responseJson = new JSONObject(userInfoResponse);
                        if (responseJson.HasField("username"))
                        {
                            account.name = responseJson.GetField("username").str;
                        }
                        authFailed = false;
                    }
                    else
                    {
                        authFailed = true;
                    }
                }
                catch (Exception ex)
                {

                }
                return !authFailed;
            }

            private static bool ResponseIsValid(string response)
            {
                if (response.Contains("Not Found"))
                {
                    return false;
                }
        
                JSONObject responseJson = new JSONObject(response);

                if (responseJson != null && !responseJson.IsNull)
                {
                    if (responseJson.HasField("id"))
                    {
                        return true;
                    }
                    else if (responseJson.HasField("message"))
                    {
                        
                    }
                }
                else
                {

                }
                
                return false;
            }
        }

        public class Jira
        {
            public static string apiUrl = "https://jira.gbn.company/rest/api/latest";

            private static string prefsKey = "jiraUserAccount";
            private static AccountInfo _account;
            public static AccountInfo account
            {
                get
                {
                    if (_account == null)
                    {
                        _account = new AccountInfo(prefsKey);
                    }
                    return _account;
                }
                set
                {
                    if (value != null)
                    {
                        _account = value;
                    }
                    if (_account != null)
                    {
                        _account.SaveAccountToPrefs();
                    }
                }
            }

            public static bool Login(string username, string password)
            {
                account.name = username;
                account.privateToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", username, password)));
                if (Check())
                {
                    account.SaveAccountToPrefs();
                    return true;
                }
                else
                {
                    account.LoadAccountFromPrefs();
                    return false;
                }
            }

            public static bool Check()
            {
                if (string.IsNullOrEmpty(account.privateToken))
                    return false;
                string urlForCheck = apiUrl + "/mypermissions";
                UnityWebRequest www = UnityWebRequest.Get(urlForCheck);
                www.SetRequestHeader("Authorization", string.Format("Basic {0}", account.privateToken));
                www.Send();
                while (!www.isDone)
                { }
#if UNITY_2017_1_OR_NEWER
            if (!www.isNetworkError)
#else
                if (!www.isError)
#endif
                {
                    if (www.responseCode == 200)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public class AccountInfo
        {
            private string prefsKey;
            public string name;
            public string privateToken;
            public string server;

            public void Clear()
            {
                name = string.Empty;
                privateToken = string.Empty;
                server = string.Empty;
                if (EditorPrefs.HasKey(prefsKey))
                {
                    EditorPrefs.DeleteKey(prefsKey);
                }
            }

            public bool IsEmpty()
            {
                return string.IsNullOrEmpty(name) && string.IsNullOrEmpty(privateToken) && string.IsNullOrEmpty(server);
            }

            public void SaveAccountToPrefs()
            {
                string list = new JSONObject(ToDictionary()).ToString();
                EditorPrefs.SetString(prefsKey, list);
            }

            public bool LoadAccountFromPrefs()
            {
                JSONObject list = null;
                if (EditorPrefs.HasKey(prefsKey))
                {
                    list = new JSONObject(EditorPrefs.GetString(prefsKey));
                }
                if ((list != null) && (!list.IsNull))
                {
                    Dictionary<string, string> source = list.ToDictionary();
                    name = source["user"];
                    privateToken = source["private_token"];
                    server = source["server"];
                    return true;
                }
                return false;
            }
            public AccountInfo(string prefsKey)
            {
                name = string.Empty;
                privateToken = string.Empty;
                server = string.Empty;
                this.prefsKey = prefsKey;
                LoadAccountFromPrefs();
            }
            public AccountInfo(Hashtable source)
            {
                name = (string)source["user"];
                privateToken = (string)source["private_token"];
                server = (string)source["server"];
            }
            public AccountInfo(Dictionary<string, string> source)
            {
                name = source["user"];
                privateToken = source["private_token"];
                server = source["server"];
            }
            public Hashtable ToHashtable()
            {
                Hashtable result = new Hashtable();
                result.Add("user", name);
                result.Add("private_token", privateToken);
                result.Add("server", server);
                return result;
            }
            public Dictionary<string, string> ToDictionary()
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                result.Add("user", name);
                result.Add("private_token", privateToken);
                result.Add("server", server);
                return result;
            }
        }
    }
}