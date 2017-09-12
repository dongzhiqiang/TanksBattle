using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class OpActivitiySortCfg
{
    public int id;
    //活动名称
    public string opActivityName;
    //图标
    public string icon;
    //显示位置
    public int location;

    public static Dictionary<int, OpActivitiySortCfg> m_cfgs = new Dictionary<int, OpActivitiySortCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, OpActivitiySortCfg>("opActivity/opActivitiySort", "id");
    }

    public static int GetIdByLocation(int location)
    {
        for(int i=0;i<m_cfgs.Count;i++)
        {
            if(m_cfgs[i].location==location)
            {
                return i;
            }
        }
        return -1;
    }

}
