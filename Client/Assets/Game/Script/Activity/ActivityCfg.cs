using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ActivityCfg
{
    public int id;
    public string name;
    public int order;
    public string ruleIntro;

    public static Dictionary<int, ActivityCfg> m_cfgs = new Dictionary<int, ActivityCfg>();

    public static ActivityCfg Get(enSystem systemId)
    {
        int id = (int)systemId;
        ActivityCfg result;
        if(!m_cfgs.TryGetValue(id, out result))
        {
            Debuger.LogError("战争学院表没有活动id:" + id);
        }
        return result;
    }

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, ActivityCfg>("activity/activity", "id");
    }
}