using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventSurvival
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Событие крафта предмета
        /// </summary>
        /// <param name="craft">Номер предмета [1..)</param>
        public void Craft(int craft)
        {
            SendEvent("Craft", new Dictionary<string, string>() { { "craft", craft.ToString() } });
        }

        /// <summary>
        /// Просмотр карты
        /// </summary>
        public void LookAtMap()
        {
            SendEvent("Look at Map", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Убийство игроком любого персонажа в игре
        /// </summary>
        /// <param name="unit">Номер персонажа [1..)</param>
        public void KillUnit(int unit)
        {
            SendEvent("Kill Unit", new Dictionary<string, string>() { { "unit", unit.ToString() } });
        }

        /// <summary>
        /// Подбор предмета с земли (дерево, камень, мясо убитого юнита...)
        /// </summary>
        /// <param name="item">Номер предмета [1..)</param>
        public void FindItem(int item)
        {
            SendEvent("Find Item", new Dictionary<string, string>() { { "item", item.ToString() } });
        }

        /// <summary>
        /// Использование предмета.
        /// </summary>
        /// <param name="item">Номер предмета [1..)</param>
        /// <param name="action">Номер взаимодействия: 1 - израсходовать(съесть), 2 - взять в руки(для оружия), 3 - надеть(одежда и рюкзак), 4 - построить(предмет в руки не берется)</param>
        public void UseItem(int item, int action)
        {
            SendEvent("Use Item", new Dictionary<string, string>() { { "item", item.ToString() }, { "action", action.ToString() } });
        }

        /// <summary>
        /// Добавление предмета на панель быстрого доступа
        /// </summary>
        /// <param name="item">Номер предмета [1..)</param>
        public void AddToPanel(int item)
        {
            SendEvent("Add to Panel", new Dictionary<string, string>() { { "item", item.ToString() } });
        }

        /// <summary>
        /// Выбрасывание предмета
        /// </summary>
        /// <param name="item">Номер предмета [1..)</param>
        public void Throw(int item)
        {
            SendEvent("Throw", new Dictionary<string, string>() { { "item", item.ToString() } });
        }

        /// <summary>
        /// Событие вызывается когда игрок нажал на кнопку сна
        /// </summary>
        public void Sleep()
        {
            SendEvent("Sleep", new Dictionary<string, string>() { { "", "" } });
        }
    }
}