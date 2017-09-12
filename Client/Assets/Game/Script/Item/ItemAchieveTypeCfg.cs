#region Header
/**
 * 名称：道具类型属性
 
 * 日期：2015.11.24
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class ItemAchieveTypeCfg 
{
    public int id;
    public string name;
    public enSystem systemId;

    public static Dictionary<int, ItemAchieveTypeCfg> m_cfgs = new Dictionary<int, ItemAchieveTypeCfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, ItemAchieveTypeCfg>("item/itemAchieveType", "id");
    }
}
