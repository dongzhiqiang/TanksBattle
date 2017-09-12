using UnityEngine;
using System.Collections;

public class TriggerCondWeaponLimit : SceneTrigger
{

    public RoomConditionCfg m_conditionCfg;
    public bool bAchieve;
    int observer;

    public override void Init(RoomConditionCfg cfg)
    {
        m_conditionCfg = cfg;
        LevelMgr.instance.OnRoleBornCallback += OnRoleEnter;
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
        if (observer != EventMgr.Invalid_Id) { EventMgr.Remove(observer); observer = EventMgr.Invalid_Id; }
        LevelMgr.instance.OnRoleBornCallback -= OnRoleEnter;
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

    void OnRoleEnter(Role role)
    {
        if (role == null)
            return;

        if (role.IsHero)
        {
            if (observer != EventMgr.Invalid_Id) { EventMgr.Remove(observer); observer = EventMgr.Invalid_Id; }
            observer = role.Add(MSG_ROLE.WEAPON_RENDER_CHANGE, OnChangeWeapon);

            CombatPart c = role.CombatPart;
            if (c == null || c.FightWeapon == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(m_conditionCfg.stringValue1))
            {
                Debuger.LogError("通关条件配错 未填武器id 条件id ： " + m_conditionCfg.id);
                bAchieve = false;
                return;
            }

            if (m_conditionCfg.stringValue1.Contains(c.FightWeapon.id.ToString()))
                bAchieve = true;
            else
                bAchieve = false;
        }
    }

    void OnChangeWeapon(object p, object p2, object p3, EventObserver ob)
    {
        if (!bAchieve)
            return;
        Role r = ob.GetParent<Role>();

        CombatPart c = r.CombatPart;
        if (c == null || c.FightWeapon == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(m_conditionCfg.stringValue1))
        {
            Debuger.LogError("通关条件配错 未填武器id 条件id ： " + m_conditionCfg.id);
            bAchieve = false;
            return;
        }

        if (SceneMgr.SceneDebug)
            Debuger.Log("切换武器{0}", c.FightWeapon.name);

        if (m_conditionCfg.stringValue1.Contains(c.FightWeapon.id.ToString()))
            bAchieve = true;
        else
            bAchieve = false;
    }

}
