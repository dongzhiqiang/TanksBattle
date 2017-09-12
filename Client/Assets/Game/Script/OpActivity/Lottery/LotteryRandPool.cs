using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class LotteryRandPool
{
    public int randId;
    public int randPoolId;
    public int objectType;
    public string objectId;
    public int count;
    public int basicWeight;
    public int addedWeight;
    public int turnType;
    public int broadcast;

    public static Dictionary<int, LotteryRandPool> m_cfgs = new Dictionary<int, LotteryRandPool>();

    public static Dictionary<int, List<LotteryRandPool>> m_cfgsByPoolId = new Dictionary<int, List<LotteryRandPool>>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, LotteryRandPool>("opActivity/lotteryRandPool", "randId");
        m_cfgsByPoolId.Clear();
        foreach (var v in m_cfgs.Values)
        {
            var lst = m_cfgsByPoolId.GetNewIfNo(v.randPoolId);
            lst.Add(v);
        }
    }

    public static LotteryRandPool Get(int randId)
    {
        LotteryRandPool cfg;
        m_cfgs.TryGetValue(randId, out cfg);
        return cfg;
    }

    public static List<LotteryRandPool> GetByPoolId(int randPoolId)
    {
        List<LotteryRandPool> list;
        m_cfgsByPoolId.TryGetValue(randPoolId, out list);
        return list;
    }
}