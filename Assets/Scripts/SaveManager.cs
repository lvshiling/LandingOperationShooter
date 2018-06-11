using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

using System.IO;

[System.Serializable]
public class SaveArmyCell
{
    public string name;
    public float currentExp;

    public SaveArmyCell()
    {

    }
    public SaveArmyCell(string nameT, float exp)
    {
        name = nameT;
        currentExp = exp;
    }

  


}

[System.Serializable]
public class SaveCell
{
    public string characterRace;
    public List<SaveArmyCell> army;
    public int lvl;
    public bool Contain(string nameT)
    {
        if(army ==null || army.Count < 1)
        {
            army = new List<SaveArmyCell>();
            return false;
        }
        foreach(SaveArmyCell ac in army)
        {
            if(nameT == ac.name)
            {
                return true;
            }
        }

        return false;
    }

    public SaveArmyCell GetByKey(string nameT)
    {
        foreach (SaveArmyCell ac in army)
        {
            if (nameT == ac.name)
            {
                return ac;
            }
        }

        return null;
    }
}

[System.Serializable]
public class SaveContainer
{
    [XmlArray("Saves"), XmlArrayItem("Save")]
  public ShopItem[] campainSaver;

    public void Save(string path)
    {
        var serializer = new XmlSerializer(typeof(SaveContainer));
        
        using (var stream = new FileStream(path, FileMode.Create))
        {
            serializer.Serialize(stream, this);
        }
    }

    public static SaveContainer Load(string path)
    {
        var serializer = new XmlSerializer(typeof(SaveContainer));
        using (var stream = new FileStream(path, FileMode.Open))
        {
            return serializer.Deserialize(stream) as SaveContainer;
        }
    }

    //Loads the xml directly from the given string. Useful in combination with www.text.
    public static SaveContainer LoadFromText(string text)
    {
        var serializer = new XmlSerializer(typeof(SaveContainer));
        return serializer.Deserialize(new StringReader(text)) as SaveContainer;
    }
}


    [System.Serializable]
public class SaveLvlContainer
{
    [XmlArray("Saves"), XmlArrayItem("Save")]
    public ShopItemWeapon[] campainSaver;

    public void Save(string path)
    {
        var serializer = new XmlSerializer(typeof(SaveLvlContainer));

        using (var stream = new FileStream(path, FileMode.Create))
        {
            serializer.Serialize(stream, this);
        }
    }

    public static SaveLvlContainer Load(string path)
    {
        var serializer = new XmlSerializer(typeof(SaveLvlContainer));
        using (var stream = new FileStream(path, FileMode.Open))
        {
            return serializer.Deserialize(stream) as SaveLvlContainer;
        }
    }

    //Loads the xml directly from the given string. Useful in combination with www.text.
    public static SaveLvlContainer LoadFromText(string text)
    {
        var serializer = new XmlSerializer(typeof(SaveLvlContainer));
        return serializer.Deserialize(new StringReader(text)) as SaveLvlContainer;
    }



   
}
[System.Serializable]
public class SaveManager : MonoBehaviour {

    private static SaveManager _instance;
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SaveManager>();
            }
            return _instance;
        }
    }


   public SaveContainer saveData;
    public SaveLvlContainer saveLvlData;



    public void LoadData()
    {
        try
        {
#if UNITY_EDITOR

            saveData = SaveContainer.Load(Path.Combine("Assets/", "campain.xml"));
#else
         saveData = SaveContainer.Load(Path.Combine(Application.persistentDataPath, "campain.xml"));
#endif
        }
        catch
        {

        }

    }

    public void SaveData()
    {
#if UNITY_EDITOR
        saveData.Save(Path.Combine("Assets/", "campain.xml"));
#else
         saveData.Save(Path.Combine(Application.persistentDataPath, "campain.xml"));
#endif
    }

    public void LoadLvlData()
    {
#if UNITY_EDITOR
        saveLvlData = SaveLvlContainer.Load(Path.Combine("Assets/", "campainLvl.xml"));
#else
         saveLvlData = SaveLvlContainer.Load(Path.Combine(Application.persistentDataPath, "campainLvl.xml"));
#endif
    }

    public void SaveLvlData()
    {
#if UNITY_EDITOR
        saveLvlData.Save(Path.Combine("Assets/", "campainLvl.xml"));
#else
        saveLvlData.Save(Path.Combine(Application.persistentDataPath, "campainLvl.xml"));
#endif
    }

    void Start()
    {
        
    }
}
