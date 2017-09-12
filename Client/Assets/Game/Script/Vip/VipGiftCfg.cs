using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VipGiftCfg
{
    public int level = 0;
    public int vipGiftValue = 0;
    public int vipGiftDiamondCost = 0;
    public string vipGiftItemId = "";
    public string vipGiftItemNum = "";
    public string description = "";


    public static List<VipGiftCfg> m_cfg = new List<VipGiftCfg>();

    public static void Init()
    {
        m_cfg = Csv.CsvUtil.Load<VipGiftCfg>("vip/vipGift");
    }

    public static VipGiftCfg Get(int vipLevel)
    {
        if (vipLevel < 0 || vipLevel >= m_cfg.Count)
            return m_cfg[0];

        return m_cfg[vipLevel];
    }

    public List<int> GetItemIdList()
    {
        List<int> itemIds = new List<int>();

        if (!string.IsNullOrEmpty(vipGiftItemId))
        {
            string[] itemIdStr = vipGiftItemId.Split('|');
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

        if (!string.IsNullOrEmpty(vipGiftItemNum))
        {
            string[] itemNumStr = vipGiftItemNum.Split('|');
            for (int i = 0; i < itemNumStr.Length; i++)
            {
                itemNums.Add(int.Parse(itemNumStr[i]));
            }
        }
        return itemNums;
    }

   
}
