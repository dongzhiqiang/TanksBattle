using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoleMenuConfig
{
    public int menuId;
    public string menu;
}

public class RoleMenuCfg
{
    public static Dictionary<int, RoleMenuConfig> m_cfgs = new Dictionary<int, RoleMenuConfig>();
    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, RoleMenuConfig>("friend/roleMenu", "menuId");
    }

    public static RoleMenuConfig Get(int menuId)
    {
        return m_cfgs[menuId];
    }
}
