using PaperPlaneTools;
using UnityEngine;

public class GDPRWindow
{
    public bool IsGDPRScope {
        get;
        private set;
    }
#if UNITY_IOS
    private string shortTitle = "Grant permission to information about your device";
    private string infoTitle = "More Info";
    private string shortText = "Your location information (aggregated by country) and language, as well as some info about your device allow us to provide you with an external user experience.\n" +
            "\n" +
            "By clicking «Proceed» you consent to share it for internal research causes and better ads performance.\n" +
            "\n" +
            "You are not required to consent and you can withdraw it at any time.";
    private string infoText =
        "This app collects and uses information about your device (type of device and OS version), your location (aggregated by country) and your in-game activity in order to show you more relevant and customized ads, measure ads performance and provide analytics. This information might help to identify whether you are the kind of person the advertising clients want to show ads to and to verify that you are human rather than a “bot” attempting to defraud advertisers. Please read our Privacy Policy and choose whether you want or not to share your data. You can always opt-out later using your device settings. You will find the information on how to do that in our Privacy Policy.";

#else
    private string shortTitle = "Grant permission to information about your device";
    private string infoTitle = "More Info";
    private string shortText = "Your location information (aggregated by country) and language, as well as some info about your device allow us to provide you with an external user experience.\n" +
            "\n" +
            "By clicking «Allow» on the next screen you consent to share it for internal research causes and better ads performance.\n" +
            "\n" +
            "You are not required to consent and you can withdraw it at any time.";
    private string infoText = 
        "This app collects and uses information about your device (type of device and OS version), your location (aggregated by country) and your in-game activity in order to show you more relevant and customized ads, measure ads performance and provide analytics. This information might help to identify whether you are the kind of person the advertising clients want to show ads to and to verify that you are human rather than a “bot” attempting to defraud advertisers. Please read our Privacy Policy and choose whether you want or not to share your data. You can always opt-out later using your device settings. You will find the information on how to do that in our Privacy Policy.";

#endif
    private string proceedBtn = "PROCEED";

    private string infoBtn = "MORE INFO";

    private string privacyPolicyBtn = "Privacy Policy";

    private string url = "";

    private bool isConfirmed
    {
        get
        {
            return PlayerPrefs.GetInt("GDPR_CONFIRMED", 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt("GDPR_CONFIRMED", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public GDPRWindow()
    {
        IsGDPRScope = true;
        /*
        string extTexts = GBNAPI.SDKInfo.GetKey("sdk_gdpr");

        if (!string.IsNullOrEmpty(extTexts))
        {
            JSONObject textsJson = new JSONObject(extTexts);
            if (textsJson != null && textsJson.IsObject)
            {
                string[] fields = new string[] { "titleMain", "titleInfo", "textMain", "textInfo", "btnProceed", "btnInfo", "btnPolicy" };
                if (textsJson.HasFields(fields))
                {
                    shortTitle = textsJson.GetField(fields[0]).str.Replace("\\n", "\n");
                    infoTitle = textsJson.GetField(fields[1]).str.Replace("\\n", "\n");
                    shortText = textsJson.GetField(fields[2]).str.Replace("\\n", "\n");
                    infoText = textsJson.GetField(fields[3]).str.Replace("\\n", "\n");
                    proceedBtn = textsJson.GetField(fields[4]).str.Replace("\\n", "\n");
                    infoBtn = textsJson.GetField(fields[5]).str.Replace("\\n", "\n");
                    privacyPolicyBtn = textsJson.GetField(fields[6]).str.Replace("\\n", "\n");
                }
            }
        }
        */
    }

    public void Show(string privacyUrl)
    {
        url = privacyUrl;
        if (IsGDPRScope && !isConfirmed)
        {
            Request();
        }
    }
    private void Request()
    {
        var requestAlert = new Alert(shortTitle, shortText);
        requestAlert
            .SetPositiveButton(proceedBtn, OnConfirm)
            .SetNeutralButton(infoBtn, OnMoreInfo)
            .AddOptions(new AlertAndroidOptions() { Cancelable = false })
            .Show();
    }

    private void OnConfirm()
    {
        isConfirmed = true;
    }

    private void OnMoreInfo()
    {
        var infoAlert = new Alert(infoTitle, infoText);
        infoAlert
            .SetPositiveButton(proceedBtn, OnConfirm)
            .SetNeutralButton(privacyPolicyBtn, OnPrivacyPolicy)
            .AddOptions(new AlertAndroidOptions() { Cancelable = false })
            .Show();
    }

    private void OnPrivacyPolicy()
    {
        Application.OpenURL(url);
        OnMoreInfo();
    }

    public void SetShortTitle(string shortTitle)
    {
        this.shortTitle = shortTitle;
    }

    public void SetInfoTitle(string infoTitle)
    {
        this.infoTitle = infoTitle;
    }

    public void SetShortText(string shortText)
    {
        this.shortText = shortText;
    }

    public void SetInfoText(string infoText)
    {
        this.infoText = infoText;
    }
   
    public bool ConfirmationStatus
    {
        get
        {
            return !IsGDPRScope || isConfirmed;
        }
    }
}


