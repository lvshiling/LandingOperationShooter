using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventHelloNeighbor
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Событие вызывается при нахождении предмета(ключа)
        /// </summary>
        /// <param name="find_item">номер предмета</param>
        public void FindItem(int find_item)
        {
            SendEvent("Find Item", new Dictionary<string, string>() { { "find_item", find_item.ToString() } });
        }

        /// <summary>
        /// Неудачная попытка открыть дверь ключом(ключ не подошел к двери)
        /// </summary>
        /// <param name="door_number">номер двери</param>
        public void FailOpenDoor(int door_number)
        {
            SendEvent("Fail Open Door", new Dictionary<string, string>() { { "door_number", door_number.ToString() } });
        }

        /// <summary>
        /// Игрок почти прошел уровень, но все же проиграл (выполнил 90% от общего количества требуемых заданий)
        /// </summary>
        public void AlmostPassed()
        {
            SendEvent("Almost Passed", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Открытие двери
        /// </summary>
        /// <param name="door_number"></param>
        public void DoorOpen(int door_number)
        {
            SendEvent("Door Open", new Dictionary<string, string>() { { "door_number", door_number.ToString() } });
        }

        /// <summary>
        /// Закрытие двери
        /// </summary>
        /// <param name="door_number">номер двери</param>
        public void DoorClose(int door_number)
        {
            SendEvent("Door Close", new Dictionary<string, string>() { { "door_number", door_number.ToString() } });
        }

        /// <summary>
        /// Спрятаться
        /// </summary>
        public void Hide()
        {
            SendEvent("Hide", new Dictionary<string, string>() { { "", "" } });
        }
    }
}