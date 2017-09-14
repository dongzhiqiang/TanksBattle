using UnityEngine;
using System.Collections;
using WellFired;

[USequencerFriendlyName("Play Model Animation")]
[USequencerEvent("Custom/Play Model Animation")]
public class USC_ModelAnimation : USEventBase
{
    public string m_clipName = null;
    public WrapMode wrapMode = WrapMode.Default;
    public float playbackSpeed = 1.0f;

    Transform m_model;
    Animation m_Ani;
    AnimationState m_clip;

    AniFxMgr m_AniMgr;


    void Init()
    {

        if (wrapMode == WrapMode.Default || wrapMode == WrapMode.Clamp || wrapMode == WrapMode.Once)
            wrapMode = WrapMode.ClampForever;

        if (m_Ani == null && AffectedObject != null)
        {
            m_model = AffectedObject.transform.Find("model");


            if (m_model != null)
            {
                m_Ani = m_model.GetComponent<Animation>();
                if (m_Ani == null)
                    Debuger.LogError("过场动画找不到animation  " + AffectedObject.name);

                if (m_Ani[m_clipName] == null)
                    Debuger.LogError("过场动画找不到动作 " + m_clipName);

                if (Application.isPlaying)
                {
                    m_AniMgr = m_model.GetComponent<AniFxMgr>();
                    if (m_AniMgr == null)
                        Debug.LogError("过场动画找不到AniFxMgr");
                }
            }



        }
        if (m_clip == null && m_Ani != null)
        {
            m_clip = m_Ani[m_clipName];
        }

    }

    public void Update()
    {
        if (wrapMode != WrapMode.Loop && m_clip)
            Duration = m_clip.length / playbackSpeed;
    }

    public override void FireEvent()
    {
        Init();

        if (!m_clip)
        {
            Debug.Log("Attempting to play an animation on a GameObject but you haven't given the event an animation clip from USPlayAnimEvent::FireEvent");
            return;
        }

        if (!m_Ani)
        {
            Debug.Log("Attempting to play an animation on a GameObject without an Animation Component from USPlayAnimEvent.FireEvent");
            return;
        }

        if (!m_AniMgr)
        {
            Debug.Log("没有找到 AniFxMgr");
            return;
        }

        if (Application.isPlaying)  //运行模式下
        {

            m_AniMgr.Play(m_clip.name, wrapMode);
        }
        else
        {
            m_Ani.wrapMode = wrapMode;
            m_Ani.Stop();
            m_Ani.Play(m_clip.name);
            m_clip.speed = playbackSpeed;
        }
    }

    public override void ProcessEvent(float deltaTime)
    {
        Init();

        if (m_clip == null || m_Ani == null)
            return;

        if (!m_Ani.IsPlaying(m_clip.name))
        {
            m_Ani.wrapMode = wrapMode;
            m_Ani.Play(m_clip.name);
        }

        m_clip.speed = playbackSpeed;
        m_clip.time = deltaTime * playbackSpeed;
        m_clip.enabled = true;
        m_Ani.Sample();
        m_clip.enabled = false;
    }

    public override void StopEvent()
    {
        if (!AffectedObject)
            return;

        if (Application.isPlaying)
        {
        }
        else
        {
            if (m_Ani != null)
                m_Ani.Stop();
        }

        m_Ani = null;
        m_clip = null;
        m_AniMgr = null;
    }
}
