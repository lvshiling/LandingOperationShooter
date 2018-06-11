using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GBNAPI
{
    public class CompanyInfo
    {
#if FINAL_VERSION
		static bool printDebug = false;
#else
        static bool printDebug = true;
#endif
        private static readonly string fileName = "companyInfo";

        public struct CompanyInfoStruct
        {
            public readonly string key;

            private readonly string _youtube;
            public string youtube
            {
                get
                {
                    return Crypt.GBNDecrypt(_youtube, key);
                }
            }

            private readonly string _cooltool;
            public string cooltool
            {
                get
                {
                    return Crypt.GBNDecrypt(_cooltool, key);
                }
            }

            private readonly string _name;
            public string name
            {
                get
                {
                    return Crypt.GBNDecrypt(_name, key);
                }
            }
            private readonly string _store;
            public string store
            {
                get
                {
                    return Crypt.GBNDecrypt(_store, key);
                }
            }
            private readonly string _policy;
            public string policy
            {
                get
                {
                    return Crypt.GBNDecrypt(_policy, key);
                }
            }
            private readonly string _email;
            public string email
            {
                get
                {
                    return Crypt.GBNDecrypt(_email, key);
                }
            }
            private readonly string _url;
            public string url
            {
                get
                {
                    return Crypt.GBNDecrypt(_url, key);
                }
            }
            private readonly string _moregames;
            public string moregames
            {
                get
                {
                    return Crypt.GBNDecrypt(_moregames, key);
                }
            }

            public bool IsValid()
            {
                if (name == null)
                {
                    return false;
                }
                string aName = Application.companyName.ToLower().Trim().Replace(" ", "");
                string bName = name.ToLower().Trim().Replace(" ", "");
                return aName.Equals(bName);
            }

            public CompanyInfoStruct(string key, string name, string store, string policy, string email, string url, string youtube, string cooltool, string moregames = "")
            {
                this.key = key;
                _name = name;
                _store = store;
                _policy = policy;
                _email = email;
                _url = url;
                _moregames = moregames;
                _youtube = youtube;
                _cooltool = cooltool;
            }

            public CompanyInfoStruct(JSONObject jObj)
            {
                key = jObj.GetField("info_key").str;
                _name = jObj.GetField("info_name").str;
                _store = jObj.GetField("info_store").str;
                if (jObj.HasField("info_gdprpolicy") && !string.IsNullOrEmpty(jObj.GetField("info_gdprpolicy").str))
                {
                    _policy = jObj.GetField("info_gdprpolicy").str;
                }
                else
                {
                    _policy = jObj.GetField("info_policy").str;
                }

                _email = jObj.GetField("info_email").str;
                _url = jObj.GetField("info_url").str;
                if (jObj.HasField("info_moregames"))
                {
                    _moregames = jObj.GetField("info_moregames").str;
                }
                else
                {
                    _moregames = "";
                }
                _youtube = jObj.GetField("info_youtube").str;
                _cooltool = jObj.GetField("info_cooltool").str;
            }

            public override string ToString()
            {
                return "Key: " + key + "; " + "Name: " + name + "; " + "Store: " + store + "; " + "Policy URL: " + policy + "; " + "E-Mail: " + email + "; " + "URL: " + url + "; " + "More Games URL: " + moregames + "; " + "YouTube URL: " + youtube + "; " + "CoolTool URL: " + cooltool;
            }

            public Dictionary<string, string> ToDictionary()
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();

                dict.Add("key", key);
                dict.Add("name", name);
                dict.Add("store", store);
                dict.Add("policy", policy);
                dict.Add("email", email);
                dict.Add("url", url);
                dict.Add("youtube", youtube);
                dict.Add("cooltool", cooltool);

                return dict;
            }
        }

        private static CompanyInfoStruct companyInfo;

        private static bool isParsed = false;

        public static string bundleIdentifier
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

        public static CompanyInfoStruct Struct
        {
            get
            {
                if (!isParsed)
                {
                    ParseInternal();
                }
                return companyInfo;
            }
        }

        [System.Obsolete("GetStruct() is deprecated, please use Struct instead.")]
        public static CompanyInfoStruct GetStruct()
        {
            return Struct;
        }

        public static void Parse()
        {
            if (!isParsed)
            {
                ParseInternal();
            }
        }

        static void ParseInternal()
        {
#if !UNITY_EDITOR
            isParsed = true;
#endif
            TextAsset companyInfoFile = Resources.Load<TextAsset>(fileName);
            if (companyInfoFile != null)
            {
                JSONObject companyInfoJson = new JSONObject(companyInfoFile.text);
                string[] fields = { "info_name", "info_store", "info_policy", "info_email", "info_url" };
                if (companyInfoJson != null && companyInfoJson.HasFields(fields))
                {
                    companyInfo = new CompanyInfoStruct(companyInfoJson);
                    if (printDebug)
                    {
                        Debug.Log("Assets/Resources/" + fileName + ".txt is parsed successfully!");
                    }
                }
                else
                {
                    if (printDebug)
                    {
                        Debug.LogError("Assets/Resources/" + fileName + ".txt is empty or damaged!");
                    }
                    return;
                }
            }
            else
            {
                if (printDebug)
                {
                    Debug.LogError("Assets/Resources/" + fileName + ".txt is missing!");
                }
                return;
            }

            if (!companyInfo.IsValid())
            {
                if (printDebug)
                {
                    Debug.LogError("Assets/Resources/" + fileName + ".txt contains wrong information!");
                }
                return;
            }
        }
    }
}
