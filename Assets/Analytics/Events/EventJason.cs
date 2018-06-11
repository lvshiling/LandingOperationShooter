using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventJason
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Игрок почти прошел уровень, но всеравно проиграл.
        /// </summary>
        public void AlmostPassed()
        {
            SendEvent("Almost Passed", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Событие звонка в полицию.
        /// </summary>
        public void CallToPolice()
        {
            SendEvent("Call to Police", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Событие убийства юнита.
        /// </summary>
        public void Kill()
        {
            SendEvent("Kill", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Событие убийства юнита.
        /// </summary>
        public void WinLevel(int difficulty_bonus, int no_cops_bonus)
        {
            SendEvent("Win Level", new Dictionary<string, string>() { { "difficulty_bonus", difficulty_bonus .ToString() }, { "no_cops_bonus", no_cops_bonus.ToString() } });
        }
    }
}