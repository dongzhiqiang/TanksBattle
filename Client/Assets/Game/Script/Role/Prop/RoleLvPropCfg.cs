#region Header
/**
 * 名称：角色属性等级系数
 
 * 日期：2016.3.7
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class RoleLvPropCfg 
{
    public int lv;
    public float rate;
    public float defRateRole;
    public float defRateMonster;
    public float defRatePet;

    public static Dictionary<int, RoleLvPropCfg> m_cfgs = new Dictionary<int, RoleLvPropCfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, RoleLvPropCfg>("property/roleLvProp", "lv");
        
    }

    public static RoleLvPropCfg Get(int lv)
    {
        RoleLvPropCfg cfg = m_cfgs.Get(lv);
        if (cfg == null)
            Debuger.LogError("找不到对应的角色属性等级系数 lv:{0}", lv);
        return cfg;
    }
}
