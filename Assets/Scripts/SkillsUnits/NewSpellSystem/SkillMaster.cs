using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Hero))]
public class SkillMaster : MonoBehaviour
{
    public Hero hero;
    public List<Skill> skillList;
    public float[] cullDownTimers;
    public Skill selectedSkill;
    private float timer;

    private void Start()
    {
        hero = GetComponent<Hero>();
        hero.playerDeath += DestoyAllSkills;
        skillList.AddRange(transform.GetComponentsInChildren<Skill>());
        for (int i = 0; i < skillList.Count; i++)
        {
            skillList[i].hero = hero;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        CullDownUpdate();

        SelectSkill();
    }

    private void DestoyAllSkills()
    {
        foreach (Skill skill in skillList)
        {
            Destroy(skill);
        }
        Destroy(this);
    }

    public void CullDownUpdate()
    {
        for (int i = 0; i < skillList.Count; i++)
        {
            if (skillList[i].timer <= 0)
            {
                skillList[i].timer += Time.deltaTime;
            }
        }
    }

    public void SelectSkill()
    {
        List<Skill> readyList = new List<Skill>();
        for (int i = 0; i < skillList.Count; i++)
        {
            skillList[i].CheckSkill();
            if (skillList[i].isReady)
            {
                readyList.Add(skillList[i]);
            }
        }
        if (readyList.Count > 0)
        {
            int min = 0;
            int select = 0;
            for (int i = 0; i < readyList.Count; i++)
            {
                if (readyList[i].usePriority > min)
                {
                    select = i;
                }
            }
            selectedSkill = readyList[select];
        }
        else
        {
            selectedSkill = null;
        }
        if (selectedSkill is SkillWithUse)
        {
            hero.stageCurrent = HeroStage.UseSckill;
            selectedSkill.UseSkill();
        }
    }
}