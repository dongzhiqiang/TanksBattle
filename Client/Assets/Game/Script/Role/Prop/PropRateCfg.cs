#region Header
/**
 * 名称：百分比属性表 
 
 * 日期：2016.3.7
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class PropRateCfg 
{
    public string id;
    public PropertyTable props;

    public static Dictionary<string, PropRateCfg> m_cfgs = new Dictionary<string, PropRateCfg>();

    public static void Init()
    {
        m_cfgs = PropTypeCfg.Load<string, PropRateCfg>("property/propRate", "id", "props",0.0001f);    
    }

    public static PropRateCfg Get(string id)
    {
        PropRateCfg cfg = m_cfgs.Get(id);
        if (cfg == null)
            Debuger.LogError("找不到百分比属性:{0}", id);
        return cfg;
    }
    
}
