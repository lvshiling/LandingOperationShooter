using UnityEngine;
using UnityEngine.UI;

public class GBNHZCrosspromo : MonoBehaviour
{
    public Image mainImage;
    public float changeIconsTimer = 6.0f;
    public float faderTimer = 0.5f;
    public Sprite[] iconsGp;
    public Sprite[] iconsIos;
    public GameObject text;
    public Image flashFx;
   
    private float t;
    private int k = 0;

    private Sprite[] actualIcons;

    private void Awake()
    {
        CoolTool.OnResetCrosspromo += OnReset;
    }

    private void OnEnable()
    {
        OnReset(CoolTool.Crosspromo && CoolTool.Ads);
    }

    private void OnReset(bool state)
    {
        gameObject.SetActive(state && CoolTool.Ads);
    }

    void Start()
    {
        text.SetActive(false);
        if (mainImage == null)
        {
            mainImage = GetComponent<Image>();
        }

        flashFx.color = Color.clear;

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            actualIcons = iconsIos;
        }
        else
        {
            actualIcons = iconsGp;
        }

        mainImage.sprite = actualIcons[0];  
    }
        
    void Update()
    {
        if (flashFx.color.a > 0)
        {
            flashFx.color -= Color.black * (Time.unscaledDeltaTime / faderTimer);
        }
        t += Time.unscaledDeltaTime;
        if (t > changeIconsTimer)
        {
            t = 0.0f;
            flashFx.color = Color.white;
            k++;
            if (k > actualIcons.Length - 1)
            {
                k = 0;
            }
            mainImage.sprite = actualIcons[k];
        }
    }

    public void ShowCrosspromo()
    {
        if (GBNAPI.Network.IsConnected())
        {
            GBNHZshow.Instance.Show(false);
        }
        else
        {
            GBNAPI.Dialogs.NoInternetAccessWarning();
        }
    }

    private void OnDestroy()
    {
        CoolTool.OnResetCrosspromo -= OnReset;
    }
}
