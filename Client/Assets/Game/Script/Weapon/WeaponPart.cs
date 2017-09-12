#region Header
/**
 * 名称：武器部件
 
 * 日期：2016.4.9
 * 描述：武器技能升级、武器技能的铭文升级、武器属性攻击
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class WeaponPart : RolePart
{
    #region Fields
    int m_curWeapon;
    List<Weapon> m_weapons = new List<Weapon>();

    Dictionary<string, WeaponSkill> m_weaponSkills = new Dictionary<string, WeaponSkill>();//技能的索引
    Dictionary<int, WeaponSkillTalent> m_talents = new Dictionary<int, WeaponSkillTalent>();//铭文的索引
    Dictionary<int, Weapon> m_weaponsByWeaponId = new Dictionary<int, Weapon>();
    #endregion


    #region Properties
    public override enPart Type { get { return enPart.weapon; } }

    public int CurWeaponIdx { get { return m_curWeapon; } set { m_curWeapon = value; } }
    
    public Weapon CurWeapon { get { return m_weapons[m_curWeapon]; } }
    
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
        WeaponInfoVo weaponInfo = vo.weapons;
        m_curWeapon = weaponInfo.curWeapon;
        m_weapons.Clear();
        m_weaponSkills.Clear();
        m_talents.Clear();
        m_weaponsByWeaponId.Clear();
        if (weaponInfo.weapons!= null)
            m_weapons.AddRange(weaponInfo.weapons);

        Weapon w;
        WeaponSkill s;
        WeaponSkillTalent t;
        SystemSkillCfg skillCfg;
        HeroTalentCfg talentCfg;
        for (int i = 0, len = (int)enEquipPos.maxWeapon - (int)enEquipPos.minWeapon + 1; i < len; ++i)
        {
            
            if (i >= m_weapons.Count)
            {
                w = new Weapon();
                this.m_weapons.Add(w);
            }
            else
                w = this.m_weapons[i];
            w.Init(this.Parent, i);
            if (w.Cfg != null )
                m_weaponsByWeaponId[w.Cfg.id] = w; 


            for (int j = 0;j< w.skills.Count; ++j)
            {
                s = w.skills[j];
                skillCfg = s.SkillCfg;
                if(skillCfg != null)
                {
                    if(m_weaponSkills.ContainsKey(skillCfg.id))
                    {
                        Debuger.LogError("技能id重复:{0}", skillCfg.id);
                    }
                    m_weaponSkills[skillCfg.id] = s;
                    
                }
                else
                {
                    //Debuger.LogError("武器技能找不到配置,第{0}个武器，第{1}个技能",i,j);
                }

                for(int k =0;k< s.talents.Count;++k)
                {
                    t = s.talents[k];
                    talentCfg = t.Cfg;
                    if (talentCfg != null)
                    {
                        if (m_talents.ContainsKey(talentCfg.id))
                        {
                            Debuger.LogError("铭文，一个铭文只能属于一个技能，铭文id:{0}", talentCfg.id);
                        }
                        m_talents[talentCfg.id] = t;
                        
                    }
                    else
                    {
                        Debuger.LogError("找不到铭文,第{0}个武器，第{1}个技能,第{2}个铭文",i,j,k);
                    }
                }
            }
            
        }

        

    }
    
    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
       
       
    }

    public override void OnClear()
    {
        

        m_weapons.Clear();
        m_weaponSkills.Clear();
    }
    #endregion


    #region Private Methods
    
    #endregion
    
    public Weapon GetWeapon(int idx)
    {
        return m_weapons[idx];
    }
    
    public Weapon GetWeaponByWeaponId(int id)
    {
        return m_weaponsByWeaponId.Get(id);
    }


    public WeaponSkill GetSkill(string skillId)
    {
        return m_weaponSkills.Get(skillId);
    }

    public WeaponSkillTalent GetTalent(int talentId)
    {
        return m_talents.Get(talentId);
    }

    public void InitCheckWeaponTip()
    {
        CheckWeaponTip();
        m_parent.Add(MSG_ROLE.ITEM_CHANGE, OnCheckWeaponTip);
        m_parent.AddPropChange(enProp.level, OnCheckWeaponTip);
        m_parent.AddPropChange(enProp.gold, OnCheckWeaponTip);
        m_parent.Add(MSG_ROLE.WEAPON_CHANGE, OnCheckWeaponTip);
        m_parent.Add(MSG_ROLE.WEAPON_SKILL_CHANGE, OnCheckWeaponTip);
        m_parent.Add(MSG_ROLE.WEAPON_TALENT_CHANGE, OnCheckWeaponTip);
    }

    bool HasWeaponCanOperate()
    {
        Equip equip;
        for (enEquipPos i = enEquipPos.minWeapon; i <= enEquipPos.maxWeapon; ++i)
        {
            equip = m_parent.EquipsPart.GetEquip(i);
            if (equip != null)
            {
                Weapon w = m_parent.WeaponPart.GetWeapon((int)equip.Cfg.posIndex - (int)enEquipPos.minWeapon);
                if (w != null && w.CanOperate() && !equip.IsLockedWeapon() && m_weapons[m_curWeapon] == w)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void CheckWeaponTip()
    {
        SystemMgr.instance.SetTip(enSystem.weapon, HasWeaponCanOperate());
    }

    void OnCheckWeaponTip()
    {
        CheckWeaponTip();
    }
}
