using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingAnim : MonoBehaviour
{

    public float delta = 0.5f;
    public string[] texts;

    float nextTime;
    int i;
    Text txt;

    private void Awake()
    {
        txt = GetComponent<Text>();
        txt.text = texts[i];
        nextTime = Time.unscaledTime + delta;
    }

    void Update()
    {
        if (nextTime < Time.unscaledTime)
        {
            i = ++i % texts.Length;            
            txt.text = texts[i];
            nextTime = Time.unscaledTime + delta;
        }
    }
}
