using UnityEngine;
using System.Collections;

public class TriggerGroupDeadNum : SceneTrigger
{

    public EventCfg_GroupDeadNum m_eventCfg;

    int m_count = 0;    //记录死亡个数

    public override void Init(EventCfg eventCfg)
    {
        m_eventCfg = eventCfg as EventCfg_GroupDeadNum;

        if (m_eventCfg.bDeadStart)
            LevelMgr.instance.OnRoleDeadStartCallback += OnNpcDead;
        else
            LevelMgr.instance.OnRoleDeadCallback += OnNpcDead;
    }

    public override bool bReach()
    {
        if (m_count >= m_eventCfg.deadNum)
            return true;
        else
            return false;
    }

    public void OnNpcDead(Role role)
    {
        if (role.GetFlag(m_eventCfg.groupId) > 0)
            m_count++;

        if (bReach())
        {
            SceneEventMgr.instance.FireAction(mFlag);
        }

    }

    public override void OnRelease()
    {
        if (m_eventCfg.bDeadStart)
            LevelMgr.instance.OnRoleDeadStartCallback -= OnNpcDead;
        else
            LevelMgr.instance.OnRoleDeadCallback -= OnNpcDead;
    }
}
