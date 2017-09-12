using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExchangeShopCfg
{
    public int id;

    public string  name;

    public long refreshTime;

    public string diamondCost;

    public int moneyId;

    public string groupId;

    public string itemNum;

    public string description;

    public string type;

     public static Dictionary<int, ExchangeShopCfg> m_cfgs = new Dictionary<int, ExchangeShopCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, ExchangeShopCfg>("exchangeShop/exchangeShop", "id");
    }
    public List<int> GetDiamondCostList()
    {
        List<int> diamondCosts = new List<int>();

        if (!string.IsNullOrEmpty(diamondCost))
        {
            string[] diamondCostStr = diamondCost.Split('|');
            for (int i = 0; i < diamondCostStr.Length; i++)
            {
                diamondCosts.Add(int.Parse(diamondCostStr[i]));
            }
        }
        return diamondCosts;
    }
    public List<int> GetItemIdList()
    {
        List<int> groupIds = new List<int>();

        if (!string.IsNullOrEmpty(groupId))
        {
            string[] groupIdStr = groupId.Split('|');
            for (int i = 0; i < groupIdStr.Length; i++)
            {
                groupIds.Add(int.Parse(groupIdStr[i]));
            }
        }
        return groupIds;
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

public enum enShopType
{
    arenaShop = 1, //竞技场商店
    teamShop = 2,  //组队商店
    bossShop = 3,  //boss商店
    corpsShop = 4, //公会商店
    lotteryShop = 5, //高级宝藏商店
    lotteryTopShop = 6, //顶级宝藏商店
    warriorMedalShop = 7, //勇士勋章商店
}
