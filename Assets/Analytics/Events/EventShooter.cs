using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventShooter
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Игрок почти прошел уровень, но все же проиграл.
        /// </summary>
        public void AlmostPassed()
        {
            SendEvent("Almost Passed", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Событие вызывается когда игрок убил любого юнита в игре.
        /// </summary>
        public void KillUnit()
        {
            SendEvent("Kill Unit", new Dictionary<string, string>() { { "", "" } });
        }
    }
}