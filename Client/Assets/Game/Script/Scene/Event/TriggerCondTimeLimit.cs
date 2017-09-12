using UnityEngine;
using System.Collections;

public class TriggerCondTimeLimit : SceneTrigger
{

    public RoomConditionCfg m_conditionCfg;
    public bool bAchieve;
    public int second = 0;
    public float startTime = 0;
    public override void Init(RoomConditionCfg cfg)
    {
        m_conditionCfg = cfg;
    }

    public override void Start()
    {
        base.Start();
        bAchieve = true;
        startTime = TimeMgr.instance.logicTime;
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
        string desc = string.Format(m_conditionCfg.desc, second);
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
        if (mRun && bAchieve)   //超过时间就显示失败
        {
            second = (int)(TimeMgr.instance.logicTime - startTime);
            if (second >= m_conditionCfg.intValue1)
            {
                bAchieve = false;
            }
        }
    }


}
