#region Header
/**
 * 名称：元素属性配置
 
 * 日期：2016.4.25
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public enum enElement
{

    none=0,//无属性
    fire=1,//火
    ice=2,//冰
    thunder=3,//雷
    dark=4,//冥
    max =4,
    inherit = 5,//继承，这个用于看伤害事件是覆盖还是使用角色的元素属性
}

public class ElementCfg
{
    public int weaponId;
    public int elementId;
    public string name = "";
    public string desc = "";
    public List<float> eventPercents;
    public List<string> events;

    public static string[] Element_Icons = new string[] {"", "ui_pet_anniu_04", "ui_pet_anniu_02", "ui_pet_anniu_01", "ui_pet_anniu_03" };//火冰雷冥对应的图标名
    public static string[] Element_Names = new string[] { "无", "火", "冰", "雷", "冥","不改" };
    public static Dictionary<int, Dictionary<int, ElementCfg>> s_cfgs = new Dictionary<int, Dictionary<int, ElementCfg>>();

    public enElement ElementType { get { return (enElement)elementId; } }

    public static void Init()
    {
        s_cfgs.Clear();
        List<ElementCfg> l = Csv.CsvUtil.Load<ElementCfg>("role/element");
        foreach (ElementCfg cfg in l)
        {
            s_cfgs.GetNewIfNo(cfg.weaponId)[cfg.elementId] = cfg;
        }
    }

    public static void PreLoad()
    {
        foreach (var v1 in s_cfgs.Values)
        {
            foreach (var elementCfg in v1.Values)
            {
                foreach (var eventGroupId in elementCfg.events)
                {
                    SkillEventGroupCfg.PreLoad(eventGroupId);
                }
                
            }
        }
    }

    public static ElementCfg Get(int weaponId,int elementId)
    {
        Dictionary<int, ElementCfg> d = s_cfgs.Get(weaponId);
        if (d == null)
        {
            Debuger.LogError("元素属性表找不到配置:{0} {1}", weaponId, elementId);
            return null;
        }
        ElementCfg cfg = d.Get(elementId);
        if (cfg == null)
        {
            Debuger.LogError("元素属性表找不到配置:{0} {1}", weaponId, elementId);
            return null;
        }
        return cfg;
    }

    public static ElementCfg GetRoleElementCfg(Role source)
    {
        RoleCfg srcCfg = source.Cfg;
        if (srcCfg.RolePropType != enRolePropType.role)
            return null;

        WeaponCfg weaponCfg = source.CombatPart.FightWeapon;
        if (weaponCfg == null)
            return null;

        WeaponPart weaponPart = source.WeaponPart;
        if (weaponPart == null)
            return null;

        Weapon weapon = weaponPart.GetWeaponByWeaponId(weaponCfg.id);
        if (weapon == null)
            return null;
        return weapon.CurElementCfg;
    }

    //如果不是主角那么从角色配置读取，如果是主角，那么从当前武器读取
    public static enElement GetRoleElement(Role source)
    {
        RoleCfg srcCfg = source.Cfg;
        if (srcCfg.RolePropType != enRolePropType.role)
            return srcCfg.element;

        WeaponCfg weaponCfg = source.CombatPart.FightWeapon;
        if (weaponCfg == null)
            return srcCfg.element;

        WeaponPart weaponPart = source.WeaponPart;
        if (weaponPart == null)
            return srcCfg.element;

        Weapon weapon = weaponPart.GetWeaponByWeaponId(weaponCfg.id);
        if (weapon == null)
            return srcCfg.element;
        return weapon.CurElementType;
    }

    public string GetEventGroup()
    {
        if (eventPercents == null)
            return string.Empty;

        float rand = UnityEngine.Random.value;
        float v = 0;
        for (int i =0;i<eventPercents.Count&& i<events.Count; ++i)
        {
            v += eventPercents[i];
            if (rand <= v)
                return events[i];
        }

        return string.Empty;
    }
}
