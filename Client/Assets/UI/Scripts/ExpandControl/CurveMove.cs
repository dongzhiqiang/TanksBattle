
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


public class CurveMove : MonoBehaviour
{

    public float m_duration=1f;

    public bool m_playOnEnable = true;
    public bool m_resetOnEnable = true;
    public Vector3 m_startPos;
    public Vector3 m_endPos;
    public float m_height;

    bool m_isPlaying = false;
    float m_startTime;


    void Start()
    {
        if(m_duration<=0)
        {
            return;
        }
        m_startTime = Time.time;
        m_isPlaying = true;
        Reset();
    }

    void Reset()
    {
        gameObject.transform.localPosition = m_startPos;
    }

    void OnEnable()
    {
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
            m_isPlaying = false;
        }
        else if (m_playOnEnable && !m_resetOnEnable)
        {
            if (!m_isPlaying)
                Start();

        }
    }

    void Update()
    {
        if(m_isPlaying)
        {
            float factor = (Time.time - m_startTime) / m_duration;
            if(factor>=1f)
            {
                Stop();
                return;
            }
            // height
            float curHeight = m_height*(1 - 4 * (factor - 0.5f) * (factor - 0.5f));
            float curX = Mathf.Lerp(m_startPos.x, m_endPos.x, factor);
            float curY = Mathf.Lerp(m_startPos.y, m_endPos.y, factor)+curHeight;
            float curZ = Mathf.Lerp(m_startPos.z, m_endPos.z, factor);
            gameObject.transform.localPosition = new Vector3(curX, curY, curZ);
        }
    }

    //开始
    public void ResetPlay()
    {
        Start();
    }

    //重置到第一帧且暂停
    public void ResetStop()
    {
        Reset();
    }

    //直接跳到结束
    public void Stop()
    {
        gameObject.transform.localPosition = m_endPos;
        m_isPlaying = false;
    }

    
}
