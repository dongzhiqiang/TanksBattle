using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ArenaBuyCfg
{
    public int arenaBuyNum;
    public int price;
 

    public static List<ArenaBuyCfg> m_cfg = new List<ArenaBuyCfg>();

    public static void Init()
    {
        m_cfg = Csv.CsvUtil.Load<ArenaBuyCfg>("vip/arenaBuy");
    }

    public static ArenaBuyCfg Get(int arenaBuyNum)
    {
        if (arenaBuyNum < 0 )
            return m_cfg[0];

        if (arenaBuyNum >= m_cfg.Count)
            return m_cfg[m_cfg.Count - 1];

        return m_cfg[arenaBuyNum];
    }

}
