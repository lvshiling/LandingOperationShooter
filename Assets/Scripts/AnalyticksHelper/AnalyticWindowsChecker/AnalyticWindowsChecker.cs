using AnalyticsPack;
using UnityEngine;

public class AnalyticWindowsChecker : MonoBehaviour
{
    private static AnalyticWindowsChecker _instance;

    public static AnalyticWindowsChecker Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AnalyticWindowsChecker>();
            }
            return _instance;
        }
    }

    [System.Serializable]
    public struct Window
    {
        public GameObject gameObjectWnd;
        public bool isActive;
        public Menu menu;

        [HideInInspector] public WindowCheck windowCheck;
    }

    public Window[] windows;

    private void Awake()
    {
        if (windows == null || windows.Length <= 0)
        {
            Debug.LogError("Список окон пуст");
            return;
        }

        InitWindowChecks();
        UpdateCheckerWindow();
    }

    private void InitWindowChecks()
    {
        for (int i = 0; i < windows.Length; i++)
        {
            windows[i].gameObjectWnd.AddComponent<WindowCheck>();
            windows[i].windowCheck = windows[i].gameObjectWnd.GetComponent<WindowCheck>();
        }
    }

    public void UpdateCheckerWindow()
    {
        for (int i = 0; i < windows.Length; i++)
        {
            if (windows[i].windowCheck != null)
            {
                windows[i].isActive = windows[i].windowCheck.IsActive;
            }
        }

        SetAnalyticsCurrMenu();
    }

    private void SetAnalyticsCurrMenu()
    {
        for (int i = 0; i < windows.Length; i++)
        {
            if (windows[i].isActive == true)
            {
                Analytics.Instance.CurrentMenu = windows[i].menu;
            }
        }
    }

    private void Update()
    {
        UpdateCheckerWindow();
    }
}