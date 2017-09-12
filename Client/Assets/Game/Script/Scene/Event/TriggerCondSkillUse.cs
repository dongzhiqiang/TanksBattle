using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriggerCondSkillUse : SceneTrigger
{

    public RoomConditionCfg m_conditionCfg;
    public bool bAchieve;

    int m_observer;
    int m_count;

    string[] m_skillIds = { };
    public override void Init(RoomConditionCfg cfg)
    {
        m_conditionCfg = cfg;
        if (!string.IsNullOrEmpty(m_conditionCfg.stringValue1))
            m_skillIds = m_conditionCfg.stringValue1.Split('|'); ;

        LevelMgr.instance.OnRoleBornCallback += OnNpcBorn;
    }

    public override void Start()
    {
        base.Start();
        if (m_conditionCfg.intValue1 == 0)
            bAchieve = true;
        else
            bAchieve = false;


    }

    public override bool bReach()
    {
        return bAchieve;
    }
    public override void OnRelease()
    {
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
        LevelMgr.instance.OnRoleBornCallback -= OnNpcBorn;
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

    void OnNpcBorn(Role role)
    {

        if (role != null && role.IsHero && role.State == Role.enState.alive)
        {
            if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
            m_observer = role.Add(MSG_ROLE.SKILL, OnUseSkill);
        }
    }

    void OnUseSkill(object p)
    {
        if (m_skillIds.Length <= 0)
        {
            Debuger.LogError("通关条件配错 技能id 条件id ： " + m_conditionCfg.id);
            bAchieve = false;
            return;
        }

        Skill curSkill = (Skill)p;
        for (int i = 0; i < m_skillIds.Length; i++)
        {
            if (curSkill.Cfg.skillId == m_skillIds[i])
            {
                m_count++;
                break;
            }
        }

        if (m_count <= m_conditionCfg.intValue1)
            bAchieve = true;
        else
            bAchieve = false;
    }
}