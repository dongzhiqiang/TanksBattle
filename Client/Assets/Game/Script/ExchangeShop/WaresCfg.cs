using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaresCfg
{
    public int id;

    public int  groupId;

    public int itemId;

    public int itemNum;

    public int price;

    public int  sureAppear;

    public int weight;

    public int hasEffect;

    public static Dictionary<int, WaresCfg> m_cfgs = new Dictionary<int, WaresCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, WaresCfg>("exchangeShop/wares", "id");
    }    
}
