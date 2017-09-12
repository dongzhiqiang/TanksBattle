using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FriendMaxCfg
{
    public int level;
    public int maxFriend;

    public static Dictionary<int, FriendMaxCfg> m_cfg = new Dictionary<int, FriendMaxCfg>();

    public static void Init()
    {
        m_cfg = Csv.CsvUtil.Load<int, FriendMaxCfg>("friend/friendMax", "level");
    }

    public static FriendMaxCfg Get(int level)
    {
        FriendMaxCfg cfg;
        if (m_cfg.TryGetValue(level, out cfg))
            return cfg;
        else
        {
            Debug.LogError("找不到对应的等级配置");
            return null;
        }
    }

}
