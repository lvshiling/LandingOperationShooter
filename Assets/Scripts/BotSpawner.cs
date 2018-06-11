using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotSpawner : MonoBehaviour
{
    public HeroStatus status;
    public List<UnitAI> botPrefabs;
    public List<UnitAI> bots;
    public int maxBot;
    public List<Transform> spawnPoints;

    public bool isRespawn;
    private float timer;
    public float respawnTimer;
    public int countOnPoint;

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        //if (GameController.Instance.isStartGame)
        //{
            timer += Time.deltaTime;
            if (timer > respawnTimer)
            {
                SpawnBot();
            }
        //}
    }

    public void SpawnBot()
    {
        if (bots.Count < maxBot)
        {
            List<Transform> tempTransform = spawnPoints.Where(x => x.childCount < countOnPoint).ToList();
            int rnd = Random.Range(0, tempTransform.Count);
            bots.Add(Instantiate(botPrefabs[Random.Range(0, botPrefabs.Count)], tempTransform[rnd].position, tempTransform[rnd].rotation, tempTransform[rnd]));
            Hero temp = bots[bots.Count - 1] as Hero;
            temp.heroCastle = tempTransform[rnd].gameObject;
            temp.Status = status;

            //ПАРАМЕТРЫ В СООТВЕТСТВИИ С УРОВНЕМ СЛОЖНОСТИ
            temp.attackF = GameModeManager.Instance.GetGameMode().damagBot;
            temp.maxHealth = GameModeManager.Instance.GetGameMode().botHealth;
            {
                temp.currentHealth = temp.maxHealth;
            }
        }
        else
        {
            if (!isRespawn)
            {
                enabled = false;
            }
        }
    }

    /// <summary>
    /// применяет парметры для уже заспавнишехся ботов при старте игры
    /// </summary>
    public void SetParamsBots()
    {
        Hero temp;
        for (int i = 0; i < bots.Count; i++)
        {
            temp = bots[i] as Hero;

            //ПАРАМЕТРЫ В СООТВЕТСТВИИ С УРОВНЕМ СЛОЖНОСТИ
            temp.attackF = GameModeManager.Instance.GetGameMode().damagBot;
            temp.maxHealth = GameModeManager.Instance.GetGameMode().botHealth;
            {
                temp.currentHealth = temp.maxHealth;
            }
        }
    }

    public void Clear()
    {
        bots.Where(x => x != null).ToList().ForEach(x => Destroy(x.gameObject));
        bots.Clear();
        this.enabled = true;
    }
}