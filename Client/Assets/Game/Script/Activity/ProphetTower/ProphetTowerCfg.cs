using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProphetTowerCfg
{
    #region Fields

    public int id;
    public string roomId;
    public int rewardId;
    public int stage;
    public int[] range;
    public int isBoss;
    #endregion

    public static Dictionary<int, ProphetTowerCfg> m_cfg = new Dictionary<int, ProphetTowerCfg>();

    public static void Init()
    {
        m_cfg = Csv.CsvUtil.Load<int, ProphetTowerCfg>("activity/prophetTower", "id");
    }

    public static ProphetTowerCfg Get(int id)
    {
        ProphetTowerCfg cfg;
        m_cfg.TryGetValue(id ,out cfg);
        if (cfg == null)
            Debuger.LogError("没有找到预言者之塔" + id + "层对应的配置");
        return cfg;
    }
}
