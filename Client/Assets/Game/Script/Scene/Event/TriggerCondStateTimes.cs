using UnityEngine;
using System.Collections;

public class TriggerCondStateTimes : SceneTrigger
{

    public RoomConditionCfg m_conditionCfg;
    public bool bAchieve;

    int ob;
    int enterNum = 0;
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
        if (ob != EventMgr.Invalid_Id) { EventMgr.Remove(ob); ob = EventMgr.Invalid_Id; }
        LevelMgr.instance.OnRoleBornCallback -= OnNpcBorn;
    }

    public override string GetDesc()
    {
        string desc = string.Format(m_conditionCfg.desc, enterNum);
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
            if (ob != EventMgr.Invalid_Id) { EventMgr.Remove(ob); ob = EventMgr.Invalid_Id; }
            ob = role.Add(MSG_ROLE.BUFF_ADD, OnRoleState);
        }
    }

    void OnRoleState(object p, object p2, object p3, EventObserver ob)
    {
        if (!bAchieve)  //失败后就不增加数了
            return;

        BuffCfg cfg = (BuffCfg)p;
        if (cfg.useful == 0)  //0 有害状态 1 有益状态
        {
            if (SceneMgr.SceneDebug)
                Debuger.Log("主角添加状态{0}", cfg.name);
            enterNum++;
            bAchieve = enterNum < m_conditionCfg.intValue1;
        }
    }
}