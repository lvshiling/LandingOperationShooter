using UnityEngine;
using UnityEngine.UI;

public class GBNTextLocalizator : MonoBehaviour
{
    [SerializeField]
    private string textKey;

    public string TextKey
    {
        get { return textKey; }
        set
        {
            textKey = value;
            Refresh();
        }
    }

    private Text text;

    void Refresh()
    {
#if LOCALE_VERSION
        if (textKey != "" && GBNLocalization.Instance != null)
        {
            text = GetComponent<Text>();
            var t = GBNLocalization.Instance.GetText(textKey);
            if (t != "") text.text = t;
        }
#endif
    }

#if LOCALE_VERSION
    void Start()
    {
        Refresh();
    }

    void OnEnable()
    {
        GBNEventManager.AddEventListener(GBNEvent.UPDATE_GUI, Refresh);
        Refresh();
    }

    void OnDisable()
    {
        GBNEventManager.RemoveEventListener(GBNEvent.UPDATE_GUI, Refresh);
    }
#endif
}
