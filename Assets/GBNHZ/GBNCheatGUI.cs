using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GBNCheatGUI : MonoBehaviour
{
    public static Action OnWinLevel, OnClearPrefs;
    public static Action<int> OnAddMoney;

    [SerializeField]
    private int clicksToActivate = 10;
    [SerializeField]
    private GameObject[] controls;
    private int countTrueClick = 0;
    private bool isLeftLastClick;

    
    private static bool _activeCheat = false;
    public static bool activeCheat
    {
        private set
        {
            _activeCheat = value;
        }
        get
        {
            return _activeCheat;
        }
    }
        
    private void Start()
    {
        foreach (GameObject it in controls)
        {
            it.SetActive(activeCheat);
        }

        GBNHZinit.AddOnLoadListener(OnAdsLoaded);
    }

    private void OnDestroy()
    {
        GBNHZinit.RemoveOnLoadListener(OnAdsLoaded);
    }

    private void OnAdsLoaded(bool ok)
    {

    }

    private void Update()
    {
#if !SCREENSHOT_VERSION
        if (!CoolTool.cheats)
        {
            return;
        }
#endif
        if (!activeCheat)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (countTrueClick == 0)
                {
                    isLeftLastClick = Input.mousePosition.x < Screen.width * 0.25f;
                    countTrueClick++;
                }
                else
                {
                    bool left = Input.mousePosition.x < Screen.width * 0.25f;
                    bool right = Input.mousePosition.x > 0.75f * Screen.width;
                    if ((isLeftLastClick && right) || (!isLeftLastClick && left))
                    {
                        countTrueClick++;                                                
                    }                    
                    else
                    {
                        countTrueClick = 0;                        
                    }
                    isLeftLastClick = left;
                    if (countTrueClick >= clicksToActivate)
                    {                        
                        activeCheat = true;
                        foreach (GameObject it in controls)
                        {
                            it.SetActive(true);
                        }
                    }
                }
            }
        }
    }

    public void AddMoney(int amount)
    {
        if (OnAddMoney != null)
        {
            OnAddMoney.Invoke(amount);
        }
    }

    public void WinLevel()
    {
        if (OnWinLevel != null)
        {
            OnWinLevel.Invoke();
        }
    }

    public void ClearPrefs()
    {
        PlayerPrefs.DeleteAll();
        if (OnClearPrefs != null)
        {
            OnClearPrefs.Invoke();
        }
    }
    public void ConsoleToggle() {
        GBNAPI.Console console = GetComponent<GBNAPI.Console>();
        if (console != null)
        {
            console.ShowToggle();
        }
    }

}
