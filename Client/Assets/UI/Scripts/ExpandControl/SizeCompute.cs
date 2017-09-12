using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class SizeComputeItemHeight
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
                value = m_object.transform.localPosition.y;
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

// 暂时只提供高度的自动计算
public class SizeCompute : MonoBehaviour
{
    public List<SizeComputeItemHeight> m_height;

    void Update()
    {
        float height = 0;
        for(int i=0; i<m_height.Count; i++)
        {
            height += m_height[i].GetValue();
        }
        var trans = GetComponent<RectTransform>();
        if (trans.sizeDelta.y != height)
            trans.sizeDelta = new Vector2(trans.sizeDelta.x, height);
    }
}