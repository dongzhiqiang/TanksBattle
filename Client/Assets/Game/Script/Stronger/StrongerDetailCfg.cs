using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class StrongerDetailCfg
{
    public int id;
    public int strongerId;
    public string icon;
    public int quality;
    public string name;
    public string description;
    public int star;
    public string type;

    public static List<StrongerDetailCfg> m_cfgs = new List<StrongerDetailCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<StrongerDetailCfg>("stronger/strongerDetail");
    }

    public static List<StrongerDetailCfg> GetStrongerDetailListByStrongerId(int strongerId)
    {
        List<StrongerDetailCfg> strongerDetailList = new List<StrongerDetailCfg>();
        for(int i=0;i<m_cfgs.Count;++i)
        {
            if (m_cfgs[i].strongerId == strongerId&&StrongerMgr.instance.IsStrongerOpen(m_cfgs[i].type))
                strongerDetailList.Add(m_cfgs[i]);
        }
        return strongerDetailList;
    }
}
