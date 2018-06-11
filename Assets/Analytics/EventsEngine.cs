using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventsEngine : MonoBehaviour
    {
        public Dictionary<string, string> eventsParams;

        public Dictionary<string, string> GetAllParamsFormated()
        {
            eventsParams = Analytics.Instance.extEvent.GetAllParams();
            return eventsParams;
        }

        public static void AddParameter(Dictionary<string, string> p, string key, int val)
        {
            p.Add(key, val.ToString());
        }

        public static void AddParameter(Dictionary<string, string> p, string key, float val)
        {
            p.Add(key, val.ToString(Analytics.FLOAT_FORMAT));
        }
    }
}