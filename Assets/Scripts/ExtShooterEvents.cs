using AnalyticsPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtShooterEvents : MonoBehaviour, IExtEvent
{
    
    //общее
    public int money;
    public float health;/*0..1*/

    //Во время игры
    public int gun_cur;
    public int ammo_cur;
    public float health_cur;/*0..1*/
    public int score_enemy;

    public void Start()
    {
        if (FindObjectOfType<Analytics>() != null)
        {
            FindObjectOfType<Analytics>().extEvent = this;
        }
    }

    public Dictionary<string, string> GetAllParams()
    {
        Dictionary<string, string> allParams = new Dictionary<string, string>();

        //общее
        InitComonParams();

        EventsEngine.AddParameter(allParams, "money", money);
        EventsEngine.AddParameter(allParams, "health", health);

        //во время игры
        if (GameController.Instance.isStartGame)
        {
            InitGameTrueParams();

            EventsEngine.AddParameter(allParams, "gun_cur", gun_cur);
            EventsEngine.AddParameter(allParams, "ammo_cur", ammo_cur);
            EventsEngine.AddParameter(allParams, "health_cur", health_cur);
            EventsEngine.AddParameter(allParams, "score_enemy", score_enemy);
        }

        return allParams;
    }

    void InitComonParams()
    {
        money = GameController.Instance.moneyPlayer;

        SkillItem skillItemHealth = ShopManager.Instance.GetSkillByName("health");
        health = (float)skillItemHealth.currentValue / skillItemHealth.maxCount;
    }

    void InitGameTrueParams()
    {
        gun_cur = GamePlayController.Instance.playerController.weapon.ID;

        ammo_cur = GamePlayController.Instance.playerController.weapon.ammoCount;

        health_cur = GamePlayController.Instance.playerController.CurrentHealth / GamePlayController.Instance.playerController.maxHealth;

        score_enemy = GamePlayController.Instance.spawnerHunters.maxBot - GameController.Instance.killCount;
    }
}
