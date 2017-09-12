#region Header
/**
 * 名称：WeaponSkill
 
 * 日期：2016.4.10
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public class WeaponSkill
{
    public int lv = 1;
    public List<WeaponSkillTalent> talents = new List<WeaponSkillTalent>();

    int idx;//第几个技能
    Weapon parent;//所属武器
    RoleSystemSkillCfg roleSkillCfg;
    SystemSkillCfg skillCfg;

    public RoleSystemSkillCfg RoleSkillCfg{get{return roleSkillCfg;}}
    public SystemSkillCfg SkillCfg    {get { return skillCfg;}}
    public Weapon Parent{get { return this.parent; } }
    public int Idx { get { return this.idx; } }
    public int TalentCount { get { return roleSkillCfg != null && roleSkillCfg.talent != null ? roleSkillCfg.talent.Length : 0; } }

    public void Init(Weapon parent,int idx)
    {
        this.parent = parent;
        this.idx = idx;

        roleSkillCfg = null;
        skillCfg = null;
        WeaponCfg weaponCfg = parent.Cfg;
        if (weaponCfg != null)
        {
            string skillId = weaponCfg.GetSkillId((enSkillPos)idx);
            if (!string.IsNullOrEmpty(skillId))
            {
                roleSkillCfg = RoleSystemSkillCfg.Get(this.parent.Parent.Cfg.id, skillId);
                skillCfg = SystemSkillCfg.Get(this.parent.Parent.Cfg.id, skillId);
            }
        }

        if(roleSkillCfg != null)
        {
            for (int i = 0, len = (int)TalentCount; i < len; ++i)
            {
                WeaponSkillTalent t;
                if (i >= talents.Count)
                {
                    t = new WeaponSkillTalent();
                    this.talents.Add(t);
                }
                else
                    t = this.talents[i];
                t.Init(this, i);
            }
        }
        
    }
    
    //返回能连击几下和下一个解锁的等级
    public void GetComboLimit(out int comboLimit,out int limitLv)
    {
        comboLimit = -1;
        limitLv = -1;
        if (roleSkillCfg == null)
            return;
        for (int i = 0; i < roleSkillCfg.comboLimit.Length; ++i)
        {
            if (this.lv < roleSkillCfg.comboLimit[i])
            {
                comboLimit = i + 1;
                limitLv = roleSkillCfg.comboLimit[i];
                return;
            }
        }       
    }

    public WeaponSkillTalent GetTalent(int idx)
    {
        return talents[idx];
    }

}
