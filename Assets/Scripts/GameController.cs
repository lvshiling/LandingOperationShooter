using AnalyticsPack;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class MissionPreSet
{
    public int lvl;
    public int maxmoney;
    public int maxcount;
    public int rewardUnit;
    public int rewardGold;
}

public enum GameStage
{
    Menu,
    SelectModeScreen,
    ShopMission,
    ShopSkirmish,
    Game,
    EndGame
}

public enum GameMode
{
    Skirmish,
    Campain
}

[System.Serializable]
public class MapSettings
{
    public GameObject loca;
    public Color fogColor;
    public int fogStart;
    public int fogEnd;
    public Material skyBox;
}

public class GameController : MonoBehaviour
{
    private static GameController _instance;

    public static GameController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameController>();
            }
            return _instance;
        }
    }

    public GameStage stage;

    private int money;

    public int moneyPlayer
    {
        get
        {
            return money;
        }
        set
        {
            money = value;
            Save();
        }
    }

    [Header("Параметры игрока")]
    public string winText;

    public string loseText;

    public GameObject winTitle;
    public GameObject loseTitle;

    public GameObject btnReplay;

    [Header("Настройка уровней")]
    public int rewardLvl;

    public int rewardBot;
    public int killCount;
    public int totalKillReward;

    public GameObject cutScene;
    public bool isStartGame;

    [Header("Магазин")]
    public int rewardMoney;
    public int amazonMoney;

    // Use this for initialization
    private void Start()
    {
        GamePlayController.Instance.SwitchMode(false);
        CanvasManager.Instance.OpenScreen(MenusAtGame.LoadingScreenUI);
        Load();
        AddListenersOnScreen();
        GBNCheatGUI.OnAddMoney += CheatBtn;
        GBNCheatGUI.OnClearPrefs += ClearData;
        GBNCheatGUI.OnWinLevel += CheatWin;
        //GBNCheatGUI.OnLoseLevel += CheatLose;
        GBNHZinit.AddOnLoadListener(OnLoad);
        DOTween.defaultTimeScaleIndependent = true;
    }

    private void OnLoad(bool isOk)
    {
        //SoundButtonAdd();
        SndManager.Instance.Play("Menu");
    }

    private void SoundButtonAdd()
    {
        /* Canvas[] canvas = FindObjectsOfType<Canvas>();
         for (int i = 0; i < canvas.Length; i++)
         {
             Button[] buttons = canvas[i].GetComponentsInChildren<Button>(true);
             foreach (Button bt in buttons)
             {
                 bt.onClick.AddListener(() => OnClickAnyButtons());
             }
         }*/
        for (int i = 0; i < CanvasManager.Instance.screens.Count; i++)
        {
            if (CanvasManager.Instance.screens[i].screenName == "MainMenuUI" || CanvasManager.Instance.screens[i].screenName == "ShopUI"
                || CanvasManager.Instance.screens[i].screenName == "UpgradeWeapon")
            {
                Button[] buttons = CanvasManager.Instance.screens[i].GetComponentsInChildren<Button>(true);
                foreach (Button bt in buttons)
                {
                    bt.onClick.AddListener(OnClickAnyButtons);
                }
            }
        }
    }

    private void OnClickAnyButtons()
    {
        SndManager.Instance.Play("btnClick");
    }

    public void CheatBtn(int money)
    {
        this.moneyPlayer += money;
        CanvasManager.Instance.UpdateScreenMoney(this.moneyPlayer);
    }

    public void CheatWin()
    {
        if (isStartGame == true)
        {
            killCount = 10;
        }
    }

    public void CheatLose()
    {
        GamePlayController.Instance.playerController.CurrentHealth = -1;
    }

    public void ClearData()
    {
        moneyPlayer = 0;
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(0);
    }

    public void GiveMeSomeReward(int moneyT, int armycountT)
    {
        moneyPlayer += moneyT;
        CanvasManager.Instance.UpdateScreenMoney(moneyPlayer);
    }

    public void SetIsStartGame(bool isStartGame)
    {
        this.isStartGame = false;
    }

    public void StartGame()
    {
        SndManager.Instance.Play("Game");
        CanvasManager.Instance.OpenScreen(MenusAtGame.GameUI);

        if (CanvasManager.Instance.cdFill != null)
        {
            CanvasManager.Instance.cdFill.gameObject.SetActive(true);// если вдруг выключено читом
        }

        stage = GameStage.Game;
        GamePlayController.Instance.Init();
        Time.timeScale = 1;

        int indexLevel = GetComponent<GameModeManager>().indexMode;

        //ПАРАМЕТРЫ В СООТВЕТСТВИИ С УРОВНЕМ СЛОЖНОСТИ
        rewardBot = GameModeManager.Instance.GetGameMode().rewardBot;
        rewardLvl = rewardBot * 10;

        isStartGame = true;

        SndManager.Instance.SoundValueOn();

        GamePlayController.Instance.spawnerHunters.SetParamsBots();

        AnalyticsWizard.Instance.StartLevel(1, indexLevel + 1);
    }

    public void StartGameFromCutScene()
    {
        if (cutScene != null)
        {
            cutScene.SetActive(true);
            CanvasManager.Instance.OpenScreen(MenusAtGame.CutSceneUI);
        }
        else
        {
            StartGame();
        }
    }

    public void EndGame(bool isWin)
    {
        CanvasManager.Instance.OpenScreen(MenusAtGame.EndGameUI);
        GamePlayController.Instance.SwitchMode(false);
        //GamePlayController.Instance.spawner.Clear();
        GamePlayController.Instance.spawnerHunters.Clear();
        //GamePlayController.Instance.spawnerBoss.Clear();
        //GamePlayController.Instance.spawnerBoss.gameObject.SetActive(false);
        Time.timeScale = 0;
        if (isWin)
        {
            winTitle.SetActive(true);
            loseTitle.SetActive(false);
            btnReplay.SetActive(false);
            CanvasManager.Instance.UpdateEndGameUI(rewardLvl, winText, totalKillReward /*killCount*rewardBot*/);

            if (CanvasManager.Instance.endGameFlash != null)
            {
                CanvasManager.Instance.endGameFlash.DOBlendableColor(Color.white, 0.2f);
            }

            moneyPlayer += rewardLvl;
            AnalyticsWizard.Instance.WinLevel(GetComponent<GameModeManager>().indexMode + 1);
        }
        else
        {
            AnalyticsWizard.Instance.SetMenu(Menu.LoseWindow);
            loseTitle.SetActive(true);
            winTitle.SetActive(false);
            btnReplay.SetActive(true);

            if (CanvasManager.Instance.endGameFlash != null)
                CanvasManager.Instance.endGameFlash.DOBlendableColor(Color.black, 0.2f);

            CanvasManager.Instance.UpdateEndGameUI(0, loseText, totalKillReward/*killCount*rewardBot*/);
            AnalyticsWizard.Instance.LoseLevel(GetComponent<GameModeManager>().indexMode + 1);

            if ((GamePlayController.Instance.spawnerHunters.maxBot - killCount) <= 1)
            {
                Analytics.Instance.events.shooter.AlmostPassed();
            }
        }

        killCount = 0;
        //stage = GameStage.Menu;

        Save();
    }

    private void OnDestroy()
    {
        RemoveListenersOnScreen();
        Save();
    }

    public void SetPause(bool on)
    {
        if (on)
        {
            Time.timeScale = 0;
            CanvasManager.Instance.OpenScreen(MenusAtGame.PauseUI);
            GBNHZshow.Instance.Show("pause");
        }
        else
        {
            AnalyticsWizard.Instance.TapResume();
            CanvasManager.Instance.OpenScreen(MenusAtGame.GameUI);
            Time.timeScale = 1;
        }
    }

    public void Pause(bool on)
    {
        if (on)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public void Replay()
    {
        AnalyticsWizard.Instance.TapRestart();
        //GamePlayController.Instance.spawner.Clear();
        GamePlayController.Instance.spawnerHunters.Clear();
        //GamePlayController.Instance.spawnerBoss.Clear();
        //GamePlayController.Instance.spawnerBoss.gameObject.SetActive(false);
        // StartGame();

        StartGameFromCutScene();
    }

    #region Screens

    public void AddListenersOnScreen()
    {
        foreach (ScreenUI ui in CanvasManager.Instance.screens)
        {
            ui.onClose += OffScreenChange;
            ui.onInit += OnScreenChange;
        }
    }

    public void RemoveListenersOnScreen()
    {
        foreach (ScreenUI ui in CanvasManager.Instance.screens)
        {
            ui.onClose -= OffScreenChange;
            ui.onInit -= OnScreenChange;
        }
    }

    public void OnScreenChange(string screenName)
    {
        MenusAtGame menu = ToMenuAtGames(screenName);
        switch (menu)
        {
            case MenusAtGame.ShopUI:
                {
                    killCount = 0;
                    SndManager.Instance.Play("Menu");
                    stage = GameStage.Menu;
                    CanvasManager.Instance.UpdateScreenMoney(moneyPlayer);
                    if (Analytics.Instance.CurrentMenu != Menu.ShopWindow)
                    {
                        AnalyticsWizard.Instance.TapShop();
                    }
                    AnalyticsWizard.Instance.SetMenu(Menu.ShopWindow);
                    break;
                }
            case MenusAtGame.MainMenuUI:
                {
                    killCount = 0;
                    GBNHZshow.Instance.SetGameSceneFlag(false);
                    SndManager.Instance.Play("Menu");
                    stage = GameStage.Menu;
                    //AnalyticsWizard.Instance.TapMainMenu();
                    AnalyticsWizard.Instance.SetMenu(Menu.MainMenu);
                    break;
                }
            case MenusAtGame.PauseUI:
                {
                    AnalyticsWizard.Instance.TapPause();
                    AnalyticsWizard.Instance.SetMenu(Menu.PauseWindow);
                    break;
                }
            case MenusAtGame.GameUI:
                {
                    if (stage == GameStage.Menu)
                    {
                        if (Analytics.Instance.CurrentMenu != Menu.LoseWindow && Analytics.Instance.CurrentMenu != Menu.ShopWindow)
                        {
                            AnalyticsWizard.Instance.TapPlayGame();
                        }
                    }

                    if (GamePlayController.Instance.dirLight != null)
                    {
                        GamePlayController.Instance.dirLight.intensity = GamePlayController.Instance.gameLight;
                    }

                    CanvasManager.Instance.UpdateScreenMoney(moneyPlayer);
                    AnalyticsWizard.Instance.SetMenu(Menu.GameWindow);
                    break;
                }
            case MenusAtGame.LevelsUI:
                {
                    killCount = 0;
                    SndManager.Instance.Play("Menu");
                    stage = GameStage.Menu;
                    GenerateLevelsScreen();
                    AnalyticsWizard.Instance.TapLevels();
                    AnalyticsWizard.Instance.SetMenu(Menu.Levels);
                    break;
                }
            case MenusAtGame.EndGameUI:
                {
                    SndManager.Instance.Play("End");
                    AnalyticsWizard.Instance.SetMenu(Menu.WinWindow);
                    break;
                }
            case MenusAtGame.NoMoneyUI:
                {
                    AnalyticsWizard.Instance.NoMoney();
                    AnalyticsWizard.Instance.SetMenu(Menu.NoMoneyWindow);
                    break;
                }
        }
    }

    public void OffScreenChange(string screenName)
    {
        MenusAtGame menu = ToMenuAtGames(screenName);
        switch (menu)
        {
            case MenusAtGame.GameUI:
                {
                    GamePlayController.Instance.dirLight.intensity = GamePlayController.Instance.menuLight;
                    GamePlayController.Instance.mainCamera.transform.position = GamePlayController.Instance.menuCamPos.position;
                    GamePlayController.Instance.mainCamera.transform.rotation = GamePlayController.Instance.menuCamPos.rotation;
                    break;
                }
            case MenusAtGame.MainMenuUI:
                {
                    GBNHZshow.Instance.SetGameSceneFlag(true);
                    break;
                }
        }
    }

    #endregion Screens

    public void RewardMoney()
    {
        moneyPlayer += rewardMoney;
        AnalyticsWizard.Instance.CompleteReward(1);
        CanvasManager.Instance.UpdateScreenMoney(moneyPlayer);
    }

    public void CloseNoMoney()
    {
        if (stage == GameStage.Menu)
        {
            CanvasManager.Instance.OpenScreen(MenusAtGame.ShopUI);
        }
        else if (stage == GameStage.Game)
        {
            CanvasManager.Instance.OpenScreen(MenusAtGame.GameUI);
        }
    }

    private MenusAtGame ToMenuAtGames(string nameUI)
    {
        foreach (MenusAtGame menu in System.Enum.GetValues(typeof(MenusAtGame)))
        {
            if (nameUI == menu.ToString())
            {
                return menu;
            }
        }
        return 0;
    }

    public void Load()
    {
#if ADS_VERSION
        moneyPlayer = PlayerPrefs.GetInt("money", 0);
#else
         moneyPlayer = PlayerPrefs.GetInt("money", amazonMoney);
#endif
    }

    public void Save()
    {
        PlayerPrefs.SetInt("money", moneyPlayer);
    }

    public void GenerateLevelsScreen()
    {
        // CanvasManager.Instance.InitLevels(chapter, (chapter-1) * lvlsInChapter, lvlsInChapter, lvl);
    }
}