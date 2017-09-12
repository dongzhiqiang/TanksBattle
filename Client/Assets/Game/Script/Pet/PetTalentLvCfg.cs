using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class PetTalentLvCfg
{
    public int id;
    public string upgradeCost;
    //public PropertyTable props;

    public static Dictionary<int, PetTalentLvCfg> m_cfgs = new Dictionary<int, PetTalentLvCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, PetTalentLvCfg>("pet/petTalentLv", "id");
    }

    public static int GetCostGold(int id, int level)
    {
        PetTalentLvCfg cfg = m_cfgs[id + level - 1];
        string[] costs = cfg.upgradeCost.Split(',');
        foreach (string costStr in costs)
        {
            if (costStr != "")
            {
                string[] costPair = costStr.Split('|');
                CostItem costItem = new CostItem();
                if (int.Parse(costPair[0]) == ITEM_ID.GOLD)
                {
                    return int.Parse(costPair[1]);
                }
            }
        }
        return 0;
    }

    public static List<CostItem> GetCost(int id, int level)
    {
        PetTalentLvCfg cfg = m_cfgs[id + level - 1];
        string[] costs = cfg.upgradeCost.Split(',');
        List<CostItem> result = new List<CostItem>();
        foreach (string costStr in costs)
        {
            if (costStr != "")
            {
                string[] costPair = costStr.Split('|');
                CostItem costItem = new CostItem();
                costItem.itemId = int.Parse(costPair[0]);
                costItem.num = int.Parse(costPair[1]);
                result.Add(costItem);
            }
        }
        return result;
    }
}