#region Header
/**
 * 名称：道具属性
 
 * 日期：2015.11.24
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class FlameCfg 
{
    public int id;
    /** 名称*/
    public string name;
    /** 图标*/
    public string icon;
    /** 模型*/
    public string mod;
    /** 需要等级*/
    public int needLevel;
    /** 每经验值消耗金币*/
    public int costGold;
    /** 属性上限*/
    public string[] attributeLimit;
    /** 属性上限cxt*/
    public List<AddPropCxt> attrLimitCxts;

    public static Dictionary<int, FlameCfg> m_cfgs = new Dictionary<int, FlameCfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, FlameCfg>("flame/flame", "id");
        foreach (FlameCfg cfg in m_cfgs.Values)
        {
            cfg.InitCxt();
        }
    }

    void InitCxt()
    {
        attrLimitCxts = new List<AddPropCxt>();
        foreach (string s in attributeLimit)
        {
            AddPropCxt cxt = new AddPropCxt(s);
            if (cxt == null)
            {
                Debuger.LogError("圣火属性上限解析出错.圣火id:{0}", id);
                continue;
            }
            attrLimitCxts.Add(cxt);
        }
    }
}
