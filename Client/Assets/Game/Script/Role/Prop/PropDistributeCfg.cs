#region Header
/**
 * 名称：属性分配比例表
 
 * 日期：2016.3.7
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class PropDistributeCfg 
{
    public string id;
    public PropertyTable props;

    public static Dictionary<string, PropDistributeCfg> m_cfgs = new Dictionary<string, PropDistributeCfg>();
    public static void Init()
    {
        m_cfgs = PropTypeCfg.Load<string, PropDistributeCfg>("property/propDistribute", "id", "props");
    }

    public static PropDistributeCfg Get(string id)
    {
        PropDistributeCfg cfg = m_cfgs.Get(id);
        if (cfg == null)
            Debuger.LogError("找不到属性分配比例表:{0}", id);
        return cfg;
    }
    
}
