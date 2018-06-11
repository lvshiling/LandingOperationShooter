using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum HeroStage
{
    Walk,
    Run,
    Shop,
    Attack,
    UseSckill,
    HealAllies,
    OtherControl,
    Flee,
    Free,
    Death
}

public enum HeroClass
{
    Warrior,
    Ranger,
    Cliric,
    Mage
}

public enum HeroStatus
{
    Allies,
    Enemy,
    Dead,
    Neutral,
    Aggro
}

[System.Serializable]
public struct EnemyPrefer
{
    public string name;
    public float wishKill;
}

public struct CheckTarget
{
    public float minHealth;
    public float maxHealth;
    public float distance;
    public float enemyAggro;
    public float mainResult;
}

[System.Serializable]
public class AttackPreferList
{
    public float minHealth;
    public float maxHealth;
    public float minDistance;
    public float classPrefer;

    public AttackPreferList()
    {
        if (minHealth == 0 && maxHealth == 0 && minDistance == 0 && classPrefer == 0)
        {
            minDistance = 1;
            minHealth = 0;
            maxHealth = 0;
            classPrefer = 1;
        }
    }
}

public delegate void PlayerDamageTakenEventHandler(float damageM, float damageF);

public delegate void PlayerAttackEventHandler(float damageM, float damageF, Transform source);

public delegate void PlayerDeath();

public class Hero : UnitAI
{
    public event PlayerDamageTakenEventHandler heroDamage;

    public event PlayerAttackEventHandler heroAttack;

    public event PlayerDeath playerDeath;

    [Header("Статистика персонажа lvlup")]
    public int xpForDeath;

    [Header("Настройка объекта")]
    public Image hpImage;

    public GameObject attackPrefab;
    public Animator anim;
    public Animation animat;

    public Animation animIdle;/**/

    // public BlendCounts blendCounts;
    public NavMeshAgent agent;

    public HeroStage stageCurrent;
    public GameObject heroCastle;
    private Vector3 wantToWalk;

    public List<string> attackMusic;
    public List<string> runMusic;
    public List<string> impactMusic;
    public List<string> deathMusic;

    public SphereCollider aggroTrigger;
    public bool damageTaken;
    private bool alreadyDead;

    [Header("Настройка объекта Walk")]
    public float walkRadius;

    public float walkTime;
    public float walkTimeCount;
    public float remainigDistance = 2;
    public int reward;

    [Header("Настройка объекта Attack")]
    private Dictionary<UnitAI, float> tempResult;

    private GameObject arrow;
    public int enemyNumber;

    public float attackRadius;
    public float attackTime;
    public float attackCount;
    public float attackRadiusRemainig;

    [SerializeField]
    private float radius;

    [SerializeField]
    public float searchRadius
    {
        get
        {
            return radius;
        }
        set
        {
            aggroTrigger.radius = value;
            radius = value;
        }
    }

    public float startSearchRadius;
    public Transform muzzlePosition;
    public GameObject muzzleFx;
    public GameObject deathFx;
    public GameObject[] damageFx;
    public GameObject hitFx;
    public bool skillControllAttack;
    private CheckTarget checkTargetParametrs;

    [Header("Настройка объекта HealAllies")]
    public int alliesNumber;

    public float healRadius;
    public bool isSeeEveryThing;
    private float dist;
    public bool isBoss = false;

    // public List<TargetPrefer> targetClass;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!agent.isOnNavMesh)
        {
            agent.enabled = false;
        }
    }

    private void Start()
    {
        agent.enabled = true;
        tempResult = new Dictionary<UnitAI, float>();
        if (aggroTrigger != null)
        {
            aggroTrigger.radius = searchRadius;
        }

        sizeObject = sizeObject == 0 ? 1 : sizeObject;
        currentMana = maxMana;
        currentHealth = maxHealth;

        if (attackList.minHealth == 0 && attackList.maxHealth == 0 && attackList.minDistance == 0 && attackList.classPrefer == 0)
        {
            attackList = new AttackPreferList();
        }

        if (Status != HeroStatus.Neutral)
        {
            //gameObject.tag = Status == HeroStatus.Enemy ? "Enemy" : "Allies";
            if (Status == HeroStatus.Aggro)
            {
                gameObject.tag = "Aggro";
            }
            else if (Status == HeroStatus.Enemy)
            {
                gameObject.tag = "Enemy";
            }
            else
            {
                gameObject.tag = "Allies";
            }
        }
        agent.speed = normalSpeed;
        ChangeState(HeroStage.Walk);
        if (damageFx != null && damageFx.Length > 0 && damageFx[0] != null)
        {
            FindPossibleDamagePosition();
        }
        if (muzzleFx != null && muzzlePosition != null)
        {
            muzzleFx = Instantiate(muzzleFx, muzzlePosition.position, muzzlePosition.rotation, muzzlePosition);
            muzzleFx.SetActive(false);
        }
        if (deathFx != null)
        {
            deathFx = Instantiate(deathFx, transform.position, transform.rotation, transform);
            deathFx.transform.localScale *= sizeObject / 2;
            deathFx.SetActive(false);
        }
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

    public void FindPossibleDamagePosition()
    {
        if (damageFx != null && damageFx.Length > 0)
        {
            int n = (int)(maxHealth + sizeObject) / 10;
            Vector3[] tempVectors = new Vector3[n];
            Mesh tempMesh = transform.GetComponentInChildren<MeshFilter>().mesh;
            GameObject tempFx = damageFx[0];

            if (tempMesh != null)
            {
                damageFx = new GameObject[n];
                for (int i = 0; i < tempVectors.Length; i++)
                {
                    tempVectors[i] = tempMesh.vertices[Random.RandomRange(0, tempMesh.vertices.Length)];
                    damageFx[i] = Instantiate(tempFx, transform.position, Quaternion.identity, transform);
                    damageFx[i].transform.localPosition += tempVectors[i] * 1 / 3;
                    damageFx[i].SetActive(false);
                }
            }
        }
    }

    private float slayDelay = 1.0f;

    private void FixedUpdate()
    {
        if (Time.timeScale != 0)
        {
            //LvlUpdate();
            DeathCheck();
            if (stageCurrent == HeroStage.OtherControl) { }
            else
            {
                StatsUpdate();
                if (stageCurrent == HeroStage.UseSckill)
                {
                    //
                }
                else
                {
                    CheckAttack();
                }
                if (stageCurrent == HeroStage.Walk)
                {
                    CurrentWalk();
                }
            }
        }

        slayDelay -= Time.deltaTime;
        //foreach (var a in enemyHero)//костыль поиска врагов
        for (int i = enemyHero.Count - 1; i >= 0; i--)
        {
            if ((enemyHero[i] == null || enemyHero[i].Status == HeroStatus.Dead) && slayDelay <= 0.0f)
            {
                enemyHero.RemoveAt(i);
                slayDelay = 1.0f;
                //enemyHero.re
                //Debug.Log(gameObject.name + " Ошибка поиска ");
            }
        }

        if (isBoss && GamePlayController.Instance.player.gameObject.activeInHierarchy)
        {
            isBoss = false;//переделать

            bool find = false;
            for (int i = 0; i < enemyHero.Count; i++)
            {
                if (GamePlayController.Instance.player.gameObject == enemyHero[i].gameObject)
                {
                    find = true;
                }
            }
            if (!find)
            {
                enemyHero.Add(GamePlayController.Instance.player.GetComponent<UnitAI>());
            }

            target = GamePlayController.Instance.player.transform;
        }
    }

    public void Walk()
    {
        if (normalSpeed != 0)
        {
            agent.SetDestination(wantToWalk);
        }
    }

    /*
    override public IEnumerator IEDamageTake(float damageM, float damageF)
    {
        yield return new WaitForSeconds(0.1f);

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

        currentHealth += armoryF + armoryM;
        if (damageFx != null && damageFx.Length > 0 && (maxHealth - currentHealth) * 100 / maxHealth < Random.Range(0, 100))
        {
            damageFx[Random.Range(0, damageFx.Length)].SetActive(true);
        }
        damageTaken = true;
        try
        {
            heroDamage(armoryM, armoryF);
        }
        catch
        {
        }
    }
    */

    override public void DamageTake(float damageM, float damageF)
    {
        //anim.Play("Hit");

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

        currentHealth += armoryF + armoryM;
        if (damageFx != null && damageFx.Length > 0 && (maxHealth - currentHealth) * 100 / maxHealth < Random.Range(0, 100))
        {
            damageFx[Random.Range(0, damageFx.Length)].SetActive(true);
        }
        damageTaken = true;
        try
        {
            heroDamage(armoryM, armoryF);
        }
        catch
        {
        }
    }

    public void RandomWalk()
    {
        if (normalSpeed > 0)
        {
            wantToWalk = heroCastle.transform.position + new Vector3(Random.Range(-walkRadius, walkRadius), 0, Random.Range(-walkRadius, walkRadius));
        }
    }

    public void CurrentWalk()
    {
        if (agent.isOnNavMesh && agent.isActiveAndEnabled && (agent.remainingDistance < remainigDistance || agent.isPathStale))
        {
            RandomWalk();
            Walk();
            ChangeState(HeroStage.Walk);
            walkTime++;
            if (walkTime > walkTimeCount)
            {
                walkTime = 0;
                walkRadius *= 1.1f;
            }
        }
    }

    public bool AgentCheck()
    {
        return agent.remainingDistance != 0
            && target != null
            && agent.isOnNavMesh
            && agent.isActiveAndEnabled
            && !float.IsInfinity(agent.remainingDistance)
            && (agent.destination - target.position).magnitude < 1;
    }

    public void CheckAttack()
    {
        if (enemyHero.Count > 0)
        {
            //if (agent.speed > 0)/**/
            //{
            //    ChangeState(HeroStage.Walk);
            //}

            CheckTargets();
            //if (agent.speed == 0)/**/
            //{
            //    agent.speed = normalSpeed;
            //}
            if (normalSpeed != 0)
            {
                if (target != null)
                {
                    agent.SetDestination(target.position);

                    agent.speed = normalSpeed;
                }
            }
            if (agent.pathPending)
            {
                dist = Vector3.Distance(transform.position, target.position);
            }
            else
            {
                dist = agent.remainingDistance;
            }

            if (AgentCheck() && dist < attackRadius + enemyHero[enemyNumber].sizeObject + sizeObject)
            {
                agent.speed = 0;
                Attack();
            }
            else if (!isSeeEveryThing && AgentCheck() && dist > 1.2f * searchRadius)
            {
                target = null;
                enemyHero.RemoveAt(enemyNumber);
                ChangeState(HeroStage.Walk);
            }

            ChangeState(HeroStage.Attack);
        }
        else
        {
            if (stageCurrent != HeroStage.Walk)
            {
                agent.speed = normalSpeed;
                ChangeState(HeroStage.Walk);

                agent.speed = normalSpeed;
            }
        }
    }

    public void CheckMinHealth()
    {
        float min = enemyHero[0].currentHealth;
        UnitAI target = enemyHero[0];
        for (int i = 1; i < enemyHero.Count; i++)
        {
            if (enemyHero[i].currentHealth < min)
            {
                min = enemyHero[i].currentHealth;
                target = enemyHero[i];
                enemyNumber = i;
            }
        }
        this.target = target.transform;
    }

    public void CheckClose()
    {
        if (enemyHero[0] != null)
        {
            float min = (transform.position - enemyHero[0].transform.position).magnitude;
            UnitAI target = enemyHero[0];
            float distance;
            enemyNumber = 0;
            for (int i = 1; i < enemyHero.Count; i++)
            {
                if (enemyHero[i] != null)
                {
                    distance = (transform.position - enemyHero[i].transform.position).magnitude;
                    if (distance < min)
                    {
                        min = distance;
                        target = enemyHero[i];
                        enemyNumber = i;
                    }
                }
            }
            this.target = target.transform;
        }
    }

    public float CheckMaxDistance()
    {
        if (enemyHero[0] != null)
        {
            float max = (transform.position - enemyHero[0].transform.position).magnitude;
            UnitAI target = enemyHero[0];
            float distance;
            enemyNumber = 0;

            for (int i = 1; i < enemyHero.Count; i++)
            {
                if (enemyHero[i] != null)
                {
                    distance = (transform.position - enemyHero[i].transform.position).magnitude;
                    if (distance > max)
                    {
                        max = distance;
                        target = enemyHero[i];
                        enemyNumber = i;
                    }
                }
            }
            this.target = target.transform;
            return max;
        }
        return 1;
    }

    public UnitAI CheckMax()
    {
        float max = enemyHero[0].currentHealth;
        UnitAI target = enemyHero[0];
        for (int i = 1; i < enemyHero.Count; i++)
        {
            if (enemyHero[i] != null)
            {
                if (enemyHero[i].currentHealth > max)
                {
                    max = enemyHero[i].currentHealth;
                    target = enemyHero[i];
                    enemyNumber = i;
                }
            }
        }
        this.target = target.transform;
        return target;
    }

    public void Attack()
    {
        attackTime += Time.deltaTime;
        if (attackTime > attackDelay && attackDelay > 0)
        {
            agent.speed = 0;

            attackTime = 0;
            MadeAttack();
        }
        else
        {
            if (anim != null)
            {
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                {
                    var a = 0;// Random.Range(0, blendCounts.idleCount);
                    anim.SetFloat("random", a);
                    //anim.Play("Idle");
                    agent.speed = normalSpeed;
                    ChangeState(HeroStage.Walk);
                }
            }
            else if (animat != null)
            {
                animat.CrossFade("Idle");
            }
        }
    }

    public void MakeRangeAttack()
    {
        try
        {
            for (int i = 0; i < impactMusic.Count; i++)
            {
                SndManager.Instance.Play(impactMusic[i]);
            }
            if (hitFx != null)
            {
                DestroyObject(Instantiate(hitFx, target.position + new Vector3(0, sizeObject, 0), Quaternion.identity), 5.0f);
            }

            heroAttack(attackM, attackF, target);
            if (!skillControllAttack)
            {
                skillControllAttack = false;

                enemyHero[enemyNumber].DamageTake(attackM, attackF);
            }
        }
        catch
        {
        }
    }

    private IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(0.9f);
        if (tag != "Corpse")
        {
            enemyHero[enemyNumber].DamageTake(attackM, attackF);
        }
    }

    private IEnumerator AttackDelay2()
    {
        //yield return new WaitForSeconds(1f);
        if (tag != "Corpse")
        {
            if (enemyHero.Count > enemyNumber)
            {
                if (enemyHero[enemyNumber].transform == target)
                {
                    enemyHero[enemyNumber].DamageTake(attackM, attackF);

                    TestDebugLog.Instance.DebugLog("Враг Атакует");
                }
            }
        }

        yield return new WaitForSeconds(1f);
        //heroAttack(attackF, attackM, transform);
    }

    public virtual void MadeAttack()
    {
        // TestDebugLog.Instance.DebugLog(string.Concat("Бот ", gameObject.name, " атакует"));

        if (muzzleFx != null && !muzzleFx.activeSelf)
        {
            muzzleFx.SetActive(true);
            StartCoroutine(Deactivator(1, muzzleFx));
        }
        for (int i = 0; i < attackMusic.Count; i++)
        {
            SndManager.Instance.Play(attackMusic[i]);
        }
        switch (Class)
        {
            case HeroClass.Warrior:

                if (Random.Range(0, 100) > 80)
                {
                    SndManager.Instance.Play("hit" + Random.Range(1, 7));
                }
                try
                {
                    heroAttack(attackF, attackM, transform);
                    //StartCoroutine(AttackDelay2());
                }
                catch
                {
                }
                if (!skillControllAttack)
                {
                    skillControllAttack = false;
                    //enemyHero[enemyNumber].DamageTake(attackM, attackF);
                    if (tag == "Aggro")
                    {
                        StartCoroutine(AttackDelay2());
                    }
                    else
                    {
                        StartCoroutine(AttackDelay());
                    }
                    //enemyHero[enemyNumber].IEDamageTake(attackM, attackF);
                }
                break;

            case HeroClass.Ranger:
                if (Random.Range(0, 100) > 70)
                {
                    SndManager.Instance.Play("Arrow" + Random.Range(1, 4));
                }
                arrow = Instantiate(attackPrefab, transform.position + Vector3.forward + Vector3.up, Quaternion.identity) as GameObject;
                arrow.GetComponent<SC_Projectile>().FireProjectile(this, enemyHero[enemyNumber], attackM, attackF);
                break;

            case HeroClass.Cliric:

                enemyHero[enemyNumber].DamageTake(attackM, attackF);
                break;

            case HeroClass.Mage:
                SndManager.Instance.Play("mageF" + Random.Range(1, 5));
                arrow = Instantiate(attackPrefab, transform.position + Vector3.forward + Vector3.up, Quaternion.identity) as GameObject;
                arrow.GetComponent<SC_Projectile>().FireProjectile(this, enemyHero[enemyNumber], attackM, attackF);
                break;
        }
        if (anim != null)
        {
            anim.SetFloat("random", Random.Range(0, 2));
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                anim.Play("Attack");
            }
            //anim.GetCurrentAnimatorStateInfo(0).cl
            // }
        }
        else if (animat != null)
        {
            animat.CrossFade("Attack");
            //anim.Play("Attack");
            //animat.pla
        }
    }

    public IEnumerator Deactivator(int time, GameObject temp)
    {
        yield return new WaitForSeconds(time);
        temp.SetActive(false);
        StopCoroutine("Deactivator");
    }

    public void LvlUpdate()
    {
        if (xpPoint >= lvlCup)
        {
            lvl++;
            xpPoint -= lvlCup;
            LvlUp();
        }
    }

    public void LvlUp()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
    }

    public void DeathCheck()
    {
        if (!alreadyDead)
        {
            if (currentHealth <= 0)
            {
                alreadyDead = true;
                Death();
                for (int i = 0; i < enemyHero.Count; i++)
                {
                    if (enemyHero[i].target == transform)
                    {
                        enemyHero[i].target = null;
                    }
                    enemyHero[i].enemyHero.Remove(this);
                    if (enemyHero[i].GetType() == typeof(PlayerController))
                    {
                        /**/
                        //GameController.Instance.GiveMeSomeReward(reward, 0);
                        //GameController.Instance.totalKillReward += reward;
                        //GameController.Instance.killCount++;
                    }
                }
                for (int i = 0; i < alliesHero.Count; i++)
                {
                    alliesHero[i].alliesHero.Remove(this);
                }
            }
        }
    }

    public void Death()
    {
        try
        {
            playerDeath();
        }
        catch
        {
#if !FINAL_VERSION
            Debug.Log("Никто не хочет знать, что со мной");
#endif
        }
        agent.enabled = false;
        tag = "Corpse";
        Status = HeroStatus.Dead;
        ChangeState(HeroStage.Death);
    }

    public void DelayDeath()
    {
        StartCoroutine(Move(3));
        StartCoroutine(DestroyObj(6));
    }

    public IEnumerator DestroyObj(int timer)
    {
        yield return new WaitForSeconds(timer);

        GamePlayController.Instance.spawnerHunters.bots.Remove(this);/**/

        Destroy(gameObject);
    }

    public IEnumerator Move(int timer)
    {
        yield return new WaitForSeconds(timer);
        gameObject.AddComponent<MoveObjDown>();
    }

    public void StatsUpdate()
    {
        if (currentMana < 0)
        {
            currentMana = 0;
        }
        if (currentMana > maxMana)
        {
            currentMana = maxMana;
        }
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        if (hpImage)
        {
            hpImage.fillAmount = currentHealth / maxHealth;
        }
    }

    public void ChangeState(HeroStage Stage)
    {
        stageCurrent = Stage;
        switch (Stage)
        {
            case HeroStage.Walk:
                for (int i = 0; i < runMusic.Count; i++)
                {
                    SndManager.Instance.Play(runMusic[i]);
                }
                if (anim != null)
                {
                    var a = 0;// Random.Range(0, blendCounts.runCount);
                    //anim.SetFloat("random", a);
                    anim.Play("Walk");
                    //Anim.SetFloat("Speed", 1);
                }
                else
                if (animat != null)
                {
                    animat.CrossFade("Walk");
                }
                break;

            case HeroStage.Attack:
                if (agent.speed > 0)
                {
                    for (int i = 0; i < runMusic.Count; i++)
                    {
                        SndManager.Instance.Play(runMusic[i]);
                    }

                    if (anim != null)
                    {
                        //Random.Range(0, blendCounts.runCount);
                        anim.SetFloat("random", 0);
                        anim.Play("Walk");
                        // Anim.SetFloat("Speed", 1);
                    }
                    else
                    if (animat != null)
                    {
                        animat.CrossFade("Walk");
                    }
                }
                break;

            case HeroStage.Run:
                if (anim != null)
                {
                    anim.Play("Run");
                    //Anim.SetFloat("Speed", 1);
                }
                else
             if (animat != null)
                {
                    animat.CrossFade("Run");
                }
                break;

            case HeroStage.Shop:
                break;

            case HeroStage.UseSckill:
                break;

            case HeroStage.Death:
                agent.speed = 0;
                if (gameObject.name.Contains("Boss"))
                {
                    GamePlayController.Instance.isBossSlayed = true;
                    CanvasManager.Instance.UpdateMission(3, true, false);
                    CanvasManager.Instance.cdFill.gameObject.SetActive(true);
                }
                DelayDeath();
                for (int i = 0; i < deathMusic.Count; i++)
                {
                    SndManager.Instance.Play(deathMusic[i]);
                }
                if (deathFx != null)
                {
                    deathFx.SetActive(true);
                }
                if (Random.Range(0, 100) > 50)
                {
                    SndManager.Instance.Play("Death" + Random.Range(1, 4));
                }
                if (anim != null)
                {
                    /* var a = Random.Range(0, blendCounts.deathCount);
                     anim.SetFloat("random", a);*/
                    anim.Play("Death");
                    //Anim.SetFloat("Speed", 1);
                }
                else
                if (animat != null)
                {
                    var a = Random.Range(0, 100) > 49 ? 1 : 2;
                    animat.CrossFade("Death"/* + a*/);
                }
                ChangeState(HeroStage.OtherControl);
                break;

            case HeroStage.Flee:
                agent.speed = normalSpeed;
                if (agent.speed > 0)
                {
                    if (anim != null)
                    {
                        anim.Play("Walk");
                        //Anim.SetFloat("Speed", 1);
                    }
                    else
                    if (animat != null)
                    {
                        animat.CrossFade("Walk");
                    }
                    break;
                }
                break;
        }
    }

    public void CheckTargets()
    {
        if (enemyHero.Count > 0)
        {
            tempResult.Clear();
            for (int i = 0; i < enemyHero.Count; i++)
            {
                if (enemyHero[i] != null)
                {
                    checkTargetParametrs.minHealth = 1 - (enemyHero[i].currentHealth / CheckMax().currentHealth);
                    checkTargetParametrs.maxHealth = enemyHero[i].currentHealth / CheckMax().currentHealth;
                    checkTargetParametrs.distance = 1 - ((transform.position - enemyHero[i].transform.position).magnitude / CheckMaxDistance());
                    //checkTargetParametrs.enemyAggro = TargetHelper.Instance.GetAggro(targetClass, enemyHero[i].unitClass);
                    checkTargetParametrs.mainResult = checkTargetParametrs.minHealth * attackList.minHealth + checkTargetParametrs.maxHealth * attackList.maxHealth + checkTargetParametrs.distance * attackList.minDistance + checkTargetParametrs.enemyAggro * attackList.classPrefer;
                    tempResult.Add(enemyHero[i], checkTargetParametrs.mainResult);
                }
            }
            var s = enemyHero[0];
            float min = 0;
            foreach (var item in tempResult)
            {
                if (item.Value > min)
                {
                    s = item.Key;
                    min = item.Value;
                }
            }
            if (s != null)
            {
                target = s.transform;
                enemyNumber = enemyHero.FindIndex(x => x == s);
            }
        }
    }

    public float CheckAggro(string EnemyName)
    {
        foreach (EnemyPrefer ep in AggroList)
        {
            if (ep.name == EnemyName)
            {
                return ep.wishKill;
            }
        }
        return 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Status == HeroStatus.Allies || Status == HeroStatus.Neutral)
        {
            if (other.tag == "Enemy")
            {
                bool find = false;
                for (int i = 0; i < enemyHero.Count; i++)
                {
                    if (other.gameObject == enemyHero[i].gameObject)
                    {
                        find = true;
                    }
                }
                if (!find)
                {
                    enemyHero.Add(other.GetComponent<UnitAI>());
                }
            }
            if (other.tag == "Allies")
            {
                bool find = false;
                for (int i = 0; i < alliesHero.Count; i++)
                {
                    if (other.gameObject == alliesHero[i].gameObject)
                    {
                        find = true;
                    }
                }
                if (!find)
                {
                    alliesHero.Add(other.GetComponent<UnitAI>());
                }
            }
        }
        else if (Status == HeroStatus.Enemy) //для мобов зомби и босс
        {
            if (other.tag == "Allies" || other.tag == "Aggro") //сюда придумать тег
            {
                bool find = false;
                for (int i = 0; i < enemyHero.Count; i++)
                {
                    if (other.gameObject == enemyHero[i].gameObject)
                    {
                        find = true;
                    }
                }
                if (!find)
                {
                    enemyHero.Add(other.GetComponent<UnitAI>());
                }
            }
            if (other.tag == "Enemy")
            {
                bool find = false;
                for (int i = 0; i < alliesHero.Count; i++)
                {
                    if (other.gameObject == alliesHero[i].gameObject)
                    {
                        find = true;
                    }
                }
                if (!find)
                {
                    alliesHero.Add(other.GetComponent<UnitAI>());
                }
            }
        }
        else if (Status == HeroStatus.Aggro)//new Охотник
        {
            if (other.tag == "Enemy" || other.tag == "Allies" || other.tag == "Aggro")
            {
                bool find = false;
                for (int i = 0; i < enemyHero.Count; i++)
                {
                    if (other.gameObject == enemyHero[i].gameObject)
                    {
                        find = true;
                    }
                }
                if (!find)
                {
                    enemyHero.Add(other.GetComponent<UnitAI>());
                }
            }
        }
    }
}