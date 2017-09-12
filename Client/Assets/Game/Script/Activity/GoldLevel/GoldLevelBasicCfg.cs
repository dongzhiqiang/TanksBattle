using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GoldLevelBasicCfg
{
    public int dayMaxCnt;
    public int coolDown;
    public int flyGoldGap;
    public int flyGoldShowNum;
    public int rateBHP;
    public int rateAHP;
    public int rateSHP;
    public int rateSSHP;
    public int rateSSSHP;
    public int rateCBuf;
    public int rateBBuf;
    public int rateABuf;
    public int rateSBuf;
    public int rateSSBuf;
    public int rateSSSBuf;

    public static List<GoldLevelBasicCfg> m_cfgs = new List<GoldLevelBasicCfg>();

    public static GoldLevelBasicCfg Get()
    {
        return m_cfgs[0];
    }

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<GoldLevelBasicCfg>("activity/goldLevelBasic");
    }

    public static string GetRate(int curHP, int hpMax)
    {
        if (hpMax <= 0)
            return "C";

        var ratio = Mathf.Clamp((hpMax - curHP) * 100.0f / hpMax, 0, 100);
        var cfg = m_cfgs[0];
        if (ratio < cfg.rateBHP)
            return "C";
        if (ratio < cfg.rateAHP)
            return "B";
        if (ratio < cfg.rateSHP)
            return "A";
        if (ratio < cfg.rateSSHP)
            return "S";
        if (ratio < cfg.rateSSSHP)
            return "SS";
        else
            return "SSS";
    }

    public static int GetBufId(string rate)
    {
        var cfg = m_cfgs[0];
        switch (rate)
        {
            case "C":
                return cfg.rateCBuf;
            case "B":
                return cfg.rateBBuf;
            case "A":
                return cfg.rateABuf;
            case "S":
                return cfg.rateSBuf;
            case "SS":
                return cfg.rateSSBuf;
            case "SSS":
                return cfg.rateSSSBuf;
            default:
                return cfg.rateCBuf;
        }
    }
}