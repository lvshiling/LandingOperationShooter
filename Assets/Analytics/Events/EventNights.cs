using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventNights
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Закрытие двери
        /// </summary>
        public void CloseDoor()
        {
            SendEvent("Close Door", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Открытие двери
        /// </summary>
        public void OpenDoor()
        {
            SendEvent("Open Door", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Смена вида камеры
        /// </summary>
        public void ChangeCameraView()
        {
            SendEvent("Change Camera View", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// игрок почти прошел уровень, но все же проиграл
        /// </summary>
        public void AlmostPassed()
        {
            SendEvent("Almost Passed", new Dictionary<string, string>() { { "", "" } });
        }
    }
}