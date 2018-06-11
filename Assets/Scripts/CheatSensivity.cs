using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheatSensivity : MonoBehaviour {

    public Text valueText;
    public Slider slider;


    public void OnValueChanged()
    {
        GamePlayController.Instance.OnSensivityChange(slider.value);
        valueText.text = slider.value.ToString();
    }
}
