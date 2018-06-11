using UnityEngine;
using System.Collections;

public class ButtonYouTube : MonoBehaviour
{
    private void Awake()
    {
        CoolTool.OnResetYoutube += OnReset;
    }

    private void OnEnable()
    {
        OnReset(CoolTool.Youtube);
    }

    private void OnReset(bool state)
    {
        gameObject.SetActive(state);
    }

    public void OpenUrlYouTube()
    {
        Application.OpenURL(GBNAPI.CompanyInfo.Struct.youtube);
    }

    private void OnDestroy()
    {
        CoolTool.OnResetYoutube -= OnReset;
    }
}
