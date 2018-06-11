using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect : MonoBehaviour {
    public Hero source;
    public Skill skillSource;
    public float startValue;
    public Hero hero;
    public GameObject prefab;

    public virtual void DoEffect()
    {
        if(prefab != null)
        {
            prefab = Instantiate(prefab, transform.position + Vector3.up * 0.5f, Quaternion.identity, transform);
        }
      
        hero = GetComponent<Hero>();
    }

   public virtual void OnDestroy()
    {
        Destroy(prefab);        
    }
}
