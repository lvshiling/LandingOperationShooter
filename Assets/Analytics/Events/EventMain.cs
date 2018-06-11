using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventMain
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Событие запуска приложения пользователем
        /// </summary>
        public void StartGame()
        {
            SendEvent("Start Game", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Нажатие кнопки, ведущей непосредственно в игровую сцену
        /// </summary>
        /// <param name="menu">Номер меню</param>
        public void TapPlayGame(Menu menu)
        {
            SendEvent("Tap Play Game", new Dictionary<string, string>() { { "menu", ((int)menu).ToString() } });
        }

        /// <summary>
        /// Нажатие кнопки More Games
        /// </summary>
        /// <param name="menu">Номер меню</param>
        public void TapMoreGames(Menu menu)
        {
            SendEvent("Tap More Games", new Dictionary<string, string>() { { "menu", ((int)menu).ToString() } });
        }

        /// <summary>
        /// Нажатие кнопки Rate Game
        /// </summary>
        /// <param name="menu">Номер меню</param>
        public void TapRateGame(Menu menu)
        {
            SendEvent("Tap Rate Game", new Dictionary<string, string>() { { "menu", ((int)menu).ToString() } });
        }

        /// <summary>
        /// Нажатие кнопки Crosspromo
        /// </summary>
        /// <param name="menu">Номер меню</param>
        public void TapCrosspromo(Menu menu)
        {
            SendEvent("Tap Crosspromo", new Dictionary<string, string>() { { "menu", ((int)menu).ToString() } });
        }

        /// <summary>
        /// Нажатие кнопки YouTube
        /// </summary>
        /// <param name="menu">Номер меню</param>
        public void TapYouTube(Menu menu)
        {
            SendEvent("Tap YouTube", new Dictionary<string, string>() { { "menu", ((int)menu).ToString() } });
        }

        /// <summary>
        /// Нажатие кнопки, непосредственно ведущей в главное меню
        /// </summary>
        /// <param name="menu">Номер меню</param>
        public void TapMainMenu(Menu menu)
        {
            SendEvent("Tap Main Menu", new Dictionary<string, string>() { { "menu", ((int)menu).ToString() } });
        }

        /// <summary>
        /// Нажатие кнопки перезапуска уровня/игры
        /// </summary>
        /// <param name="menu">Номер меню</param>
        public void TapRestart(Menu menu)
        {
            SendEvent("Tap Restart", new Dictionary<string, string>() { { "menu", ((int)menu).ToString() } });
        }

        /// <summary>
        /// Нажатие кнопки паузы
        /// </summary>
        /// <param name="menu">Номер меню</param>
        public void TapPause(Menu menu)
        {
            SendEvent("Tap Pause", new Dictionary<string, string>() { { "menu", ((int)menu).ToString() } });
        }

        /// <summary>
        /// Нажатие кнопки продолжить игру
        /// </summary>
        /// <param name="menu">Номер меню</param>
        public void TapResume(Menu menu)
        {
            SendEvent("Tap Resume", new Dictionary<string, string>() { { "menu", ((int)menu).ToString() } });
        }

        /// <summary>
        /// Нажатие кнопки Next и переход к следующему уровню
        /// </summary>
        /// <param name="menu">Номер меню если он есть, иначе -1</param>
        public void TapNextLevel(Menu menu)
        {
            SendEvent("Tap Next Level", new Dictionary<string, string>() { { "menu", ((int)menu).ToString() } });
        }

        /// <summary>
        /// Нажатие кнопки Privacy Policy
        /// </summary>
        /// <param name="menu">Номер меню</param>
        public void TapPrivacyPolicy(Menu menu)
        {
            SendEvent("Tap Privacy Policy", new Dictionary<string, string>() { { "menu", ((int)menu).ToString() } });
        }

        /// <summary>
        /// Нажатие кнопки, непосредственно ведущей в игровой магазин
        /// </summary>
        /// <param name="menu">Номер меню</param>
        public void TapShop(Menu menu)
        {
            SendEvent("Tap Shop", new Dictionary<string, string>() { { "menu", ((int)menu).ToString() } });
        }

        /// <summary>
        /// Нажатие кнопки, непосредственно ведущей в меню выбора уровней/локации/режима игры
        /// </summary>
        /// <param name="menu">Номер меню</param>
        public void TapLevels(Menu menu)
        {
            SendEvent("Tap Levels", new Dictionary<string, string>() { { "menu", ((int)menu).ToString() } });
        }

        /// <summary>
        /// Нажатие кнопки вознаграждения Reward
        /// </summary>
        /// <param name="menu">Номер меню</param>
        /// <param name="type">Тип Reward'а</param>
        public void Reward(Menu menu, int type, int item)
        {
            SendEvent("Reward", new Dictionary<string, string>() {
                { "menu", ((int)menu).ToString() },
                { "type", type.ToString() },
                { "item", item.ToString() }
            });
        }

        /// <summary>
        /// Полностью просмотренный reward(событие вызывается одновременно с получением награды)
        /// </summary>
        /// <param name="menu">Номер меню</param>
        /// <param name="type">Тип Reward'а</param>
        public void CompleteReward(Menu menu, int type, int item)
        {
            SendEvent("Complete Reward", new Dictionary<string, string>() {
                { "menu", ((int)menu).ToString() },
                { "type", type.ToString() },
                { "item", item.ToString() }
            });
        }

        /// <summary>
        /// Покупка вещи под номером
        /// </summary>
        /// <param name="type">Тип покупаемой вещи</param>
        /// <param name="item">Номер вещи внутри группы</param>
        public void BuyItem(int type, int item)
        {
            SendEvent("Buy Item", new Dictionary<string, string>() { { "type", type.ToString() }, { "item", item.ToString() } });
        }

        /// <summary>
        /// Покупка локации/уровня
        /// </summary>
        /// <param name="buy_location">Номер локации/уровня</param>
        public void BuyLocation(int buy_location)
        {
            SendEvent("Buy Location", new Dictionary<string, string>() { { "buy_location", buy_location.ToString() } });
        }

        /// <summary>
        /// Прокачка
        /// </summary>
        /// <param name="type">Номер прокачиваемой группы</param>
        /// <param name="item">Номер покупаемого апгрейда</param>
        /// <param name="level">Уровень прокачки конкретного скила</param>
        public void BuyUpgrade(int type, int item, float level)
        {
            SendEvent("Buy Upgrade", new Dictionary<string, string>() { { "type", type.ToString() }, { "item", item.ToString() }, { "level", level.ToString(Analytics.FLOAT_FORMAT) } });
        }

        /// <summary>
        /// Начало уровня
        /// </summary>
        /// <param name="type">Тип уровня(тип игры)</param>
        /// <param name="level">Номер уровня, если есть, иначе -1</param>
        public void StartLevel(int type, int level)
        {
            SendEvent("Start Level", new Dictionary<string, string>() { { "type", type.ToString() }, { "level", level.ToString() } });
        }

        /// <summary>
        /// Событие завершения туториала
        /// </summary>
        /// <param name="tutor_num">Номер туториала</param>
        public void CompleteTutorial(int tutor_num)
        {
            SendEvent("Complete Tutorial", new Dictionary<string, string>() { { "tutor_num", tutor_num.ToString() } });
        }

        /// <summary>
        /// Победа
        /// </summary>
        /// <param name="level">Номер уровня, если есть, иначе -1</param>
        public void WinLevel(int level)
        {
            SendEvent("Win Level", new Dictionary<string, string>() { { "level", level.ToString() } });
        }

        /// <summary>
        /// Поражение
        /// </summary>
        /// <param name="level">Номер уровня, если есть, иначе -1</param>
        public void Loselevel(int level)
        {
            SendEvent("Lose Level", new Dictionary<string, string>() { { "level", level.ToString() } });
        }

        /// <summary>
        /// Событие начала показа рекламы(не reward)
        /// </summary>
        /// <param name="menu">Номер меню</param>
        public void Impression(Menu menu)
        {
            SendEvent("Impression", new Dictionary<string, string>() { { "menu", ((int)menu).ToString() } });
        }

        /// <summary>
        /// Событие входа в настройки
        /// </summary>
        /// <param name="menu">Номер меню</param>
        public void Settings(Menu menu)
        {
            SendEvent("Settings", new Dictionary<string, string>() { { "menu", ((int)menu).ToString() } });
        }

        /// <summary>
        /// Событие показа окна отсутствия денег
        /// </summary>
        /// <param name="menu">Номер меню</param>
        public void NoMoney(Menu menu)
        {
            SendEvent("No Money", new Dictionary<string, string>() { { "menu", ((int)menu).ToString() } });
        }
    }
}