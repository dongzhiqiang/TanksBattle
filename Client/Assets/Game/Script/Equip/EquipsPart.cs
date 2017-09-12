#region Header
/**
 * 名称：物品部件
 
 * 日期：2015.9.21
 * 描述：背包和装备
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class EquipsPart:RolePart
{
    #region Fields
    static PropertyTable tempProp = new PropertyTable();

    string m_equipRoleGUID;
    Dictionary<enEquipPos, Equip> m_equips;
    enEquipPos m_currentWeapon;
    #endregion


    #region Properties
    public override enPart Type { get { return enPart.equips; } }
    public Dictionary<enEquipPos, Equip> Equips { get { return m_equips; } }
    public string EquipRoleGUID { get { return m_equipRoleGUID; } }
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
        //Debuger.Log("装备部件初始化");
        m_equipRoleGUID = vo.props["guid"].String;
        
        if( vo.equips != null )
        {
            m_equips = new Dictionary<enEquipPos, Equip>();
            foreach (EquipVo equipVo in vo.equips)
            {
                if(equipVo == null)
                    continue;

                Equip equip = new Equip();
                equip.LoadFromVo(equipVo);
                equip.Owner = m_parent;

                EquipCfg cfg = equip.Cfg;
                m_equips.Add(cfg.posIndex, equip);
            }
        }


    }

    public void FreshWeaponState(WeaponCfg weaponCfg)
    {
        //Debuger.Log("fresh weapon 1:" + weaponCfg.id);
        for(enEquipPos pos=enEquipPos.minWeapon; pos<=enEquipPos.maxWeapon; pos++)
        {
            Equip equip = m_equips[pos];
            if (equip == null)
            {
                continue;
            }
            EquipCfg equipCfg = EquipCfg.m_cfgs[equip.EquipId];
            if (equipCfg.stateId != null)
            {
                foreach (int state in equipCfg.stateId)
                {
                    BuffPart.RemoveBuffByBuffId(state);
                }
                if (equipCfg.weaponId == weaponCfg.id)
                {
                    foreach (int state in equipCfg.stateId)
                    {
                        BuffPart.AddBuff(state);
                    }
                    //Debuger.Log("fresh weapon:" + equipCfg.id);
                }
            }
            else
            {
                //Debuger.Log("fresh weapon none state");

            }
 
        }
    }
    
    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
        //取当前武器
        Equip weapon = null;
        if (WeaponPart != null)//有武器部件，那么是主角
            weapon = WeaponPart.CurWeapon.Equip;
        else//宠物，那么取第一个武器
            weapon = GetEquip(enEquipPos.weapon1);
        foreach (Equip equip in m_equips.Values)
        {
            if (equip == null)
            {
                continue;
            }
            EquipCfg equipCfg = EquipCfg.m_cfgs[equip.EquipId];
            if (equipCfg.posIndex >= enEquipPos.minWeapon && equipCfg.posIndex <= enEquipPos.maxWeapon)
            {
                if (equip != weapon)
                    continue;
            }
            if (equipCfg.stateId != null )
            { 
                foreach(int state in equipCfg.stateId)
                {
                    BuffPart.AddBuff(state);
                }
            }
        }

        
        //添加事件
        if(WeaponPart != null)
        {
            //Debuger.Log("add eqeve");
            m_parent.Add(MSG_ROLE.WEAPON_CHANGE, OnWeaponChange);
        }
    }

    void OnWeaponChange(object weaponCfg)
    {
        FreshWeaponState((WeaponCfg)weaponCfg);
    }

    public override void OnPreLoad()
    {
        foreach (Equip equip in m_equips.Values)
        {
            if (equip == null)
            {
                continue;
            }
            EquipCfg equipCfg = EquipCfg.m_cfgs[equip.EquipId];
            if (equipCfg.stateId != null)
            {
                foreach (int state in equipCfg.stateId)
                {
                    BuffCfg.ProLoad(state);
                }
            }
        }
    }

    public override void OnFreshBaseProp(PropertyTable values, PropertyTable rates)
    {
        //Debuger.Log("添加装备的属性");
        //float oldHpMax = values.GetFloat(enProp.hpMax);
        //float oldPower = values.GetFloat(enProp.power);

        //取当前武器
        Equip weapon = null;
        if (WeaponPart != null)//有武器部件，那么是主角
            weapon = WeaponPart.CurWeapon.Equip;
        else//宠物，那么取第一个武器
            weapon = GetEquip(enEquipPos.weapon1);

        foreach (Equip equip in m_equips.Values)
        {
            if(equip == null )
            {
                continue;
            }
            EquipCfg equipCfg = EquipCfg.m_cfgs[equip.EquipId];
            //不是当前武器不加属性
            if (equipCfg.posIndex >= enEquipPos.minWeapon && equipCfg.posIndex <= enEquipPos.maxWeapon)
            {
                if (equip!= weapon)
                    continue;
            }
            equip.GetBaseProp(tempProp);
            PropertyTable.Add(values, tempProp, values);
            //values.SetFloat(enProp.power, values.GetFloat(enProp.power) + tempProp.GetFloat(enProp.power));
            //Debuger.Log("装备id:{0} 角色增加后攻击力:{1}", equip.EquipId, props.GetFloat(enProp.atk));
        }
        //Debuger.Log("装备 角色增加生命值:{0}", values.GetFloat(enProp.hpMax)-oldHpMax);
        //Debuger.Log("装备 角色增加战斗力:{0}", values.GetFloat(enProp.power) - oldPower);
    }

    public bool HasEquipCanOperate()
    {
        foreach(Equip equip in m_equips.Values)
        {
            if(equip.CanOperate)
            {
                return true;
            }
        }
        return false;
    }

    public bool HasEquipCanUpgradeOnce()
    {
        foreach (Equip equip in m_equips.Values)
        {
            if (!equip.IsNotEquipedWeapon() && (equip.CanUpgrade()||equip.CanAdvance()) )
            {
                return true;
            }
        }
        return false;
    }


    public void InitCheckEquipTip()
    {
        //if (m_parent.IsHero != null)
        //{
        CheckEquipTip();
        m_parent.Add(MSG_ROLE.EQUIP_CHANGE, OnCheckEquipTip);
        m_parent.Add(MSG_ROLE.ITEM_CHANGE, OnCheckEquipTip);
        m_parent.AddPropChange(enProp.level, OnCheckEquipTip);
        m_parent.AddPropChange(enProp.gold, OnCheckEquipTip);
        //}
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <returns>如果装备位上没有装备，就返回null</returns>
    public Equip GetEquip(enEquipPos pos)
    {
        Equip equip;
        m_equips.TryGetValue(pos, out equip);
        return equip;
    }
    #endregion


    #region Private Methods
    void CheckEquipTip()
    {
        SystemMgr.instance.SetTip(enSystem.hero, HasEquipCanOperate());
    }

    void OnCheckEquipTip()
    {
        CheckEquipTip();
    }


    #endregion
}
