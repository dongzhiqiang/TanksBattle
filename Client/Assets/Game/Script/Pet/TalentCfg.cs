#region Header
/**
 * 名称：副本固定属性
 
 * 日期：2015.11.24
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class TalentCfg 
{
    public string id;
    public string name;
    //public int type;
    public string icon;
    public string description;
    public int upgradeId;
    public int maxLevel;
    public int stateId;
    public LvValue power;
    public LvValue powerRate;

    public static Dictionary<string, TalentCfg> m_cfgs = new Dictionary<string, TalentCfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<string, TalentCfg>("pet/talent", "id");
    }

}
