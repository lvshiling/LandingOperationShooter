using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventDriver
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Выполнена подмиссия(номер в списке) чекпоинт пройден
        /// </summary>
        /// <param name="submission_complete">номер подмиссии</param>
        public void SubmissionComplete(int submission_complete)
        {
            SendEvent("Submission complete", new Dictionary<string, string>() { { "submission_complete", submission_complete.ToString() } });
        }

        /// <summary>
        /// игрок почти прошел уровень, но все же проиграл
        /// </summary>
        public void AlmostPassed()
        {
            SendEvent("Almost passed", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Победа
        /// </summary>
        /// <param name="win_reward">награда за уровень</param>
        public void Win(int win_reward)
        {
            SendEvent("Win", new Dictionary<string, string>() { { "win_reward", win_reward.ToString() } });
        }
    }
}