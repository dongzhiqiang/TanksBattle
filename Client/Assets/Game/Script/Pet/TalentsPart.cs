#region Header
/**
 * 名称：仇恨部件
 
 * 日期：2015.9.21
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TalentsPart:RolePart
{
    #region Fields
    Dictionary<string, Talent> m_talents;
    public Dictionary<int, Talent> m_buffIdx = new Dictionary<int, Talent>();
    #endregion


    #region Properties
    public override enPart Type { get { return enPart.talents; } }
    public Dictionary<string, Talent> Talents { get { return m_talents; } }
    
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
        if (vo.talents == null)
        {
            return;
        }
        m_talents = new Dictionary<string, Talent>();
        foreach (TalentVo talentVo in vo.talents)
        {
            Talent talent = Talent.Create(talentVo);
            m_talents[talent.talentId] = talent;
        }
        RoleCfg roleCfg = RoleCfg.Get(m_parent.GetString(enProp.roleId));
        for (int i = 0; i < roleCfg.talents.Count; i++)
        {
            string talentId = roleCfg.talents[i];
            if(m_talents.ContainsKey(talentId))
            {
                m_talents[talentId].pos = i;
            }
            else
            {
                Talent talent = new Talent();
                talent.talentId = talentId;
                talent.level = 1;
                talent.pos = i;
                m_talents[talent.talentId] = talent;
            }
        }

        ResetBuffIdx(true);
    }

    
    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
    }

    public override void OnPreLoad()
    {
        RoleCfg roleCfg = RoleCfg.Get(m_parent.GetString(enProp.roleId));
        for (int i = 0; i < roleCfg.talents.Count; i++)
        {
            string talentId = roleCfg.talents[i];
            if (!IsTalentEnabled(talentId))
            {
                continue;
            }
            int level = 1;
            Talent talent;
            if (Talents.TryGetValue(talentId, out talent))
            {
                level = talent.level;
            }
            TalentCfg talentCfg = TalentCfg.m_cfgs[talentId];
            BuffCfg.ProLoad(talentCfg.stateId);
        }
    }

    public override void OnClear()
    {
        m_buffIdx.Clear();
        if (m_talents != null)
            m_talents.Clear();
    }

    public Talent GetTalent(string talentId)
    {
        Talent talent;
        if (m_talents.ContainsKey(talentId))
        {
            talent = m_talents[talentId];
        }
        else
        {
            /*
            talent = new Talent();
            talent.talentId = talentId;
            talent.level = 1;
             */
            Debuger.LogError("Talent没有初始化：" + talentId);
            return null;
        }
        return talent;
    }


    public void AddOrUpdateTalent(Talent talent)
    {
        m_talents[talent.talentId] = talent;
        ResetBuffIdx();
    }

    public void AddOrUpdateTalent(TalentVo talentVo)
    {
        Talent talent;
        if (m_talents.TryGetValue(talentVo.talentId, out talent))
        {
            talent.LoadFromVo(talentVo);
			ResetBuffIdx();
        }
        else
        {
            Debuger.LogError("Talent没有初始化：" + talentVo.talentId);
            talent = Talent.Create(talentVo);
            AddOrUpdateTalent(talent);
        }
        m_parent.Fire(MSG_ROLE.PET_TALENT_CHANGE);
    }

    public Talent GetTalentByBuffId(int buffId)
    {
        
        return m_buffIdx.Get(buffId);
    }

    public bool IsTalentEnabled(string talentId)
    {
        //TalentCfg talentCfg = TalentCfg.m_cfgs[talentId];
        Talent talent = m_talents[talentId];
        TalentPosCfg talentPosCfg = TalentPosCfg.m_cfgs[talent.pos];
        return m_parent.GetInt(enProp.advLv) >= talentPosCfg.needAdvLv;
    }

    public override void OnFreshBaseProp(PropertyTable values, PropertyTable rates)
    {
        //float oldPower = values.GetFloat(enProp.power);
        //float oldPowerRate = rates.GetFloat(enProp.power);
        /*
        RoleCfg roleCfg = RoleCfg.Get(m_parent.GetString(enProp.roleId));
        for (int i = 0; i < roleCfg.talents.Count; i++)
        {
            string talentId = roleCfg.talents[i];
            if (!IsTalentEnabled(talentId))
            {
                continue;
            }
            int level = 1;
            Talent talent;
            if (Talents.TryGetValue(talentId, out talent))
            {
                level = talent.level;
            }
            TalentCfg talentCfg = TalentCfg.m_cfgs[talentId];
            values.AddFloat(enProp.power, talentCfg.power.GetByLv(level), true);
            rates.AddFloat(enProp.power, talentCfg.powerRate.GetByLv(level), true);
        }
         */
        //Debuger.Log("宠物天赋 角色增加战斗力:{0}", values.GetFloat(enProp.power) - oldPower);
        //Debuger.Log("宠物天赋 角色增加战斗力系数:{0}", rates.GetFloat(enProp.power) - oldPowerRate);
    }
    #endregion

    #region Private Methods
    void ResetBuffIdx(bool unFreshBaseProp=false)
    {
        this.PropPart.UnFreshBaseProp = true;
        Dictionary<int, Talent> buffIds = new Dictionary<int, Talent>();
        foreach (Talent talent in m_talents.Values)
        {
            TalentCfg talentCfg = TalentCfg.m_cfgs[talent.talentId];
            buffIds[talentCfg.stateId] = talent;
        }
        
        //删掉
        foreach(var buffId in m_buffIdx.Keys)
        {
			int num = this.BuffPart.RemoveBuffByBuffId (buffId);
			if (num!=1)
			{
				Debuger.LogError("逻辑错误，宠物天赋状态要删除的时候发现数量不为1:{0} {1}", buffId,num);
			}
        }
		m_buffIdx.Clear();

        //加上
        foreach (var pair in buffIds)
        {
			m_buffIdx.Add(pair.Key, pair.Value);
			this.BuffPart.AddBuff(pair.Key);
        }
        
        this.PropPart.UnFreshBaseProp = false;
        if (!unFreshBaseProp)
            this.PropPart.FreshBaseProp();

    }
    #endregion
}
