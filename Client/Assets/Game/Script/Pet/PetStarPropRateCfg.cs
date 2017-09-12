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




public class PetStarPropRateCfg 
{
    public int star;
    public float baseRate;
    public float lvRate;

    public static Dictionary<int, PetStarPropRateCfg> m_cfgs = new Dictionary<int, PetStarPropRateCfg>();
    public static void Init()

    {
        m_cfgs = Csv.CsvUtil.Load<int, PetStarPropRateCfg>("pet/petStarPropRate", "star");
    }

    public static PetStarPropRateCfg Get(int star)
    {
        PetStarPropRateCfg cfg = m_cfgs.Get(star);
        if (cfg == null)
        {
            Debuger.LogError("找不到宠物星级系数,星级:{0}", star);
            return star != 1 ? Get(1) : null;

        }
       
        return cfg;
    }
}
