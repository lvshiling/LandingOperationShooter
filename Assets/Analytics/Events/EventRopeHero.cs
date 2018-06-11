using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventRopeHero
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Успешное выполнение миссии.
        /// </summary>
        public void MissionSuccess(int reward)
        {
            SendEvent("Mission Success", new Dictionary<string, string>() { { "reward", reward.ToString() } });
        }

        /// <summary>
        /// Нажатие кнопки использования веревки.
        /// </summary>
        public void UseRope()
        {
            SendEvent("Use Rope", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Убийство игроком любого юнита в игре.
        /// </summary>
        public void KillUnit(int aggressive)
        {
            SendEvent("Kill Unit", new Dictionary<string, string>() { { "aggressive", aggressive.ToString() } });
        }

        /// <summary>
        /// Открытие карты.
        /// </summary>
        public void LookAtMap()
        {
            SendEvent("Look at Map", new Dictionary<string, string>() { { "", "" } });
        }
    }
}