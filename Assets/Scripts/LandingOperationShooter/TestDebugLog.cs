using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDebugLog : MonoBehaviour {

    private static TestDebugLog _instance;
    public static TestDebugLog Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TestDebugLog>();

                if(_instance == null)
                {
                    GameObject testDebugLog = new GameObject();

                    _instance = testDebugLog.AddComponent<TestDebugLog>();
                }
            }
            return _instance;
        }
    }

    public void DebugLog<T>(T masage)
    {
#if !FINAL_VERSION
        Debug.Log(masage);
#endif
    }
}
