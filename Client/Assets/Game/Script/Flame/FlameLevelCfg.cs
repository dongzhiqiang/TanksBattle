#region Header
/**
 * 名称：RoleSystemSkillCfg
 
 * 日期：2016.4.5
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class FlameLevelCfg 
{
    public int level;
    public int id;
    public string fx;
    public string attributeId;
    public int exp;
    public float power;
    public float powerRate;


    public static Dictionary<int, Dictionary<int, FlameLevelCfg>> s_cfgs = new Dictionary<int, Dictionary<int, FlameLevelCfg>>();
    public static void Init()
    {
        s_cfgs.Clear();
        List<FlameLevelCfg> l = Csv.CsvUtil.Load<FlameLevelCfg>("flame/flameLevel");
        foreach (FlameLevelCfg cfg in l)
        {
            s_cfgs.GetNewIfNo(cfg.id)[cfg.level] = cfg;
        }
    }

    public static FlameLevelCfg Get(int id, int level)
    {
        Dictionary<int, FlameLevelCfg> d = s_cfgs.Get(id);
        if (d == null)
        {
            Debuger.LogError("圣火等级表找不到圣火:{0} {1}",id,level);
            return null;
        }
        FlameLevelCfg cfg;
        if(!d.TryGetValue(level, out cfg))
        {
            return null;
        }
        return cfg;
    }
}
