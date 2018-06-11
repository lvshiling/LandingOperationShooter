using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GBNAPI
{
    public class SDKInfo : MonoBehaviour
    {

#if FINAL_VERSION
        static bool printDebug = false;
#else
        static bool printDebug = true;
#endif
        private static readonly string fileName = "companyInfo";

        private static Dictionary<string, string> parsedData;

        private static bool isParsed = false;

        private static string key = "";

        /// <summary>
        /// Returns all existing fields and keys as dictionary.
        /// </summary>
        /// <param name="fieldName">The name of the field to get the key.</param>
        public static Dictionary<string, string> ToDictionary()
        {
            if (!isParsed)
            {
                ParseInternal();
            }

            if (parsedData != null)
            {
                Dictionary<string, string> output = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> kvp in parsedData)
                {
                    output.Add(kvp.Key, Crypt.GBNDecrypt(kvp.Value, key));
                }
                return output;
            }

            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Returns the key for the specified field name or NULL if the field with the specified name does not exist.
        /// </summary>
        /// <param name="fieldName">The name of the field to get the key.</param>
        public static string GetKey(string fieldName)
        {
            if (!isParsed)
            {
                ParseInternal();
            }

            if (parsedData != null && parsedData.ContainsKey(fieldName))
            {
                string res = "";
                parsedData.TryGetValue(fieldName, out res);
                return Crypt.GBNDecrypt(res, key);
            }
            
            return null;
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
            TextAsset infoFile = Resources.Load<TextAsset>(fileName);

            parsedData = new Dictionary<string, string>();

            if (infoFile != null)
            {
                JSONObject infoJson = new JSONObject(infoFile.text);

                if (infoJson != null && infoJson.HasField("info_sdk"))
                {
                    if (infoJson.HasField("info_key"))
                    {
                        key = infoJson.GetField("info_key").str;
                    }

                    JSONObject sdkObj = infoJson.GetField("info_sdk");

                    if (!sdkObj.IsObject)
                    {
                        if (printDebug)
                        {
                            Debug.LogError("SDK part of Assets/Resources/" + fileName + ".txt contains wrong information!");
                        }
                        return;
                    }

                    parsedData = sdkObj.ToDictionary();

                    if (printDebug)
                    {
                        Debug.Log("SDK part of Assets/Resources/" + fileName + ".txt is parsed successfully!");
                    }
                }
                else
                {
                    if (printDebug)
                    {
                        Debug.Log("Assets/Resources/" + fileName + ".txt has no SDK part!");
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
        }
    }

}
