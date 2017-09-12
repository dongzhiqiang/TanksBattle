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




public class TreasureLevelCfg 
{
    public int level;
    public int id;
    public string attributeId;
    public int pieceNum;
    public int costGold;
    public float power;
    public float powerRate;
    public string description;
    public string skillId;
    public int skillLevel=1;

    public static Dictionary<int, Dictionary<int, TreasureLevelCfg>> s_cfgs = new Dictionary<int, Dictionary<int, TreasureLevelCfg>>();
    public static void Init()
    {
        s_cfgs.Clear();
        List<TreasureLevelCfg> l = Csv.CsvUtil.Load<TreasureLevelCfg>("treasure/treasureLevel");
        foreach (TreasureLevelCfg cfg in l)
        {
            s_cfgs.GetNewIfNo(cfg.id)[cfg.level] = cfg;
        }
    }

    public static TreasureLevelCfg Get(int id, int level)
    {
        Dictionary<int, TreasureLevelCfg> d = s_cfgs.Get(id);
        if (d == null)
        {
            Debuger.LogError("神器等级表找不到神器:{0} {1}", id, level);
            return null;
        }
        TreasureLevelCfg cfg;
        if(!d.TryGetValue(level, out cfg))
        {
            return null;
        }
        return cfg;
    }
}
