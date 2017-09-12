using UnityEngine;
using System.Collections;

public class TriggerCondFinishLevel : SceneTrigger {

    public RoomConditionCfg m_conditionCfg;
    public bool bAchieve;
    int ob;
    public override void Init(RoomConditionCfg cfg)
    {
        m_conditionCfg = cfg;
        ob = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.WIN, OnFinish);
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
        if (ob != EventMgr.Invalid_Id) { EventMgr.Remove(ob); ob = EventMgr.Invalid_Id; }
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

    public void OnFinish()
    {
        bAchieve = true;
        
    }

}
