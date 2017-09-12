using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class PosComputeItemY
{
    public bool m_negative;
    public float m_value;
    public bool m_useGameObj;
    public GameObject m_object;
    public bool m_useHeightElseY;

    public float GetValue()
    {
        float value;
        if(m_useGameObj)
        {
            if (!m_object.activeSelf)
            {
                return 0f;
            }
            if(m_useHeightElseY)
            {
                value = m_object.GetComponent<RectTransform>().sizeDelta.y;
            }
            else
            {
                value = m_object.GetComponent<RectTransform>().anchoredPosition.y;
            }
        }
        else
        {
            value = m_value;
        }
        if(m_negative)
        {
            value = -value;
        }
        return value;
    }
}

// 暂时只提供Y的自动计算
public class PosCompute : MonoBehaviour
{
    public List<PosComputeItemY> m_ys;

    void Update()
    {
        float y = 0;
        for (int i = 0; i < m_ys.Count; i++)
        {
            y += m_ys[i].GetValue();
        }
        float x = GetComponent<RectTransform>().anchoredPosition.x;
        GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y);
    }
}