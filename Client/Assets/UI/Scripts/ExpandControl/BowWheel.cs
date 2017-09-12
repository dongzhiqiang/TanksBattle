using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class BowWheel : MonoBehaviour
{
    /** 自身半径 */
    public float m_radius=1;
    /** 弧度偏移 */
    public float m_shiftDegree;
    /** 是否与传入角度同向 */
    public bool m_sameDirection = true;

    public void SetRouteDistance(float distance)
    {
        float radian = distance / m_radius;
        if(!m_sameDirection)
        {
            radian = -radian;
        }
        radian = Mathf.Repeat(radian, Mathf.PI);
        float degree = radian * Mathf.Rad2Deg + m_shiftDegree;
        gameObject.transform.eulerAngles = new Vector3(0, 0, degree);
    }
}