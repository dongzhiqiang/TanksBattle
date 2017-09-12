using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class StrongerProgressCfg
{
    public int id;
    public int progress;
    public string text;

    public static List<StrongerProgressCfg> m_cfgs = new List<StrongerProgressCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<StrongerProgressCfg>("stronger/strongerProgress");
    }

    public static string GetTextByProgress(int progress)
    {
        for(int i=0;i<m_cfgs.Count;++i)
        {
            if(i< m_cfgs.Count-1)
            {
                if (progress >= m_cfgs[i].progress && progress < m_cfgs[i + 1].progress)
                    return m_cfgs[i].text;
            }
            else
            {
                if (progress == m_cfgs[i].progress)
                    return m_cfgs[i].text;
            }
        }
        Debug.LogError("变强进度值错误");
        return "";
    }
}
