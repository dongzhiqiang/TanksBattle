#region Header
/**
 * 名称：转换被击
 
 * 日期：2016.7.27
 * 描述：
将他人对自己的浮空和击飞事件转为被击事件
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
public class BuffChangeHitCfg : BuffExCfg
{
    public override bool Init(string[] pp)
    {
    
        return true;
    }
}

public class BuffChangeHit : Buff
{
    public BuffChangeHitCfg ExCfg { get { return (BuffChangeHitCfg)m_cfg.exCfg; } }
    int m_observer;

    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
        if (m_observer != EventMgr.Invalid_Id)
        {
            Debuger.LogError("逻辑错误，转换被击初始化的时候发现监听没有清空");
            EventMgr.Remove(m_observer); 
            m_observer = EventMgr.Invalid_Id;
        }
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        if (m_count > 1)
        {
            Debuger.LogError("转换被击不需要执行多次，状态id:{0}", m_cfg.id);
            return;
        }

        //监听状态创建否决，有要免疫的状态就否决掉
        m_observer = m_parent.AddVote(MSG_ROLE.CHANGE_HIT_EVENT, OnEvent);
    }

    //结束
    public override void OnBuffStop(bool isClear) {
      
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
    }

    //有状态添加的时候，否决掉
    bool OnEvent(object p, object p2, object p3, EventObserver ob)
    {
        return false;
    }
    
  
}

