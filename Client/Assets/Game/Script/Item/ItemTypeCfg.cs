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




public class ItemTypeCfg 
{
    public int type;
    public int sort;

    public static Dictionary<int, ItemTypeCfg> m_cfgs = new Dictionary<int, ItemTypeCfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, ItemTypeCfg>("item/itemType", "type");
    }
}
