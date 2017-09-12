using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class EquipRouseCostCfg
{
    public string id;
    public string cost;

    public static Dictionary<string, EquipRouseCostCfg> m_cfgs = new Dictionary<string, EquipRouseCostCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<string, EquipRouseCostCfg>("equip/equipRouseCost", "id");

    }

    public static List<CostItem> GetCost(string id)
    {
        EquipRouseCostCfg cfg = m_cfgs[id];
        string[] costs = m_cfgs[id].cost.Split(',');
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