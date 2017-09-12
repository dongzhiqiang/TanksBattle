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




public class TalentPosCfg 
{
    public int id;
    public int needAdvLv;

    public static Dictionary<int, TalentPosCfg> m_cfgs = new Dictionary<int, TalentPosCfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, TalentPosCfg>("pet/talentPos", "id");
    }

}
