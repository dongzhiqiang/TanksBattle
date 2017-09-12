using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


public class UIPowerUpAttributeItem : MonoBehaviour
{
    public TextEx m_attrName;
    public TextEx m_attrValue;
    public TextEx m_plus;
    public GameObject m_aniExit;
    public GameObject m_aniEnter;

    public void Init(string attrName, string attrValue, bool plus)
    {
        m_attrName.text = attrName;
        m_attrValue.text = attrValue;
        m_aniExit.SetActive(false);
        m_aniEnter.SetActive(false);
        m_aniEnter.SetActive(true);
        if (plus)
        {
            m_plus.text = "+";
        }
        else
        {
            m_plus.text = "-";
        }
    }

    public void PlayExit()
    {
        m_aniEnter.SetActive(false);
        m_aniExit.SetActive(true);
    }
}
