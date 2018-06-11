using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SimpleBar : MonoBehaviour
{
    public int level
    {
        set
        {
            UpdateBar(value);            
        }
    }

    public int maxLevel
    {
        set
        {
            max = value;
        }
    }

    [SerializeField]
    Sprite segmentOn;
    [SerializeField]
    Sprite segmentOff;
    [SerializeField]
    Sprite segmentBlocked;
    
  public  int max = 10;
            
    void UpdateBar(int level)
    {
        int i = 0;
        for (; i < transform.childCount && i < max; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(true);
            transform.GetChild(i).GetComponent<Image>().sprite = (i < level) ? segmentOn : segmentOff;
        }
        for (; i < transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(false);
           // transform.GetChild(i).GetComponent<Image>().sprite = segmentBlocked; если необходимо показать заблокированные элементы - раскоментить
        }
    }
}
