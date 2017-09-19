using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriggerCondPetAlive : SceneTrigger
{

    public RoomConditionCfg m_conditionCfg;
    public bool bAchieve;
    public override void Init(RoomConditionCfg cfg)
    {
        m_conditionCfg = cfg;
    }

    public override void Start()
    {
        base.Start();
        bAchieve = true;

        LevelMgr.instance.OnRoleDeadCallback += OnNpcDead;
    }

    public override bool bReach()
    {
        return bAchieve;
    }
    public override void OnRelease()
    {
        LevelMgr.instance.OnRoleDeadCallback -= OnNpcDead;
    }

    public override string GetDesc()
    {
        string desc = m_conditionCfg.desc;
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
        Role hero = RoleMgr.instance.Hero;
        if (hero == null || hero.State != Role.enState.alive)
        {
            bAchieve = false;
            return;
        }
        
    }
}
