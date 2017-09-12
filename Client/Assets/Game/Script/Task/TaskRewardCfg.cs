using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class TaskRewardCfg  {

    public int id;
    //
  //  public int location;

    public string taskName;
    //
    public string description;

    public string icon;

    public string  taskType;
    //
    public string taskField;

    public int taskProp;

    public string taskRewardTime;

    public string itemId;

    public string itemNum;

    public int vitality;  

    public int level;

    public int quality;

    public static Dictionary<int, TaskRewardCfg> m_cfgs = new Dictionary<int, TaskRewardCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, TaskRewardCfg>("task/taskReward", "id");
    }

    public List<string> GetTaskFieldList()
    {
        List<string> taskFields = new List<string>();

        if (!string.IsNullOrEmpty(taskField))
        {
            string[] taskFieldStr = taskField.Split('|');
            for (int i = 0; i < taskFieldStr.Length; i++)
            {
                taskFields.Add(taskFieldStr[i]);
            }
        }
        return taskFields;
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

    public  static List<TaskRewardCfg> GetCfgsByLevelAndVip(int level,int vipLevel)
    {
        List<TaskRewardCfg> taskRewardIds = new List<TaskRewardCfg>();
        for(int i=0;i<m_cfgs.Count;++i)
        {
            if(level>= m_cfgs[i + 1].level)
            {
                enTaskType taskType=enTaskType.normalLv;
                try
                {
                    taskType = (enTaskType)Enum.Parse(typeof(enTaskType), m_cfgs[i + 1].taskType);
                }
                catch(Exception err)
                {
                    continue;
                }
                if (taskType != enTaskType.vip)
                {
                    taskRewardIds.Add(m_cfgs[i + 1]);
                }
                else
                {
                    if(vipLevel >=m_cfgs[i + 1].taskProp - 1)
                    {
                        taskRewardIds.Add(m_cfgs[i + 1]);
                    }
                }
            }
        }       
        return taskRewardIds;
    }


}
