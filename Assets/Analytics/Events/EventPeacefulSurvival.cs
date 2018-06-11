using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventPeacefulSurvival
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Убийство игроком любого персонажа.
        /// </summary>
        public void KillUnit(int unit)
        {
            SendEvent("Kill Unit", new Dictionary<string, string>() { { "unit", unit.ToString() } });
        }

        /// <summary>
        /// Подбор предмета с земли (дерево, камень, мясо убитого юнита и т.д.).
        /// </summary>
        public void FindItem(int item)
        {
            SendEvent("Find Item", new Dictionary<string, string>() { { "item", item.ToString() } });
        }

        /// <summary>
        /// Использование предмета.
        /// </summary>
        public void UseItem(int item, int action)
        {
            SendEvent("Use Item", new Dictionary<string, string>() { { "item", item.ToString() }, { "action", action.ToString() } });
        }

        /// <summary>
        /// Сброс мира.
        /// </summary>
        public void ResetWorld()
        {
            SendEvent("Reset World", new Dictionary<string, string>() { { "", "" } });
        }
    }
}