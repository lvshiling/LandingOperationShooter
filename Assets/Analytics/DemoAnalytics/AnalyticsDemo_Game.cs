using AnalyticsPack;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnalyticsDemo_Game : MonoBehaviour
{
    public void Start()
    {
        Analytics.Instance.CurrentMenu = Menu.GameWindow;
    }

    public void MainMenu()
    {
        Analytics.Instance.events.main.TapMainMenu(Analytics.Instance.CurrentMenu);
        SceneManager.LoadScene(0);
    }

    public void Restart()
    {
        Analytics.Instance.events.main.TapRestart(Analytics.Instance.CurrentMenu);
        SceneManager.LoadScene(1);
    }
}