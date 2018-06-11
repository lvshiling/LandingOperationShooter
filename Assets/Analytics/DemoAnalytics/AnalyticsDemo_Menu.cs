using AnalyticsPack;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnalyticsDemo_Menu : MonoBehaviour
{
    public GameObject popupOptions;

    public void Start()
    {
        Analytics.Instance.CurrentMenu = Menu.MainMenu;
    }

    public void StartGame()
    {
        Analytics.Instance.events.main.TapPlayGame(Analytics.Instance.CurrentMenu);
        SceneManager.LoadScene(1);
    }

    public void Options()
    {
        if (!popupOptions.activeSelf)
        {
            Analytics.Instance.events.main.Settings(Analytics.Instance.CurrentMenu);
            Analytics.Instance.CurrentMenu = Menu.SettingsWindow;
        }
        else
        {
            Analytics.Instance.events.main.Settings(Analytics.Instance.CurrentMenu);
            Analytics.Instance.CurrentMenu = Menu.MainMenu;
        }
        popupOptions.SetActive(!popupOptions.activeSelf);
    }
}