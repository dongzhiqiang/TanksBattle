using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class HadesLevelModeCfg
{
    public int mode;
    public string roomId;
    public int openLevel;
    public string bossId;
    public string waveFlag;


    public static Dictionary<int, HadesLevelModeCfg> m_cfgs = new Dictionary<int, HadesLevelModeCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, HadesLevelModeCfg>("activity/hadesLevelMode", "mode");
    }
}