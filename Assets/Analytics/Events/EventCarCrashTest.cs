using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventCarCrashTest
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Событие вызывается в момент, когда происходит подсчет очков в конце раунда.
        /// </summary>
        public void WinLevel(int reward)
        {
            SendEvent("Win Level", new Dictionary<string, string>() { { "reward", reward.ToString() } });
        }

        /// <summary>
        /// Нажатие кнопки восстановления авто (если есть).
        /// </summary>
        public void TapRestoring()
        {
            SendEvent("Tap Restoring", new Dictionary<string, string>() { { "", "" } });
        }
    }
}