using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VitalityCfg
{
    public int id;

    public int boxNum;

    public int vitality;

    public string itemId;

    public string itemNum;

    public int level;

    public static Dictionary<int, VitalityCfg> m_cfgs = new Dictionary<int, VitalityCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, VitalityCfg>("task/vitalityReward", "id");
    }    
    public List<int> GetItemIdList()
    {
        List<int> itemIds = new List<int>();

        if (!string.IsNullOrEmpty(itemId))
        {
            string[] itemIdStr = itemId.Split('|');
            for (int i = 0; i < itemIdStr.Length; i++)
            {
                itemIds.Add(int.Parse(itemIdStr[i]));
            }
        }
        return itemIds;
    }

    public List<int> GetItemNumList()
    {
        List<int> itemNums = new List<int>();

        if (!string.IsNullOrEmpty(itemNum))
        {
            string[] itemNumStr = itemNum.Split('|');
            for (int i = 0; i < itemNumStr.Length; i++)
            {
                itemNums.Add(int.Parse(itemNumStr[i]));
            }
        }
        return itemNums;
    }

    public static float GetTotalVitality()
    {
        return m_cfgs[m_cfgs.Count].vitality;
    }

    public static List<VitalityCfg> GetListByLevel(int curLevel)
    {
        List<VitalityCfg> vitalityCfgList = new List<VitalityCfg>();

        List<int> levels = new List<int>();
        for(int i=1;i<m_cfgs.Count+1;++i)
        {
            if(!levels.Contains(m_cfgs[i].level))
            {
                levels.Add(m_cfgs[i].level);
            }
        }
        int level = -1;
        for(int i=0;i<levels.Count;++i)
        {
            if(levels[i]>curLevel)
            {
                level = levels[i - 1];
                break;
            }
        }
        if(level==-1)
        {
            level = levels[levels.Count - 1];
        }
        for (int i = 1; i < m_cfgs.Count + 1; ++i)
        {
            if(m_cfgs[i].level==level)
            {
                vitalityCfgList.Add(m_cfgs[i]);
            }
        }
        return vitalityCfgList;
    }



}
