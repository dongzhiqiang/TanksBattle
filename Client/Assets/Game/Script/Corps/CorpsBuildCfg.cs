using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CorpsBuildConfig
{
    public int id;
    public string name;
    public int openVipLv;
    public int dailyNum;
    public string cost;
    public int contri;
    public int corpsConstr;
    public string nameImg;
}
public class CorpsBuildCfg
{
    public static Dictionary<int, CorpsBuildConfig> m_cfgs = new Dictionary<int, CorpsBuildConfig>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, CorpsBuildConfig>("corps/corpsBuild", "id");
    }

    public static CorpsBuildConfig Get(int id)
    {
        return m_cfgs.Get(id);
    }

}
