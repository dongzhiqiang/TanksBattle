using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundCfg
{
    public int soundId;
    public int volume;
    public bool needLoop;
    public string path;
    public bool needPreload;
    //3d音效超出这个距离声音开始减小
    public int soundMinDist;
    //3d音效超出这个距离声音不再减小
    public int soundMaxDist;
    //3d音效无限制添加 0:有限制 1：无限制
    public int unMaxLimit;

    public static Dictionary<int, SoundCfg> m_cfgs = new Dictionary<int, SoundCfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, SoundCfg>("sound/sound", "soundId");
        //判断预加载
        foreach(SoundCfg cfg in m_cfgs.Values)
        {
            if (cfg.needPreload)
                SoundMgr.instance.PreLoad(cfg.soundId);
        }
    }

    public static SoundCfg Get(int soundId)
    {
        CheckInit();
        SoundCfg soundCfg;
        if (m_cfgs.TryGetValue(soundId, out soundCfg))
            return soundCfg;
        else
        {
            Debuger.LogError("找不到音效:{0}", soundId);
            return null;
        }
            
    }

    public static void CheckInit()
    {
        if (m_cfgs.Count == 0)
            Init();
    }
}
