using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class StaminaBuyCfg
{
    public int staminaBuyNum;
    public int price;


    public static List<StaminaBuyCfg> m_cfg = new List<StaminaBuyCfg>();

    public static void Init()
    {
        m_cfg = Csv.CsvUtil.Load<StaminaBuyCfg>("vip/staminaBuy");
    }

    public static StaminaBuyCfg Get(int arenaBuyNum)
    {
        if (arenaBuyNum < 0)
            return m_cfg[0];

        if (arenaBuyNum >= m_cfg.Count)
            return m_cfg[m_cfg.Count - 1];

        return m_cfg[arenaBuyNum];
    }

}
