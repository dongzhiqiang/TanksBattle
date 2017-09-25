#region Header
/**
 * 名称：道具属性
 * 日期：2015.11.24
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum enEquipPos
{
    minNormal   = 0,    //为了编程方便，定一个最小值枚举
    shoulder    = 0,    //护肩
    hand        = 1,    //护手
    waist       = 2,    //护腰
    boots       = 3,    //护靴
    amulet      = 4,    //护身符
    ring        = 5,    //神戒
    maxNormal   = 5,    //为了编程方便，定一个最大值枚举
    minWeapon   = 6,    //为了编程方便，定一个最小值枚举
    weapon1     = 6,    //混沌之刃
    weapon2     = 7,    //宙斯之剑
    weapon3     = 8,    //野蛮之锤
    weapon4     = 9,    //斯巴达武装
    maxWeapon   = 9,    //为了编程方便，定一个最大值枚举
    equipCount = 10,   //装备数量
}


public class EquipCfg 
{
    public int id;
    /** 名称*/
    public string name;
    /** 装备位*/
    public enEquipPos posIndex;
    /** 状态ID*/
    public int[] stateId;
    /** 属性ID*/
    public string attributeId;
    /** 属性分配*/
    public string attributeAllocateId;
    /** 允许出售*/
    public bool isSell;
    /** 价格*/
    public string priceId;
    /** 星级*/
    public int star;
    /** 觉醒后装备ID*/
    public int rouseEquipId;
    /** 觉醒消耗ID*/
    public string rouseCostId;
    /** 武器ID*/
    public int weaponId=0;
    /** 图标*/
    public string icon;
    /** 觉醒描述*/
    public string rouseDescription;
    /** 战斗力初值*/
    public float power;
    /** 战斗力倍数*/
    public float powerRate;

    public static Dictionary<int, EquipCfg> m_cfgs = new Dictionary<int, EquipCfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, EquipCfg>("equip/equip", "id");
    }
}
