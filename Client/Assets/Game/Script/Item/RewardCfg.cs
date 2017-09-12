using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class RewardItem
{
    public int itemId;
    public int itemNum;
}


public class RewardCfg
{
    public int id;
    public string rewards1;
    public string rewards2;
    public string rewards3;
    public string rewards4;
    public string rewards5;
    public string rewards6;
    public string rewardsDefinite;

    public static Dictionary<int, RewardCfg> m_cfgs = new Dictionary<int, RewardCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, RewardCfg>("reward/reward", "id");

    }

    public static List<RewardItem> GetRewardsDefinite(int id)
    {
        RewardCfg cfg = m_cfgs[id];
        List<RewardItem> result = new List<RewardItem>();
        if(cfg.rewardsDefinite==null)
        {
            return result;
        }
        string[] rewards = cfg.rewardsDefinite.Split(',');
        foreach (string rewardStr in rewards)
        {
            if (rewardStr != "")
            {
                string[] rewardPair = rewardStr.Split('|');
                RewardItem rewardItem = new RewardItem();
                rewardItem.itemId = int.Parse(rewardPair[0]);
                rewardItem.itemNum = int.Parse(rewardPair[1]);
                result.Add(rewardItem);
            }
        }
        return result;
    }
}