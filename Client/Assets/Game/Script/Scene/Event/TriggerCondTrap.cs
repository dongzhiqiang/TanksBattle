using UnityEngine;
using System.Collections;

public class TriggerCondTrap : SceneTrigger
{

    public RoomConditionCfg m_conditionCfg;
    public bool bAchieve;

    int observer;
    int observer2;
    int m_count = 0;
    public override void Init(RoomConditionCfg cfg)
    {
        m_conditionCfg = cfg;
        LevelMgr.instance.OnRoleBornCallback += OnNpcBorn;
    }

    public override void Start()
    {
        base.Start();
        bAchieve = true;

    }

    public override bool bReach()
    {
        return bAchieve;
    }
    public override void OnRelease()
    {
        if (observer != EventMgr.Invalid_Id) { EventMgr.Remove(observer); observer = EventMgr.Invalid_Id; }
        if (observer2 != EventMgr.Invalid_Id) { EventMgr.Remove(observer2); observer2 = EventMgr.Invalid_Id; }
        LevelMgr.instance.OnRoleBornCallback -= OnNpcBorn;
    }

    public override string GetDesc()
    {
        string desc = string.Format(m_conditionCfg.desc, m_count);
        if (bAchieve)
            desc = string.Format("<color=green>{0}</color>", desc);
        return desc;
    }

    public override RoomConditionCfg GetConditionCfg()
    {
        return m_conditionCfg;
    }

    void OnNpcBorn(Role role)
    {
        if (role == null)
            return;

        if (role.IsHero)
        {
            if (observer != EventMgr.Invalid_Id) { EventMgr.Remove(observer); observer = EventMgr.Invalid_Id; }
            if (observer2 != EventMgr.Invalid_Id) { EventMgr.Remove(observer2); observer2 = EventMgr.Invalid_Id; }
            observer = role.Add(MSG_ROLE.BUFF_ADD, OnRoleState);
            observer2 = role.Add(MSG_ROLE.BEHIT, OnRoleHit);
        }
    }

    void OnRoleState(object p, object p2, object p3, EventObserver ob)
    {
        if (!bAchieve)  //失败后就不增加数了
            return;

        BuffCfg cfg = (BuffCfg)p;
        Role source = (Role)p2;
        if (cfg.useful == 0 && source.Cfg.roleType == enRoleType.trap)  //0 有害状态 1 有益状态
        {
            m_count++;
            bAchieve = m_count < m_conditionCfg.intValue1;
        }
    }

    void OnRoleHit(object p, object p2, object p3, EventObserver ob)
    {
        if (!bAchieve)  //失败后就不增加数了
            return;

        Role source = (Role)p;
        if (source.Cfg.roleType == enRoleType.trap)
        {
            m_count++;
            bAchieve = m_count < m_conditionCfg.intValue1;
        }
    }
}
