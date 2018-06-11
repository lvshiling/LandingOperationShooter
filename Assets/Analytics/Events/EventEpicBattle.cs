using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventEpicBattle
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Событие вызывается при использовании во время игры глобальной способности.
        /// </summary>
        public void UseGlobal()
        {
            SendEvent("Use Global", new Dictionary<string, string>() { { "", "" } });
        }
    }
}