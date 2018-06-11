using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventSurgery
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Игрок почти прошел уровень, но проиграл.
        /// </summary>
        public void AlmostPassed()
        {
            SendEvent("Almost Passed", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Событие вызывается когда игрок нажал на кнопку действия на панели.
        /// </summary>
        public void Action(int type)
        {
            SendEvent("Action", new Dictionary<string, string>() { { "type", type.ToString() } });
        }
    }
}