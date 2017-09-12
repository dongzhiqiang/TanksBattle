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


public class PetSkillsPart:RolePart
{
    #region Fields
    Dictionary<string, PetSkill> m_petSkills;
    #endregion


    #region Properties
    public override enPart Type { get { return enPart.petSkills; } }
    public Dictionary<string, PetSkill> PetSkills { get { return m_petSkills; } }
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
        if (vo.petSkills == null)
        {
            return;
        }
        m_petSkills = new Dictionary<string, PetSkill>();
        foreach (PetSkillVo petSkillVo in vo.petSkills)
        {
            PetSkill petSkill = PetSkill.Create(petSkillVo);
            m_petSkills[petSkill.skillId] = petSkill;
        }

    }

    public override void OnFreshBaseProp(PropertyTable values, PropertyTable rates)
    {
        //float oldPowerRate = rates.GetFloat(enProp.power);
        /*
        List<PetSkill> petSkills = new List<PetSkill>();

        PetSkill petSkill = new PetSkill();
        petSkills.Add(GetPetSkill(m_parent.Cfg.atkUpSkill));
        foreach (string skillId in m_parent.Cfg.skills)
        {
            petSkills.Add(GetPetSkill(skillId));
        }

        foreach(PetSkill skill in petSkills)
        {
            if(!IsSkillEnabled(skill.skillId))
            {
                continue;
            }
            RoleSystemSkillCfg roleSkillCfg = RoleSystemSkillCfg.Get(m_parent.GetString(enProp.roleId), skill.skillId);
            rates.AddFloat(enProp.power, roleSkillCfg.powerRate != null ? roleSkillCfg.powerRate.GetByLv(skill.level) : 0, true);
        }
         */
        //Debuger.Log("宠物技能 角色增加战斗力系数:{0}", rates.GetFloat(enProp.power) - oldPowerRate);
    }

    
    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {

    }

    public override void OnClear()
    {
        if (m_petSkills != null)
            m_petSkills.Clear();
    }

    public PetSkill GetPetSkill(string skillId)
    {
        PetSkill petSkill;
        if (m_petSkills.ContainsKey(skillId))
        {
            petSkill = m_petSkills[skillId];
        }
        else
        {
            petSkill = new PetSkill();
            petSkill.skillId = skillId;
            petSkill.level = 1;
        }
        return petSkill;
    }


    public void AddOrUpdatePetSkill(PetSkill petSkill)
    {
        m_petSkills[petSkill.skillId] = petSkill;
    }

    public void AddOrUpdatePetSkill(PetSkillVo petSkillVo)
    {
        PetSkill petSkill;
        if (m_petSkills.TryGetValue(petSkillVo.skillId, out petSkill))
        {
            petSkill.LoadFromVo(petSkillVo);
        }
        else
        {
            petSkill = PetSkill.Create(petSkillVo);
            AddOrUpdatePetSkill(petSkill);
        }
        m_parent.Fire(MSG_ROLE.PET_SKILL_CHANGE);
    }

    public bool IsSkillEnabled(string skillId)
    {
        RoleSystemSkillCfg roleSkillCfg = RoleSystemSkillCfg.Get(m_parent.Cfg.id, skillId);
        if (roleSkillCfg == null)
        {
            Debuger.LogError("判断宠物技能有没有开启的时候发现角色技能表里没有配置:{0} {1}", m_parent.Cfg.id, skillId);
            return true;
        }
        else
        {
            return m_parent.GetInt(enProp.star) >= roleSkillCfg.needPetStar;
        }

        
    }
    #endregion

    #region Private Methods
    
    #endregion
}
