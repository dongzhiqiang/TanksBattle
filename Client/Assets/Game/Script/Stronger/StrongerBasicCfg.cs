using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class StrongerBasicCfg
{
    public int id;
    public string name;
    public int type;

    public static List<StrongerBasicCfg> m_cfgs = new List<StrongerBasicCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<StrongerBasicCfg>("stronger/strongerBasic");
    }  
}
