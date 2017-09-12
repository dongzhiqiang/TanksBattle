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
public enum enFlyerTargetType
{
    source,     //0释放者
    target,     //1目标
    autoTarget, //2目标(没有的话自动找最近的敌人)
    hate,       //3仇恨目标
    hateNew,    //4仇恨值目标
    hateNewNotFind,//5仇恨值目标(不自动查找)
    closestSame,//6最近的友方
    closestEnemy,//7最近的敌方
    closestNeutral,//8最近的中立阵营
    parent,//9主人
    hero,//10主角
    max
}


public enum enFlyerDir
{
    none,
    forward,
    look,
    back,
    selfTurn,
}

[JsonCanInherit]
public class FlyerPathCfg
{
    public static string[] FlyerTargetTypeName = new string[] { "释放者", "目标",
        "目标(没有的话自动找最近的敌人)", "仇恨目标", "仇恨值目标", "仇恨值目标(不自动查找)",
        "最近的友方", "最近的敌方", "最近的中立阵营","主人","主角"};
    public static string[] DirTypeName = new string[] { "无", "前进方向", "看着对象", "背对对象", "自转" };
    public virtual enFlyerPathType Type { get{return enFlyerPathType.empty;} }
    protected virtual bool DirTypeSupport { get { return false; } }

    public enFlyerDir _dirType = enFlyerDir.none;
    public float _selfTurnSpeed = 360;

#if UNITY_EDITOR
    public void Draw() {
        if (this.DirTypeSupport)
        {
            _dirType = (enFlyerDir)EditorGUILayout.Popup("方向", (int)_dirType, DirTypeName);
            if (_dirType == enFlyerDir.selfTurn)
                _selfTurnSpeed = EditorGUILayout.FloatField("自转速度", _selfTurnSpeed);
        }
        
        OnDraw();
    }
    protected virtual void OnDraw() { }

#endif

}

public class FlyerPath :IdType
{
    protected Flyer m_flyer;
    protected Transform m_root;
    protected FlyerPathCfg m_cfg;

    public void Init(Flyer flyer)
    {
        m_flyer = flyer;
        m_root = flyer.Root;
        m_cfg = flyer.Cfg.pathCfg;
        OnInit();
    }

    
    public virtual void OnInit()
    {
        
    }

    public virtual void OnUpdate()
    {

    }

    public virtual void OnStop()
    {

    }

    //0默认自己，1释放者，2别人,3仇恨目标,4仇恨值目标,5仇恨值目标(不自动查找),6最近的友方,7最近的敌方,8最近的中立阵营,9主人,10主角
    protected Role GetRole(enFlyerTargetType t)
    {
        return m_flyer.GetRole(t);
    }

    protected void SetDir(Vector3 offset)
    {
        Vector3 look = m_flyer.Target!=null? m_flyer.Target.RoleModel.Tran.position - m_root.position:Vector3.zero;
        SetDir(offset, look);
    }


    protected void SetDir(Vector3 offset,Vector3 look)
    {
        
        switch (m_cfg._dirType)
        {
            case enFlyerDir.none: break;
            case enFlyerDir.forward: if (offset != Vector3.zero) m_root.forward = offset; break;
            case enFlyerDir.look: if (look != Vector3.zero) m_root.forward = look; break;
            case enFlyerDir.back: if (look != Vector3.zero) m_root.forward = -look; break;
            case enFlyerDir.selfTurn:
                {
                    Vector3 v = m_root.forward;
                    v.y = 0;
                    if (v != Vector3.zero)
                    {
                        v = Quaternion.Euler(0, TimeMgr.instance.logicDelta * m_cfg._selfTurnSpeed, 0) * v;
                        m_root.forward = v;
                    }
                }
                break;
            default: LogError("未知的类型:{0}", m_cfg._dirType); break;
        }
    }
    public void LogError(string format, params object[] ps)
    {
        string s = string.Format("飞出物出错:{0}", this.m_flyer.Cfg.file);
        Debuger.LogError(s + format, ps);
    }
}

