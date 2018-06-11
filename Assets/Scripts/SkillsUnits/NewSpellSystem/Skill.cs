using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class Skill : MonoBehaviour {
    public string skillName;
    public int manacost;
    public float castTime;
    public float cdTime;
    public float durationTime;
    public float castRange;
    public float skillPower;//использовать по умолчанию
    [HideInInspector]
    public float timer;
    [Range(0,100)]
    public float usePriority;
    public bool isReady;
    public Hero hero;
    public Sprite skillImage;


    abstract public  void CheckSkill();
    abstract public void UseSkill();
}
