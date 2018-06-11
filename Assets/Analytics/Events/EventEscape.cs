using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventEscape
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Смерть на пути к выходу
        /// </summary>
        public void AlmostPassed()
        {
            SendEvent("Almost Passed", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Выполнена подмиссия
        /// </summary>
        /// <param name="submission_complete">Номер миссии</param>
        public void SubmissionComplete(int submission_complete)
        {
            SendEvent("Submission Complete", new Dictionary<string, string>() { { "submission_complete", submission_complete.ToString() } });
        }

        /// <summary>
        /// Убийство врага
        /// </summary>
        public void Kill(int enem_type)
        {
            SendEvent("Kill", new Dictionary<string, string>() { { "enem_type", enem_type.ToString() } });
        }

        /// <summary>
        /// Находка ключа или аптечки
        /// </summary>
        /// <param name="item_type">номер предмета</param>
        public void FindItem(int item_type)
        {
            SendEvent("Find Item", new Dictionary<string, string>() { { "item_type", item_type.ToString() } });
        }
    }
}