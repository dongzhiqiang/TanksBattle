using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriggerNpcDead : SceneTrigger {

    public EventCfg_NpcDead m_eventCfg;

    int m_count = 0;    //记录死亡个数

    public override void Init(EventCfg eventCfg)
    {
        m_eventCfg = eventCfg as EventCfg_NpcDead;

        if (m_eventCfg.bDeadStart)
            LevelMgr.instance.OnRoleDeadStartCallback += OnNpcDead;
        else
            LevelMgr.instance.OnRoleDeadCallback += OnNpcDead;
    }

    public override bool bReach()
    {
        if (m_count >= m_eventCfg.count)
            return true;
        else
            return false;
    }

    public void OnNpcDead(Role role)
    {
        if (role.GetString(enProp.roleId) == m_eventCfg.npcID)
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
