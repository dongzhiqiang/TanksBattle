using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GuardLevelWaveCfg
{
    public int id;
    public int point;
    public int monsterNum;
    public string propRate;

    public static Dictionary<int, GuardLevelWaveCfg> m_cfgs = new Dictionary<int, GuardLevelWaveCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, GuardLevelWaveCfg>("activity/guardLevelWave", "id");
    }
}