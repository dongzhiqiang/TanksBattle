using UnityEngine;
using System.Collections;

public class TriggerCondHurtNum : SceneTrigger
{

    public RoomConditionCfg m_conditionCfg;
    public bool bAchieve;

    int ob;
    int ob2;
    int hurtNum = 0;
    public override void Init(RoomConditionCfg cfg)
    {
        m_conditionCfg = cfg;

        ob = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.ROLEENTER, OnRoleEnter);
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
        if (ob != EventMgr.Invalid_Id) { EventMgr.Remove(ob); ob = EventMgr.Invalid_Id; }
        if (ob2 != EventMgr.Invalid_Id) { EventMgr.Remove(ob2); ob2 = EventMgr.Invalid_Id; }

    }

    public override string GetDesc()
    {
        string desc = string.Format(m_conditionCfg.desc, hurtNum);
        if (bAchieve)
            desc = string.Format("<color=green>{0}</color>", desc);
        return desc;
    }

    public override RoomConditionCfg GetConditionCfg()
    {
        return m_conditionCfg;
    }

    public void OnRoleEnter(object param)
    {
        Role role = param as Role;
        if (role == null)
            return ;

        if (role.Cfg.id == m_conditionCfg.stringValue1)
            ob2 = role.Add(MSG_ROLE.BEHIT, OnRoleHurt);
    }

    public void OnRoleHurt()
    {
        if (!bAchieve)  //失败后就不增加数了
            return;
        hurtNum++;
        bAchieve = hurtNum < m_conditionCfg.intValue1;
        
    }
}