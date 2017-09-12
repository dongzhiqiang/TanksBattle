#region Header
/**
 * 名称：增减受击伤害
 
 * 日期：2016.4.18
 * 描述：
值或百分比
伤害值 = 最终伤害*(1+增减攻击伤害+增减受击伤害)
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class BuffAddDefDamageCfg : BuffExCfg
{
    public LvValue value;

    public override bool Init(string[] pp)
    {
        if (pp.Length < 1)
            return false;
        
        value = new LvValue(pp[0]);
        if (value.error)
            return false;


        return true;
    }
}


public class BuffAddDefDamage : Buff
{
    public BuffAddDefDamageCfg ExCfg { get { return (BuffAddDefDamageCfg)m_cfg.exCfg; } }
    int m_observer;

    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
       
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        if (m_count > 1)
        {
            Debuger.LogError("增减受击伤害状态不需要执行多次，状态id:{0}", m_cfg.id);
            return;
        }
        //监听伤害计算
        m_observer = m_parent.Add(MSG_ROLE.ADD_DEF_DAMAGE, OnEvent);
    }

    //结束
    public override void OnBuffStop(bool isClear) {
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }

    }

    void OnEvent(object p, object p2, object p3, EventObserver ob)
    {
        ValueObject<float> addDefDamage = (ValueObject<float>)p;
        
        addDefDamage.Value += this.GetLvValue(ExCfg.value);
    }
}

