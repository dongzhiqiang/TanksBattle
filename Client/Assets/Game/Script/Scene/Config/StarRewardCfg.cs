using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StarRewardCfg
{
    #region Fields
    public string id = "";
    public int[][] normalReward;
    public int[][] specialReward;
    #endregion

    //这里暂时用列表 RoomNode 表里也是按int类型填的
    public static List<StarRewardCfg> mStarRewardList = new List<StarRewardCfg>();
    public static void Init()
    {
        mStarRewardList = Csv.CsvUtil.Load<StarRewardCfg>("room/starReward");
    }

    public static StarRewardCfg Get(string nodeId)
    {
        foreach (StarRewardCfg cfg in mStarRewardList)
        {
            if (cfg.id == nodeId)
                return cfg;
        }
        return null;
    }

}
