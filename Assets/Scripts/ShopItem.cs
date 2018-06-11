using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

[CreateAssetMenu(fileName = "Data", menuName = "Shop/default", order = 1)]
[XmlInclude(typeof(ShopItemObject))]
public class ShopItem : ScriptableObject
{
    public string itemName;
    public bool isSelected;
    public bool isBuy;
    public int ID;
    public int cost;
    [XmlIgnore]
    public Sprite sprite;

    public virtual void Save()
    {
        PlayerPrefs.SetInt("item" + this.GetType().ToString() + ID.ToString() + "IsBuy", System.Convert.ToInt32(isBuy));
        PlayerPrefs.SetInt("item" + this.GetType().ToString() + ID.ToString() + "IsSelect", System.Convert.ToInt32(isSelected));
    }

    public virtual void Load()
    {
        isBuy = System.Convert.ToBoolean(PlayerPrefs.GetInt("item" + this.GetType().ToString() + ID.ToString() + "IsBuy", 0 ));
        isSelected = System.Convert.ToBoolean(PlayerPrefs.GetInt("item" + this.GetType().ToString() + ID.ToString() + "IsSelect", 0));
    }
}
