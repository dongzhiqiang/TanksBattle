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




public class PetBondCfg 
{
    public int id;
    public string name;
    public string pet1;
    public string pet2;
    public string pet3;
    public string pet4;
    public string[] param;
    public string desc;
    public LvValue powerRate;
    public List<AddPropCxt> m_rateCxts;

    public static Dictionary<int, PetBondCfg> m_cfgs = new Dictionary<int, PetBondCfg>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, PetBondCfg>("pet/petBond", "id"); 
        foreach(PetBondCfg cfg in m_cfgs.Values)
        {
            cfg.InitCxt();
        }
    }

    void InitCxt()
    {
        m_rateCxts = new List<AddPropCxt>();
        foreach (string s in param)
        {
            AddPropCxt cxt = new AddPropCxt(s);
            if (cxt == null)
            {
                Debuger.LogError("缘分加成解析出错.缘分id:{0}", id);
                continue;
            }
            m_rateCxts.Add(cxt);
        }
    }

    public static List<string> GetBondPets(int id)
    {
        PetBondCfg petBondCfg = m_cfgs[id];
        List<string> result = new List<string>();
        if (petBondCfg.pet1 != "" && petBondCfg.pet1!=null) result.Add(petBondCfg.pet1);
        if (petBondCfg.pet2 != "" && petBondCfg.pet2 != null) result.Add(petBondCfg.pet2);
        if (petBondCfg.pet3 != "" && petBondCfg.pet3 != null) result.Add(petBondCfg.pet3);
        if (petBondCfg.pet4 != "" && petBondCfg.pet4 != null) result.Add(petBondCfg.pet4);
        return result;
    }
}
