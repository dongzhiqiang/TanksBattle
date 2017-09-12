using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundUICfg
{
    public string imgName;
    public int soundId;
    public string imgFunc;

    public static Dictionary<string, SoundUICfg> m_cfgs = new Dictionary<string,SoundUICfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<string, SoundUICfg>("sound/UIsound", "imgName");
    }

    public static SoundUICfg Get(string m_name)
    {
        CheckInit();
        SoundUICfg soundCfg ;
        if (m_cfgs.TryGetValue(m_name, out soundCfg))
            return soundCfg;
        else
            return null;
    }
    public static void CheckInit()
    {
        if (m_cfgs.Count == 0)
            Init();
    }

}
