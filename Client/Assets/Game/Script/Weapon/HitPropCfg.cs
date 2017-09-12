#region Header
/**
 * 名称：打击属性表
 
 * 日期：2016.7.25
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//打击属性
public enum enHitDef
{
    none,
    strong,
    normal,
    weak,
    max
}


public class HitPropCfg
{
    public int id;
    public string name = "";
    public string desc = "";
    public string icon="";
    public float noneDef = 0;
    public float strongDef = 0;
    public float normalDef = 0;
    public float weakDef = 0;

    public static List<HitPropCfg> s_cfgs = new List<HitPropCfg>();
    public static Dictionary<string, HitPropCfg> s_nameIdx = new Dictionary<string, HitPropCfg>();

    float[] defs = new float[(int)enHitDef.max];

    public static void Init()
    {
        s_nameIdx.Clear();
        s_cfgs = Csv.CsvUtil.Load< HitPropCfg>("role/hitProp");  
        foreach (HitPropCfg cfg in s_cfgs)
        {
            cfg.defs[(int)enHitDef.none] = cfg.noneDef;
            cfg.defs[(int)enHitDef.strong] = cfg.strongDef;
            cfg.defs[(int)enHitDef.normal] = cfg.normalDef;
            cfg.defs[(int)enHitDef.weak] = cfg.weakDef;
            s_nameIdx[cfg.name] = cfg;
        }
    }


    public static HitPropCfg Get(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        return s_nameIdx.Get(name);
    }
    
    public float  GetRate(enHitDef hitProp)
    {
        return defs[(int)hitProp];
    }

    public static List<enHitDef> GetDefs(string[] hitDefType) {
        List<enHitDef> l = new List<enHitDef>(s_cfgs.Count);
        for (int i = 0;i< s_cfgs.Count;++i)
        {
            var cfg = s_cfgs[i];
            var defType = hitDefType != null && i <=hitDefType.Length  ? hitDefType[i] :null ;
            if (string.IsNullOrEmpty(defType))
            {
                l.Add(enHitDef.none);
                continue;
            }

            switch (defType)
            {
                case "强": l.Add(enHitDef.strong); break;
                case "中": l.Add(enHitDef.normal); break;
                case "弱": l.Add(enHitDef.weak); break;
                case "无": l.Add(enHitDef.none); break;
                default: Debuger.LogError("被打击属性找不到类型,请检查role表:{0}", defType); l.Add(enHitDef.none); break;
            }
        }

        return l;
    }
    
}
