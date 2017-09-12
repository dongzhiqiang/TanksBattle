using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrowthTaskCfg
{
    public int id;

    public int stage;

    public string name;

    public string description;

    public string param;

    public string itemId;

    public string itemNum;

    public string type;

    public string prop;

    public string icon;

    public int quality;

    public static Dictionary<int, GrowthTaskCfg> m_cfgs = new Dictionary<int, GrowthTaskCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, GrowthTaskCfg>("task/growthTask", "id");
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

    public List<int> GetParamList()
    {
        List<int> Params = new List<int>();

        if (!string.IsNullOrEmpty(param))
        {
            string[] paramStr = param.Split('|');
            for (int i = 0; i < paramStr.Length; i++)
            {
                Params.Add(int.Parse(paramStr[i]));
            }
        }
        return Params;
    }

    public static List<GrowthTaskCfg> GetGrowthTaskCfgByStage(int stage)
    {
        List<GrowthTaskCfg> growthTaskCfgList = new List<GrowthTaskCfg>();
        for(int i=1;i<m_cfgs.Count+1;++i)
        {
            if(m_cfgs[i].stage==stage)
            {
                growthTaskCfgList.Add(m_cfgs[i]);
            }
        }
        return growthTaskCfgList;
    }


}
