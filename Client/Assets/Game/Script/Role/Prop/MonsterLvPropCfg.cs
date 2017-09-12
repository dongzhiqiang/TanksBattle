#region Header
/**
 * 名称：怪物属性等级系数
 
 * 日期：2016.3.7
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class MonsterLvPropCfg 
{
    public int lv;
    public PropertyTable props;

    public static Dictionary<int, MonsterLvPropCfg> m_cfgs = new Dictionary<int, MonsterLvPropCfg>();
    public static void Init()
    {
        m_cfgs = PropTypeCfg.Load<int, MonsterLvPropCfg>("property/monsterLvProp", "lv", "props");
        
    }

    public static MonsterLvPropCfg Get(int lv)
    {
        MonsterLvPropCfg cfg = m_cfgs.Get(lv);
        if (cfg == null)
            Debuger.LogError("找不到对应的怪物属性等级系数 lv:{0}", lv);
        return cfg;
    }
}
