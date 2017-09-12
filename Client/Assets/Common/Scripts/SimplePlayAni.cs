using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Animations;
//AnimatorController ctrl = ani.runtimeAnimatorController as AnimatorController;
//AnimatorControllerLayer layer = ctrl.GetLayer(0);
//int scount = layer.stateMachine.stateCount;
//State st = layer.stateMachine.GetState(i);
//st.name

public class SimplePlayAni : MonoBehaviour
{
    public enum enTrigger
    {//触发类型
        always,//没有播放其他动作的时候播放
        start,//开始时播放
        interval,//间隔时间
    }
    [System.Serializable]
    public class AniInfo
    {
        public string m_aniName;//动作名
        public WrapMode wrapMode;//播放模式
        public enTrigger trigger = enTrigger.always;
        public float intervalMin = 0;
        public float intervalMax = 0;

        [System.NonSerialized]
        public float curInterval = 0;

        [System.NonSerialized]
        public float m_lastPlayIntervalTime = 0;

        public bool IsCD
        {
            get
            {
                return m_lastPlayIntervalTime != 0 && Time.time <= m_lastPlayIntervalTime + curInterval;
            }

        }

    }

    public Animator m_animator;
    public Animation m_animation;
    public AniInfo[] m_aniInfos;

    AniInfo m_curInfo;
    float m_curLastPlayTime = 0;

    float CurDuration
    {
        get
        {
            if (m_curInfo == null)
            {
                Debuger.LogError("不能在还没有当前动作的情况下获取动作时间");
                return 0;
            }

            if (m_animator != null)
            {
                return m_animator.GetCurrentAnimatorStateInfo(0).length;
            }

            if (m_animation != null)
            {
                return m_animation[m_curInfo.m_aniName].length;
            }
            return 0;
        }
    }

    bool IsEnd
    {
        get
        {
            if (m_curInfo == null)
                return true;

            if (m_curInfo.wrapMode == WrapMode.Loop || m_curInfo.wrapMode == WrapMode.PingPong)
                return false;

            return Time.time >= m_curLastPlayTime + CurDuration;
        }
    }



    // Use this for initialization
    void Start()
    {
        //找到start的播放
        List<AniInfo> l = GetInfoByTriggerType(enTrigger.start);
        if (l.Count != 0)
        {
            Play(l[0]);
            return;
        }

        //没有的话找到always的播放
        l = GetInfoByTriggerType(enTrigger.always);
        if (l.Count != 0)
        {
            Play(l[0]);
            return;
        }
    }

    void OnEnable()
    {
        //找到start的播放
        List<AniInfo> l = GetInfoByTriggerType(enTrigger.start);
        if (l.Count != 0)
        {
            Play(l[0]);
            return;
        }

        //没有的话找到always的播放
        l = GetInfoByTriggerType(enTrigger.always);
        if (l.Count != 0)
        {
            Play(l[0]);
            return;
        }
    }


    // Update is called once per frame
    void Update()
    {
        //如果间隔时间播放的有那么播放
        if (m_curInfo.trigger == enTrigger.always)
        {
            if (m_aniInfos != null)
            {
                foreach (var info in m_aniInfos)
                {
                    if (info.trigger == enTrigger.interval && !info.IsCD)
                    {
                        Play(info);
                        return;
                    }
                }
            }
                

        }
        //如果当前的已经结束，那么找到下一个播放
        if (IsEnd)
        {
            if (m_aniInfos!=null)
            {
                foreach (var info in m_aniInfos)
                {
                    if (info.trigger == enTrigger.interval && !info.IsCD)
                    {
                        Play(info);
                        return;
                    }
                }

                foreach (var info in m_aniInfos)
                {
                    if (info.trigger == enTrigger.always)
                    {
                        Play(info);
                        return;
                    }
                }
            }
            
        }

    }



    List<AniInfo> GetInfoByTriggerType(enTrigger t)
    {
        List<AniInfo> l = new List<AniInfo>();
        if (m_aniInfos == null)
            return l;
        foreach (var info in m_aniInfos)
        {
            if (info.trigger == t)
                l.Add(info);
        }
        return l;
    }

    void Play(AniInfo info)
    {
        m_curInfo = info;
        if (m_animator != null)
        {
            m_animator.Rebind();
            m_animator.Play(m_curInfo.m_aniName);
        }
        else if (m_animation != null)
        {
            m_animation.wrapMode = m_curInfo.wrapMode;
            m_animation.PlayQueued(m_curInfo.m_aniName, QueueMode.PlayNow);
        }
        m_curInfo.m_lastPlayIntervalTime = Time.time;
        if (m_curInfo.intervalMin < m_curInfo.intervalMax)
            m_curInfo.curInterval = Random.Range(m_curInfo.intervalMin, m_curInfo.intervalMax);
        else
            m_curInfo.curInterval = m_curInfo.intervalMin;

        m_curLastPlayTime = Time.time;
    }
}
