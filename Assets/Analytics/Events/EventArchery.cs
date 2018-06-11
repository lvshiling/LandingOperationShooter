using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventArchery
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Поражение.
        /// </summary>
        public void LoseLevel(int reason)
        {
            SendEvent("Lose Level", new Dictionary<string, string>() { { "reason", reason.ToString() } });
        }
    }
}