using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GuardLevelBasicCfg
{
    public int dayMaxCnt;
    public int maxWave;
    public int maxEvaluation;
    public int limitTime;
    public int coolDown;
    public float skillRange;
    public string skillId;

    public static List<GuardLevelBasicCfg> m_cfgs = new List<GuardLevelBasicCfg>();

    public static GuardLevelBasicCfg Get()
    {
        return m_cfgs[0];
    }

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<GuardLevelBasicCfg>("activity/guardLevelBasic");
    }
}