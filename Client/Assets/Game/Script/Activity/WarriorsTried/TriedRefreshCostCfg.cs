using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TriedRefreshCostConfig
{
    public int num;
    public int cost;
}
public class TriedRefreshCostCfg
{
    static Dictionary<int, TriedRefreshCostConfig> m_cfgs = new Dictionary<int, TriedRefreshCostConfig>();
    const int TopRefresh = 8;
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, TriedRefreshCostConfig>("warriorsTried/refreshCost", "num");
    }

    public static TriedRefreshCostConfig Get(int num)
    {
        return m_cfgs.Get(num <= TopRefresh ? num : TopRefresh);
    }
}
