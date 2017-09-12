using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


//简单操作控件
public class SimpleHandle : MonoBehaviour {
    //[System.Serializable]
    //public class TriggerEvent : UnityEvent { }

    public bool m_playOnEnable=true;
    public bool m_resetOnEnable=true;
    public Handle m_handle=new Handle();

    public UnityEvent m_onEnd;

    bool m_cached = false;

    public bool IsPlaying { get{return m_handle.IsPlaying;}}

    public float CurTime { get { return m_handle.IsPlaying==false?0:m_handle.CurTime; } }

    void Cache()
    {
        if (m_cached)
            return;
        m_cached = true;
        m_handle.m_onEnd = OnEnd;
    }


    void Start()
    {
        Cache();
    }

    void OnEnable()
    {
        Cache();
        if (m_playOnEnable && m_resetOnEnable)
        {
            ResetPlay();
        }
        else if (!m_playOnEnable && m_resetOnEnable)
        {
            ResetStop();
        }
        else if (!m_playOnEnable && !m_resetOnEnable)
        {
            m_handle.Clear();//暂停就好
        }
        else if (m_playOnEnable && !m_resetOnEnable)
        {
            if(!m_handle.IsPlaying)
                m_handle.Start();//没有运行就运行
        }
    }

    public void LateUpdate()
    {
        m_handle.Update();
        EditorUtil.SetDirty(this);
    }
    //开始
    public void ResetPlay()
    {
        m_handle.Start();
    }

    //重置到第一帧且暂停
    public void ResetStop()
    {
        m_handle.Reset(true);
    }

    //直接跳到结束
    public void Stop()
    {
        m_handle.End();
    }

    public void OnEnd()
    {
        if (m_onEnd != null)
            m_onEnd.Invoke();
    }
}
