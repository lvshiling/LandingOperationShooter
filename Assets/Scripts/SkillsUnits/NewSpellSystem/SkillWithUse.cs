using System.Collections;
using UnityEngine;

public class SkillWithUse : Skill
{
    public override void CheckSkill()
    {
        if (timer >= 0)
        {
            if (hero.currentMana >= manacost)
            {
                if (hero.agent.remainingDistance >= 2 && hero.agent.remainingDistance <= castRange)
                {
                    if (SpecificCheckSkill())
                    {
                        isReady = true;
                    }
                }
            }
        }
    }

    public virtual bool SpecificCheckSkill()
    {
        return true;
    }

    public override void UseSkill()
    {
        isReady = false;
        timer -= cdTime;
        hero.currentMana -= manacost;
        hero.ChangeState(HeroStage.UseSckill);
        StartCoroutine(CastSkill());
    }

    public virtual IEnumerator SkillDuration()
    {
        yield return new WaitForSeconds(durationTime);
    }

    public virtual IEnumerator CastSkill()
    {
        hero.agent.speed = 0;
        yield return new WaitForSeconds(castTime);
        hero.agent.speed = hero.normalSpeed;
        hero.stageCurrent = HeroStage.Free;
    }
}