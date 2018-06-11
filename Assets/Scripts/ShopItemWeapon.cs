using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Shop/default", order = 1)]
[System.Serializable]
public class ShopItemWeapon : ShopItem
{
    public float damage;
    public float accuracy;
    public int ammoCount;
    public float reloadTime;
    public List<SkillItem> skills;

    public float attackDelay;

    public bool isRateOfFire;/*если true то attackDelay считается как (1/rateOfFire) */
    public float rateOfFire;//скорость стрельбы в сек.

    public float attackDistance;
    public Animator animator;
    public GameObject shotEffect;
}