using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnalyticsPack;

public class ConcreteExtEvent : MonoBehaviour, IExtEvent
{
    public int selected_track;
    public int selected_auto;

    public void Start()
    {
        if (FindObjectOfType<Analytics>() != null)
        {
            FindObjectOfType<Analytics>().extEvent = this;
        }
    }

    public Dictionary<string, string> GetAllParams()
    {
        Dictionary<string, string> allParams = new Dictionary<string, string>();

        EventsEngine.AddParameter(allParams, "selected_track", selected_track);
        EventsEngine.AddParameter(allParams, "selected_auto", selected_auto);

        return allParams;
    }
}