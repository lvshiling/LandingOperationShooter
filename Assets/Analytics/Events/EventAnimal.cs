using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventAnimal
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Действие в игре
        /// </summary>
        /// <param name="action_in_game">Номер действия</param>
        public void Action(int action_in_game)
        {
            SendEvent("Action", new Dictionary<string, string>() { { "action_in_game", action_in_game.ToString() } });
        }

        /// <summary>
        /// Начало миссии в игре
        /// </summary>
        /// <param name="submission_start">Номер миссии</param>
        public void SubmissionStart(int submission_start)
        {
            SendEvent("Submission Start", new Dictionary<string, string>() { { "submission_start", submission_start.ToString() } });
        }

        /// <summary>
        /// Выполненная миссия
        /// </summary>
        /// <param name="submission_complete">Номер миссии</param>
        public void SubmissionComplete(int submission_complete)
        {
            SendEvent("Submission Complete", new Dictionary<string, string>() { { "submission_complete", submission_complete.ToString() } });
        }

        /// <summary>
        /// Начало движения
        /// </summary>
        public void StartMovement(int type)
        {
            SendEvent("Start Movement", new Dictionary<string, string>() { { "type", type.ToString() } });
        }
    }
}