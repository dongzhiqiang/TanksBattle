using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GoldLevelModeCfg
{
    public class RewardCfg
    {
        public RewardCfg(int itemId, int count)
        {
            this.itemId = itemId;
            this.count = count;
        }

        public int itemId;
        public int count;
    }

    public int mode;
    public string roomId;
    public string monsterId;
    public int openLevel;
    public int maxGold;
    public int basicGold;
    public float goldFactor;
    public int limitTime;
    public int rateBItemID;
    public int rateBItemCount;
    public int rateAItemID;
    public int rateAItemCount;
    public int rateSItemID;
    public int rateSItemCount;
    public int rateSSItemID;
    public int rateSSItemCount;
    public int rateSSSItemID;
    public int rateSSSItemCount;
    

    public static Dictionary<int, GoldLevelModeCfg> m_cfgs = new Dictionary<int, GoldLevelModeCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, GoldLevelModeCfg>("activity/goldLevelMode", "mode");
    }

    public static GoldLevelModeCfg Get(int mode)
    {
        GoldLevelModeCfg modeCfg;
        if (!m_cfgs.TryGetValue(mode, out modeCfg))
            return null;
        else
            return modeCfg;
    }

    public static RewardCfg GetRewardCfg(int mode, string rate)
    {
        var cfg = Get(mode);
        if (cfg == null)
            return new RewardCfg(0, 0);

        switch (rate)
        {
            case "C":
                return new RewardCfg(0, 0);
            case "B":
                return new RewardCfg(cfg.rateBItemID, cfg.rateBItemCount);
            case "A":
                return new RewardCfg(cfg.rateAItemID, cfg.rateAItemCount);
            case "S":
                return new RewardCfg(cfg.rateSItemID, cfg.rateSItemCount);
            case "SS":
                return new RewardCfg(cfg.rateSSItemID, cfg.rateSSItemCount);
            case "SSS":
                return new RewardCfg(cfg.rateSSSItemID, cfg.rateSSSItemCount);
            default:
                return new RewardCfg(0, 0);
        }
    }

    /// <summary>
    /// 获取累计奖励物品数
    /// </summary>
    /// <param name="mode"></param>
    /// <param name="rate"></param>
    /// <returns></returns>
    public static int GetAccRewardNum(int mode, string rate)
    {
        var cfg = Get(mode);
        if (cfg == null)
            return 0;

        var num = 0;
        switch (rate)
        {
            case "SSS":
                num = cfg.rateSSSItemCount + cfg.rateSSItemCount + cfg.rateSItemCount + cfg.rateAItemCount + cfg.rateBItemCount;
                break;
            case "SS":
                num = cfg.rateSSItemCount + cfg.rateSItemCount + cfg.rateAItemCount + cfg.rateBItemCount;
                break;
            case "S":
                num = cfg.rateSItemCount + cfg.rateAItemCount + cfg.rateBItemCount;
                break;
            case "A":
                num = cfg.rateAItemCount + cfg.rateBItemCount;
                break;
            case "B":
                num = cfg.rateBItemCount;
                break;
        }
        return num;
    }
}