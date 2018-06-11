using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSkillWindow : MonoBehaviour {

    public Image itemImage;
    public Transform content;
    public List<SkillItemObject> skillItemObjects;
    public SkillItemObject prefabSkill;

    public void Init(Action<int,int> onupdate, ShopItemWeapon item, UpgradeCosts[] upgradeCosts)
    {
        itemImage.sprite = item.sprite;

        skillItemObjects.ForEach(x => Destroy(x.gameObject));
        skillItemObjects = new List<SkillItemObject>();
        prefabSkill.gameObject.SetActive(true);
        for (int i = 0; i < item.skills.Count; i++)
        {
            if (item.skills[i].currentValue < item.skills[i].maxCount)
            {
                item.skills[i].cost = upgradeCosts[item.skills[i].currentValue].cost;
            }
            skillItemObjects.Add(Instantiate(prefabSkill, content));
            skillItemObjects[i].Init(item.skills[i].itemName, item.skills[i].ID,item.ID, onupdate, item.skills[i].sprite, item.skills[i].cost, item.skills[i].maxCount, item.skills[i].currentValue);
        }
        prefabSkill.gameObject.SetActive(false);
    }


    public void UpdateData(ShopItemWeapon item, UpgradeCosts[] upgradeCosts)
    {
        itemImage.sprite = item.sprite;
        if (skillItemObjects != null && skillItemObjects.Count > 0)
        {
            for (int i = 0; i < skillItemObjects.Count; i++)
            {
                if (item.skills[i].currentValue < item.skills[i].maxCount)
                {
                    item.skills[i].cost = upgradeCosts[item.skills[i].currentValue].cost;
                }
                skillItemObjects[i].Init(item.skills[i].maxCount, item.skills[i].currentValue, item.skills[i].cost);
            }
        }
    }
}
