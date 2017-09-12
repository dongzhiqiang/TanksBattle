#region Header

#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TreasureInfo
{
    public string icon;
    public string skillId;
}

public class TreasurePart : RolePart
{
    #region Fields
    List<int> m_battleTreasure = new List<int>();
    Dictionary<int, Treasure> m_treasures = new Dictionary<int, Treasure>();
    Dictionary<string, int> m_skillTreasureMap = new Dictionary<string, int>();
    List<TreasureInfo> m_treasureInfos = new List<TreasureInfo>();
    #endregion


    #region Properties
    public override enPart Type { get { return enPart.treasure; } }
    public Dictionary<int, Treasure> Treasures { get { return m_treasures; } }
    public List<int> BattleTreasures { get { return m_battleTreasure; } }
    public List<TreasureInfo> BattleTreasureInfos { get { return m_treasureInfos; } }
    #endregion




    #region Frame    
    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        
        return true;
    }

    //网络数据初始化
    public override void OnNetInit(FullRoleInfoVo vo)
    {
        if (vo.treasures == null) return;

        TreasureInfoVo treasureInfo = vo.treasures;
        m_battleTreasure = treasureInfo.battleTreasure;
        m_treasures.Clear();


        foreach(TreasureVo treasureVo in vo.treasures.treasures.Values)
        {
            Treasure treasure = Treasure.Create(treasureVo);
            AddOrUpdateTreasure(treasure);
        }

        UpdateSkillTreasureMap();
    }
    
    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
       
       
    }

    public override void OnClear()
    {
        m_battleTreasure.Clear();
        m_treasures.Clear();
    }

    public void UpdateBattleTreasure(List<int> battleTreasure)
    {
        m_battleTreasure = battleTreasure;
        UpdateSkillTreasureMap();
    }

    void AddOrUpdateTreasure(Treasure treasure)
    {

        if (m_treasures == null)
            return;

        m_treasures[treasure.treasureId] = treasure;

    }

    public void AddOrUpdateTreasure(TreasureVo treasureVo)
    {
        if (m_treasures == null)
            return;

        Treasure treasure;
        if (m_treasures.TryGetValue(treasureVo.treasureId, out treasure))
        {
            treasure.LoadFromVo(treasureVo);
        }
        else
        {
            treasure = Treasure.Create(treasureVo);
            AddOrUpdateTreasure(treasure);
        }

        UpdateSkillTreasureMap();

        m_parent.Fire(MSG_ROLE.TREASURE_CHANGE, null);
    }

    public Treasure GetTreasure(int treasureId)
    {
        Treasure result;
        if (m_treasures.TryGetValue(treasureId, out result))
        {
            return result;
        }
        else
        {
            return null;
        }
    }

    public int GetTreasurePos(int treasureId)
    {
        return m_battleTreasure.IndexOf(treasureId);
    }

    public void SetBattle(int treasureId)
    {
        if(m_battleTreasure.Count>=3)
        {
            return;
        }
        if(GetTreasure(treasureId)==null)
        {
            return;
        }
        List<int> battleTreasures = new List<int>();
        battleTreasures.AddRange(m_battleTreasure);
        battleTreasures.Add(treasureId);
        NetMgr.instance.TreasureHandler.SendChangeBattleTreasure(battleTreasures);
    }

    public void CancelBattle(int treasureId)
    {
        int index = m_battleTreasure.IndexOf(treasureId);
        if(index < 0)
        {
            return;
        }

        List<int> battleTreasures = new List<int>();
        battleTreasures.AddRange(m_battleTreasure);
        battleTreasures.Remove(treasureId);
        NetMgr.instance.TreasureHandler.SendChangeBattleTreasure(battleTreasures);
    }

    public override void OnFreshBaseProp(PropertyTable values, PropertyTable rates)
    {
        //float oldHpMax = values.GetFloat(enProp.hpMax);
        //float oldPower = values.GetFloat(enProp.power);
        //float oldPowerRate = rates.GetFloat(enProp.power);
        foreach (Treasure treasure in m_treasures.Values)
        {
            TreasureLevelCfg levelCfg = TreasureLevelCfg.Get(treasure.treasureId, treasure.level);
            if (levelCfg == null)
            {
                continue;
            }
            PropValueCfg valueCfg = PropValueCfg.Get(levelCfg.attributeId);
            if (valueCfg == null)
            {
                continue;
            }
            PropertyTable.Add(values, valueCfg.props, values);
            /*
            values.SetFloat(enProp.power, values.GetFloat(enProp.power) + levelCfg.power);
            rates.SetFloat(enProp.power, rates.GetFloat(enProp.power) + levelCfg.powerRate);
             */
        }
        //Debuger.Log("神器 角色增加生命值:{0}", values.GetFloat(enProp.hpMax)-oldHpMax);
        //Debuger.Log("神器 角色增加战斗力:{0}", values.GetFloat(enProp.power) - oldPower);
        //Debuger.Log("神器 角色增加战斗力系数:{0}", rates.GetFloat(enProp.power) - oldPowerRate);
    }

    public int GetTreasurePower()
    {
        float power = 0;

        foreach (Treasure treasure in m_treasures.Values)
        {
            TreasureLevelCfg levelCfg = TreasureLevelCfg.Get(treasure.treasureId, treasure.level);
            if (levelCfg == null)
            {
                continue;
            }
            power += levelCfg.power;
        }

        return Mathf.FloorToInt(power);
    }

    void UpdateSkillTreasureMap()
    {
        m_skillTreasureMap.Clear();
        m_treasureInfos.Clear();
        for(int i=0; i<m_battleTreasure.Count; i++)
        {
            Treasure treasure = m_treasures[m_battleTreasure[i]];
            TreasureCfg treasureCfg = TreasureCfg.Get(treasure.treasureId);
            TreasureLevelCfg levelCfg = TreasureLevelCfg.Get(treasure.treasureId, treasure.level);
            if (!string.IsNullOrEmpty(levelCfg.skillId))
            {
                TreasureInfo treasureInfo = new TreasureInfo();
                treasureInfo.icon = treasureCfg.icon;
                treasureInfo.skillId = levelCfg.skillId;
                m_treasureInfos.Add(treasureInfo);
                m_skillTreasureMap[levelCfg.skillId] = levelCfg.skillLevel;
            }
                
        }
    }

    public int GetTreasureSkillLevel(string skillId)
    {
        int skillLevel;
        if (m_skillTreasureMap.TryGetValue(skillId, out skillLevel))
        {
            return skillLevel;
        }
        return -1;
    }

    public TreasureInfo GetBattleTreasure(int idx)
    {
        if (idx < 0 || idx >= m_treasureInfos.Count)
            return null;
        return m_treasureInfos[idx];
    }

    public void InitCheckTreasureTip()
    {
        //if (m_parent.IsHero != null)
        //{
        CheckTreasureTip();
        m_parent.Add(MSG_ROLE.ITEM_CHANGE, OnCheckTreausreTip);
        m_parent.Add(MSG_ROLE.TREASURE_CHANGE, OnCheckTreausreTip);
    }

    public bool HasOperateTreausre()
    {
        foreach (TreasureCfg cfg in TreasureCfg.m_cfgs.Values)
        {
            if(Treasure.CanUpgrade(cfg.id))
            {
                return true;
            }
        }
        return false;
    }


    #endregion

    #region Private Methods
    void CheckTreasureTip()
    {
        SystemMgr.instance.SetTip(enSystem.treasure, HasOperateTreausre());
    }

    void OnCheckTreausreTip()
    {
        CheckTreasureTip();
    }

    #endregion


}
