using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class BowChain : MonoBehaviour
{
    /** 是否和传入半径使用同一半径 */
    public bool m_outRadiusUsed;
    /** 自身半径，如果使用传入半径可以不设 */
    public float m_radius;
    /** 每段锁链角度 */
    public float m_degreePerNode;
    /** 超出屏幕外锁链离中心锁链角度 */
    public float m_outDegree;
    /** 角度偏移(顺时针) */
    public float m_shiftDegree;
    /** 元件池 */
    public List<GameObject> m_pool;
    /** 元件模板 */
    public GameObject m_template;
    /** 是否与传入角度同向 */
    public bool m_sameDirection = true;
    private float m_outRadius;
    private float m_radianPerNode;
    private float m_outRadian;
    private float m_shiftRadian;
    private Vector2 m_center;
    private bool m_cache =false;

    void Awake()
    {
        Cache();
    }

    void Cache()
    {
        if (m_cache) return;
        m_cache = true;
       m_radianPerNode = Mathf.Deg2Rad * m_degreePerNode;
        m_outRadian = Mathf.Deg2Rad * m_outDegree;
        m_shiftRadian = Mathf.Deg2Rad * m_shiftDegree;
        if (m_radianPerNode <= 0)
        {
            m_radianPerNode = 1;
        }
        foreach (GameObject node in m_pool)
        {
            node.SetActive(false);
        }
    }

    //由bowlistview调用，传初始关联数据(半径)
    public void InitChain(float outRadius)
    {
        Cache();
        m_radianPerNode = Mathf.Deg2Rad * m_degreePerNode;
        m_outRadian = Mathf.Deg2Rad * m_outDegree;
        m_shiftRadian = Mathf.Deg2Rad * m_shiftDegree;
        if (m_radianPerNode <= 0)
        {
            m_radianPerNode = 1;
        }
        foreach (GameObject node in m_pool)
        {
            node.SetActive(false);
        }
        if (m_template == null)
        {
            m_template = m_pool[0];
        }
        else
        {
            m_template.SetActive(false);
        }

        m_outRadius = outRadius;
        if(m_outRadiusUsed)
        {
            m_radius = outRadius;
        }
        m_center = new Vector2();
        m_center.x = -m_radius * Mathf.Cos(m_shiftRadian);
        m_center.y = m_radius * Mathf.Sin(m_shiftRadian);
    }

    GameObject CreateNode()
    {
        GameObject result;
        result = GameObject.Instantiate(m_template);
        result.transform.parent = this.transform;
        result.transform.localScale = Vector3.one;
        result.gameObject.SetActive(true);
        return result;
    }

    // 获取锁链下方末端的弧度
    public float GetEndRadian(float radian)
    {
        Cache();
        if (!m_outRadiusUsed)
        {
            return 0;
        }
        float baseRadian = radian - Mathf.Floor(radian / m_radianPerNode) * m_radianPerNode;
        float nodeRadian = -baseRadian;
        while (nodeRadian < m_outRadian)
        {
            nodeRadian = nodeRadian + m_radianPerNode;
        }
        return nodeRadian + m_radianPerNode / 2 + radian;
    }

    //由bowlistview调用，传关联数据(弧度
    public void UpdateRadian(float radian, float maxNodeRaidan, bool leaveScreen)
    {
        Cache();
        //Debug.LogError("center:"+m_center.x + "," + m_center.y);
        // Debug.LogError("radius:" + m_radius);
        float baseRadian;
        if(m_outRadiusUsed)
        {
            baseRadian = radian;
        }
        else
        {
            baseRadian = radian * m_outRadius / m_radius;
        }
        if(!m_sameDirection)
        {
            baseRadian = -baseRadian;
        }
        //Debug.LogError("s-radian:" + baseRadian);
       // Debug.LogError("radian-p:" + m_radianPerNode);
        baseRadian = baseRadian - Mathf.Floor(baseRadian / m_radianPerNode) * m_radianPerNode;
        //Debug.LogError("r-radian:" + baseRadian);
        while( -baseRadian - m_radianPerNode > -m_outRadian)
        {
            baseRadian = baseRadian + m_radianPerNode;
        }
        bool chainEnd = false;
        for(int i=0; i<10; i++)
        {
            if(m_pool.Count<=i)
            {
                if (chainEnd) break;
                m_pool.Add(CreateNode());
            }
            GameObject node = m_pool[i];
            float localRadian = i * m_radianPerNode - baseRadian;
            float farRadian = localRadian + radian;
            if(localRadian >= m_outRadian)
            {
                node.SetActive(false);
                chainEnd = true;
                continue;
            }
            if (m_outRadiusUsed && leaveScreen)
            {
                if (farRadian > maxNodeRaidan)
                {
                    node.SetActive(false);
                    chainEnd = true;
                    continue;
                }
            }
            node.SetActive(true);

            //Debug.LogError("localRadian:" + localRadian);
            //Debug.LogError("shifted rad:" + localRadian + m_shiftRadian);
            //Debug.LogError("dxy:" + m_radius * Mathf.Cos(localRadian + m_shiftRadian) + "," + (-m_radius * Mathf.Sin(localRadian + m_shiftRadian))); 
            node.transform.localEulerAngles = new Vector3(0, 0, -localRadian*Mathf.Rad2Deg);
            node.transform.localPosition = new Vector3(m_center.x + m_radius * Mathf.Cos(localRadian + m_shiftRadian), m_center.y - m_radius * Mathf.Sin(localRadian + m_shiftRadian));
        }
    }
}