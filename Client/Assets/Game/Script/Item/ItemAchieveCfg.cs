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




public class ItemAchieveCfg 
{
    public int id;
    public int type;
    public string param;
    public string text;

    public static Dictionary<int, ItemAchieveCfg> m_cfgs = new Dictionary<int, ItemAchieveCfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, ItemAchieveCfg>("item/itemAchieve", "id");
    }
}
