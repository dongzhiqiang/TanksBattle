using UnityEngine;
using System.Collections;


//动作事件
public class QteEventAni : QteEvent
{
    public static string[] ObjTypeName = { "主角", "敌人", "镜头" };

    public enQteEventObjType m_objType = enQteEventObjType.Qte_Obj_Hero;
    public string aniName = "";
    public WrapMode wrapMode = WrapMode.ClampForever;

    AnimationState m_clip;
    AniFxMgr m_AniMgr;
    Animation m_Ani;

    public override void Init()
    {
        Transform mod = null;
        if (m_objType == enQteEventObjType.Qte_Obj_Camera && CurQte.ModelCamera != null)
        {
            mod = CurQte.ModelCamera;
        }
        else if (m_objType == enQteEventObjType.Qte_Obj_Hero && CurQte.ModelHero != null)
        {
            mod = CurQte.ModelHero.Find("model");
        }
        else if (m_objType == enQteEventObjType.Qte_Obj_Target && CurQte.ModelTarget != null)
        {
            mod = CurQte.ModelTarget.Find("model");
        }

        if (mod != null)
        {
            m_Ani = mod.GetComponent<Animation>();
            m_AniMgr = mod.GetComponent<AniFxMgr>();
        }

        if (m_Ani == null)
        {
            //Debug.LogError("模型类型" + m_objType + "未找到Animation组件");
            return;
        }
        if (!string.IsNullOrEmpty(aniName))
        {
            m_clip = m_Ani[aniName];
            if (m_clip == null)
            {
                Debug.LogError(m_objType+"没有找到动作" + aniName);
                return;
            }
        }
    }

    public override void Start()
    {
    
        if (Application.isPlaying)  //运行模式下
        {
            if (m_objType == enQteEventObjType.Qte_Obj_Target && CurQte.Target != null)
            {
                CurQte.Target.AniPart.Play(aniName, wrapMode);
            }
            else if (m_objType == enQteEventObjType.Qte_Obj_Hero && CurQte.Hero != null)
            {
                CurQte.Hero.AniPart.Play(aniName, wrapMode);
            }
            else if (m_objType == enQteEventObjType.Qte_Obj_Camera)
            {
                m_Ani.wrapMode = wrapMode;
                m_Ani.Stop();
                m_Ani.Play(m_clip.name);
            }
        }
        else
        {
            if (m_clip == null)
                return;

            if (string.IsNullOrEmpty(m_clip.name))
            {
                Debug.LogError("找不到动作" + m_clip.name);
                return;
            }
            m_Ani.wrapMode = wrapMode;
            m_Ani.Stop();
            m_Ani.Play(m_clip.name);
        }
    }
    public override void Update(float time)
    {

#if UNITY_EDITOR
        if (m_clip == null || m_Ani == null)
            return;

        if (!m_Ani.IsPlaying(m_clip.name))
        {
            m_Ani.wrapMode = wrapMode;
            m_Ani.Play(m_clip.name);
        }

        m_clip.speed = 1.0f;
        m_clip.time = time;
        m_clip.enabled = true;
        m_Ani.Sample();
        m_clip.enabled = false;
#endif
    }
    public override void Stop()
    {
        if (m_Ani != null && m_clip != null)
        {
            m_clip.time = 0;
            m_clip.enabled = true;
            m_Ani.Sample();
            m_clip.enabled = false;

            m_Ani.Stop();
        }
        m_Ani = null;
        m_clip = null;
        m_AniMgr = null;
    }
}
