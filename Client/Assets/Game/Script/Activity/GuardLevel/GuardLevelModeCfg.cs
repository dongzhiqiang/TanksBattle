using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GuardLevelModeCfg
{
    public int mode;
    public string roomId;
    public int openLevel;


    public static Dictionary<int, GuardLevelModeCfg> m_cfgs = new Dictionary<int, GuardLevelModeCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, GuardLevelModeCfg>("activity/guardLevelMode", "mode");
    }
}