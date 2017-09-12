using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class EliteLevelBasicCfg
{
    public int dayMaxCnt;
    public int costStamina;

    public static List<EliteLevelBasicCfg> m_cfgs = new List<EliteLevelBasicCfg>();

    public static EliteLevelBasicCfg Get()
    {
        return m_cfgs[0];
    }

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<EliteLevelBasicCfg>("activity/eliteLevelBasic");
    }
}