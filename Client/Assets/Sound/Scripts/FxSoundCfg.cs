#region Header
/**
 * 名称：特效音效
 
 * 日期：2016.5.4
 * 描述：当一个特效播放的时候播放对应的音效
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FxSoundCfg
{
    public string mod;
    public int soundId;
    public bool stopIfEnd;
    

    public static Dictionary<string, FxSoundCfg> s_cfgs = new Dictionary<string, FxSoundCfg>();
    static HashSet<string> s_preLoads = new HashSet<string>();

    public static void Init()
    {
        s_cfgs = Csv.CsvUtil.Load<string, FxSoundCfg>("sound/fxSound", "mod");
    }

    public static FxSoundCfg Get(string fxMod)
    {
        FxSoundCfg cfg ;
        if (s_cfgs.TryGetValue(fxMod, out cfg))
            return cfg;
        else
            return null;
    }

    public static void PreLoad(string mod)
    {
        if (s_preLoads.Contains(mod))
            return;
        s_preLoads.Add(mod);

        FxSoundCfg cfg = Get(mod);
        if (cfg == null)
            return;
        SoundMgr.instance.PreLoad(cfg.soundId);
    }

}
