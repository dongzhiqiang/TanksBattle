using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class EquipAdvanceCostCfg
{
    public string id;
    public string cost;
    public string costPet;

    public static Dictionary<string, EquipAdvanceCostCfg> m_cfgs = new Dictionary<string, EquipAdvanceCostCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<string, EquipAdvanceCostCfg>("equip/equipAdvanceCost", "id");

    }

    public static List<CostItem> GetCost(string id, bool isPet)
    {
        EquipAdvanceCostCfg cfg = m_cfgs[id];
        string[] costs;
        if(isPet)
        {
            costs = m_cfgs[id].costPet.Split(',');
        }
        else
        {
            costs = m_cfgs[id].cost.Split(',');
        }
        
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