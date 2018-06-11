using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventAngryAnimal
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Событие вызывается когда игрок убил бота
        /// </summary>
        /// <param name="bot_type">Тип бота</param>
        public void Kill(int bot_type)
        {
            SendEvent("Kill", new Dictionary<string, string>() { { "bot_type", bot_type.ToString() } });
        }
    }
}