using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowCheck : MonoBehaviour {

    private bool _isActive;

    public bool IsActive
    {
        set
        {
            _isActive = value;
        }

        get
        {
            return _isActive;
        }
    }

    private void OnEnable()
    {
        IsActive = true;
    }

    private void OnDisable()
    {
        IsActive = false;
    }
}
