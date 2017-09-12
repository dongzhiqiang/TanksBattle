using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CorpsFuncConfig
{
    public int funcId;
    public string funcName;
}

public class CorpsFuncCfg
{
    public static Dictionary<int, CorpsFuncConfig> m_cfgs = new Dictionary<int, CorpsFuncConfig>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, CorpsFuncConfig>("corps/func", "funcId");
    }

    public static CorpsFuncConfig Get(int id)
    {
        return m_cfgs[id];
    }
}
