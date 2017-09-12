#region Header
/**
 * 名称：免疫伤害反弹
 
 * 日期：2016.3.21
 * 描述：
对敌人造成伤害时免疫其对可能自己造成的伤害反弹
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class BuffAvoidDamageReflectCfg : BuffExCfg
{
  


    public override bool Init(string[] pp)
    {
  
        return true;
    }
}
public class BuffAvoidDamageReflect : Buff
{
    public BuffAvoidDamageReflectCfg ExCfg { get { return (BuffAvoidDamageReflectCfg)m_cfg.exCfg; } }

    int m_observer;

    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
    

    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        if (m_count > 1)
        {
            Debuger.LogError("免疫伤害反弹不需要执行多次，状态id:{0}", m_cfg.id);
            return;
        }

        //监听状态创建否决，有要免疫的状态就否决掉
        m_observer = m_parent.AddVote(MSG_ROLE.DAMAGE_REFLECT_AVOID, OnEvent);
    }

    //结束
    public override void OnBuffStop(bool isClear) {

        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
    }

    //伤害反弹的时候否决掉
    bool OnEvent(object p, object p2, object p3, EventObserver ob)
    {
        return false;
    }
    
  
}

