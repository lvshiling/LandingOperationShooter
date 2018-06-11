using AnalyticsPack;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public enum PlayerStage
{
    attack,
    recharge,
    run,
    walk,
    die,
    idle,
}

public class PlayerController : UnitAI
{
    private const float LOWHEALTH = 0.2f;

    [Header("Не смотри на то что выше")]
    [SerializeField]
    private float hp;

    [SerializeField]
    public float CurrentHealth
    {
        get
        {
            return hp;
        }
        set
        {
            if (value > maxHealth)
            {
                value = maxHealth;
            }
            else if (value < 0)
            {
                Death();
            }
            CanvasManager.Instance.UpdateHealth(value / maxHealth);
            hp = value;
        }
    }

    public LeftJoystick leftJoystick; // the game object containing the LeftJoystick script
    public RightJoystick rightJoystick; // the game object containing the RightJoystick script
    public float regenTimeHitDelay;
    private float regenTimer;
    public float regenTimeDelay;
    private PlayerStage stage;
    public int currentWeaponType;
    public ShopItemWeapon weapon;
    private float attackTimer;
    private float lastAttackTime;
    private bool isReload;
    private bool isHide;
    private bool isSit;

    [SerializeField]
    private bool isWater;

    public int ammoCount;
    public bool isInfinityAmmo;
    private CharacterController character;
    public GameObject shoteffect;
    public Transform muzzleTransform;

    [SerializeField]
    private float sitSize;

    [SerializeField]
    private float sitAnimDuration;

    public List<SkillItem> skillItems;

    public AudioClip[] footstepsGround;
    public AudioClip[] footstepsWater;

    private List<Hero> enemyHeroes;

    [Header("компоненты")]
    [SerializeField]
    public Animator animator;

    private Vector3 screenCenter;

    private Vector3 CenterScreen()
    {
        return new Vector3(Screen.width / 2, Screen.height / 2);
    }

    void CheckWeapon2()
    {
        if(ShopManager.Instance.selectedWeapon2 && CanvasManager.Instance.btnWeaponChange.gameObject.activeSelf == false)
        {
            CanvasManager.Instance.btnWeaponChange.gameObject.SetActive(true);
        }
        else if(!ShopManager.Instance.selectedWeapon2 && CanvasManager.Instance.btnWeaponChange.gameObject.activeSelf)
        {
            CanvasManager.Instance.btnWeaponChange.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        character = GetComponent<CharacterController>();
        enemyHeroes = new List<Hero>();

        CanvasManager.Instance.ammo.text = ammoCount.ToString();

        screenCenter = CenterScreen();

        CheckWeapon2();
    }

    public bool isMobileMove = true;

    protected virtual void Update()
    {
        Regeneration();

        CheckEnemyToAim();/**/
        CheckAttack();/**/

        CheckWater();

        CheckWeapon2();

        if (isMobileMove)
        {
            animator.SetFloat("Speed", Mathf.Abs(leftJoystick.GetInputDirection().y));
            GamePlayController.Instance.player.GetMove(leftJoystick.GetInputDirection());
        }
        else
        {
            animator.SetFloat("Speed", Mathf.Abs(leftJoystick.GetInputDirectionPC().y));
            GamePlayController.Instance.player.GetMove(leftJoystick.GetInputDirectionPC());
        }

        GamePlayController.Instance.player.m_MouseLook.GetRot(rightJoystick.GetInputDirection());
    }

    public Transform FindMuzzle()
    {
        Transform[] temp = transform.GetComponentsInChildren<Transform>();
        foreach (Transform t in temp)
        {
            if (t.name == "FxPlace")
            {
                return t;
            }
        }
        return null;
    }

    public void ChangeState(string state)
    {
        ChangeState(StringToEnum<PlayerStage>(state));
    }

    public void ChangeState(PlayerStage state)
    {
        stage = state;
        switch (stage)
        {
            case PlayerStage.attack:
                {
                    OnAttack();
                    break;
                }
            case PlayerStage.recharge:
                {
                    Recharge();
                    break;
                }
        }
        CanvasManager.Instance.ammo.text = ammoCount.ToString() + "/" + (weapon.ammoCount + (int)weapon.skills[1].lvlValues[weapon.skills[1].currentValue]).ToString();
    }

    private Hero enemy;

    /// <summary>
    /// Атака, если враг в прицеле
    /// </summary>
    protected virtual void CheckAttack()
    {
        if (ammoCount != 0)
        {
            attackTimer += Time.deltaTime;

            if (!isHide)
            {
                if (weapon.isRateOfFire)/**/
                {
                    weapon.attackDelay = GetAttackDelay();
                }

                if (attackTimer > (weapon.attackDelay + (isReload ? weapon.reloadTime : 0)))
                {
                    // < --Так было-- >

                    //RaycastHit hit;
                    //Ray ray = new Ray(transform.position, transform.forward);
                    //if (Physics.SphereCast(ray, 1, out hit, weapon.attackDistance))
                    //{
                    //    if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Aggro"))
                    //    {
                    //        ChangeState(PlayerStage.attack);

                    //        hit.collider.GetComponent<UnitI>().DamageTake(0, weapon.damage + (int)weapon.skills[0].lvlValues[weapon.skills[0].currentValue]);
                    //        CanvasManager.Instance.OnHitAimAnimation();
                    //    }
                    //}

                    if (isEnemyToAim)
                    {
                        ChangeState(PlayerStage.attack);

                        hit.collider.GetComponent<UnitI>().DamageTake(0, weapon.damage + (int)weapon.skills[0].lvlValues[weapon.skills[0].currentValue]);

                        CanvasManager.Instance.OnHitAimAnimation();

                        if (hit.collider.GetComponent<Hero>())
                        {
                            enemy = hit.collider.GetComponent<Hero>();

                            CheckKillCount(enemy);
                        }
                    }
                }
            }
        }
    }

    private float GetAttackDelay()
    {
        return 1 / weapon.rateOfFire;
    }

    /// <summary>
    /// Увеличение счетчика количества убитых
    /// </summary>
    /// <param hero="в кого стреляем"></param>
    public void CheckKillCount(Hero hero)
    {
        if (enemy.currentHealth <= 0)
        {
            GameController.Instance.GiveMeSomeReward(enemy.reward, 0);
            GameController.Instance.totalKillReward += enemy.reward;
            GameController.Instance.killCount++;

            CanvasManager.Instance.SetCountAlive(GamePlayController.Instance.spawnerHunters.maxBot - GameController.Instance.killCount);

            Analytics.Instance.events.shooter.KillUnit();
        }
    }

    public RectTransform aim;
    public bool isEnemyToAim;
    private RaycastHit hit;
    private Ray ray;

    /// <summary>
    /// в прицеле ли враг
    /// </summary>
    private void CheckEnemyToAim()
    {
        // Ray ray = new Ray(transform.position, transform.forward);
        ray = Camera.main.ScreenPointToRay(aim.position);

        if (Physics.Raycast(ray, out hit/*, weapon.attackDistance*/))
        {
            if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Aggro"))
            {
                isEnemyToAim = true;
            }
            else
            {
                isEnemyToAim = false;
            }
        }
    }

    public void ClickAttack()
    {
        ChangeState(PlayerStage.attack);

        if (isEnemyToAim)
        {
            if (hit.collider.GetComponent<UnitI>() != null)
            {
                hit.collider.GetComponent<UnitI>().DamageTake(0, weapon.damage + (int)weapon.skills[0].lvlValues[weapon.skills[0].currentValue]);

                // hit.collider.GetComponent<Hero>().attackRadius = 100;/**/

                CanvasManager.Instance.OnHitAimAnimation();
            }
        }
    }

    protected virtual void CheckWater()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, -1 * transform.up);
        Debug.DrawRay(transform.position, -1 * transform.up, Color.red);
        if (Physics.Raycast(ray, out hit, 10.0f))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Water"))
            {
                isWater = true;
                GamePlayController.Instance.player.m_FootstepSounds = footstepsWater;
            }
            else
            {
                isWater = false;
                GamePlayController.Instance.player.m_FootstepSounds = footstepsGround;
            }
        }
        else
        {
            isWater = false;
            GamePlayController.Instance.player.m_FootstepSounds = footstepsWater;
        }

        GamePlayController.Instance.player.m_IsWalking = isSit || isWater || CrossPlatformInputManager.GetAxis("Vertical") < 0;
    }

    public IEnumerator Deactivator(float time, GameObject temp)
    {
        yield return new WaitForSeconds(time);
        temp.SetActive(false);
        StopCoroutine("Deactivator");
    }

    void SoundWeaponAttackPlay()
    { 
        int typeWeapon = isSecondWeapon ? 1 : 0;

        SndManager.Instance.Play("weapon" + typeWeapon);
    }

    public virtual void OnAttack()
    {
        if (ammoCount != 0)
        {
            attackTimer = 0;
            animator.SetTrigger("Attack");
            animator.SetInteger("Weapon", currentWeaponType);

            if (shoteffect != null && !shoteffect.activeSelf)
            {
                shoteffect.SetActive(true);
                StartCoroutine(Deactivator(0.1f, shoteffect));
            }
            //SndManager.Instance.Play("weapon" + currentWeaponType.ToString());

            SoundWeaponAttackPlay();
        }

        if (!isInfinityAmmo)
        {
            ammoCount--;

            if (ammoCount <= 0)
            {
                ammoCount = 0;
                ChangeState(PlayerStage.recharge);
            }
            CanvasManager.Instance.ammo.text = ammoCount.ToString() + "/" + (weapon.ammoCount + (int)weapon.skills[1].lvlValues[weapon.skills[1].currentValue]).ToString();
        }
    }

    //смена оружия
    private bool isSecondWeapon = false;

    //или спрятать, если нет
    public virtual void OnHide()
    {
        //оно работает
        //если среди винтовок что-то куплено, то
        //GamePlayController.Instance.InitPlayer(ShopManager.Instance.shopItems[UnityEngine.Random.Range(0,6)]);
        if (ShopManager.Instance.selectedWeapon2)
        {
            if (!isSecondWeapon)
            {
                GamePlayController.Instance.InitPlayer(ShopManager.Instance.selectedWeapon2 as ShopItemWeapon);
                isSecondWeapon = true;
            }
            else
            {
                GamePlayController.Instance.InitPlayer(ShopManager.Instance.selectedWeapon as ShopItemWeapon);
                isSecondWeapon = false;
            }
        }
        else
        {
            isHide = !isHide;
            CanvasManager.Instance.HideWeapon(isHide);
            if (isHide)
            {
                animator.SetTrigger("Hide");
                animator.SetInteger("Weapon", currentWeaponType);
            }
            else
            {
                animator.SetTrigger("UnHide");
                animator.SetInteger("Weapon", currentWeaponType);
            }
        }
    }

    public void ChangeWeapon()
    {
        if (ShopManager.Instance.selectedWeapon2)
        {
            if (!isSecondWeapon)
            {
                GamePlayController.Instance.InitPlayer(ShopManager.Instance.selectedWeapon2 as ShopItemWeapon);
                isSecondWeapon = true;
            }
            else
            {
                GamePlayController.Instance.InitPlayer(ShopManager.Instance.selectedWeapon as ShopItemWeapon);
                isSecondWeapon = false;
            }
        }
    }

    public virtual void OnHide(bool hide)
    {
        isHide = hide;
        CanvasManager.Instance.HideWeapon(isHide);
        if (isHide)
        {
            animator.SetTrigger("Hide");
            animator.SetInteger("Weapon", currentWeaponType);
        }
        else
        {
            animator.SetTrigger("UnHide");
            animator.SetInteger("Weapon", currentWeaponType);
        }
    }

    protected virtual void Recharge()
    {
        isReload = true;
        animator.SetTrigger("Recharge");
        animator.SetTrigger("Reload");
        animator.SetInteger("Weapon", currentWeaponType);
        //ammoCount = weapon.ammoCount + (int)weapon.skills[1].lvlValues[weapon.skills[1].currentValue];
        //CanvasManager.Instance.ammo.text = ammoCount.ToString() + "/" + ammoCount;

        StartCoroutine(EndRecharge());
    }

    private IEnumerator EndRecharge()
    {
        yield return new WaitForSeconds(1.25f);
        OnRechargeEnd();
    }

    public void OnRechargeEnd()
    {
        isReload = false;
        ammoCount = weapon.ammoCount + (int)weapon.skills[1].lvlValues[weapon.skills[1].currentValue];
        CanvasManager.Instance.ammo.text = ammoCount.ToString() + "/" + ammoCount;
    }

    /*
    public override IEnumerator IEDamageTake(float damageM, float damageF)
    {
        yield return new WaitForSeconds(1.1f);
        float armoryF = armorF - damageF;
        if (armoryF > 0)
            armoryF = 0;
        float armoryM = armorM - damageM;
        if (armoryM > 0)
            armoryM = 0;
        CurrentHealth += armoryF + armoryM;
        lastAttackTime = Time.time;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(CanvasManager.Instance.gameFlash.DOFade(0.3f, 0.2f))
            .Append(CanvasManager.Instance.gameFlash.DOFade(0, 0.1f));
    }
    */

    override public void DamageTake(float damageM, float damageF)
    {
        float armoryF = armorF - damageF;
        if (armoryF > 0)
        {
            armoryF = 0;
        }

        float armoryM = armorM - damageM;
        if (armoryM > 0)
        {
            armoryM = 0;
        }

        CurrentHealth += armoryF + armoryM;
        lastAttackTime = Time.time;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(CanvasManager.Instance.gameFlash.DOFade(0.3f, 0.2f))
            .Append(CanvasManager.Instance.gameFlash.DOFade(0, 0.1f));
    }

    private void Regeneration()
    {
        regenTimer += Time.deltaTime;
        if (LOWHEALTH > CurrentHealth / maxHealth)
        {
            SndManager.Instance.Play("LowHp");
        }
        else
        {
            SndManager.Instance.Stop("LowHp");
        }
        if (lastAttackTime + regenTimeHitDelay < Time.time && regenTimer > regenTimeDelay)
        {
            regenTimer = 0;
            CurrentHealth += regenHealth;
        }
    }

    protected virtual void Death()
    {
        GamePlayController.Instance.CheckWin(false);

        SndManager.Instance.Play("DeathPlayer");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CheckPoint"))
        {
            GamePlayController.Instance.CheckPoint();
        }
    }

    public void ChangePose()
    {
        isSit = !isSit;
        enemyHeroes.Clear();
        enemyHeroes.AddRange(FindObjectsOfType<Hero>());
        if (isSit)
        {
            DOTween.To(() => GamePlayController.Instance.player.m_OriginalCameraPosition, x => GamePlayController.Instance.player.m_OriginalCameraPosition = x, new Vector3(0, sitSize, 0), sitAnimDuration);

            enemyHeroes.ForEach(x => x.searchRadius /= 2);
        }
        else
        {
            DOTween.To(() => GamePlayController.Instance.player.m_OriginalCameraPosition, x => GamePlayController.Instance.player.m_OriginalCameraPosition = x, new Vector3(0, 0.8f, 0), sitAnimDuration);
            enemyHeroes.ForEach(x => x.searchRadius *= 2);
        }
    }

    private T StringToEnum<T>(string nameUI) where T : struct, IConvertible
    {
        T temp = new T();
        foreach (T menu in System.Enum.GetValues(typeof(T)))
        {
            if (nameUI == menu.ToString())
            {
                temp = menu;
                return menu;
            }
        }
        return temp;
    }

    private bool isTakeAim = false;

    public void SetTakeAim()
    {
        isTakeAim = !animator.GetBool("TakeAim");

        if (isTakeAim)
        {
            animator.ResetTrigger("TakeAimFalse");
            animator.SetTrigger("TakeAimTrue");
            animator.SetBool("TakeAim", true);
        }
        else
        {
            animator.SetTrigger("TakeAimFalse");
            animator.ResetTrigger("TakeAimTrue");
            animator.SetBool("TakeAim", false);
        }
    }
}