using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventTrain
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Проезд станции без остановки
        /// </summary>
        /// <param name="station">номер станции</param>
        public void SkipStation(int station)
        {
            SendEvent("Skip Station", new Dictionary<string, string>() { { "station", station.ToString() } });
        }

        /// <summary>
        /// Событие нажатия кнопки открытия дверей
        /// </summary>
        /// <param name="station">номер станции</param>
        public void OpenDoors(int station)
        {
            SendEvent("Open Doors", new Dictionary<string, string>() { { "station", station.ToString() } });
        }

        /// <summary>
        /// Событие нажатия кнопки закрытия дверей
        /// </summary>
        /// <param name="station">номер станции</param>
        public void CloseDoors(int station)
        {
            SendEvent("Close Doors", new Dictionary<string, string>() { { "station", station.ToString() } });
        }

        /// <summary>
        /// Посадка всех пассажиров со станции
        /// </summary>
        public void TakeInAllPassangers()
        {
            SendEvent("Take in All Passangers", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Штраф за превышение скорости
        /// </summary>
        public void OverSpeed()
        {
            SendEvent("Over Speed", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Событие получения опыта
        /// </summary>
        /// <param name="cause">откуда получен опыт(успешное соблюдение скоростного режима, остановка перед красным семафором)</param>
        public void GetExperience(int cause)
        {
            SendEvent("Get Experience", new Dictionary<string, string>() { { "cause", cause.ToString() } });
        }

        /// <summary>
        /// Штраф за проезд семафора на запрещающий сигнал
        /// </summary>
        public void RedSemaphore()
        {
            SendEvent("Red Semaphore", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Успешная остановка на станции/окончание миссии
        /// </summary>
        /// <param name="earned_money">количество денег заработанных за выполнение миссии</param>
        /// <param name="distance">расстояние, пройденное поездом за время выполнения миссии</param>
        /// <param name="number_of_carriages">количество вагонов, требуемое для миссии</param>
        /// <param name="station">номер станции</param>
        public void SuccessfulStop(int earned_money, int distance, int number_of_carriages, int station)
        {
            SendEvent("Successful Stop", new Dictionary<string, string>() {
                { "earned_money", earned_money.ToString() },
                { "distance", distance.ToString() },
                { "num_of_carriag", number_of_carriages.ToString() },
                { "station", station.ToString() } }
            );
        }
    }
}