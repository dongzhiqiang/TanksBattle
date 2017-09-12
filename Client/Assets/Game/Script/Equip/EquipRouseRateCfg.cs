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




public class EquipRouseRateCfg 
{
    public int id;
    public float baseRate;
    public float lvRate;

    public static Dictionary<int, EquipRouseRateCfg> m_cfgs = new Dictionary<int, EquipRouseRateCfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, EquipRouseRateCfg>("equip/equipRouseRate", "id");
    }

}
