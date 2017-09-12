using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System;
using System.IO;

public enum StoryType
{
    STORY_TALK,   //对话
    STORY_MOVIE,    //剧情
    STORY_POP,      //气泡
}

[JsonCanInherit]
public class StoryCfg
{
    public StoryType type;
    public int time;
}

public class StoryTalkCfg : StoryCfg
{
    public string roleId;
    public string content;
    public int localIdx;
    public int soundId;
    public StoryTalkCfg()
    {
        type = StoryType.STORY_TALK;
    }

}

public class StoryMovieCfg : StoryCfg
{
    public string movieId = "";
    public string prefab = "";
    public bool isLoop = false;
    public int soundId = 0;
    public StoryMovieCfg()
    {
        type = StoryType.STORY_MOVIE;
    }
}

public class StoryPopCfg : StoryCfg
{
    public string roleId = "";
    public string content = "";

    public StoryPopCfg()
    {
        type = StoryType.STORY_POP;
    }
}

public class StorySaveCfg
{
    public List<StoryCfg> storyList = new List<StoryCfg>();
    public bool canSpeed = true;
    public static Dictionary<string, StorySaveCfg> m_Cfg = new Dictionary<string, StorySaveCfg>();
    public static void Init()
    {
        m_Cfg.Clear();
    }
    
    public static StorySaveCfg GetCfg(string storyId)
    {
        StorySaveCfg cfg;
        if (m_Cfg.TryGetValue(storyId, out cfg))
            return cfg;

        string path = string.Format("story/{0}", storyId);
        cfg = Util.LoadJsonFile<StorySaveCfg>(path);
        if (cfg == null)
        {
            return null;
        }
        m_Cfg[storyId] = cfg;
        return cfg;
    }

    public void SaveCfg(string filename)
    {
        Util.SaveJsonFile(filename, this);
    }
}

