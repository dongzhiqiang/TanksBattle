using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EquipPosConfig
{
    public int id;
    public int ui;
}
public class EquipPosCfg
{
    static Dictionary<int, EquipPosConfig> m_dic = new Dictionary<int, EquipPosConfig>();

    public static void Init()
    {
        m_dic = Csv.CsvUtil.Load<int, EquipPosConfig>("equip/equipPos", "id");
    }

    public static EquipPosConfig Get(int pos)
    {
        return m_dic.Get(pos);
    }
}
