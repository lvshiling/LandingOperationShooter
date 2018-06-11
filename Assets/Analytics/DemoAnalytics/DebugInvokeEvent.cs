using AnalyticsPack;
using UnityEngine;

public class DebugInvokeEvent : MonoBehaviour
{
    public void PushEvent()
    {
        Analytics.Instance.events.main.TapPlayGame(Analytics.Instance.CurrentMenu);
    }
}