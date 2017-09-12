using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriedStarConfig
{
    public int key;
    public int group;
    public int star;
    public int weight;
    public int rewardId;
}
public class TriedStarCfg
{
    static Dictionary<int, TriedStarConfig> m_cfgs = new Dictionary<int, TriedStarConfig>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, TriedStarConfig>("warriorsTried/starTried", "key");  
    }

    public static TriedStarConfig Get(int star)
    {
        return m_cfgs.Get(star);
    }
}
