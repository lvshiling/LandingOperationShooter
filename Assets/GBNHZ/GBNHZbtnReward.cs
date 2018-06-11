using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class GBNHZbtnReward : MonoBehaviour
{
    public Text text;
    public string TextErn = "";//%V%
    public string TextLoading = "Loading...";
    public int rewardType = 0;

    [SerializeField]
    private bool autoAddToGBNHZshow = true;

    private long btnId;

    public UnityEvent onShow;

    private void Awake()
    {
        if (autoAddToGBNHZshow)
        {
            GBNHZshow.AddRewardButton(gameObject);
        }

        if (text != null && !string.IsNullOrEmpty(TextLoading))
        {
            text.text = TextLoading;
        }
    }

    void Start()
    {
        btnId = GBNHZshow.GenerateRewardId(gameObject, rewardType);

        GBNEventManager.AddEventListener(GBNEvent.REWARD_SHOWN + btnId, OnShowInvoke);

        GBNHZinit.AddOnLoadListener(EndLoading);
    }

    void OnShowInvoke() {
        if (onShow != null)
        {
            onShow.Invoke();
        }
    }

    private void OnDestroy()
    {
        GBNEventManager.RemoveEventListener(GBNEvent.REWARD_SHOWN + btnId, OnShowInvoke);

        GBNHZinit.RemoveOnLoadListener(EndLoading);

        if (autoAddToGBNHZshow)
        {
            GBNHZshow.RemoveRewardButton(gameObject);
        }
    }

    public void EndLoading(bool ok)
    {
        if (text != null && !string.IsNullOrEmpty(TextErn))
        {
            if ((int)GBNHZinit.GetDelay("reward" + rewardType) > 0)
            {
                text.text = TextErn.Replace("%V%", ((int)GBNHZinit.GetDelay("reward" + rewardType)).ToString());
            }
            else
            {
                text.text = TextErn.Replace("%V%", "");
            }
        }
    }

    public void ShowRewardedVideo()
    {
        GBNHZshow.Instance.ShowReward(btnId);        
    }
}
