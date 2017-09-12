using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


public class UIEquipAttributeItem : MonoBehaviour
{
    public Text m_attribute;
    public Text m_value;
    public Text m_addValue;

    public void Init(string name, string value, string addvalue)
    {
        m_attribute.text = name;
        m_value.text = value;
        if(addvalue != null)
        {
            m_addValue.gameObject.SetActive(true);
            m_addValue.text = addvalue;
        }
        else
        {
            m_addValue.gameObject.SetActive(false);
        }
    }
}
