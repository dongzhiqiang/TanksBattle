using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class VenusLevelButtonCfg
{
    public float time;
    public int num;
    public string fx;

    public static List<VenusLevelButtonCfg> m_cfgs = new List<VenusLevelButtonCfg>();

    public static VenusLevelButtonCfg Get(int index)
    {
        return m_cfgs[index];
    }

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<VenusLevelButtonCfg>("activity/venusLevelButton");
    }
}