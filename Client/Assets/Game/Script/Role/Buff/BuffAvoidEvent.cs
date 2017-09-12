#region Header
/**
 * 名称：免疫事件
 
 * 日期：2016.2.26
 * 描述：
目标事件类型|优先级|免疫对象类型,目标事件类型|优先级|免疫对象类型.....
这里指的技能事件组的事件，如果优先级高于目标事件的优先级，那么事件不被执行
目标事件类型,被击,浮空,击飞,伤害,移动,卡帧,特效,跳起,技能,动作特效,推镜,触发状态
优先级，0默认
免疫对象类型，0默认只免疫别人对自己的事件，1只免疫自己对自己的事件，2免疫所有事件
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
public class BuffAvoidEventCfg : BuffExCfg
{
    public Dictionary<enSkillEventType, AvoidEventCxt> cxts = new Dictionary<enSkillEventType, AvoidEventCxt>();
    public AvoidEventCxt all = null;

    public override bool Init(string[] pp)
    {
        foreach (string s in pp)
        {
            if (string.IsNullOrEmpty(s))
                continue;

            AvoidEventCxt cxt = new AvoidEventCxt(s);
            if (cxt.error)
                return false;
            cxts[cxt.eventType] = cxt;
            if (cxt.eventType == enSkillEventType.max)
                all = cxt;
        }
        return true;
    }
}

public class AvoidEventCxt
{
    public enSkillEventType eventType;
    public int priority;
    public int avoidTargetType;//0默认只免疫别人对自己的事件，1只免疫自己对自己的事件，2免疫所有事件
    public bool error=false;

    public AvoidEventCxt(string s)
    {
        if(string.IsNullOrEmpty(s))
        {
            error = true;
            return;
        }
        
        string[] pp = s.Split('|');
        if (pp.Length < 1)
        {
            error = true;
            return;
        }
        
        
        if (pp[0] == "全部")
        {
            eventType = enSkillEventType.max;
        }
        else
        {
            eventType = SkillEventFactory.GetEventType(pp[0]);
            if (eventType == enSkillEventType.max)
            {
                error = true;
                return;
            }
        }
        

        if (pp.Length < 2 || !int.TryParse(pp[1], out priority))
            priority = 0;

        
        if (pp.Length < 3 || !int.TryParse(pp[2], out avoidTargetType))
            avoidTargetType= 0;
    }
}
public class BuffAvoidEvent: Buff
{
    public BuffAvoidEventCfg ExCfg { get { return (BuffAvoidEventCfg)m_cfg.exCfg; } }
    int m_observer;

    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
        if (m_observer != EventMgr.Invalid_Id)
        {
            Debuger.LogError("逻辑错误，免疫事件初始化的时候发现监听没有清空");
            EventMgr.Remove(m_observer); 
            m_observer = EventMgr.Invalid_Id;
        }
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        if (m_count > 1)
        {
            Debuger.LogError("免疫事件不需要执行多次，状态id:{0}", m_cfg.id);
            return;
        }

        //监听状态创建否决，有要免疫的状态就否决掉
        m_observer = m_parent.AddVote(MSG_ROLE.SKILL_EVENT_AVOID, OnEvent);
    }

    //结束
    public override void OnBuffStop(bool isClear) {
      
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
    }

    //有状态添加的时候，否决掉
    bool OnEvent(object p, object p2, object p3, EventObserver ob)
    {
        Role source = (Role)p;
        SkillEventCfg cfg = (SkillEventCfg)p2;

        AvoidEventCxt cxt;
        if (!ExCfg.cxts.TryGetValue(cfg.Type, out cxt))
        {
            if(ExCfg.all == null)
                return true;
            cxt = ExCfg.all;
        }
            

        //免疫对象类型，0默认只免疫别人对自己的事件，1只免疫自己对自己的事件，2免疫所有事件
        switch (cxt.avoidTargetType)
        {
            case 0:if(source== m_parent)return true;else break;
            case 1: if (source != m_parent) return true; else break;
            case 2:break;
            default:break;
        }

        //目标事件的优先级比免疫状态优先级高，不能否决
        if (cfg.priority > cxt.priority)
            return true;

        return false;
    }
    
  
}

