using UnityEngine;
using System.Collections;

public class TriggerRoleEnter : SceneTrigger
{
    public EventCfg_RoleEnter m_eventCfg;

    int ob;
    bool bReached = false;

    public override void Init(EventCfg eventCfg)
    {
        m_eventCfg = eventCfg as EventCfg_RoleEnter;

        ob = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.ROLEENTER, OnRoleEnter);
    }


    public override bool bReach()
    {
        return bReached;
    }


    public void OnRoleEnter(object param)
    {
        Role role = param as Role;
        if (role == null)
            return;

        //监听的是主角并且进入的也是主角
        if (string.IsNullOrEmpty(m_eventCfg.flagId) && role.IsHero)
        {
            bReached = true;
            SceneEventMgr.instance.FireAction(mFlag);
        }
        //监听的标记与进入角色的标记一致
        else if (!string.IsNullOrEmpty(m_eventCfg.flagId) && role.GetFlag(m_eventCfg.flagId) > 0)
        {
            bReached = true;
            SceneEventMgr.instance.FireAction(mFlag);
        }
    }

    public override void OnRelease()
    {
        if (ob != EventMgr.Invalid_Id) { EventMgr.Remove(ob); ob = EventMgr.Invalid_Id; }
    }
}
