using UnityEngine;
using System.Collections;

public class TriggerCondContinueHit : SceneTrigger
{

    public RoomConditionCfg m_conditionCfg;
    public bool bAchieve;

    int m_count;
    UILevelAreaJoystick m_joyStick;

    public override void Init(RoomConditionCfg cfg)
    {
        m_conditionCfg = cfg;
    }

    public override void Start()
    {
        base.Start();
        bAchieve = false;

        m_joyStick = UIMgr.instance.Get<UILevel>().Get<UILevelAreaJoystick>();
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
        string desc = string.Format(m_conditionCfg.desc, m_count);
        if (bAchieve)
            desc = string.Format("<color=green>{0}</color>", desc);
        return desc;
    }

    public override RoomConditionCfg GetConditionCfg()
    {
        return m_conditionCfg;
    }

    public override void Update()
    {
        if (bAchieve)
            return;

        m_count = m_joyStick.MaxHitNum;
        if (m_count >= m_conditionCfg.intValue1)
            bAchieve = true;
    }
}
