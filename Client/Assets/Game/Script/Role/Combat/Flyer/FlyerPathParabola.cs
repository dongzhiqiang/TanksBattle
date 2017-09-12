#region Header
/**
 * 名称：飞出物弹道配置
 
 * 日期：2016.1.19
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FlyerPathCfgParabola : FlyerPathCfg
{   
    public float speed = 5;
    public float height = 4;
    public float frame = 30;
    public float heightOffset = 0f;
    

    public override enFlyerPathType Type { get{return enFlyerPathType.parabola; } }
    protected override bool DirTypeSupport { get { return true; } }
#if UNITY_EDITOR
    protected override void OnDraw()
    {
        speed = EditorGUILayout.FloatField("速度", speed);
        height = EditorGUILayout.FloatField("高度", height);
        heightOffset = EditorGUILayout.FloatField("落地点偏移", heightOffset);
        using (new AutoEditorTipButton("超过这个帧数，飞出物就会在地板停留"))
            frame = EditorGUILayout.FloatField("帧数", frame);
     }
#endif
}
public class FlyerPathParabola : FlyerPath
{
    FlyerPathCfgParabola Cfg {get{return (FlyerPathCfgParabola)m_cfg;}}

    float m_g;
    float m_v0;
    float m_duration;
    float m_beginTime;
    Vector3 m_beginPos = Vector3.zero;
    Vector3 m_beginDir2d = Vector3.zero;
    
    public override void OnInit()
    {
        if (Cfg.frame == 0 || Cfg.height == 0 || (Cfg.height > 0 && Cfg.height < Cfg.heightOffset) || (Cfg.height < 0 && Cfg.height >= -Cfg.heightOffset))
        {
            LogError("参数出错,不能构成抛物线");
            return;
        }

        //相关公式：s=v0t+at^2/2  vt=v0+at vt^2+v0^2 =2as
        m_duration = Cfg.frame / 30f;
        if (Cfg.height > 0)
        {
            m_g = -(4 * Cfg.height + 4 * Mathf.Sqrt(Cfg.height * (Cfg.height - Cfg.heightOffset)) - 2 * Cfg.heightOffset) / (m_duration * m_duration);
            m_v0 = Mathf.Sqrt(-2 * m_g * Cfg.height);
        }
        else
        {
            m_v0 = 0;
            m_g = 2 * (Cfg.height + Cfg.heightOffset) / (m_duration * m_duration);
        }

        m_beginPos = m_root.position;
        m_beginDir2d = m_root.forward;
        m_beginDir2d.y = 0;
        m_beginDir2d.Normalize();
        m_beginTime = TimeMgr.instance.logicTime;
    }

    //s=v0t+at^2/2
    float VGTToS(float vo, float g, float t)
    {
        return vo * t + g * t * t / 2;
    }

    public override void OnUpdate()
    {
        if (Cfg.frame == 0 || Cfg.height == 0 || (Cfg.height > 0 && Cfg.height < Cfg.heightOffset) || (Cfg.height < 0 && Cfg.height >= -Cfg.heightOffset))
            return;
        
        float duration = (TimeMgr.instance.logicTime - m_beginTime);
        if (duration >= m_duration)
            duration = m_duration;

        //位置
        Vector3 lastPos = m_root.position;
        Vector3 curPos = m_beginPos + m_beginDir2d * Cfg.speed * duration + Vector3.up * VGTToS(m_v0, m_g, duration);
        if(curPos!= lastPos)
            m_root.position = curPos;

        //方向
        SetDir(curPos - lastPos);
    }

    public override void OnStop()
    {

    }
}