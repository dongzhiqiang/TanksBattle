using UnityEngine;
using System.Collections;

public class TriggerCondBloodLimit : SceneTrigger
{

    public RoomConditionCfg m_conditionCfg;
    public bool bAchieve;
    int ob2;
    int ob3;
    int m_percent;
    public override void Init(RoomConditionCfg cfg)
    {
        m_conditionCfg = cfg;
        m_percent = 100;
        LevelMgr.instance.OnRoleBornCallback += OnRoleEnter;
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
        if (ob2 != EventMgr.Invalid_Id) { EventMgr.Remove(ob2); ob2 = EventMgr.Invalid_Id; }
        if (ob3 != EventMgr.Invalid_Id) { EventMgr.Remove(ob3); ob3 = EventMgr.Invalid_Id; }

        LevelMgr.instance.OnRoleBornCallback -= OnRoleEnter;
    }
    public override string GetDesc()
    {
        string desc = string.Format(m_conditionCfg.desc, m_percent);
        if (bAchieve)
            desc = string.Format("<color=green>{0}</color>", desc);
        return desc;

    }

    public override RoomConditionCfg GetConditionCfg()
    {
        return m_conditionCfg;
    }

    public void  OnRoleEnter(Role role)
    {
        if (role == null)
            return ;

        if (role.Cfg.id == m_conditionCfg.stringValue1)
        {
            ob2 = role.AddPropChange(enProp.hp, OnHpChanged);
            ob3 = role.AddPropChange(enProp.hpMax, OnHpChanged);
        }
        
    }

    public void OnHpChanged(object param,object param2,object param3, EventObserver observer)
    {
        if (!bAchieve)  //失败后就不检测了
            return ;

        Role role = observer.GetParent<Role>();
        float f = role.GetInt(enProp.hp) / role.GetFloat(enProp.hpMax);
        m_percent = (int)(f * 100);
        if (m_percent > m_conditionCfg.intValue1)
            bAchieve = true;
        else
            bAchieve = false;
    }

    

}