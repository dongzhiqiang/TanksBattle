using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriggerCondRoleDeadAlive : SceneTrigger
{

    public RoomConditionCfg m_conditionCfg;
    public bool bAchieve;

    bool isAwaysAlive = false;  //是否是总是存活的类型

    string[] m_roleIds = { };
    string[] m_groupIds = { };
    public override void Init(RoomConditionCfg cfg)
    {
        m_conditionCfg = cfg;

        if (!string.IsNullOrEmpty(m_conditionCfg.stringValue1))
            m_roleIds = m_conditionCfg.stringValue1.Split('|');
        if (!string.IsNullOrEmpty(m_conditionCfg.stringValue2))
            m_groupIds = m_conditionCfg.stringValue2.Split('|');
        LevelMgr.instance.OnRoleDeadCallback += OnNpcDead;
        LevelMgr.instance.OnRoleBornCallback += OnNpcBorn;

    }

    public override void Start()
    {
        base.Start();
        if (m_conditionCfg.intValue1 == 0)
        {
            isAwaysAlive = false;
            bAchieve = false;
        }
        else if (m_conditionCfg.intValue1 == 1)
        {
            isAwaysAlive = true;
            bAchieve = true;
        }

    }

    public override bool bReach()
    {
        return bAchieve;
    }
    public override void OnRelease()
    {
        LevelMgr.instance.OnRoleDeadCallback -= OnNpcDead;
        LevelMgr.instance.OnRoleBornCallback -= OnNpcBorn;
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

    void OnNpcBorn(Role role)
    {
        if (role == null)
            return;

        if (m_roleIds.Length <= 0 && m_groupIds.Length > 0)   //用刷新点确定的怪物
        {
            for (int i = 0; i < m_groupIds.Length; i++)
            {
                if (role.GetFlag(m_groupIds[i]) > 0)
                {
                    if (isAwaysAlive)
                        bAchieve = true;
                    else
                        bAchieve = false;
                    break;
                }
            }
        }
        else if (m_roleIds.Length > 0 && m_groupIds.Length <= 0)   //用roleId确定的怪物
        {
            for (int i = 0; i < m_roleIds.Length; i++)
            {
                if (role.Cfg.id == m_roleIds[i])
                {
                    if (isAwaysAlive)
                        bAchieve = true;
                    else
                        bAchieve = false;
                    break;
                }

            }
        }
    }
    void OnNpcDead(Role role)
    {
        if (role == null)
            return;

        if (m_roleIds.Length <= 0 && m_groupIds.Length > 0)   //用刷新点确定的怪物
        {
            for (int i = 0; i < m_groupIds.Length; i++)
            {
                if (role.GetFlag(m_groupIds[i]) > 0)
                {
                    if (isAwaysAlive)
                        bAchieve = false;
                    else
                        bAchieve = true;
                    break;
                }
            }
        }
        else if (m_roleIds.Length > 0 && m_groupIds.Length <= 0)   //用roleId确定的怪物
        {
            for (int i = 0; i < m_roleIds.Length; i++)
            {
                if (role.Cfg.id == m_roleIds[i])
                {
                    if (isAwaysAlive)
                        bAchieve = false;
                    else
                        bAchieve = true;
                    break;
                }
            }
        }
    }
}
