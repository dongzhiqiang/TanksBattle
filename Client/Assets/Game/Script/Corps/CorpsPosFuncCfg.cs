using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CorpsPosFuncConfig
{
    public int posLevel;
    public string posName;
    public string posFunc;
}

public class CorpsPosFuncCfg
{
    public static Dictionary<int, CorpsPosFuncConfig> m_cfgs = new Dictionary<int, CorpsPosFuncConfig>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, CorpsPosFuncConfig>("corps/posFunc", "posLevel");
    }

    public static CorpsPosFuncConfig Get(int level)
    {
        return m_cfgs[level];
    }

}
