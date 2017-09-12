using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CheckInRewardCfg
{
    //唯一id
    public int id;
    //月份
    public int month;
    //签到次数
    public int checkInNums;
    //奖励道具id
    public int itemId;
    //道具数量
    public int itemNums;
    //奖励双倍的vip等级
    public int vipLevel;

    public static Dictionary<int, CheckInRewardCfg> m_cfgs = new Dictionary<int, CheckInRewardCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, CheckInRewardCfg>("opActivity/checkInReward", "id");      
    }

    public static int GetLengthByMonth(int month)
    {
        int monthNums = 0;
        for(int i=1;i<m_cfgs.Count+1;i++)
        {
            if(m_cfgs[i].month==month)
            {
                monthNums++;
            }
        }
        return monthNums;
    }

    public static int GetFirstIdByMonth(int month)
    {
        for (int i = 1; i < m_cfgs.Count + 1; i++)
        {
            if (m_cfgs[i].month == month)
            {
                return i;
            }
        }
        return -1;
    }

}
