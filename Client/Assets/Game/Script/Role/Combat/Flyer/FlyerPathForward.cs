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

public class FlyerPathCfgForward : FlyerPathCfg
{
    public float speed =5;
    public override enFlyerPathType Type { get{return enFlyerPathType.forward;} }
#if UNITY_EDITOR
    protected override void OnDraw()
    {
        speed = EditorGUILayout.FloatField("速度", speed);
    }
#endif
}
public class FlyerPathForward : FlyerPath
{
    FlyerPathCfgForward Cfg {get{return (FlyerPathCfgForward)m_cfg;}} 
    public override void OnInit()
    {

    }

    public override void OnUpdate()
    {
        m_root.position += m_root.forward*Cfg.speed*Time.deltaTime;
    }

    public override void OnStop()
    {

    }
}