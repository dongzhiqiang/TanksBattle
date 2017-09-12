using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VipCfg
{
    public int level = 0;
    public int totalRecharge = 0;
    public int staminaBuyNum = 0;
    public int specialLvlResetNum = 0;
    public int arenaBuyNum = 0;
    public int sweepLvlTimes = 0;
    public int arenaFreezeTime = 0;
    public int warriorSweep = 0;
    public string description = "";
  

    public static List<VipCfg> m_cfg = new List<VipCfg>();

    public static void Init()
    {
        m_cfg = Csv.CsvUtil.Load<VipCfg>("vip/vip");
    }

    public static VipCfg Get(int vipLevel)
    {
        if (vipLevel < 0 || vipLevel >= m_cfg.Count)
            return m_cfg[0];

        return m_cfg[vipLevel];
    }
    
    public List<string> GetDescriptionList()
    {
        List<string> descriptions = new List<string>();

        if (!string.IsNullOrEmpty(description))
        {
            string[] descriptionStr = description.Split('|');
            for (int i = 0; i < descriptionStr.Length; i++)
            {
                descriptions.Add(descriptionStr[i]);
            }
        }
        return descriptions;
    }

    public static int GetWarrSweepVipLv()
    {
        for(int i = 0,len = m_cfg.Count; i<len; ++i)
        {
            if (m_cfg[i].warriorSweep == 1)
                return i;
        }
        return -1;
    }
}
