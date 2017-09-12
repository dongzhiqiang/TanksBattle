#region Header
/**
 * 名称：SkillLvRateCfg
 
 * 日期：2016.4.5
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class SkillLvRateCfg 
{
    public string id;
    public int lv;
    public float rate;
    


    public static Dictionary<string, Dictionary<int, SkillLvRateCfg>> m_cfgs = new Dictionary<string, Dictionary<int, SkillLvRateCfg>>();
    public static void Init()
    {
        m_cfgs.Clear();
        
        List<SkillLvRateCfg> l = Csv.CsvUtil.Load< SkillLvRateCfg>("systemSkill/skillLvRate");
        foreach (SkillLvRateCfg cfg in l)
        {
            m_cfgs.GetNewIfNo(cfg.id)[cfg.lv] = cfg;
        }
    }

    public static SkillLvRateCfg Get(string id, int lv)
    {
        Dictionary<int, SkillLvRateCfg> d = m_cfgs.Get(id);
        if (d == null)
        {
            Debuger.LogError("技能的等级系数表找不到配置:{0} {1}", id, lv);
            return null;
        }
        SkillLvRateCfg cfg = d.Get(lv);
        if (cfg == null)
        {
            Debuger.LogError("技能的等级系数表找不到配置:{0} {1}", id, lv);
            return null;
        }
        return cfg;
    }

}
