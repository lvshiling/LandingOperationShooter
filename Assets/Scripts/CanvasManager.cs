using AnalyticsPack;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    private static CanvasManager _instance;

    public static CanvasManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CanvasManager>();
            }
            return _instance;
        }
    }

    [SerializeField]
    public List<ScreenUI> screens;

    [SerializeField]
    private Text[] moneyText;

    [Header("Магазин")]
    public List<GameObject> panelsShop;

    public List<Image> topButtons;
    public Sprite activeTab;
    public Sprite nonActiveTab;

    public Button btnShopNext;
    public Button btnBackToMainMenu;
    public Button btnBackToGameMenu;

    /* [Header("Меню Уровней")]
     public Text chapterText;
     public LevelObject levelPrefab;
     public List<LevelObject> levelsList;
     public Transform levelsParent;*/

    [Header("Экран Игры")]
    public Image aim;

    public Text rewardText;
    public Text rewardBotText;
    public Text titleEndGame;
    public GameObject exitGroup;
    public GameObject clueGroup;
    public GameObject bossGroup;
    public Image clueBack;
    public Sprite[] cluesSprite;
    public Image hpBar;
    public Text ammo;
    public Image endGameFlash;
    public Image gameFlash;
    public Image checkPointFlash;
    public Image hideWeaponImage;
    public Sprite weaponHide;
    public Sprite weaponUnHide;
    public Image cdFill;

    public Text countAlive;//количество живых врагов

    public Button btnWeaponChange;

    public void SetCountAlive(int count)
    {
        if (count < 0)
        {
            count = 0;
        }

        countAlive.text = count.ToString() + " ALIVE";
    }

    public void UpdateScreenMoney(int value)
    {
        for (int i = 0; i < moneyText.Length; i++)
        {
            moneyText[i].text = value.ToString();
        }
    }

    #region Shop

    public void OnSelectItem()
    {
    }

    public void SwitchPanel(int i)
    {
        panelsShop.ForEach(x => x.SetActive(false));
        panelsShop[i].SetActive(true);
        topButtons.ForEach(x => x.sprite = nonActiveTab);
        topButtons[i].sprite = activeTab;
    }

    #endregion Shop

    #region Game

    public void UpdateEndGameUI(int reward, string title, int rewardBot)
    {
        if (rewardText != null)
            rewardText.text = reward.ToString();

        if (rewardBotText != null)
            rewardBotText.text = rewardBot.ToString();

        if (titleEndGame != null)
            titleEndGame.text = title;
    }

    public void HideWeapon(bool isHide)
    {
        if (hideWeaponImage != null)
        {
            hideWeaponImage.sprite = isHide ? weaponHide : weaponUnHide;
        }
    }

    public void HideWeaponChange(bool isHide)
    {
        btnWeaponChange.gameObject.SetActive(isHide);
    }

    public void SetText(int a, Text text, out int temp)
    {
        temp = a;
        text.text = a.ToString();
    }

    public void UpdateMission(int clue, bool isExit, bool isClue, bool isBoss = false)
    {
        //сюда и босса
        //bossGroup.SetActive(isBoss); /**/
        //exitGroup.SetActive(isExit);
        //clueGroup.SetActive(isClue);
        //clueBack.sprite = cluesSprite[clue];
    }

    public void UpdateHealth(float value)
    {
        hpBar.fillAmount = value;
    }

    public void UpdateCullDown(float cdTime)
    {
        cdFill.fillAmount = 0;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(cdFill.DOFillAmount(1, cdTime))
            .Append(cdFill.DOColor(Color.green, 0.2f))
            .Append(cdFill.DOColor(Color.white, 0.1f));
    }

    public void RotateAim(float duration)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(aim.rectTransform.DORotate(-aim.rectTransform.rotation.eulerAngles, duration / 2))
            .Append(aim.rectTransform.DORotate(aim.rectTransform.rotation.eulerAngles, duration / 2));
    }

    public void OnHitAimAnimation()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(aim.DOFade(1, 0.2f))
            .Append(aim.rectTransform.DOScale(1.5f, 0.2f))
            .Append(aim.DOFade(0, 0.2f))
            .Join(aim.rectTransform.DOScale(1, 0.2f));
    }

    #endregion Game

    #region OpenScreen

    public void OpenScreen(int index)
    {
        if (screens[index].GetType() != typeof(PopUpUI))
        {
            for (int i = 0; i < screens.Count; i++)
            {
                if (i != index)
                {
                    if (screens[i].gameObject.activeSelf)
                    {
                        screens[i].SetActive(false);
                    }
                }
            }
        }
        screens[index].gameObject.SetActive(true);
        screens[index].SetActive(true);
    }

    public void OpenScreen(MenusAtGame screenName)
    {
        ScreenUI target = screens.Where(x => x.screenName == screenName.ToString()).FirstOrDefault();

        if (screenName == MenusAtGame.MainMenuUI)
        {
            Time.timeScale = 1.0f;
            GameController.Instance.SetIsStartGame(false);

            SndManager.Instance.SoundFxValueOff();
        }

        for (int i = 0; i < screens.Count; i++)
        {
            if (screens[i].name != screenName.ToString())
            {
                if (target.GetType() != typeof(PopUpUI))
                {
                    if (screens[i].gameObject.activeSelf)
                    {
                        screens[i].SetActive(false);
                    }
                }
            }
            else
            {
                screens[i].gameObject.SetActive(true);
                screens[i].SetActive(true);
            }
        }

        TestDebugLog.Instance.DebugLog("CurrentMenu = " + Analytics.Instance.CurrentMenu);
    }

    public void OpenScreen(string screenName)
    {
        MenusAtGame temp = (MenusAtGame)System.Enum.Parse(typeof(MenusAtGame), screenName);
        OpenScreen(temp);
    }

    #endregion OpenScreen

    public void OpenShop()
    {
        if (GameController.Instance.isStartGame)
        {
            btnShopNext.gameObject.SetActive(false);
            btnBackToMainMenu.gameObject.SetActive(false);
            btnBackToGameMenu.gameObject.SetActive(true);
        }
        else
        {
            btnShopNext.gameObject.SetActive(true);
            btnBackToMainMenu.gameObject.SetActive(true);
            btnBackToGameMenu.gameObject.SetActive(false);
        }

        OpenScreen(MenusAtGame.ShopUI);
    }

    //public void CloseShop()
    //{
    //    if (GameController.Instance.isStartGame)
    //    {
    //    }
    //    else
    //    {
    //    }
    //}

    #region Levels

    /*
    public void InitLevels(int chapter, int startlvl, int lvlCount,int currentlvl)
    {
        chapterText.text = "Chapter "+chapter.ToString();
        foreach(LevelObject lvl in levelsList)
        {
            DestroyImmediate(lvl.gameObject);
        }
        levelsList.Clear();
        int a = 0;
        for (int i =0; i < lvlCount; i++)
        {
            levelsList.Add(Instantiate(levelPrefab, levelsParent));
            a = startlvl + i+1;
            levelsList[i].Init(currentlvl, a);
        }
    }
    */

    #endregion Levels
}