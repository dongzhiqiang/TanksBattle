#region Header
/**
 * 名称：状态定义表
 
 * 日期：2015.11.24
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;



public class BuffType
{
    public enBuff id= enBuff.min;//状态类型的枚举
    public string type = "";//状态类型的名字
    public bool overlay = true;//状态本身是不是可以叠加，比如变身状态，本身是不可能叠加的


    public static Dictionary<enBuff, BuffType> m_cfgs = new Dictionary<enBuff, BuffType>();
    public static Dictionary<string, BuffType> m_cfgsByKey = new Dictionary<string, BuffType>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<enBuff, BuffType>("systemSkill/buffType", "id");
        m_cfgsByKey.Clear();
        foreach (BuffType cfg in m_cfgs.Values)
            m_cfgsByKey[cfg.type] = cfg;
    }

    public static BuffType Get(enBuff b)
    {
        return m_cfgs.Get(b);
    }


    public static BuffType Get(string typeName)
    {
        BuffType cfg = m_cfgsByKey.Get(typeName);
        
        return cfg;
    }
}
