using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class LotteryPreview
{
    public int showId;
    public int showPoolId;
    public int objectType;
    public string objectId;
    public int objectCnt;

    public static Dictionary<int, LotteryPreview> m_cfgs = new Dictionary<int, LotteryPreview>();

    public static Dictionary<int, List<LotteryPreview>> m_cfgsByPoolId = new Dictionary<int, List<LotteryPreview>>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, LotteryPreview>("opActivity/lotteryPreview", "showId");
        m_cfgsByPoolId.Clear();
        foreach (var v in m_cfgs.Values)
        {
            var lst = m_cfgsByPoolId.GetNewIfNo(v.showPoolId);
            lst.Add(v);
        }
    }

    public static LotteryPreview Get(int showId)
    {
        LotteryPreview cfg;
        m_cfgs.TryGetValue(showId, out cfg);
        return cfg;
    }

    public static List<LotteryPreview> GetByPoolId(int showPoolId)
    {
        List<LotteryPreview> list;
        m_cfgsByPoolId.TryGetValue(showPoolId, out list);
        return list;
    }
}