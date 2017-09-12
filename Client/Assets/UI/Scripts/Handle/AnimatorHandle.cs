using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;


public class AnimatorHandle : MonoBehaviour
{
    public string m_curAni;
    public bool m_playOnEnable;
    public Animator m_ani;


    void Reset()
    {
        if (m_ani == null)
            m_ani = this.GetComponent<Animator>();

    }

    void OnEnable()
    {
        if (m_playOnEnable)
            Play();
    }

    public void Play()
    {
        if (m_ani == null || string.IsNullOrEmpty(m_curAni))
            return;

        //让上一个动画直接跳到最后一帧
        var info = m_ani.GetCurrentAnimatorStateInfo(0);
        m_ani.Play(info.shortNameHash, 0, 1.0f);
        m_ani.Update(0);

        m_ani.enabled = false;
        m_ani.Play(m_curAni, 0, 0);
        m_ani.enabled = true;
        m_ani.Update(0);
    }

}
