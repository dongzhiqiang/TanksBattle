#region Header
/**
 * 名称：Weapon
 
 * 日期：2016.4.10
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public class Weapon
{
    public List<WeaponSkill> skills = new List<WeaponSkill>();
    public List<int> elements;

    int idx;//第几个武器
    Role parent;
    List<WeaponSkill> skillsHaveTalent = new List<WeaponSkill>();

    public Equip Equip { get { return this.parent.EquipsPart.GetEquip((enEquipPos)((int)(enEquipPos.minWeapon) + idx)); } }
    public WeaponCfg Cfg { get { return WeaponCfg.Get(this.Equip.Cfg.weaponId); } }
    public Role Parent { get { return this.parent; } }
    public int Idx { get { return idx; } }
    public ElementCfg CurElementCfg { get { return GetElementCfg(0); } }
    public enElement CurElementType { get { return GetElementType(0); } }
    public void Init(Role parent,int idx)
    {
        this.idx = idx;
        this.parent = parent;
        for (int i = 0, len = (int)enSkillPos.max; i < len; ++i)
        {
            WeaponSkill s;
            if (i >= skills.Count)
            {
                s = new WeaponSkill();
                this.skills.Add(s);
            }
            else
                s = this.skills[i];
            s.Init(this, i);
            if (s.TalentCount > 0)
                skillsHaveTalent.Add(s);
        }

    }

    public WeaponSkill GetSkill(int i)
    {
        return this.skills[i];
    }
    
    public List<WeaponSkill> GetSkillsHaveTelent()
    {
        return skillsHaveTalent;
    }

    public ElementCfg GetElementCfg(int idx)
    {
        if (elements == null || elements.Count <= idx)
            return ElementCfg.Get(Cfg.id,1);//取默认的元素
        return ElementCfg.Get(Cfg.id, elements[idx]);
    }

    public enElement GetElementType(int idx)
    {
        if (elements == null || elements.Count <= idx)
            return enElement.fire;
        return (enElement)elements[idx];
    }

    public bool CanOperate()
    {
        return CanUpgradeSkill() || CanUpgradeTalent();
    }

    public bool CanUpgradeSkill()
    {
        int len = (int)enSkillPos.max;

        for (int i = 0; i < len; ++i)
        {
            WeaponSkill skill = GetSkill(i);
            //已经满级
            if (skill.lv >= ConfigValue.GetInt("maxSkillLevel"))
            {
                continue;
            }

            //不能超过角色等级
            if (skill.lv >= RoleMgr.instance.Hero.GetInt(enProp.level))
            {
                continue;
            }

            //钱不够
            var roleSkillCfg = skill.RoleSkillCfg;
            if(roleSkillCfg==null)
            {
                continue;
            }
            List<CostItem> costItems = SkillLvCostCfg.GetCost(roleSkillCfg.levelCostId, skill.lv);
            int needItemId;
            if (!RoleMgr.instance.Hero.ItemsPart.CanCost(costItems, out needItemId))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    public bool CanUpgradeTalent()
    {
        List<WeaponSkill> l = GetSkillsHaveTelent();
        for (int i = 0; i < l.Count; ++i)
        {
            WeaponSkill skill = l[i];
            for (int j = 0; j < skill.TalentCount; ++j)
            {
                WeaponSkillTalent talent = skill.GetTalent(j);
                //已经满级
                if (talent.lv >= ConfigValue.GetInt("maxTalentLevel"))
                {
                    continue;
                }

                //钱不够
                List<CostItem> costItems = SkillLvCostCfg.GetCost(talent.Cfg.levelCostId, talent.lv);
                int needItemId;
                if (!RoleMgr.instance.Hero.ItemsPart.CanCost(costItems, out needItemId))
                {
                    continue;
                }

                return true;
            }
        }
        return false;
    }
}
