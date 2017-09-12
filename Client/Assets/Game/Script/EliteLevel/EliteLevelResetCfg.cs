using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class EliteLevelResetCfg
{
    public int count;
    public int costDiamond;



    public static Dictionary<int, EliteLevelResetCfg> m_cfgs = new Dictionary<int, EliteLevelResetCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, EliteLevelResetCfg>("activity/eliteLevelReset", "count");
    }
}