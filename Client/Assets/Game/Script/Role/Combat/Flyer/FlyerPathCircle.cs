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

public class FlyerPathCfgCircle : FlyerPathCfg
{
    
    public Vector3 offset = Vector3.up;
    
    public float speed = 20;
    public float angleSpeed = 180;
    public float radMin = 2f;
    public float radMax = 5f;
    public float radPeriod = 4f;
    public float upAxisMin = -30f;
    public float upAxisMax = 30f;
    public float upAxisPeriod = 4;

    //public float speed = 10;
    //public float radius = 3;
    //public float slope = 0;//倾斜角度
    //public bool fullCircleAfterCatch = true;//当追上目标以后完完全全相对于目标做圆环，如果不勾上，那么目标移动的时候将是渐变跟随而非完全跟随
    
    public override enFlyerPathType Type { get{return enFlyerPathType.circle; } }
    protected override bool DirTypeSupport { get { return true; } }
#if UNITY_EDITOR
    protected override void OnDraw()
    {
        
        offset = EditorGUILayout.Vector3Field("偏移", offset);
        
        speed = EditorGUILayout.FloatField("速度", speed);
        angleSpeed = EditorGUILayout.FloatField("角速度", angleSpeed);
        using (new AutoEditorTipButton("半径变化"))
        {
            EditorGUILayout.PrefixLabel("半径变化");
            radMin = EditorGUILayout.FloatField(radMin);
            radMax = EditorGUILayout.FloatField(radMax);
        }
        radPeriod = EditorGUILayout.FloatField("半径变化周期", radPeriod);

        using (new AutoEditorTipButton("up轴变化角"))
        {
            EditorGUILayout.PrefixLabel("up轴变化角");
            upAxisMin = EditorGUILayout.FloatField(upAxisMin);
            upAxisMax = EditorGUILayout.FloatField(upAxisMax);
        }
        upAxisPeriod = EditorGUILayout.FloatField("up轴变化周期", upAxisPeriod);

        EditorGUILayout.HelpBox(string.Format("由角速度和最大半径算出的移动速度为{0}", 2 * Mathf.PI * radMax * angleSpeed / 360f), MessageType.None);
        //speed = EditorGUILayout.FloatField("速度", speed);
        //radius = EditorGUILayout.FloatField("半径", radius);
        //slope = EditorGUILayout.FloatField("倾斜角度", slope);
        //using(new AutoEditorTipButton("当追上目标以后完完全全相对于目标做圆环，如果不勾上，那么目标移动的时候将是渐变跟随而非完全跟随"))
        //    fullCircleAfterCatch = EditorGUILayout.Toggle("完全跟随",fullCircleAfterCatch);
    }
#endif
}
public class FlyerPathCircle : FlyerPath
{
    FlyerPathCfgCircle Cfg {get{return (FlyerPathCfgCircle)m_cfg;}}

    float m_lastAngle = 0;
    
    //bool m_isCatch;
    //float m_lastAngle;
    //float m_angleSpeed;
    public override void OnInit()
    {
        //m_isCatch = false;
        //m_angleSpeed = 360f * Cfg.speed / (2 * Mathf.PI * Cfg.radius);
        m_lastAngle = 0;
    }

    public override void OnUpdate()
    {
        //if (Cfg.radius== 0 || Cfg.speed == 0)
        //    return;

        //找到对象
        Role target = this.m_flyer.Target;
        if (target == null)//找不到则销毁
        {
            m_flyer.Stop();
            return;
        }

        //计算一些相关的值
        Vector3 forward = target.transform.forward;
        forward.y = 0;
        if (forward == Vector3.zero)
            forward = Vector3.forward;
        Vector3 targetPos = target.RoleModel.Tran.position + target.transform.TransformDirection(Cfg.offset);
        Vector3 look = targetPos - m_root.position;
        if (look == Vector3.zero)//link为空下面很多都计算不了，这里随便取个方向
            look = Vector3.forward * 0.5f;
        

        //半径变化
        float radius = Cfg.radMax <= Cfg.radMin? Cfg.radMax: Mathf.Lerp(Cfg.radMin, Cfg.radMax, (Mathf.Sin(Time.time * 2 * Mathf.PI / Cfg.radPeriod) + 1f) * 0.5f);

        //角速度   //fix，角速度不用变化了，角速度不变的情况下缩放半径，速度将变化,这里再有一个变化的话很怪
        // float rotateSpeed = m_rotSpeedMax <= m_rotSpeedMin ? m_rotSpeedMax : Mathf.Lerp(m_rotSpeedMin, m_rotSpeedMax, (Mathf.Sin( Time.time * 2 * Mathf.PI/ m_period) + 1f) * 0.5f);
        m_lastAngle = Mathf.Repeat(m_lastAngle + Cfg.angleSpeed * Time.deltaTime, 360);
        
        //对称轴变化，本来是绕着Vector3.up，这里做下正弦摇摆，来让物体上下移动
        Quaternion rot = Cfg.upAxisMax <= Cfg.upAxisMin ? Quaternion.AngleAxis(Cfg.upAxisMax, forward) : Quaternion.Lerp(Quaternion.AngleAxis(-Cfg.upAxisMax, forward), Quaternion.AngleAxis(Cfg.upAxisMax, forward), (Mathf.Sin(Time.time * 2 * Mathf.PI / Cfg.upAxisPeriod) + 1f) * 0.5f);
        Vector3 pos = targetPos + (Quaternion.AngleAxis(m_lastAngle, rot * Vector3.up) * forward) * radius;

        //位置
        Vector3 offset = pos - m_root.position;
        float d = Cfg.speed * Time.deltaTime;
        if (d * d >= offset.sqrMagnitude)
            m_root.position = pos;
        else
            m_root.position += offset.normalized * d;

        //方向
        SetDir(offset, look);
    }
    
    void LegacyCircle()
    {
        ////找到对象
        //Role target = GetRole(Cfg.targetType);
        //if (target == null)//找不到则销毁
        //{
        //    m_flyer.Stop();
        //    return;
        //}

        //Vector3 targetPos = target.RoleModel.Tran.position + Cfg.offset;
        //Vector3 offset = Vector3.zero;
        //Vector3 look = targetPos - m_root.position;
        //if (look == Vector3.zero)//link为空下面很多都计算不了，这里随便取个方向
        //    look = Vector3.forward * 0.5f;
        //Vector3 forward = target.transform.forward;// Vector3.ProjectOnPlane(, -slopeNormal);

        ////相对于目标的完全环绕(不渐变)
        //if (Cfg.fullCircleAfterCatch && m_isCatch)
        //{
        //    //倾斜面法线
        //    Vector3 slopeNormal = Quaternion.Euler(0, target.transform.eulerAngles.y, 0) * Quaternion.Euler(0, 0, Cfg.slope) * Vector3.up;
        //    m_lastAngle = Mathf.Repeat(m_lastAngle + m_angleSpeed * Time.deltaTime, 360);
        //    forward = Quaternion.AngleAxis(m_lastAngle, -slopeNormal) * forward;
        //    Vector3 oldPos = m_root.position;
        //    m_root.position = targetPos + forward.normalized * Cfg.radius;
        //    offset = m_root.position - oldPos; //Vector3.Cross(forward, slopeNormal);
        //}
        ////渐变
        //else
        //{

        //    //倾斜面法线
        //    Vector3 slopeNormal = Quaternion.Euler(0, target.transform.eulerAngles.y, 0) * Quaternion.Euler(0, 0, Cfg.slope) * Vector3.up;

        //    //在倾斜面法线上的投影
        //    Vector3 projectSlopeNormal = Vector3.Project(look, slopeNormal);

        //    //在倾斜面上的投影
        //    Vector3 projectLook = Vector3.ProjectOnPlane(look, slopeNormal);


        //    //计算出连线的切线，注意这里不是到圆周上的切线，等下计算比例后连线和切线相加就是圆周上的切线了
        //    //圆周上的切线tangent = (Quaternion.AngleAxis(Mathf.Asin(m_radius/ dis) * Mathf.Rad2Deg, up)* link).normalized;
        //    Vector3 tangent = Vector3.Cross(projectLook, -slopeNormal).normalized;//为了逆时针，法线得取反

        //    //修改link指向切点
        //    float dis = projectLook.magnitude;
        //    Vector3 link = dis == 0 ? Vector3.zero : projectLook * ((dis - Cfg.radius) / dis);

        //    float disMin = Cfg.speed * Time.deltaTime;
        //    float disRad = Mathf.Abs(dis - Cfg.radius);
        //    float disProject = projectSlopeNormal.magnitude;

        //    //计算向心，向面和切线的位移
        //    //用比例值计算
        //    if (disRad > (disMin / 4) && disProject > disMin)
        //    {
        //        float factorProject = disProject / (disRad + disProject);
        //        float factorTangent;
        //        float factorLink;
        //        //在圆内
        //        if (dis < Cfg.radius)
        //            factorTangent = Mathf.Lerp(1, 0, disRad / Cfg.radius);
        //        else //在圆外
        //        {
        //            float disTangent = Cfg.radius * dis / Mathf.Sqrt(dis * dis - Cfg.radius * Cfg.radius);//这个公式是根据和圆周切线的三角函数计算而来
        //            factorTangent = disTangent / (disTangent + dis);
        //        }

        //        factorTangent = (1 - factorProject) * factorTangent;
        //        factorLink = 1 - factorProject - factorTangent;

        //        projectSlopeNormal = projectSlopeNormal * (factorProject * disMin / disProject);
        //        tangent = tangent * (factorTangent * disMin);
        //        link = link * (factorLink * disMin / disRad);
        //        //Debug.LogError("1");
        //    }
        //    //不在面上做圆周运动了，向面至少占一半
        //    else if (disProject > disMin)
        //    {
        //        tangent = (0.5f * disMin) * tangent;
        //        //link不变，不然会有抖动
        //        projectSlopeNormal = projectSlopeNormal * (0.5f * disMin / disProject);
        //        //Debug.LogError("2");
        //    }
        //    //在面上了，但是不是圆周
        //    else if (disRad > (disMin / 4))
        //    {
        //        float factorTangent;
        //        //在圆内
        //        if (dis < Cfg.radius)
        //            factorTangent = Mathf.Lerp(1, 0, disRad / Cfg.radius);
        //        else //在圆外
        //        {
        //            float disTangent = Cfg.radius * dis / Mathf.Sqrt(dis * dis - Cfg.radius * Cfg.radius);//这个公式是根据和圆周切线的三角函数计算而来
        //            factorTangent = disTangent / (disTangent + dis);
        //        }

        //        tangent = tangent * (factorTangent * disMin);
        //        link = link * ((1 - factorTangent) * disMin / disRad);
        //        //projectSlopeNormal不变，不然会有抖动
        //        //Debug.LogError("3");
        //    }
        //    //在面上做圆周运动了，此时切线基本占全部
        //    else
        //    {
        //        m_isCatch = true;
        //        tangent = tangent * disMin;
        //        //link不变，不然会有抖动
        //        //projectSlopeNormal不变，不然会有抖动


        //        //Debug.LogError("4");
        //    }

        //    offset = tangent + link + projectSlopeNormal;
        //    m_root.position += offset;

        //    //如果要转变为完全跟随，必须算出角度
        //    if (m_isCatch && Cfg.fullCircleAfterCatch)
        //    {
        //        projectLook = Vector3.ProjectOnPlane(m_root.position - targetPos, slopeNormal);
        //        m_lastAngle = Vector3.Angle(projectLook, forward);

        //        if (m_lastAngle != 0 && Vector3.Dot(-slopeNormal, Vector3.Cross(forward, projectLook)) < 0)
        //            m_lastAngle = 360 - m_lastAngle;
        //    }
        //}
    }

    public override void OnStop()
    {

    }
}