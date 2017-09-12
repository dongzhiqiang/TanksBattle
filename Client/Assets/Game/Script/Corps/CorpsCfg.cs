using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CorpsConfig
{
    //等级
    public int level;
    //人数上限
    public int maxMember;
    //升级所需要的建设度
    public int upValue;
}
public class CorpsCfg
{
    static Dictionary<int, CorpsConfig> m_cfgs = new Dictionary<int, CorpsConfig>();
    //公会开放的最高等级
    static int topLevel;
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, CorpsConfig>("corps/corps", "level");
        topLevel = m_cfgs.Count;
    }

    public static CorpsConfig Get(int level)
    {
        return m_cfgs.Get(level);
    }
    //获取公会最高等级
    public static int GetCorpsTopLevel()
    {
        return topLevel;
    }
}
