using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class PetUpgradeCostCfg
{
    public string id;
    public int exp;

    public static Dictionary<string, PetUpgradeCostCfg> m_cfgs = new Dictionary<string, PetUpgradeCostCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<string, PetUpgradeCostCfg>("pet/petUpgradeCost", "id");

    }

    public static int GetCostExp(string id, int level)
    {
        PetUpgradeCostCfg cfg = m_cfgs[id + "_" + level];
        return cfg.exp;
    }
}