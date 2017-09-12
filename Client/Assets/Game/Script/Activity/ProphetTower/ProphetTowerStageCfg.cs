using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProphetTowerStageCfg
{
    #region Fields

    public int id;
    public int rewardId;
    #endregion

    public static Dictionary<int, ProphetTowerStageCfg> m_cfg = new Dictionary<int, ProphetTowerStageCfg>();

    public static void Init()
    {
        m_cfg = Csv.CsvUtil.Load<int, ProphetTowerStageCfg>("activity/prophetTowerStageReward", "id");
    }

    public static ProphetTowerStageCfg Get(int id)
    {
        ProphetTowerStageCfg cfg;
        m_cfg.TryGetValue(id, out cfg);
        if (cfg == null)
            Debuger.LogError("没有找到预言者之塔阶段奖励" + id + "层对应的配置");
        return cfg;
    }
}
