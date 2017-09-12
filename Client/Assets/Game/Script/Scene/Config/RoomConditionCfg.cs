using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum ConditionType : int
{
    FINISH_LEVEL = 0,   //通关副本
    BLOOD_LIMIT = 1,    //限血通关
    TIME_LIMIT = 2,     //限时通关
    OPEN_BOX = 3,       //开启宝箱
    ROLE_DEAD = 4,      //角色不死
    REACH_AREA = 5,     //到达区域
    KILL_TIMES = 6,     //完成连杀
    KILL_MONSTER = 7,   //杀怪个数
    WEAPON_LIMIT = 8,   //限用武器
    SKILL_USE = 9,      //使用技能
    PET_USE = 10,       //限用宠物
    STATE_TIMES = 11,   //状态次数
    BREAK_ITEM = 12,    //打碎物品
    HURT_NUM = 13,      //被击次数
    PET_USE2 = 14,      //限用宠物
}
public class RoomConditionCfg
{
    #region Fields
    public int id;
    public string type;
    public string desc;
    public string endDesc;
    public int intValue1 = -1;
    public int intValue2 = -1;
    public string stringValue1;
    public string stringValue2;
    #endregion

    public static Dictionary<int, RoomConditionCfg> m_cfgs = new Dictionary<int, RoomConditionCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, RoomConditionCfg>("room/conditions", "id");
    }

    public static RoomConditionCfg GetCfg(int id)
    {
        RoomConditionCfg cfg;
        m_cfgs.TryGetValue(id, out cfg);
        return cfg;
    }
}
