using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class EquipUpgradeCostCfg
{
    public string id;
    public string cost;

    public static Dictionary<string, EquipUpgradeCostCfg> m_cfgs = new Dictionary<string, EquipUpgradeCostCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<string, EquipUpgradeCostCfg>("equip/equipUpgradeCost", "id");

    }

    public static List<CostItem> GetCost(string id)
    {
        EquipUpgradeCostCfg cfg = m_cfgs[id];
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