using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CorpsLogCfg
{
    public int id;
    public string logDesc;

    public static Dictionary<int, CorpsLogCfg> m_cfgs = new Dictionary<int, CorpsLogCfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, CorpsLogCfg>("corps/corpsLog", "id");
    }

    public static CorpsLogCfg Get(int id)
    {
        return m_cfgs[id];
    }
}
