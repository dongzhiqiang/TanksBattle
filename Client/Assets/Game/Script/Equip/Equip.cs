using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Equip
{
    #region Fields
    int m_equipId;
    int m_level;
    int m_advLv;
    Role m_owner;
    #endregion

    #region Properties
    public int EquipId { get { return m_equipId; } set { m_equipId = value; } }
    public int Level { get { return m_level; } set { m_level = value; } }
    public int AdvLv { get { return m_advLv; } set { m_advLv = value; } }
    public EquipCfg Cfg { get{
        EquipCfg cfg = EquipCfg.m_cfgs.Get(m_equipId);
        if (cfg == null)
        {
            Debuger.LogError("找不到装备id:{0}", m_equipId);
            return null;
        }
        return cfg; 
    }}

    static PropertyTable temp = new PropertyTable();
    
    public Role Owner { get { return m_owner; } set { m_owner = value; } }
    public bool CanOperate
    {
        get
        {
            return !IsNotEquipedWeapon() && !IsLockedWeapon() && (CanUpgrade() || CanAdvance() || CanRouse());
        }
    }
    #endregion

    public static Equip CreateFromVo(EquipVo vo)
    {
        Equip equip = new Equip();
        equip.LoadFromVo(vo);
        return equip;
    }

    public void LoadFromVo(EquipVo vo)
    {
        m_equipId = vo.equipId;
        m_level = vo.level;
        m_advLv = vo.advLv;
    }

    public bool IsWeapon() // 这个是判断装备是否武器用的，不要改！！！
    {
        EquipCfg equipCfg = EquipCfg.m_cfgs[m_equipId];
        return equipCfg.posIndex <= enEquipPos.maxWeapon && equipCfg.posIndex >= enEquipPos.minWeapon;
    }

    public bool IsNotEquipedWeapon()
    {
        EquipCfg equipCfg = EquipCfg.m_cfgs[m_equipId];
        return IsWeapon() && (m_owner.WeaponPart != null && m_owner.WeaponPart.CurWeapon != null && m_owner.WeaponPart.CurWeapon.Equip != this);
    }


    public bool CanUpgrade()
    {
        EquipCfg equipCfg = EquipCfg.m_cfgs[m_equipId];
        if (m_level >= m_owner.PropPart.GetInt(enProp.level))
        {
            return false;
        }
        EquipAdvanceRateCfg advCfg = EquipAdvanceRateCfg.m_cfgs[m_advLv];
        if (m_level >= advCfg.maxLv)
        {
            return false;
        }
        // check items
        List<CostItem> costItems = EquipUpgradeCostCfg.GetCost((int)(equipCfg.posIndex) + "_" + (m_level));
        int needItemId;
        if (!RoleMgr.instance.Hero.ItemsPart.CanCost(costItems, out needItemId))
        {
            return false;
        }
        return true;
    }

    public bool CanAdvance()
    {
        EquipCfg equipCfg = EquipCfg.m_cfgs[m_equipId];
        EquipAdvanceRateCfg advCfg = EquipAdvanceRateCfg.m_cfgs[m_advLv];
        if (m_level < advCfg.needLv)
        {
            return false;
        }
        // check items
        List<CostItem> costItems = EquipAdvanceCostCfg.GetCost((int)(equipCfg.posIndex) + "_" + (m_advLv), !Owner.IsHero);
        int needItemId;
        if (!RoleMgr.instance.Hero.ItemsPart.CanCost(costItems, out needItemId))
        {
            return false;
        }
        return true;
    }

    public bool CanRouse()
    {
        EquipCfg equipCfg = EquipCfg.m_cfgs[m_equipId];
        if(equipCfg.rouseEquipId==0)
        {
            return false;
        }
        // check items
        List<CostItem> costItems = EquipRouseCostCfg.GetCost(equipCfg.rouseCostId);
        int needItemId;
        if (!RoleMgr.instance.Hero.ItemsPart.CanCost(costItems, out needItemId))
        {
            return false;
        }
        return true;
    }

    public static void GetEquipBaseProp(PropertyTable props, EquipCfg equipCfg, int level, int advLv, int star)
    {
        PropValueCfg propCfg = PropValueCfg.Get(equipCfg.attributeId);
        EquipAdvanceRateCfg advanceRateCfg = EquipAdvanceRateCfg.m_cfgs[advLv];
        EquipRouseRateCfg rouseRateCfg = EquipRouseRateCfg.m_cfgs[star];
        PropertyTable.Mul(1f + advanceRateCfg.baseRate + rouseRateCfg.baseRate, propCfg.props, props);

        float lvRate = RoleLvPropCfg.Get(level).rate * PropBasicCfg.instance.equipPoint * (1f + advanceRateCfg.lvRate + rouseRateCfg.lvRate);

        PropertyTable.Mul(RoleTypePropCfg.roleTypeProp, PropDistributeCfg.Get(equipCfg.attributeAllocateId).props, temp);
        PropertyTable.Mul(lvRate, temp, temp);
        PropertyTable.Add(props, temp, props);

        float equipPower = equipCfg.power*(1f + advanceRateCfg.baseRate + rouseRateCfg.baseRate);
        equipPower += lvRate;
        equipPower *= PropBasicCfg.instance.powerRate;
        props.SetInt(enProp.power, (int)equipPower);
    }

    public void GetBaseProp(PropertyTable props)
    {
        EquipCfg equipCfg = EquipCfg.m_cfgs[EquipId];
        GetEquipBaseProp(props, equipCfg, Level, AdvLv, equipCfg.star);
    }
    //武器是否未开启 （weaponIdx 0 1 2 3）
    public static bool IsWeaponLocked(int weaponIdx) 
    {
        string msg;
        //武器的开启顺序做了一些调整，不改变部位那么代码就这么处理
        if (weaponIdx == enEquipPos.weapon4 - enEquipPos.minWeapon)  //斯巴达武装
            return false;
        else if (weaponIdx == enEquipPos.weapon1 - enEquipPos.minWeapon)  //刃
            return !SystemMgr.instance.IsEnabled(enSystem.weapon3, out msg);
        else if(weaponIdx == enEquipPos.weapon2 - enEquipPos.minWeapon)  //剑
            return !SystemMgr.instance.IsEnabled(enSystem.weapon1, out msg);
        else                                                                                                               //锤
            return !SystemMgr.instance.IsEnabled(enSystem.weapon2, out msg);

        //   return weaponIdx != 2 && (!SystemMgr.instance.IsEnabled(enSystem.weapon2, out msg) || !SystemMgr.instance.IsEnabled(enSystem.weapon1+ weaponIdx - 1, out msg)); 
    }
 

    public bool IsLockedWeapon()
    {
        return IsWeapon() && IsWeaponLocked(Cfg.posIndex-enEquipPos.minWeapon);
    }
}