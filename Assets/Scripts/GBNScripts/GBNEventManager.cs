using System.Collections.Generic;
using UnityEngine.Events;

public class GBNEvent
{
    public static string NO_MONEY = "no_money";
    public static string CLOUD_LOAD_START = "cloud_load_start";
    public static string CLOUD_LOADED = "cloud_loaded";
    public static string BUY = "buy";
    public static string UPDATE_GUI = "update_gui";
    public static string REWARD_SHOWN = "reward_shown";
    public static string REWARD_CANCELED = "reward_canceled";
    

}

public class GBNEventManager
{
    private readonly Dictionary<string, UnityEvent> eventDictionary;

    public static GBNEventManager instance = null;

    public static GBNEventManager Instance
    {
        get { return instance ?? (instance = new GBNEventManager()); }
    }

    private GBNEventManager()
    {
        eventDictionary = new Dictionary<string, UnityEvent>();
    }

    public static void AddEventListener(string eventName, UnityAction listener)
    {
        UnityEvent thisEvent = null;
        if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEvent();
            thisEvent.AddListener(listener);
            Instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void RemoveEventListener(string eventName, UnityAction listener)
    {
        UnityEvent thisEvent = null;
        if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public static void TriggerEvent(string eventName)
    {
        UnityEvent thisEvent = null;
        if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke();
        }
    }
}
