using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelRewardCfg
{
    public int id;

    public int level;

    public string itemId;

    public string itemNum;

    public string description;

    public static Dictionary<int, LevelRewardCfg> m_cfgs = new Dictionary<int, LevelRewardCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, LevelRewardCfg>("opActivity/levelReward", "id");
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


}
