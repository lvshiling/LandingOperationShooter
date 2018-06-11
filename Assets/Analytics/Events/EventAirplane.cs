using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventAirplane
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Игрок почти выиграл, но все же проиграл.
        /// </summary>
        public void AlmostPassed()
        {
            SendEvent("Almost Passed", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Смена режима управления автомобиль/самолет.
        /// </summary>
        public void ChangeControlMode()
        {
            SendEvent("Change Control Mode", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Прохождение кольца.
        /// </summary>
        public void RingPassed()
        {
            SendEvent("Ring Passed", new Dictionary<string, string>() { { "", "" } });
        }
    }
}