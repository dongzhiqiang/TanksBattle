using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class RankBasicConfig
{
    public int likeCntLimit = 0;
    public int likeRankLimit = 0;       //前多少名才可以被点赞(从1开始)
    public int[][] doLikeReward = null;   //原本是Number[][]，后面转换成{物品ID:数量}


    public static List<RankBasicConfig> m_cfgs = new List<RankBasicConfig>();

    public static RankBasicConfig Get()
    {
        return m_cfgs[0];
    }

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<RankBasicConfig>("rank/rankBasicConfig");
    }
}