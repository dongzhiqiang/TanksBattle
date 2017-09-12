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

public class FlyerPathCfgToward : FlyerPathCfg
{
    public Vector3 offset = Vector3.up;
    public float speedBegin = 3;
    public float beginFrame = 10;

    public float speedToward = 5;
    public float towardFrame = 10;
    public float turnSpeed = 360;
    public float touchDis = 0.4f;
    public int touchCount = 5;
    
    
    
    

    public override enFlyerPathType Type { get{return enFlyerPathType.toward; } }
    protected override bool DirTypeSupport { get { return true; } }
#if UNITY_EDITOR
    protected override void OnDraw()
    {
        using (new AutoFontSize(13, EditorStyles.helpBox))
        {
            string content = "一开始朝前飞行一段时间，然后追踪最近的敌人，接触到这个敌人后追踪下一个敌人，达到最大接触数后朝前飞";
            float height = EditorStyles.helpBox.CalcHeight(new GUIContent(content), 225);
            EditorGUILayout.LabelField(content, EditorStyles.helpBox, GUILayout.Height(height));
        }
        offset = EditorGUILayout.Vector3Field("敌人偏移", offset);

        speedBegin = EditorGUILayout.FloatField("开始时速度", speedBegin);
        beginFrame = EditorGUILayout.FloatField("开始帧数", beginFrame);
        speedToward = EditorGUILayout.FloatField("追踪时速度", speedToward);
        towardFrame = EditorGUILayout.FloatField("追踪帧数", towardFrame);
        turnSpeed = EditorGUILayout.FloatField("追踪转向速度", turnSpeed);
        touchDis = EditorGUILayout.FloatField("接触判定距离", touchDis);
        touchCount = EditorGUILayout.IntField("接触个数", touchCount);

    }
#endif
}
public class FlyerPathToward : FlyerPath
{
    FlyerPathCfgToward Cfg {get{return (FlyerPathCfgToward)m_cfg;}}

    float m_beginTime = 0;
    int m_touchCounter = 0;
    Role m_target;
    int m_targetId;
    Quaternion m_lastRot;
  

    public override void OnInit()
    {
        m_touchCounter = 0;
        m_beginTime = TimeMgr.instance.logicTime;
        m_target = null;
        m_lastRot = m_root.rotation;
    }

    //s=v0t+at^2/2
    float VGTToS(float vo, float g, float t)
    {
        return vo * t + g * t * t / 2;
    }

    public override void OnUpdate()
    {
        Vector3 offset = Vector3.zero;
        Vector3 look = Vector3.zero;
        if (TimeMgr.instance.logicTime - m_beginTime <= Cfg.beginFrame*Util.One_Frame)
        {
            offset = m_root.forward * Cfg.speedBegin* TimeMgr.instance.logicDelta;
        }
        else if (m_touchCounter >= Cfg.touchCount|| (Cfg.towardFrame >0 &&(TimeMgr.instance.logicTime - m_beginTime) >=(Cfg.towardFrame+ Cfg.beginFrame )* Util.One_Frame))
        {
            offset = m_root.forward * Cfg.speedToward * TimeMgr.instance.logicDelta;
        }
        else
        {
            bool hasTarget = m_target != null && !m_target.IsUnAlive(m_targetId);

            //判断是不是碰到了
            bool isTouch = false;
            Vector3 targetPos =Vector3.zero;
            if (hasTarget)
            {
                targetPos = m_target.RoleModel.Tran.position + m_target.transform.TransformDirection(Cfg.offset);
                isTouch = Vector3.Distance(targetPos, m_root.position) <= Mathf.Max(Cfg.touchDis, 0.01f);
                if (isTouch)
                    m_touchCounter++;
            }
            
            //找下目标，如果需要的话
            if (m_touchCounter < Cfg.touchCount && (isTouch || !hasTarget))
            {                
                m_target =RoleMgr.instance.GetClosestTarget(m_flyer.Source, m_root.position, enSkillEventTargetType.enemy,false,false, m_target);
                if(m_target!=null)
                {
                    m_targetId = m_target.Id;
                    targetPos = m_target.RoleModel.Tran.position + m_target.transform.TransformDirection(Cfg.offset);
                }
            }
            
            //位置
            
            if (m_target !=null &&m_touchCounter < Cfg.touchCount)
            {
                look = targetPos - m_root.position;
                if (look != Vector3.zero)
                {
                    m_lastRot = Quaternion.RotateTowards(m_lastRot, Quaternion.LookRotation(look), Cfg.turnSpeed * TimeMgr.instance.logicDelta);
                    offset = m_lastRot *Vector3.forward* (Cfg.speedToward * TimeMgr.instance.logicDelta);
                }
                else
                    offset = m_root.forward * (Cfg.speedToward * TimeMgr.instance.logicDelta);
                
            }
            else
                offset = m_root.forward * (Cfg.speedToward * TimeMgr.instance.logicDelta);
        }

        m_root.position += offset;
        //方向
        SetDir(offset, look);
    }

    public override void OnStop()
    {

    }
}