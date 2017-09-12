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




public class TreasureCfg 
{
    public int id;
    /** 名称*/
    public string name;
    /** 图标*/
    public string icon;
    /** 模型*/
    public string mod;
    /** 碎片id*/
    public int pieceId;
    /** 说明*/
    public string note;
    /** 简介*/
    public string desc;



    public static Dictionary<int, TreasureCfg> m_cfgs = new Dictionary<int, TreasureCfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, TreasureCfg>("treasure/treasure", "id");
    }

    public static TreasureCfg Get(int treasureId)
    {
        TreasureCfg result;
        if(!m_cfgs.TryGetValue(treasureId, out result))
        {
            Debuger.LogError("配置表里找不到神器id:" + treasureId);
        }
        return result;
    }

}
