#region Header
/**
 * 名称：副本固定属性
 
 * 日期：2015.11.24
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class EquipAdvanceRateCfg 
{
    public int id;
    public int needLv;
    public int maxLv;
    public int quality;
    public int qualityLv;
    public float baseRate;
    public float lvRate;

    public static Dictionary<int, EquipAdvanceRateCfg> m_cfgs = new Dictionary<int, EquipAdvanceRateCfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, EquipAdvanceRateCfg>("equip/equipAdvanceRate", "id");
    }

}
