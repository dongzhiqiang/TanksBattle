using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


public class UIFlameAttributeItem : MonoBehaviour
{
    public Text m_attribute;
    public Text m_value;
    public ImageEx m_bar;

    public void Init(string name, float value, float maxValue)
    {
        m_attribute.text = name;
        m_value.text = Mathf.Floor(value) + "/" + Mathf.Floor(maxValue);
        float rate = value/maxValue;
        if(rate>1)rate = 1;
        m_bar.fillAmount = rate;
    }
}
