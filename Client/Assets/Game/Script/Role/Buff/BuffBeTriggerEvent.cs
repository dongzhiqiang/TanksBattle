#region Header
/**
 * 名称：被动事件触发
 
 * 日期：2016.3.16
 * 描述：
事件类型,状态列表,作用对象,释放者
当别人给角色释放技能事件的时候
事件类型,被击,浮空,击飞,伤害,移动,卡帧,特效,跳起,技能,动作特效,推镜,触发状态
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
public class BuffBeTriggerEventCfg : BuffExCfg
{
    public enSkillEventType triggerType;
    public List<int> buffIds = new List<int>();
    public enBuffTargetType targetType = enBuffTargetType.self;
    public enBuffTargetType sourceType = enBuffTargetType.self;

    public override bool Init(string[] pp)
    {
        if (pp.Length < 2)
            return false;

        //事件类型
        triggerType = SkillEventFactory.GetEventType(pp[0]);
        if (triggerType == enSkillEventType.max)
            return false;

        //状态列表
        int i = 0;
        if (int.TryParse(pp[1], out i))
            buffIds.Add(i);
        else
        {
            if (!StringUtil.TryParse(pp[1].Split('|'), ref buffIds))
                return false;
        }

        //作用对象
        if (pp.Length > 2 && int.TryParse(pp[2], out i))
            targetType = (enBuffTargetType)i;
        //释放者
        if (pp.Length > 3 && int.TryParse(pp[3], out i))
            sourceType = (enBuffTargetType)i;
        return true;
    }
    public override void PreLoad()
    {
        for (int i = 0; i < buffIds.Count; ++i)
        {
            BuffCfg.ProLoad(buffIds[i]);
        }
    }
}

public class BuffBeTriggerEvent: Buff
{
    public BuffBeTriggerEventCfg ExCfg { get { return (BuffBeTriggerEventCfg)m_cfg.exCfg; } }
    int m_observer;
    
   
    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
        
       
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        if (m_count > 1)
        {
            Debuger.LogError("被动事件触发状态不需要执行多次，状态id:{0}", m_cfg.id);
            return;
        }
        //监听技能事件
        m_observer = m_parent.Add(MSG_ROLE.TARGET_SKILL_EVENT_PRE, OnEvent);
    }

    //结束
    public override void OnBuffStop(bool isClear) {
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
    }

   
    void OnEvent(object p, object p2, object p3, EventObserver ob)
    {
        Role source = (Role)p;
        SkillEventCfg cfg = (SkillEventCfg)p2;

        if (ExCfg.triggerType != cfg.Type)
            return;
        int poolId = this.Id;
        int sourceId = source.Id;
        int parentId = m_parent.Id;

        //作用对象
        Role buffTarget = this.GetRole(ExCfg.targetType, source);
        if (buffTarget == null)
            return;
        BuffPart buffPart = buffTarget.BuffPart;

        //释放者
        Role buffSource = this.GetRole(ExCfg.sourceType, source);
        if (buffSource == null)
            return;

        for (int i = 0; i < ExCfg.buffIds.Count; ++i)
        {
            
            buffPart.AddBuff(ExCfg.buffIds[i], buffSource);
            if (IsUnneedHandle(poolId, m_parent, parentId, source, sourceId))
                return;
        }
        
    }
    
  
}

