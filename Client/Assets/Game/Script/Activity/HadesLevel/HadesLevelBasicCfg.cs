using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class HadesLevelBasicCfg
{
    public int dayMaxCnt;
    public int maxWave;
    public int maxEvaluation;
    public int limitTime;

    public static List<HadesLevelBasicCfg> m_cfgs = new List<HadesLevelBasicCfg>();

    public static HadesLevelBasicCfg Get()
    {
        return m_cfgs[0];
    }

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<HadesLevelBasicCfg>("activity/hadesLevelBasic");
    }
}