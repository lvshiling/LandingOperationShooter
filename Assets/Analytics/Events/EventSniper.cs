using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventSniper
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
        public void KillUnit(int aggressive)
        {
            SendEvent("Kill Unit", new Dictionary<string, string>() { { "aggressive", aggressive.ToString() } });
        }

        /// <summary>
        /// Событие вызывается когда игрок нажал на кнопку задержки дыхания.
        /// </summary>
        public void BreathHolding()
        {
            SendEvent("Breath Holding", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Штраф, если есть.
        /// </summary>
        public void Penalty(int size)
        {
            SendEvent("Penalty", new Dictionary<string, string>() { { "size", size.ToString() } });
        }
    }
}