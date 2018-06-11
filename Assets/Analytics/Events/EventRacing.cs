using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventRacing
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Основные действия в игре
        /// </summary>
        /// <param name="action_in_race">Номер действия</param>
        public void ActionInRace(int action_in_race)
        {
            SendEvent("Action in Race", new Dictionary<string, string>() { { "action_in_race", action_in_race.ToString() } });
        }

        /// <summary>
        /// Событие прохождения круга
        /// </summary>
        public void LapPassed()
        {
            SendEvent("Lap Passed", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Событие завершения гонки
        /// </summary>
        public void RacePassed(int win)
        {
            SendEvent("Race Passed", new Dictionary<string, string>() { { "win", win.ToString() } });
        }

        /// <summary>
        /// Игрок пришел на финиш вторым
        /// </summary>
        public void AlmostPassed()
        {
            SendEvent("Almost Passed", new Dictionary<string, string>() { { "", "" } });
        }
    }
}