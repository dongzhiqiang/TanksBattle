using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class ArenaRankCfg  {
    public int rank=1;
    public int rewardId=0;
    public static List<ArenaRankCfg> m_cfg = new List<ArenaRankCfg>();

    public static void Init()
    {
        m_cfg = Csv.CsvUtil.Load<ArenaRankCfg>("activity/arenaRankCfg");
    }

    public static ArenaRankCfg Get(int rank)
    {
        if (rank < 1 || rank >= m_cfg.Count + 1)
            return m_cfg[0];

        return m_cfg[rank-1];
    }
}
