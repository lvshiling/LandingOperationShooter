using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AnalyticsPack
{
    public class Ev
    {
        public string name;
        public Dictionary<string, string> paramsCommon;
        public Dictionary<string, string> paramsPackGeneral;
        public Dictionary<string, string> paramsPackEngine;

        public Ev(string name)
        {
            this.name = name;
            paramsCommon = new Dictionary<string, string>();
        }

        public string ToBase64(string s)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
        }

        public string FromBase64(string s)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(s));
        }

        public Dictionary<string, string> GeneratePacks(Dictionary<string, string> eventParams, string ptag)
        {
            List<string> packValues = new List<string>();
            string currentValue = "";
            foreach (var kv in eventParams)
            {
                if (currentValue.Length + kv.Key.Length + kv.Value.Length + 2 >= 188)
                {
                    packValues.Add(currentValue);
                    currentValue = "";
                }
                currentValue += kv.Key + ":" + kv.Value + ";";
            }
            if (currentValue != "")
            {
                packValues.Add(currentValue);
            }

            Dictionary<string, string> packs = new Dictionary<string, string>();
            for (int i = 0; i < packValues.Count; i++)
            {
                if (packValues[i] != "")
                {
                    packs.Add(ptag + (i + 1).ToString(), ToBase64(packValues[i]));
                }
                packValues[i] = "";
            }

            return packs;
        }

        private void DebugLog(string s)
        {
#if UNITY_EDITOR
            Debug.Log(s);
#endif
        }

        public Dictionary<string, string> PackAllParams()
        {
            Dictionary<string, string> allEventParams = new Dictionary<string, string>();

            foreach (var kv in paramsCommon)
            {
                allEventParams.Add(kv.Key, kv.Value);
            }
            foreach (var kv in GeneratePacks(paramsPackGeneral, "pack_general_"))
            {
                allEventParams.Add(kv.Key, kv.Value);
            }
            foreach (var kv in GeneratePacks(paramsPackEngine, "pack_engine_"))
            {
                allEventParams.Add(kv.Key, kv.Value);
            }

            return allEventParams;
        }

        public override string ToString()
        {
            string res = "[EVENT]\t" + name;
            if (paramsCommon.Keys.Count > 0)
            {
                res += "\n[PARAMS]";
            }
            foreach (var kv in paramsCommon)
            {
                res += "\n\t" + kv.Key + ":" + kv.Value;
            }
            if (paramsPackGeneral.Keys.Count > 0)
            {
                res += "\n[GENERAL]";
            }
            foreach (var kv in paramsPackGeneral)
            {
                res += "\n\t" + kv.Key + ":" + kv.Value;
            }
            if (paramsPackEngine.Keys.Count > 0)
            {
                res += "\n[ENGINE]";
            }
            foreach (var kv in paramsPackEngine)
            {
                res += "\n\t" + kv.Key + ":" + kv.Value;
            }

            return res;
        }
    }
}