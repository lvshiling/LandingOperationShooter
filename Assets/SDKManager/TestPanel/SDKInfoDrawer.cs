using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SDKManagement;

public class SDKInfoDrawer : MonoBehaviour {


    public GameObject background;

    public Text sdkName;
    public Text sdkStatus;
    public Text sdkMessage;

    Color color = new Color();

    public void SetInfo(SDKInfo info, bool showBorder) {
        if (background != null) {
            background.SetActive(showBorder);
        }
        if (sdkName != null) {
            sdkName.text = info.name;
            sdkName.color = Color.black;
        }
        switch (info.status) {
            case SDKStatus.INITED: color = new Color(0, 0.5f, 0); break;
            case SDKStatus.TEST: color = new Color(0, 0, 0.5f); break;
            case SDKStatus.FAILED: color = Color.red; break;
            case SDKStatus.WAITING: color = new Color(0.7f, 0.7f, 0.0f); break;
            default: color = Color.gray; break;
        }

        if (sdkStatus != null)
        {
            sdkStatus.text = info.status.ToString();
            sdkStatus.color = color;
        }
        if (sdkMessage != null)
        {
            sdkMessage.text = info.message;
            sdkMessage.color = color;
        }
    }

}
