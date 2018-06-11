using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Mission  {
    public List<Transform> checkPoints;
    public Transform exitPoint;
    public List<bool> isCheck;
    public int currentCheckCount;

    public bool isCheckPass()
    {
        bool pass = true;
        if (isCheck.Count == checkPoints.Count)
        {
            for (int i = 0; i < checkPoints.Count; i++)
            {
                pass &= isCheck[i];
            }
            return pass;
        }
        return false;
    }
}
