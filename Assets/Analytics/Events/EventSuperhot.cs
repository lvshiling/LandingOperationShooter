using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventSuperhot
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
        /// Убийство игроком юнита.
        /// </summary>
        public void Kill()
        {
            SendEvent("Kill", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Смерть игрока.
        /// </summary>
        public void Death()
        {
            SendEvent("Death", new Dictionary<string, string>() { { "", "" } });
        }
    }
}