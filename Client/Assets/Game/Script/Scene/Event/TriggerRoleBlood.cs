using UnityEngine;
using System.Collections;

public class TriggerRoleBlood : SceneTrigger
{
    public EventCfg_RoleBlood m_eventCfg;

    bool bReached = false;
    bool bTriggerAgain = true;

    public override void Init(EventCfg eventCfg)
    {
        m_eventCfg = eventCfg as EventCfg_RoleBlood;

        LevelMgr.instance.OnRoleBornCallback += OnRoleEnter;
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
            role.AddPropChange(enProp.hp, OnHpChanged);
            role.AddPropChange(enProp.hpMax, OnHpChanged);
        }
        //监听的标记与进入角色的标记一致
        else if (!string.IsNullOrEmpty(m_eventCfg.flagId) && role.GetFlag(m_eventCfg.flagId) > 0)
        {
            role.AddPropChange(enProp.hp, OnHpChanged);
            role.AddPropChange(enProp.hpMax, OnHpChanged);
        }
    }


    public override void OnRelease()
    {
        LevelMgr.instance.OnRoleDeadCallback -= OnRoleEnter;
    }

    public void OnHpChanged(object param, object param2, object param3, EventObserver observer)
    {
        //if (bReached)
        //    return;

        Role role = observer.GetParent<Role>();
        float f = role.GetInt(enProp.hp) / role.GetFloat(enProp.hpMax);
        int percent = (int)(f * 100);
        //检测触发
        if (bTriggerAgain)
        {
            if (m_eventCfg.bUnder)  //血量低于百分比 就达成
            {
                if (percent <= m_eventCfg.percent)
                {
                    bReached = true;
                    bTriggerAgain = false;
                    SceneEventMgr.instance.FireAction(mFlag);
                    return;
                }
                else
                    bReached = false;
            }
            else
            {
                if (percent > m_eventCfg.percent)
                {
                    bReached = true;
                    bTriggerAgain = false;
                    SceneEventMgr.instance.FireAction(mFlag);
                    return;
                }
                else
                    bReached = false;
            }
        }
        else
        {
            //是否可再次触发
            if (m_eventCfg.bUnder)
            {
                if (percent > m_eventCfg.percent)
                {
                    bTriggerAgain = true;
                    bReached = false;
                }
            }
            else
            {
                if (percent <= m_eventCfg.percent)
                {
                    bTriggerAgain = true;
                    bReached = false;
                }
            }
        }
    }

}