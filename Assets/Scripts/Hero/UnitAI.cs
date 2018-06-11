using System.Collections.Generic;
using UnityEngine;

public abstract class UnitAI : MonoBehaviour, UnitI
{
    public string unitName;
    public string unitClass;

    [Header("Базовые характеристики")]
    public float currentHealth;

    public float maxHealth;
    public float currentMana;
    public float maxMana;
    public float normalSpeed;
    public float currentSpeed;
    public float xpPoint;
    public int lvl;
    public float lvlCup;
    public float sizeObject;

    [Header("характеристики зависящие от снаряжения")]
    public float attackF;

    public float attackM;
    public float attackDelay; // на самом деле это время между атаками
    public float armorF;
    public float armorM;
    public float regenHealth;
    public float regenMana;

    public HeroClass Class;
    public HeroStatus Status;

    public Transform target;
    public List<EnemyPrefer> AggroList;
    public AttackPreferList attackList;

    abstract public void DamageTake(float damageM, float damageF);

    //abstract public IEnumerator IEDamageTake(float damageM, float damageF);

    public List<UnitAI> alliesHero;
    public List<UnitAI> enemyHero;
}