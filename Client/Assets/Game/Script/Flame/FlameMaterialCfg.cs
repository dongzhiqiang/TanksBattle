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




public class FlameMaterialCfg 
{
    public int id;
    /** 经验*/
    public int exp;
    /** 排序*/
    public int order;

    public static Dictionary<int, FlameMaterialCfg> m_cfgs = new Dictionary<int, FlameMaterialCfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, FlameMaterialCfg>("flame/flameMaterial", "id");
    }
}
