using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WarTriedBaseConfig
{
    public int dailyNum;  //每日可以挑战的次数
    public string taskWeight;   //试炼任务权重
    public int freeRefresh;  //免费刷新次数
    public string rule;       //规则
}

public class WarTriedBaseCfg
{
    static List<WarTriedBaseConfig> m_cfgs = new List<WarTriedBaseConfig>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<WarTriedBaseConfig>("warriorsTried/base");
    }

    public static WarTriedBaseConfig Get()
    {
        return m_cfgs[0];
    }
}
