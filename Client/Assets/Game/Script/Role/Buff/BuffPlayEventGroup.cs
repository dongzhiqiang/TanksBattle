#region Header
/**
 * 名称：触发事件组
 
 * 日期：2016.3.16
 * 描述：
事件组id
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class BuffPlayEventGroupCfg : BuffExCfg
{
    public string eventGroupId;


    public override bool Init(string[] pp)
    {
        if (pp.Length < 1)
            return false;

        eventGroupId = pp[0];
        if (string.IsNullOrEmpty(eventGroupId))
            return false;

        return true;
    }

    public override void PreLoad() {
        SkillEventGroupCfg.PreLoad(eventGroupId);
    }
}
public class BuffPlayEventGroup: Buff
{
    public BuffPlayEventGroupCfg ExCfg { get { return (BuffPlayEventGroupCfg)m_cfg.exCfg; } }
    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
        

    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        CombatMgr.instance.PlayEventGroup(m_parent, ExCfg.eventGroupId, m_parent.transform.position);
    }

    //结束
    public override void OnBuffStop(bool isClear) {
       
    } 
}

