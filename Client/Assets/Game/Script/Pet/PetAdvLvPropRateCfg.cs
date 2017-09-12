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




public class PetAdvLvPropRateCfg 
{
    public int advLv;
    public int needLv;
    public int quality;
    public int qualityLevel;
    public int maxTalentLv;
    public float baseRate;
    public float lvRate;

    public static Dictionary<int, PetAdvLvPropRateCfg> m_cfgs = new Dictionary<int, PetAdvLvPropRateCfg>();
    public static void Init()

    {
        m_cfgs = Csv.CsvUtil.Load<int, PetAdvLvPropRateCfg>("pet/petAdvLvPropRate", "advLv");
    }

    public static PetAdvLvPropRateCfg Get(int advLv)
    {
        PetAdvLvPropRateCfg cfg = m_cfgs.Get(advLv);
        if (cfg == null)
        {
            Debuger.LogError("找不到宠物等阶系数,等阶:{0}", advLv);
            return advLv !=1?Get(1):null;
            
        }

        return cfg;
    }
}
