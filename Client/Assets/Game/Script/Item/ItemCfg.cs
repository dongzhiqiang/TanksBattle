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




public class ItemCfg 
{
    public int id;
    /** 名称*/
    public string name;
    /** 子类型*/
    public int type;
    /** 品质*/
    public int quality;
    /** 品质等级*/
    public int qualityLevel;
    /** 状态ID*/
    public string stateId;
    /** 允许出售*/
    public bool isSell;
    /** 价格*/
    public string priceId;
    /** 使用获得值*/
    public string useValue1;
    /** 大图标*/
    public string icon;
    /** 小图标*/
    public string iconSmall;
    /** 描述*/
    public string description;
    /** 获取途径*/
    public int[] achieve;

    public static Dictionary<int, ItemCfg> m_cfgs = new Dictionary<int, ItemCfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, ItemCfg>("item/item", "id");
    }

    public static ItemCfg Get(int itemId)
    {
        ItemCfg cfg;
        m_cfgs.TryGetValue(itemId, out cfg);
        return cfg;
    }
}
