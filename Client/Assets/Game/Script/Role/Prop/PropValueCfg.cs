#region Header
/**
 * 名称：固定属性表 
 
 * 日期：2016.3.7
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class PropValueCfg 
{
    public string id;
    public PropertyTable props;

    public static Dictionary<string, PropValueCfg> m_cfgs = new Dictionary<string, PropValueCfg>();
    public static void Init()
    {
        m_cfgs = PropTypeCfg.Load<string, PropValueCfg>("property/propValue", "id", "props");
    }

    public static PropValueCfg Get(string id)
    {
        PropValueCfg cfg = m_cfgs.Get(id);
        if (cfg == null)
            Debuger.LogError("找不到固定属性:{0}", id);
        return cfg;
    }
    
}
