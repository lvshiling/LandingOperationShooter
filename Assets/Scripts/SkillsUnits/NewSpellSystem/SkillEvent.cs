using UnityEngine;

public class SkillEvent : Skill
{
    public virtual void Start()
    {
        hero.heroDamage += OnDamageHasTaken;
        hero.heroAttack += OnDamageDealt;
    }

    public override void CheckSkill()
    {
    }

    public virtual void OnDamageDealt(float damageF, float damageM, Transform source)
    {
        //Сотвори чудо
    }

    public virtual void OnDamageHasTaken(float damageF, float damageM)
    {
        //Сотвори чудо
    }

    public override void UseSkill()
    {
    }

    private void OnDestroy()
    {
        hero.heroDamage -= OnDamageHasTaken;
        hero.heroAttack -= OnDamageDealt;
    }
}