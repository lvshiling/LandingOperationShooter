using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class GamePlayController : MonoBehaviour
{
    #region SingleTon

    private static GamePlayController _instance;

    public static GamePlayController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GamePlayController>();
            }
            return _instance;
        }
    }

    #endregion SingleTon

    // Use this for initialization
    public Transform animTransform;

    public Animator[] animators;
    public FirstPersonController player;
    public PlayerController playerController;
    public Camera mainCamera;
    public Transform targetForBrainCamera;

    public Transform[] spawnPointsPlayer;
    public Transform[] spawnPointsClue;
    public Transform[] spawnPointsExit;
    public int checkPointsCount;
    public Transform currentCheckPoint;
    public Transform menuCamPos;
    public float menuLight;
    public float gameLight;
    public Light dirLight;
    public float demonCDTime;

    //public BotSpawner spawner;

    public BotSpawner spawnerHunters;

    //public BotSpawner spawnerBoss;

    public GameObject startMobs;

    public Mission mission;
    public bool isBossSlayed = false;

    public int killCountWin;

    private void Start()
    {
        if (!CheckGlVersion.Instance.CheckEffectPossible())
        {
            player.GetComponentInChildren<UnityEngine.PostProcessing.PostProcessingBehaviour>(true).enabled = false;
            mainCamera.GetComponentInChildren<UnityEngine.PostProcessing.PostProcessingBehaviour>(true).enabled = false;
            player.GetComponentInChildren<UnityStandardAssets.ImageEffects.ColorCorrectionLookup>(true).enabled = false;
            mainCamera.GetComponentInChildren<UnityStandardAssets.ImageEffects.ColorCorrectionLookup>(true).enabled = false;
        }
    }

    public void Init()
    {
        SwitchMode(true);
        InitMission();
        InitPlayer(ShopManager.Instance.selectedWeapon as ShopItemWeapon);

        StartCoroutine(CheckWinCoroutine());/**/
    }

    public void SwitchMode(bool isGame)
    {
        mainCamera.gameObject.SetActive(!isGame);
        player.gameObject.SetActive(isGame);
        startMobs.gameObject.SetActive(!isGame);

        Time.timeScale = 1;//!

        //RenderSettings.fog = isGame;
    }

    public void InitMission()
    {
        player.transform.position = spawnPointsPlayer[Random.Range(0, spawnPointsPlayer.Length)].position;
        foreach (Transform tr in spawnPointsClue)
        {
            tr.gameObject.SetActive(false);
        }
        foreach (Transform tr in spawnPointsExit)
        {
            tr.gameObject.SetActive(false);
        }
        foreach (Hero hero in FindObjectsOfType<Hero>())
        {
            hero.searchRadius = hero.startSearchRadius;
            hero.isSeeEveryThing = false;
        }
        mission = new Mission();
        mission.checkPoints = new List<Transform>();
        mission.isCheck = new List<bool>();
        int count = 0;
        while (count < checkPointsCount)
        {
            Transform temp = spawnPointsClue[Random.Range(0, spawnPointsClue.Length)];
            if (!mission.checkPoints.Contains(temp))
            {
                mission.checkPoints.Add(temp);
                mission.isCheck.Add(false);
                count++;
            }
        }
        mission.exitPoint = spawnPointsExit[Random.Range(0, spawnPointsExit.Length)];

        currentCheckPoint = mission.checkPoints[0];
        mission.checkPoints.ForEach(x => x.gameObject.SetActive(false));
        currentCheckPoint.gameObject.SetActive(true);
        CanvasManager.Instance.UpdateMission(0, false, true);
    }

    public void InitPlayer(ShopItemWeapon weapon)
    {
        //weapon.ammoCount += (int)weapon.skills[1].lvlValues[weapon.skills[1].currentValue];//(int)ShopManager.Instance.GetSkillByName("ammo").lvlValues[ShopManager.Instance.GetSkillByName("ammo").currentValue];
        //weapon.damage += (int)weapon.skills[0].lvlValues[weapon.skills[0].currentValue];                     //(int)ShopManager.Instance.GetSkillByName("damage").lvlValues[ShopManager.Instance.GetSkillByName("ammo").currentValue];

        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }

        //пушка
        if (playerController.animator == null || (playerController.weapon == null || playerController.weapon != weapon))
        {
            if (playerController.animator != null)
            {
                DestroyImmediate(playerController.animator.gameObject);
            }

            playerController.animator = Instantiate(weapon.animator.gameObject, animTransform.position, animTransform.rotation, animTransform.parent).GetComponent<Animator>();

            playerController.OnHide(false);
        }

        playerController.weapon = weapon;
        playerController.ammoCount = weapon.ammoCount + (int)weapon.skills[1].lvlValues[weapon.skills[1].currentValue];
        CanvasManager.Instance.ammo.text = (weapon.ammoCount + (int)weapon.skills[1].lvlValues[weapon.skills[1].currentValue]).ToString() + "/" + (weapon.ammoCount + (int)weapon.skills[1].lvlValues[weapon.skills[1].currentValue]).ToString();

        if (playerController.shoteffect == null)
        {
            playerController.muzzleTransform = playerController.FindMuzzle();
            playerController.shoteffect = Instantiate(weapon.shotEffect, playerController.muzzleTransform.position, playerController.muzzleTransform.rotation, playerController.muzzleTransform);
            playerController.shoteffect.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            playerController.shoteffect.SetActive(false);
        }

        //RenderSettings.fogEndDistance = ShopManager.Instance.GetSkillByName("flash").lvlValues[ShopManager.Instance.GetSkillByName("flash").maxCount-1/*ShopManager.Instance.GetSkillByName("flash").currentValue*/];
        playerController.regenHealth = ShopManager.Instance.GetSkillValueByName("regen");

        playerController.maxHealth = ShopManager.Instance.GetSkillValueByName("health");
        playerController.CurrentHealth = playerController.maxHealth;
    }

    public void CheckWin(bool isWin)
    {
        GameController.Instance.EndGame(isWin);
    }

    public void OnSensivityChange(float value)
    {
        player.m_MouseLook.XSensitivity = value;
    }

    public void CheckPoint()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(CanvasManager.Instance.checkPointFlash.DOFade(1, 0.2f))
            .Append(CanvasManager.Instance.checkPointFlash.DOFade(0, 0.2f));

        SndManager.Instance.Play("Exit");
        mission.currentCheckCount++;
        if (mission.currentCheckCount == mission.checkPoints.Count)
        {
            foreach (Hero hero in FindObjectsOfType<Hero>())
            {
                if (!hero.enemyHero.Contains(player.GetComponent<UnitAI>()))
                {
                    hero.enemyHero.Add(player.GetComponent<UnitAI>());
                }
                hero.isSeeEveryThing = true;
            }
            mission.checkPoints[mission.currentCheckCount - 1].gameObject.SetActive(false);
            currentCheckPoint = mission.exitPoint;
            currentCheckPoint.gameObject.SetActive(true);
            SndManager.Instance.Play("End");
            CanvasManager.Instance.UpdateMission(mission.currentCheckCount, true, false);

            //spawnerBoss.gameObject.SetActive(true);

            CanvasManager.Instance.UpdateMission(mission.currentCheckCount, false, false, true);
            CanvasManager.Instance.cdFill.gameObject.SetActive(false);
        }
        else if (mission.currentCheckCount < mission.checkPoints.Count)
        {
            mission.isCheck[mission.currentCheckCount - 1] = true;
            mission.checkPoints[mission.currentCheckCount - 1].gameObject.SetActive(false);
            CanvasManager.Instance.UpdateMission(mission.currentCheckCount, false, true);
            currentCheckPoint = mission.checkPoints[mission.currentCheckCount];
            currentCheckPoint.gameObject.SetActive(true);
        }
        else
        {
            // CheckWin(true);
        }
    }

    public void TargetFly()
    {
        if (CanvasManager.Instance.cdFill.fillAmount == 1)
        {
            CanvasManager.Instance.OpenScreen(MenusAtGame.DemonUI);
            SndManager.Instance.Play("Demon");
            CanvasManager.Instance.UpdateCullDown(demonCDTime);
            //RenderSettings.fog = false;
            Sequence sequence = DOTween.Sequence();
            Vector3 dir = currentCheckPoint.position - player.transform.position;
            Vector3 dirFinish = (currentCheckPoint.position - dir * 15 + currentCheckPoint.up * 10) - currentCheckPoint.position;
            Quaternion rot = Quaternion.LookRotation(dir);
            Quaternion rotFinish = Quaternion.LookRotation(dirFinish);
            Quaternion rotPlayer = player.m_Camera.transform.rotation;
            dir.Normalize();
            sequence
                .Append(player.transform.DORotateQuaternion(rot, 1).OnComplete(() => CameraPosSet(out rotPlayer)))
                .Append(mainCamera.transform.DOMove(player.transform.position + (dir * 10 + player.transform.up * 20), 1f).SetEase(Ease.InExpo))
                .Append(mainCamera.transform.DOMove(currentCheckPoint.position - dir * 15 + currentCheckPoint.up * 10, 2f))
                .Append(mainCamera.transform.DORotate(rot.eulerAngles + new Vector3(30, 0, 0), 0.5f))
                .Append(mainCamera.transform.DOShakePosition(1, 0.1f, 5, 90, false, true))
                .Append(mainCamera.transform.DOMove(player.m_Camera.transform.position, 2f))
                .AppendCallback(() => CanvasManager.Instance.OpenScreen(MenusAtGame.GameUI))
                .AppendCallback(() => SndManager.Instance.Stop("Demon"))
                .OnComplete(() => SwitchMode(true));
        }
    }

    public void BreakFly()
    {
        mainCamera.DOKill();
        Sequence sequence = DOTween.Sequence();
        sequence
              .Append(mainCamera.transform.DOMove(player.m_Camera.transform.position, 1f))
              .AppendCallback(() => CanvasManager.Instance.OpenScreen(MenusAtGame.GameUI))
              .AppendCallback(() => SndManager.Instance.Stop("Demon"))
              .OnComplete(() => SwitchMode(true));
    }

    public void CameraPosSet(out Quaternion rot)
    {
        rot = player.m_Camera.transform.rotation;
        mainCamera.transform.position = player.m_Camera.transform.position;
        mainCamera.transform.rotation = player.m_Camera.transform.rotation;
        SwitchMode(false);
    }

    public IEnumerator CheckWinCoroutine()//Корутина, в которой проверяется, выграли мы или нет.
    {
        while (true)
        {
            if (GameController.Instance.killCount >= killCountWin)
            {
                CheckWin(true);

                StopCoroutine(CheckWinCoroutine());
            }

            yield return new WaitForSeconds(1f);
        }
    }
}