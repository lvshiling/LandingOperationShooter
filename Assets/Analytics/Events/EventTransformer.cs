using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventTransformer
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Поражение.
        /// </summary>
        public void LoseLevel(int jump_count, int punch_count, int kick_count, int gun_hit_count)
        {
            SendEvent("Lose Level", new Dictionary<string, string>() {
                { "jump_count", jump_count.ToString() },
                { "punch_count", punch_count.ToString() },
                { "kick_count", kick_count.ToString() },
                { "gun_hit_count", gun_hit_count.ToString() }
            });
        }

        /// <summary>
        /// Взятие бонуса восстанавливающего целостность.
        /// </summary>
        public void GetIntegrityBonus()
        {
            SendEvent("Get Integrity Bonus", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Взятие бонуса восстанавливающего броню.
        /// </summary>
        public void GetShieldBonus()
        {
            SendEvent("Get Shield Bonus", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Взятие бонуса восстанавливающего энергию.
        /// </summary>
        public void GetEnergyBonus()
        {
            SendEvent("Get Energy Bonus", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Событие вызывается когда игрок убил любого юнита в игре.
        /// </summary>
        public void KillUnit(int aggresive)
        {
            SendEvent("Kill Unit", new Dictionary<string, string>() { { "aggresive", aggresive.ToString() } });
        }
    }
}