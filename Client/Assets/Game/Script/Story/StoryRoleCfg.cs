using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StoryRoleCfg
{

    #region Fields
    public string id = "";      //人物ID
    public string name = "";    //名字
    public string bust = "";    //半身像
    #endregion

    public static Dictionary<string, StoryRoleCfg> m_cfg = new Dictionary<string,StoryRoleCfg>();

    public static void Init()
    {
        m_cfg = Csv.CsvUtil.Load<string, StoryRoleCfg>("story/storyRole", "id");
    }

    public static StoryRoleCfg Get(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        StoryRoleCfg cfg = m_cfg.Get(id);
        if (cfg == null)
            Debuger.LogError("剧情角色表里没有配置角色:{0}", id);
        return cfg;
    }

    
}
