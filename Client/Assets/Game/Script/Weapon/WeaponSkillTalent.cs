#region Header
/**
 * 名称：WeaponSkillTalent
 
 * 日期：201x.xx.xx
 * 描述：新建类的时候建议用这个模板
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public class WeaponSkillTalent
{
    public int lv = 0;

    int idx;//第几个铭文
    WeaponSkill parent;//所属技能
    HeroTalentCfg cfg;

    public HeroTalentCfg Cfg { get{return cfg;}}
    public WeaponSkill Parent{get { return this.parent; }}
    public int Idx { get { return this.idx; } }


    public void Init(WeaponSkill parent, int idx)
    {
        this.parent = parent;
        this.idx = idx;

        cfg = null;
        var roleSkillCfg= parent.RoleSkillCfg;
        if (roleSkillCfg != null)
        {
            cfg = HeroTalentCfg.Get(roleSkillCfg.talent[idx]);
        }

    }
}
