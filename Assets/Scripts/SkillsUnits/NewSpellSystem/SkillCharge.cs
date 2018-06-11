using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCharge : SkillWithUse {

    bool isActive;

    public override bool SpecificCheckSkill()
    {
        if(hero.target != null)
        {
            return true;
        }
        return false;
    }

    private void Start()
    {

    }

    public override void UseSkill()
    {
        base.UseSkill();
        hero.agent.speed = hero.normalSpeed * 4.0f;
        hero.anim.Play("Run");
        hero.agent.SetDestination(hero.target.position);
        isActive = true;
    }

    public override IEnumerator CastSkill()
    {
        hero.agent.speed = 0;
        yield return new WaitForSeconds(castTime);
        hero.agent.speed = hero.normalSpeed * 4.0f;
    }

    private void Update()
    {
        if (isActive)
        {
            if (hero.target != null && hero.agent.isOnNavMesh && hero.agent.isActiveAndEnabled)
            {
                if (hero.enemyHero[hero.enemyNumber] != null)  //осторожнее с этим
                {
                    if (Vector3.Distance(hero.transform.position, hero.agent.destination) < hero.attackRadius + hero.enemyHero[hero.enemyNumber].sizeObject + hero.sizeObject)
                    {
                        if (Vector3.Distance(hero.transform.position, hero.target.position) < hero.attackRadius + hero.enemyHero[hero.enemyNumber].sizeObject + hero.sizeObject)
                        {
                            hero.ChangeState(HeroStage.Free);
                            isActive = false;
                            hero.agent.speed = 0;
                            hero.attackF += 10;
                            hero.Attack();
                            hero.attackF -= 10;
                        }
                        else
                        {

                            hero.ChangeState(HeroStage.Free);
                            isActive = false;
                            hero.agent.speed = hero.normalSpeed;
                        }
                    }
                }
            }
        }
    }
}
