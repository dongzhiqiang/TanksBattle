#region Header
/**
 * 名称：SkillLvCostCfg
 
 * 日期：2015.11.24
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class SkillLvCostCfg 
{
    public int id;
    public List<CostItem> upgradeCost;

    public static Dictionary<int, SkillLvCostCfg> m_cfgs = new Dictionary<int, SkillLvCostCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, SkillLvCostCfg>("systemSkill/skillLvCost", "id");

    }

    public static SkillLvCostCfg Get(int id, int level)
    {
        SkillLvCostCfg cfg = m_cfgs.Get(id + level);
        if(cfg == null)
        {
            Debuger.LogError("技能等级消耗表找不到id:{0}", id + level);
            return null;
        }
        return cfg;
    }

    public static int GetCostGold(int id, int level)
    {
        SkillLvCostCfg cfg = Get(id, level);
        int add = 0;
        for(int i=0;i< cfg.upgradeCost.Count;++i)
        {
            if (cfg.upgradeCost[i].itemId == ITEM_ID.GOLD)
                add += cfg.upgradeCost[i].num;
        }
        
        return add;
    }

    //获取消耗，除了金币、经验等不需要显示的
    public static List<CostItem> GetCostShow(int id, int level)
    {
        List<CostItem> l = new List<CostItem>();
        SkillLvCostCfg cfg = Get(id, level);

        CostItem item;
        for (int i = 0; i < cfg.upgradeCost.Count; ++i)
        {
            item = cfg.upgradeCost[i];
            if (item.itemId != ITEM_ID.GOLD && item.itemId != ITEM_ID.EXP)
            {
                l.Add(item);
            }
                
        }
        return l;
    }

    static List<CostItem> emptyList = new List<CostItem>();
    public static List<CostItem> GetCost(int id, int level)
    {
        var c = Get(id, level);
        if (c == null)
            return emptyList;
        return c.upgradeCost;
    }
}
