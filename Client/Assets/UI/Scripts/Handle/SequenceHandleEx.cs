using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

//用来记录序列的类，由于序列化类引用自己会导致死锁，所以需要这么个外部类
//注意同个时间段的子处理合并的时候，要继承即时属性(isDurationInvalid)
public class SequenceHandleEx : SequenceHandle, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler   
{
    //static Vector2 Invalid_Pos = new Vector2(10000,10000);
    static float Invalid_Float = 10000;

    public Handle m_handle=new Handle();
    public float m_dir = 0;//正向是哪个方向
    public float m_dirSize=100;
    public bool m_loop=true;
    public bool m_endScroll =true;
    public float m_endScrollDuration = 0.5f;
    public bool m_clickToEnd= true;

    public Action<int> m_onDrag;
    public Action<int> m_onDragEnd;
    public Func<bool> m_onCanDrag;

    
    float m_lastFactor=0;
    float m_targetFactor = Invalid_Float;
    float m_beginEndScrollTime;
    bool m_canBeginDrag=false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!eventData.dragging && m_clickToEnd && m_targetFactor == Invalid_Float && (m_onCanDrag == null || m_onCanDrag()==true))
        {
            m_lastFactor = 0;
            m_targetFactor=1;
            m_beginEndScrollTime = Time.time;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        m_targetFactor = Invalid_Float;
        m_lastFactor = 0;
        if (m_onCanDrag!= null)
            m_canBeginDrag = m_onCanDrag();
        else 
            m_canBeginDrag =true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.delta;
        Vector2 dir = Quaternion.Euler(0, 0, m_dir) * Vector3.right;
        Vector2 p = Util.Project(delta, dir);
        //Debuger.LogError("方向：{0}", Quaternion.Euler(0, 0, m_dir) * Vector3.right);
        //Debug.DrawRay(transform.position,Quaternion.Euler(0, 0, m_dir) * Vector3.right,Color.red,5);
        float add = p.magnitude / m_dirSize;
        if (Vector2.Dot(dir, p)<0)//加还是减
            add =-add;

        int iLast = Mathf.FloorToInt(m_lastFactor);
        m_lastFactor+=add;
        if(!m_loop)
            m_lastFactor = Mathf.Clamp01(m_lastFactor);
        int iCur = Mathf.FloorToInt(m_lastFactor);
        if (iCur != iLast && m_loop)
        {
            if(m_onDrag != null)
                m_onDrag(iCur);
            //Debuger.Log("滚到;{0}", iCur);
        }
            

        m_handle.SetFactor(m_lastFactor - iCur, true);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnDrag(eventData);

        if (!m_endScroll)
        {
            m_onDragEnd(Mathf.RoundToInt(m_lastFactor));
            return;
        }
            
        m_targetFactor = Mathf.Round(m_lastFactor);
        m_beginEndScrollTime = Time.time;
    }

    void Update()
    {
        if(m_targetFactor== Invalid_Float)
            return;
        float duration = Mathf.Abs(m_targetFactor-m_lastFactor)*m_endScrollDuration;
        float lerpFactor = Mathf.Clamp01( duration==0?1:(Time.time - m_beginEndScrollTime)/duration);
        float factor = Mathf.Lerp(m_lastFactor, m_targetFactor, lerpFactor);

        if (lerpFactor == 1)
        {
            m_handle.SetFactor(0, true);
            if (m_onDragEnd!= null)
                m_onDragEnd((int)m_targetFactor);
            //Debuger.Log("回滚结束:{0}", (int)m_targetFactor);
            m_targetFactor = Invalid_Float;
            return;
        }

        m_handle.SetFactor(factor - Mathf.FloorToInt(factor), true);
    }

    public void SetFactor(float factor)
    {
        m_handle.SetFactor(0, true);
    }
}
