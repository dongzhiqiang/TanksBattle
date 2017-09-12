using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrowthTaskStageCfg
{
    public int id;

    public int minLevel;

    public int maxLevel;

    public string name;

    public static Dictionary<int, GrowthTaskStageCfg> m_cfgs = new Dictionary<int, GrowthTaskStageCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, GrowthTaskStageCfg>("task/growthTaskStage", "id");
    }
    
}
