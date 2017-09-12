#region Header
/**
 * 名称：免疫状态
 
 * 日期：2016.2.25
 * 描述：
目标状态类型|优先级,目标状态类型|优先级.....
当优先级高于目标状态的免疫优先级，加这个状态的时候身上所有的目标状态被删除，这个状态期间目标状态加不进来
优先级，0默认
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class BuffAvoidBuffCfg : BuffExCfg
{
    public Dictionary<enBuff, int> cxts = new Dictionary<enBuff, int>();


    public override bool Init(string[] pp)
    {
        foreach (string s in pp)
        {
            if (string.IsNullOrEmpty(s))
                continue;

            string[] pp2 = s.Split('|');
            if (pp2.Length < 1)
                return false;

            BuffType cfg = BuffType.Get(pp2[0]);
            if (cfg == null)
                return false;

            int priority;
            if (pp2.Length < 2 || !int.TryParse(pp2[1], out priority))
                priority = 0;

            cxts[cfg.id] = priority;
        }
        return true;
    }
}
public class BuffAvoidBuff : Buff
{
    public BuffAvoidBuffCfg ExCfg { get { return (BuffAvoidBuffCfg)m_cfg.exCfg; } }
    
    int m_observer;

    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
      
        if (m_observer != EventMgr.Invalid_Id)
        {
            Debuger.LogError("逻辑错误，免疫状态初始化的时候发现监听没有清空");
            EventMgr.Remove(m_observer); 
            m_observer = EventMgr.Invalid_Id;
        }
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        if (m_count > 1)
        {
            Debuger.LogError("免疫状态不需要执行多次，状态id:{0}", m_cfg.id);
            return;
        }

        //删除当前有的、要免疫的状态
        BuffPart buffPart =m_parent.BuffPart;
        LinkedList<Buff> buffs =buffPart.Buffs;
        Buff buff = null;
        LinkedListNode<Buff> cur = null;
        LinkedListNode<Buff> next = buffs.First;
        while (next != null)
        {
            cur = next;
            buff = cur.Value;
            next = cur.Next;

            if(buff == this)
                continue;
            int priority;
            if(!ExCfg.cxts.TryGetValue(buff.Cfg.BuffType,out priority))
                continue;
            if(buff.Cfg.priority>priority)//目标状态的优先级比免疫状态优先级高，不能否决
                continue;

            buff.Remove();
        }

        //监听状态创建否决，有要免疫的状态就否决掉
        m_observer = m_parent.AddVote(MSG_ROLE.BUFF_ADD_AVOID, OnBuffAdd);
    }

    //结束
    public override void OnBuffStop(bool isClear) {
        
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
    }

    //有状态添加的时候，否决掉
    bool OnBuffAdd(object p, object p2, object p3, EventObserver ob)
    {
        BuffCfg cfg = (BuffCfg)p;
        int priority;
        if (!ExCfg.cxts.TryGetValue(cfg.BuffType, out priority))
            return true;
        if (cfg.priority > priority)//目标状态的优先级比免疫状态优先级高，不能否决
            return true;

        return false;
    }
    
  
}

