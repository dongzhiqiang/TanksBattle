using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class VenusLevelRewardCfg
{
    public int evaluate;
    public float minPercentage;
    public int soul;
    public int stamina;



    public static Dictionary<int, VenusLevelRewardCfg> m_cfgs = new Dictionary<int, VenusLevelRewardCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, VenusLevelRewardCfg>("activity/venusLevelReward", "evaluate");
        minEvaluate = -1;
        maxEvaluate = -1;
        foreach(int key in m_cfgs.Keys)
        {
            if(minEvaluate == -1)
            {
                minEvaluate = key;
                maxEvaluate = key;
            }
            if(key<minEvaluate)
            {
                minEvaluate = key;
            }
            if (key > maxEvaluate)
            {
                maxEvaluate = key;
            }
        }
    }

    public static int minEvaluate;
    public static int maxEvaluate;
}