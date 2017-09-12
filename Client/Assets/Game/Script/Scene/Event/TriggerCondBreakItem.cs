using UnityEngine;
using System.Collections;

public class TriggerCondBreakItem : SceneTrigger
{

    public RoomConditionCfg m_conditionCfg;
    public bool bAchieve;
    public int m_count;
    public override void Init(RoomConditionCfg cfg)
    {
        m_conditionCfg = cfg;
        LevelMgr.instance.OnRoleDeadCallback += OnNpcDead;
    }

    public override void Start()
    {
        base.Start();
        bAchieve = false;

    }

    public override bool bReach()
    {
        return bAchieve;
    }
    public override void OnRelease()
    {
    }

    public override string GetDesc()
    {
        string desc;
        if (m_conditionCfg.desc.Contains("{0}"))
            desc = string.Format(m_conditionCfg.desc, m_count);
        else
            desc = m_conditionCfg.desc;

        if (bAchieve)
            desc = string.Format("<color=green>{0}</color>", desc);
        return desc;
    }

    public override RoomConditionCfg GetConditionCfg()
    {
        return m_conditionCfg;
    }

    void OnNpcDead(Role role)
    {
        if (bAchieve)
            return;

        if (role != null)
        {
            if (role.Cfg.id == m_conditionCfg.stringValue1)
                m_count++;
        }
        if (m_count >= m_conditionCfg.intValue1)
            bAchieve = true;

    }
}