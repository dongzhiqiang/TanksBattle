using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;




public class RepeatPressListener : MonoBehaviour, 
        IPointerDownHandler,
        IPointerUpHandler, 
        IPointerExitHandler
       
{
    public float m_interval = 0.5f;
    public Action<bool> m_onRepeatPress; // 参数：是否按下后的首次调用
    public Action m_onPressEnd;

    private bool m_isPointDown;
    private float m_lastInvokeTime;

    void Update()
    {
        if(m_isPointDown)
        {
            float time = Time.time;
            if (time - m_lastInvokeTime > m_interval)
            {
                m_onRepeatPress.Invoke(false);
                m_lastInvokeTime = time;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        m_isPointDown = true;
        m_onRepeatPress.Invoke(true);
        m_lastInvokeTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (m_isPointDown && m_onPressEnd!=null)
        {
            m_onPressEnd.Invoke();
        }
        m_isPointDown = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (m_isPointDown && m_onPressEnd != null)
        {
            m_onPressEnd.Invoke();
        }
        m_isPointDown = false;
    }


    public void OnDisable()
    {
        if (m_isPointDown && m_onPressEnd != null)
        {
            m_onPressEnd.Invoke();
        }
        m_isPointDown = false;
    }
}
