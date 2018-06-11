using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemObject : MonoBehaviour
{
    public Text itemName;
    public int itemID;
    public Text costText;
    public GameObject buyBtn;
    public GameObject selectObj;
    public GameObject upgradeBtn;
    public Image itemImage;
    public Action<int> onBuy;
    public Action<int> onSelect;
    public Action<int> onUpgrade;

    public void Init(string itemName, int id, bool isBuy, bool isSelect, Action<int> onbuy, Action<int> onselect, Action<int> onupgrade, Sprite sprite, int cost, bool isUpgrade)
    {
        this.itemName.text = itemName;
        itemID = id;
        costText.text = cost.ToString();
        buyBtn.SetActive(!isBuy);
        selectObj.SetActive(isSelect);
        upgradeBtn.SetActive(isBuy && isUpgrade);
        onBuy += onbuy;
        onSelect += onselect;
        onUpgrade += onupgrade;
        itemImage.sprite = sprite;
    }

    public void Init(bool isBuy, bool isSelect, bool isUpgrade)
    {
        buyBtn.SetActive(!isBuy);
        selectObj.SetActive(isSelect);
        upgradeBtn.SetActive(isBuy && isUpgrade);
    }

    public void OnBuy()
    {
        onBuy(itemID);
    }

    public void OnSelect()
    {
        onSelect(itemID);
    }

    public void OnUpgrade()
    {
        onUpgrade(itemID);
    }
}