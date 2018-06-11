using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetHelper : MonoBehaviour {

    #region SingleTon
    private static TargetHelper _instance;
    public static TargetHelper Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TargetHelper>();
            }
            return _instance;
        }
    }
    #endregion

    public int CheckCloseReturnNumber(List<Transform> obj)
    {
        int result = 0;
        if (obj[0] != null)
        {
            float min = (transform.position - obj[0].position).magnitude;
            float distance;
            
            for (int i = 1; i < obj.Count; i++)
            {
                if (obj[i] != null)
                {
                    distance = (transform.position - obj[i].position).magnitude;
                    if (distance < min)
                    {
                        min = distance;
                        result = i;
                    }
                }
            }
        }
        return result;
    }

    public float CheckCloseReturnDistance(List<Transform> obj)
    {
        int result = 0;
        float distance, min =0;
        if (obj[0] != null)
        {
           min = (transform.position - obj[0].position).magnitude;
            for (int i = 1; i < obj.Count; i++)
            {
                if (obj[i] != null)
                {
                    distance = (transform.position - obj[i].position).magnitude;
                    if (distance < min)
                    {
                        min = distance;
                        result = i;
                    }
                }
            }
        }
        return min;
    }

    public int CheckMaxReturnNumber(List<Transform> obj)
    {
        int result = 0;
        if (obj[0] != null)
        {
            float max = (transform.position - obj[0].position).magnitude;
            float distance;

            for (int i = 1; i < obj.Count; i++)
            {
                if (obj[i] != null)
                {
                    distance = (transform.position - obj[i].position).magnitude;
                    if (distance > max)
                    {
                        max = distance;
                        result = i;
                    }
                }
            }
        }
        return result;
    }

    public float CheckMaxReturnDistance(List<Transform> obj)
    {
        int result = 0;
        float distance, max = 0;
        if (obj[0] != null)
        {
            max = (transform.position - obj[0].position).magnitude;
            for (int i = 1; i < obj.Count; i++)
            {
                if (obj[i] != null)
                {
                    distance = (transform.position - obj[i].position).magnitude;
                    if (distance > max)
                    {
                        max = distance;
                        result = i;
                    }
                }
            }
        }
        return max;
    }

    public Hero CheckMaxHealth(List<Hero> possibleTargets)
    {
        if (possibleTargets[0] != null)
        {
            float max = possibleTargets[0].currentHealth;
            Hero target = possibleTargets[0];
            for (int i = 1; i < possibleTargets.Count; i++)
            {
                if (possibleTargets[i].currentHealth > max)
                {
                    max = possibleTargets[i].currentHealth;
                    target = possibleTargets[i];
                }
            }
            return target;
        }
        return null;
    }

    public Hero CheckMinHealth(List<Hero> possibleTargets)
    {
        if (possibleTargets[0] != null)
        {
            float min = possibleTargets[0].currentHealth;
            Hero target = possibleTargets[0];
            for (int i = 1; i < possibleTargets.Count; i++)
            {
                if (possibleTargets[i].currentHealth < min)
                {
                    min = possibleTargets[i].currentHealth;
                    target = possibleTargets[i];
                }
            }

            return target;
        }
        return null;
    }

    /*public float GetAggro(List<TargetPrefer> targetClass, HeroClass target)
    {
        for(int i=0; i < targetClass.Count; i++)
        {
            if (targetClass[i].heroClass == target)
            {
                return targetClass[i].wishKill;
            }
        }
        return 0;
    }

    public float GetAggro(List<TargetPrefer> targetClass, string target)
    {
        for (int i = 0; i < targetClass.Count; i++)
        {
            if (targetClass[i].unitClass == target)
            {
                return targetClass[i].wishKill;
            }
        }
        return 0;
    }
    */
}
