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




public class SkillTypeCfg 
{
    public int type;
    public string name;

    public static Dictionary<int, SkillTypeCfg> m_cfgs = new Dictionary<int, SkillTypeCfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, SkillTypeCfg>("pet/skillType", "type");
    }

}
