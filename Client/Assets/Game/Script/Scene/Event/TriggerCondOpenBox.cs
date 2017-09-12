using UnityEngine;
using System.Collections;

public class TriggerCondOpenBox : SceneTrigger
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
        string desc = string.Format(m_conditionCfg.desc);
        if (bAchieve)
            desc = string.Format("<color=green>{0}</color>", desc);
        return desc;
    }

    public override RoomConditionCfg GetConditionCfg()
    {
        return m_conditionCfg;
    }

}