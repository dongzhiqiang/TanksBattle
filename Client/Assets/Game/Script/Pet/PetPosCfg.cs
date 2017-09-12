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




public class PetPosCfg 
{
    public int id;
    public string desc;
    public int level;

    public static Dictionary<int, PetPosCfg> m_cfgs = new Dictionary<int, PetPosCfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, PetPosCfg>("pet/petPos", "id");
    }

}
