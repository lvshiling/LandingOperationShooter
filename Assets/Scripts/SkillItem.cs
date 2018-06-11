using UnityEngine;

public class SkillItem : ShopItem
{
    public int maxCount;
    public int currentValue;
    public float[] lvlValues;
    public string stringId;

    public override void Load()
    {
        currentValue = PlayerPrefs.GetInt("item" + this.GetType().ToString() + ID.ToString() + itemName, currentValue);
    }

    public override void Save()
    {
        PlayerPrefs.SetInt("item" + this.GetType().ToString() + ID.ToString() + itemName, currentValue);
    }

    public void Load(int Id)
    {
        currentValue = PlayerPrefs.GetInt("item" + this.GetType().ToString() + Id.ToString() + ID.ToString() + itemName, currentValue);
    }

    public void Save(int Id)
    {
        PlayerPrefs.SetInt("item" + this.GetType().ToString() + Id.ToString() + ID.ToString() + itemName, currentValue);
    }

    public float GetCurrentValue()
    {
        return lvlValues[currentValue];
    }
}