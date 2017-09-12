using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriggerRoleDead : SceneTrigger {
    public EventCfg_RoleDead m_eventCfg;

    bool bReached = false;

    public override void Init(EventCfg eventCfg)
    {
        m_eventCfg = eventCfg as EventCfg_RoleDead;

        if (m_eventCfg.bDeadStart)
            LevelMgr.instance.OnRoleDeadStartCallback += OnNpcDeadStart;
        else
            LevelMgr.instance.OnRoleDeadEndCallback += OnNpcDeadEnd;
    }


    public override bool bReach()
    {
        return bReached;
    }
    public void OnNpcDeadStart(Role role)
    {
        if (m_eventCfg.bDeadStart)
            OnRoleDead(role);
    }

    public void OnNpcDeadEnd(Role role)
    {
        if (!m_eventCfg.bDeadStart)
            OnRoleDead(role);
    }

    void OnRoleDead(Role role)
    {
        //监听的是主角并且进入的也是主角
        if (string.IsNullOrEmpty(m_eventCfg.flagId) && role.IsHero)
        {
            bReached = true;
            Room.instance.StartCoroutine(CoOnDead(mFlag));
        }
        //监听的标记与进入角色的标记一致
        else if (!string.IsNullOrEmpty(m_eventCfg.flagId) && role.GetFlag(m_eventCfg.flagId) > 0)
        {
            bReached = true;
            Room.instance.StartCoroutine(CoOnDead(mFlag));
        }
    }

    IEnumerator CoOnDead(string mFlag)
    {
        //yield return new WaitForSeconds(1);
        SceneEventMgr.instance.FireAction(mFlag);
        yield return 0;
    }

    public override void OnRelease()
    {
        if (m_eventCfg.bDeadStart)
            LevelMgr.instance.OnRoleDeadStartCallback -= OnNpcDeadStart;
        else
            LevelMgr.instance.OnRoleDeadEndCallback -= OnNpcDeadEnd;
    }

}
