using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsPack
{
    public class EventCooking
    {
        private void SendEvent(string n, Dictionary<string, string> p)
        {
            Analytics.Instance.events.SendEvent(n, p);
        }

        /// <summary>
        /// Выполненный рецепт
        /// </summary>
        /// <param name="order_number">номер рецепта</param>
        public void RecipesComplete(int order_number)
        {
            SendEvent("Recipes Complete", new Dictionary<string, string>() { { "order_number", order_number.ToString() } });
        }

        /// <summary>
        /// Проваленный рецепт(не хватило времени)
        /// </summary>
        /// <param name="order_number">номер рецепта</param>
        public void RecipesFail(int order_number)
        {
            SendEvent("Recipes Fail", new Dictionary<string, string>() { { "order_number", order_number.ToString() } });
        }

        /// <summary>
        /// Открытие подсказки как готовить заказ
        /// </summary>
        public void GetHint()
        {
            SendEvent("Get Hint", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Испорченный ингредиент
        /// </summary>
        public void SpoiledIngredient()
        {
            SendEvent("Spoiled Ingredient", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Изменение чувствительности управления
        /// </summary>
        public void ChangeSensivity()
        {
            SendEvent("Change Sensivity", new Dictionary<string, string>() { { "", "" } });
        }

        /// <summary>
        /// Победа
        /// </summary>
        /// <param name="stars">количество звезд, полученных за уровень</param>
        /// <param name="earned_money">количество заработанных денег(total)</param>
        /// <param name="bonus_money">бонусные очки</param>
        /// <param name="penalty_count">количество штрафов</param>
        public void Win(int stars, int earned_money, int bonus_money, int penalty_count)
        {
            SendEvent("Win", new Dictionary<string, string>() {
                { "stars", stars.ToString() } ,
                { "earned_money", earned_money.ToString() },
                { "bonus_money", bonus_money.ToString() },
                { "penalty_count", penalty_count.ToString() } }
            );
        }

        /// <summary>
        /// Покупка рецепта
        /// </summary>
        /// <param name="recipes_number">номер рецепта</param>
        public void BuyRecipes(int recipes_number)
        {
            SendEvent("Buy Recipes", new Dictionary<string, string>() { { "recipes_number", recipes_number.ToString() } });
        }
    }
}