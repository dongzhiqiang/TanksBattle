#region Header
/**
 * 名称：增减攻击伤害
 
 * 日期：2016.4.18
 * 描述：
值或百分比,受击者标记
伤害值 = 最终伤害*(1+增减攻击伤害+增减受击伤害)
受击者标记,默认为空，否则判断下受击者身上有没有这个标记，有的话才增加攻击伤害
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class BuffAddDamageCfg : BuffExCfg
{
    public LvValue value;
    public string flag;


    public override bool Init(string[] pp)
    {
        if (pp.Length < 1)
            return false;
        
        value = new LvValue(pp[0]);
        if (value.error)
            return false;

        if(pp.Length >=2)
            flag = pp[1] ;
        return true;
    }
}


public class BuffAddDamage : Buff
{
    public BuffAddDamageCfg ExCfg { get { return (BuffAddDamageCfg)m_cfg.exCfg; } }
    int m_observer;

    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
       
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        if (m_count > 1)
        {
            Debuger.LogError("增减攻击伤害状态不需要执行多次，状态id:{0}", m_cfg.id);
            return;
        }
        //监听伤害计算
        m_observer = m_parent.Add(MSG_ROLE.ADD_DAMAGE, OnEvent);
        
    }

    //结束
    public override void OnBuffStop(bool isClear) {
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }

    }

    void OnEvent(object p, object p2, object p3, EventObserver ob)
    {
        ValueObject<float> addDamage = (ValueObject<float>)p;
        Role target = (Role)p2;

        //目标身上有这个标记才增加对他的伤害
        if (!string.IsNullOrEmpty(ExCfg.flag) && target.GetFlag(ExCfg.flag) <= 0)
            return;

        addDamage.Value += this.GetLvValue(ExCfg.value);
    }
}

