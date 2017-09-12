using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class PetAdvanceCostCfg
{
    public string id;
    public string cost;

    public static Dictionary<string, PetAdvanceCostCfg> m_cfgs = new Dictionary<string, PetAdvanceCostCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<string, PetAdvanceCostCfg>("pet/petAdvanceCost", "id");

    }

    public static List<CostItem> GetCost(string id, int advanceLevel)
    {
        PetAdvanceCostCfg cfg = m_cfgs[id + "_" + advanceLevel];
        string[] costs = cfg.cost.Split(',');
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