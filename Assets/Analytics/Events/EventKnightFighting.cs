using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventKnightFighting
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
        /// Конец раунда.
        /// </summary>
        public void EndOfRound()
        {
            SendEvent("End of Round", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Ничья.
        /// </summary>
        public void Draw()
        {
            SendEvent("Draw", new Dictionary<string, string>() { { "", "" } });
        }
    }
}