using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventOffroad
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Событие прохождения четвертей пути
        /// </summary>
        /// <param name="part_of_trails">Соответствующая часть пути</param>
        public void PassPartOfTrails(float part_of_trails)
        {
            SendEvent("Pass Part of Trails", new Dictionary<string, string>() { { "part_of_trails", part_of_trails.ToString(Analytics.FLOAT_FORMAT) } });
        }

        /// <summary>
        /// Основные действия в игре
        /// </summary>
        /// <param name="action_in_race">Номер действия</param>
        public void PassActionInRace(int action_in_race)
        {
            SendEvent("Action in Race", new Dictionary<string, string>() { { "action_in_race", action_in_race.ToString() } });
        }

        /// <summary>
        /// Начало выполнение миссии
        /// </summary>
        /// <param name="start_mission_in_game">Номер миссии</param>
        public void StartMissionInGame(int start_mission_in_game)
        {
            SendEvent("Start Mission", new Dictionary<string, string>() { { "start_mission_in_game", start_mission_in_game.ToString() } });
        }
    }
}