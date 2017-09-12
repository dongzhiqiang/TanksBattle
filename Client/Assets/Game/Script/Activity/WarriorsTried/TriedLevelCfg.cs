using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriedLevelConfig
{
    public string roomId;
    public int passId;
} 

public class TriedLevelCfg
{
    static Dictionary<string, TriedLevelConfig> m_cfgs = new Dictionary<string, TriedLevelConfig>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<string, TriedLevelConfig>("warriorsTried/triedLevel", "roomId");
    }

    public static TriedLevelConfig Get(string roomId)
    {
        return m_cfgs.Get(roomId);
    }

}
