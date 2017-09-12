using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class SweepLevelCfg
{
    public int type;   //0，单次，1，多次
    public int stars;
    public int vipLv;
    public int condOp; //0，前两个条件按或运算，1，前两个条件按与运算
    public string tip; //提示文字

    public static Dictionary<int, SweepLevelCfg> m_cfgs = new Dictionary<int, SweepLevelCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, SweepLevelCfg>("room/sweepLevelCfg", "type");
    }

    public static SweepLevelCfg Get(int type)
    {
        return m_cfgs[type];
    }
}