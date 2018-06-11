using System;
using UnityEngine;
using UnityEngine.UI;

public class SkillItemObject : MonoBehaviour
{
    public Text itemName;
    public int itemID;
    public int ownerId;
    public Text costText;
    public GameObject buyBtn;
    public Image itemImage;
    public Action<int> onBuy;
    public Action<int, int> onUpgrade;
    public SimpleBar bar;

    public void Init(string itemName, int id, Action<int> onbuy, Sprite sprite, int cost, int maxcount, int currentValue)
    {
        this.itemName.text = itemName;
        itemID = id;
        costText.text = cost.ToString();
        buyBtn.SetActive(currentValue < maxcount);
        onBuy += onbuy;
        bar.maxLevel = maxcount;
        bar.level = currentValue;
        itemImage.sprite = sprite;
    }

    public void Init(string itemName, int id, int ownerId, Action<int, int> onbuy, Sprite sprite, int cost, int maxcount, int currentValue)
    {
        this.itemName.text = itemName;
        itemID = id;
        this.ownerId = ownerId;
        costText.text = cost.ToString();
        buyBtn.SetActive(currentValue < maxcount);
        onUpgrade += onbuy;
        bar.maxLevel = maxcount;
        bar.level = currentValue;
        itemImage.sprite = sprite;
    }

    public void Init(int maxcount, int currentValue, int cost)
    {
        buyBtn.SetActive(currentValue < maxcount);
        bar.level = currentValue;
        costText.text = cost.ToString();
    }

    public void OnBuy()
    {
        if (onBuy != null)
        {
            onBuy(itemID);
        }
        if (onUpgrade != null)
        {
            onUpgrade(ownerId, itemID);
        }
    }
}