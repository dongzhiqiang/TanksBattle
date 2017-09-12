#region Header
/**
 * 名称：动作音效
 
 * 日期：2016.5.4
 * 描述：一个角色播放一个动作的时候达到第几帧则播放对应的音效
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AniSoundCfg
{
  
    public string mod;
    public string ani;
    public int frame;
    public int soundId;
    public bool stopIfEnd;
    
    public static Dictionary<string, Dictionary<string, List<AniSoundCfg>> > s_cfgs = new Dictionary<string, Dictionary<string, List<AniSoundCfg>>>();
    static HashSet<string> s_preLoads = new HashSet<string>();
    public static void Init()
    {
        s_cfgs.Clear();
        s_preLoads.Clear();
        List<AniSoundCfg> l= Csv.CsvUtil.Load<AniSoundCfg>("sound/aniSound");
        foreach (var cfg in l)
        {
            var ll= s_cfgs.GetNewIfNo(cfg.mod).GetNewIfNo(cfg.ani);
            ll.Add(cfg);
            ll.SortEx(Comparer);
        }
    }

    static int Comparer(AniSoundCfg a, AniSoundCfg b)
    {
        return a.frame.CompareTo(b.frame);
    }
    
    public static void Get(string mod,string ani,ref LinkedList<AniSoundCfg> l)
    {
        l.Clear();
        var d = s_cfgs.Get(mod);
        if (d == null)
            return;

        var ll = d.Get(ani);
        if (ll == null)
            return;
        for (int i =0;i<ll.Count;++i)
        {
            l.AddLast(ll[i]);
        }
    }
    
    public static void PreLoad(string mod)
    {
        if (s_preLoads.Contains(mod))
            return;
        s_preLoads.Add(mod);

        var d = s_cfgs.Get(mod);
        if (d == null)
            return;

        foreach (var ll in d.Values)
        {
            foreach (var cfg in ll)
            {
                SoundMgr.instance.PreLoad(cfg.soundId);
            }
        }
    }

}
