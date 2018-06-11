using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomVariables
{
    public string name;
    public float value;
}

public class GBNVariables : MonoBehaviour
{
    public static GBNVariables Instance { get; private set; }

    public Dictionary<string, float> variables = new Dictionary<string, float>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetVariables(CustomVariables[] list)
    {
        foreach (CustomVariables v in list)
        {
            InitVariable(v.name, v.value);
        }
        GBNEventManager.TriggerEvent(GBNEvent.UPDATE_GUI);
    }

    public float GetVariable(string varName)
    {
        if (varName == "%REWARD%")
        {
            return (int)GBNHZinit.GetDelay("reward1".ToString());
        }
        if (!variables.ContainsKey(varName)) return 0;
        return variables[varName];
    }

    public bool GetBool(string varName)
    {
        if (!variables.ContainsKey(varName)) return false;
        return Math.Abs(variables[varName]) > 0.00001f;
    }

    public int GetInt(string varName)
    {
        if (!variables.ContainsKey(varName)) return 0;
        return (int)variables[varName];
    }

    public string GetString(string varName)
    {
        if (!variables.ContainsKey(varName)) return "";
        return variables[varName].ToString();
    }

    public void SetVariable(string varName, float value)
    {
        if (!variables.ContainsKey(varName))
        {
            variables.Add(varName, value);
        }
        else
        {
            variables[varName] = value;
        }
        GBNEventManager.TriggerEvent(GBNEvent.UPDATE_GUI);
    }

    public void SaveVariable(string varName, float value)
    {
        SetVariable(varName, value);
        PlayerPrefs.SetFloat(varName, value);
    }

    public void LoadVariable(string varName)
    {
        if (PlayerPrefs.HasKey(varName))
        {
            SetVariable(varName, PlayerPrefs.GetFloat(varName));
        }
    }

    private void InitVariable(string varName, float value)
    {
        if (PlayerPrefs.HasKey(varName))
        {
            value = PlayerPrefs.GetFloat(varName);
        }
        if (!variables.ContainsKey(varName))
        {
            variables.Add(varName, value);
        }
        else
        {
            variables[varName] = value;
        }
    }

    public bool HasVariable(string varName)
    {
        return variables.ContainsKey(varName);
    }
}
