using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class VenusLevelBasicCfg
{
    public int openTime1;
    public int closeTime1;
    public int openTime2;
    public int closeTime2;
    public string roomId;

    public static List<VenusLevelBasicCfg> m_cfgs = new List<VenusLevelBasicCfg>();

    public static VenusLevelBasicCfg Get()
    {
        return m_cfgs[0];
    }

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<VenusLevelBasicCfg>("activity/venusLevelBasic");
    }
}