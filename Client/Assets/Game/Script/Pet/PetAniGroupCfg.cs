using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PetAniGroupCfg
{
    public int id;
    public string startAni;
    public string defaultAni;
    public List<List<string>> specialAni;

    public static Dictionary<int, PetAniGroupCfg> m_cfgs = new Dictionary<int, PetAniGroupCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, PetAniGroupCfg>("role/petAniGroup", "id");
    }

    

}
