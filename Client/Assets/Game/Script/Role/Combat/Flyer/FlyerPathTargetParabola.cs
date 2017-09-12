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

public class FlyerPathCfgTargetParabola : FlyerPathCfg
{   
    public float speed = 5;
    public float height = 4;
    public float heightOffset = 0f;
    public float minDis = 5;
    public float maxDis = 100;
    

    public override enFlyerPathType Type { get{return enFlyerPathType.targetParabola; } }
    protected override bool DirTypeSupport { get { return true; } }
#if UNITY_EDITOR
    protected override void OnDraw()
    {
        using (new AutoFontSize(13, EditorStyles.helpBox))
        {
            string content = "朝目标脚下做抛物线运动，注意这里不会跟踪目标，一开始就算出轨迹";
            EditorGUILayout.LabelField(content, EditorStyles.helpBox, GUILayout.Height(EditorStyles.helpBox.CalcHeight(new GUIContent(content), 225)));
        }

        speed = EditorGUILayout.FloatField("速度", speed);
        height = EditorGUILayout.FloatField("高度", height);
        heightOffset = EditorGUILayout.FloatField("落地点偏移", heightOffset);
        using (new AutoEditorTipButton("范围，小于范围算最小值，大于范围算最大值"))
        {
            EditorGUILayout.PrefixLabel("范围限制");
            minDis = EditorGUILayout.FloatField(minDis);
            maxDis = EditorGUILayout.FloatField(maxDis);
        }
     }
#endif
}
public class FlyerPathTargetParabola : FlyerPath
{
    FlyerPathCfgTargetParabola Cfg {get{return (FlyerPathCfgTargetParabola)m_cfg;}}

    float m_g;
    float m_v0;
    float m_duration;
    float m_beginTime;
    Vector3 m_beginPos = Vector3.zero;
    Vector3 m_beginDir2d = Vector3.zero;
    
    public override void OnInit()
    {
        if ( Cfg.height == 0 || (Cfg.height > 0 && Cfg.height < Cfg.heightOffset) || (Cfg.height < 0 && Cfg.height >= -Cfg.heightOffset))
        {
            LogError("参数出错,不能构成抛物线");
            return;
        }
        if(Cfg.minDis<=0)
        {
            LogError("最小距离必须大于0");
            return;
        }



        float dis = Cfg.minDis;
        if(this.m_flyer.Target!=null)
        {
            Vector3 link = this.m_flyer.Target.transform.position- m_root.position;
            link.y = 0;
            dis = link.magnitude;
            if(dis>0.0001f)
            {
                dis = Mathf.Clamp(dis,Cfg.minDis, Cfg.maxDis );
                this.m_root.forward = link;
            }
        }

        //相关公式：s=v0t+at^2/2  vt=v0+at vt^2+v0^2 =2as
        m_duration = dis/Cfg.speed;
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
        if ( Cfg.height == 0 || (Cfg.height > 0 && Cfg.height < Cfg.heightOffset) || (Cfg.height < 0 && Cfg.height >= -Cfg.heightOffset))
            return;
        if (Cfg.minDis <= 0)
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