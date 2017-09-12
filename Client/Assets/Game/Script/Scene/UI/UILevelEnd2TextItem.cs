using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UILevelEnd2TextItem : MonoBehaviour
{
    public TextEx m_lblTitle;
    public TextEx m_lblValue;
    public TextEx m_lblOneTxt;

    public void Init(string title, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            m_lblOneTxt.gameObject.SetActive(true);
            m_lblTitle.gameObject.SetActive(false);
            m_lblValue.gameObject.SetActive(false);
            m_lblOneTxt.text = title;
            m_lblTitle.text = "";
            m_lblValue.text = "";
        }
        else
        {
            m_lblOneTxt.gameObject.SetActive(false);
            m_lblTitle.gameObject.SetActive(true);
            m_lblValue.gameObject.SetActive(true);
            m_lblOneTxt.text = "";
            m_lblTitle.text = title;
            m_lblValue.text = value;
        }        
    }
}
