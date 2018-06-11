using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public enum Languages
{
    EN = 0,
    RU = 1,
    //AL, AR, CN, DE, DK, ES, FL, FR, ID, IL, IN, IT, JP, KR, MY, NL, PH, PL, PT, RO, SE, TH, TR, UA, VN
      UR, AR, ZH, DE, DA, ES, FI, FR, ID, HE, HI, IT, JA, KO, MS, NL, FL, PL, PT, RO, SV, TH, TR, UK, VI
}

public class GBNLocalization : MonoBehaviour
{
    public static GBNLocalization Instance { get; private set; }

    public Languages language = Languages.EN;

    private SheetTable<string> table;

    public bool IsInited { get; private set; }

    [Header("Loaded Languages")]
    public List<string> languages = new List<string>();
    #region AllThoseStupidCountries
    static string[,] countriesAndCodes = new string[,]
    {
            { "Afrikaans", "EN" },//   Afrikaans.
            { "Arabic", "AR" },//  Arabic.
            { "Basque", "EN" },//  Basque. - Spain/France
            { "Belarusian", "BE" },//  Belarusian.
            { "Bulgarian", "BG" },//   Bulgarian.
            { "Catalan", "FR" }, // Catalan. - Spain/France
            { "Chinese", "ZH" },// Chinese.
            { "Czech" , "CS" },//   Czech.
            { "Danish", "DA"},//  Danish.
            { "Dutch", "NL" },//   Dutch. - Niderlands/Holland
            { "English", "EN" },// English.
            { "Estonian", "ET" },//    Estonian.
            { "Faroese", "DE" },// Faroese. - Germany/Danish
            { "Finnish", "FI" },// Finnish.
            { "French", "FR" },//  French.
            { "German", "DE" },//  German.
            { "Greek", "EL" },//   Greek.
            { "Hebrew", "HE" }, //  Hebrew. - Israel
            { "Icelandic", "IS" },//   Icelandic.
            { "Indonesian", "ID" },//  Indonesian.
            { "Italian", "IT" },// Italian.
            { "Japanese", "JA" },//    Japanese.
            { "Korean", "KO" },//  Korean.
            { "Latvian", "LV" },// Latvian.
            { "Lithuanian", "LT" },//  Lithuanian. - Litva
            { "Norwegian", "NB" },//   Norwegian.
            { "Polish", "PL" },//  Polish. - Kurwa
            { "Portuguese", "PT" },//  Portuguese.
            { "Romanian", "RO" },//    Romanian.
            { "Russian", "RU" }, // Russian.
            { "SerboCroatian", "HR" },//   Serbo-Croatian. - Serbia
            { "Slovak", "SK" },//  Slovak.
            { "Slovenian", "SL" },//   Slovenian.
            { "Spanish", "ES" },// Spanish.
            { "Swedish", "SV" }, // Swedish.
            { "Thai", "TH" },//    Thai.
            { "Turkish", "TR" },// Turkish.
            { "Ukrainian", "UK" },//   Ukrainian.
            { "Vietnamese", "VI" }, //  Vietnamese.
            { "ChineseSimplified", "ZH" },//   ChineseSimplified.
            { "ChineseTraditional", "ZH" },//  ChineseTraditional.
            { "Unknown", "EN" }, // Unknown.
            { "Hungarian", "HU" }//   Hungarian.
    };
    #endregion

    public string CountryToCode(SystemLanguage language)
    {
        return (CountryToCode(language.ToString()));
    }

    public string CountryToCode(string c)
    {
        for (int q = 0; q < countriesAndCodes.Length; q++)
        {
            if (countriesAndCodes[q, 0] == c)
            {
                return (countriesAndCodes[q, 1]);
            }
        }
        return "EN";
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (PlayerPrefs.HasKey("language"))
            {
                //language = (Languages)PlayerPrefs.GetInt("language");
                ChangeLanguage((Languages)PlayerPrefs.GetInt("language"));
            }
            else
            {
                ChangeLanguage(Application.systemLanguage);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangeLanguage(Languages newLanguage)
    {
        language = newLanguage;
        GBNEventManager.TriggerEvent(GBNEvent.UPDATE_GUI);
        PlayerPrefs.SetInt("language", (int)language);
    }

    public void ChangeLanguage(string newLanguage)
    {
        try
        {
            ChangeLanguage((Languages)System.Enum.Parse(typeof(Languages), newLanguage));
        }
        catch (System.Exception)
        {
            ChangeLanguage(Languages.EN);
            //throw;
        }
    }

    public void ChangeLanguage(SystemLanguage newLanguage)
    {
        string languageCode = CountryToCode(newLanguage);
        ChangeLanguage(languageCode);
    }


    public void Init(SheetTable<string> stringTable)
    {
        table = stringTable;
        languages = table.colNames;
        if (!IsInited)
        {
            IsInited = true;
        }
    }

    public string GetText(string key, int level = 0)
    {
        if (!IsInited) return "";
        // parsing order
        // 1 IFEXPR = %VAR%?EXPR:EXPR
        // 2 EXPR = TERM+TERM
        // 3 TERM = %VAR% 
        // 4 TERM = INT
        // 5 TERM = TEXT_KEY


        // 1 IFEXPR = %VAR_NAME%?EXPR:EXPR
        if (level < 1 && key.IndexOf("?") >= 0)
        {
            var terms = key.Split('?');
            if (terms.Length == 2 && GBNVariables.Instance.HasVariable(terms[0]) && terms[1].IndexOf(":") >= 0)
            {
                var expr = terms[1].Split(':');
                if (expr.Length == 2)
                {
                    if (GBNVariables.Instance.GetBool(terms[0]))
                    {
                        return GetText(expr[0], 1);
                    }
                    else
                    {
                        return GetText(expr[1], 1);
                    }

                }
                else
                {
                    // error in expression
                    return GetText(key, 1);
                }
            }
            else
            {
                // error in expression
                return GetText(key, 1);
            }
        }
        // 2 EXPR = TERM+EXPR
        if (level < 2 && key.IndexOf("+") >= 0)
        {
            var terms = key.Split('+');
            if (terms.Length == 2)
            {
                return GetText(terms[0], 2) + GetText(terms[1], 2);
            }
            else
            {
                // error in expression
                return GetText(key, 2);
            }
        }
        // 3 TERM = %VAR% 
        if (GBNVariables.Instance.HasVariable(key))
        {
            return GBNVariables.Instance.GetString(key);
        }
        // 4 TERM = INT

        // replace INT
        var regex = new Regex("[0-9]+");

        if (regex.Match(key).ToString() == key)
        {
            return key;
        }


        // 5 TERM = TEXT_KEY
        string lang = language.ToString();
        // check language for exist
        if (table.colNames.IndexOf(lang) < 0)
        {
            lang = "EN";
        }
        var text = table.GetCell(key, lang);
        // check translation for exist
        if (lang != "EN" && text == "")
        {
            text = table.GetCell(key, "EN");
        }
        // replace "\n"
        while (text.IndexOf("\\\\n") >= 0)
        {
            text = text.Replace("\\\\n", "\n");
        }
        // replace "/n"
        while (text.IndexOf("/n") >= 0)
        {
            text = text.Replace("/n", "\n");
        }
        // fix "\"
        while (text.IndexOf("\\\\") >= 0)
        {
            text = text.Replace("\\\\", "\\");
        }

        // replace %VARIABLES%
        regex = new Regex("%[A-Z0-9_]+%");
        var m = regex.Match(text);
        var varName = m.ToString();
        while (varName != "")
        {
            text = text.Replace(varName, GBNVariables.Instance.GetVariable(varName).ToString(CultureInfo.InvariantCulture));
            m = regex.Match(text);
            varName = m.ToString();
        }
        return text;
    }
}
