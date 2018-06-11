using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Effect
{
    SkillEffect,
    SkillEffect2
}

public class SkillAura : Skill { 
    public float auraRadius;
    public float auraPower;
    public GameObject auraPrefab;
    public HeroStatus status;
    public Effect effect;
    public List<UnitAI> auraTargets;


    public override void CheckSkill()
    {
        if (status != HeroStatus.Neutral)
        {
            auraTargets = status == HeroStatus.Allies ? hero.alliesHero : hero.enemyHero;
        }
        else
        {
            auraTargets = new List<UnitAI>();
            auraTargets.AddRange(hero.alliesHero);
            auraTargets.AddRange(hero.enemyHero);
        }

        foreach (Hero target in auraTargets)
        {
            if (target != null)
            {
                SkillEffect skillEffect = GetClass(false, target);

                if ((target.transform.position - transform.position).magnitude < auraRadius)
                {
                    if (skillEffect == null)
                    {
                        skillEffect = GetClass(true, target);
                        skillEffect.source = hero;
                        skillEffect.skillSource = this;
                        skillEffect.prefab = auraPrefab;
                        skillEffect.DoEffect();
                    }
                }
                else
                {
                    if (skillEffect != null)
                    {
                        if (skillEffect.source == hero)
                        {
                            Destroy(skillEffect);
                        }
                    }
                }
            }
        }
    }

    public virtual SkillEffect GetClass(bool isAdd, Hero target)
    {
     SkillEffect obj;
        if (!isAdd)
        {
        obj =target.gameObject.GetComponent<SkillEffect>();
        }
        else
        {
            obj = target.gameObject.AddComponent<SkillEffect>();
        }
      
        return obj;
    }

    public virtual void OnDestroy()
    {
        try
        {
            foreach (Hero target in auraTargets)
            {
                SkillEffect ha = target.gameObject.GetComponent<SkillEffect>();
                if (ha != null && ha.source == hero)
                {
                    Destroy(ha);
                }
            }
        }
        catch { }
    }

    public override void UseSkill()
    {
        throw new NotImplementedException();
    }
}
