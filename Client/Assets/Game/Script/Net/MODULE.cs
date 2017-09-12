using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 模块ID
/// </summary>
public class MODULE
{
    /// <summary>
    /// 无模块
    /// </summary>
    public const byte MODULE_NONE = 0;

    /// <summary>
    /// 账号模块
    /// </summary>
    public const byte MODULE_ACCOUNT = 1;

    /// <summary>
    /// 角色模块
    /// </summary>
    public const byte MODULE_ROLE = 2;

    /// <summary>
    /// 道具模块
    /// </summary>
    public const byte MODULE_ITEM = 3;

    /// <summary>
    /// 装备模块
    /// </summary>
    public const byte MODULE_EQUIP = 4;

    /// <summary>
    /// GM命令模块
    /// </summary>
    public const byte MODULE_GM = 5;

    /// <summary>
    /// 宠物模块
    /// </summary>
    public const byte MODULE_PET = 6;

    /// <summary>
    /// 关卡模块
    /// </summary>
    public const byte MODULE_LEVEL = 7;

    /// <summary>
    /// 活动模块
    /// </summary>
    public const byte MODULE_ACTIVITY = 8;

    /// <summary>
    /// 排行榜模块
    /// </summary>
    public const byte MODULE_RANK = 9;

    /// <summary>
    /// 武器模块
    /// </summary>
    public const byte MODULE_WEAPON = 10;

    /// <summary>
    /// 运营活动模块
    /// </summary>
    public const byte MODULE_OPACTIVITY = 11;

    /// <summary>
    /// 邮件模块
    /// </summary>
    public const byte MODULE_MAIL = 12;

    /// <summary>
    /// 系统管理模块
    /// </summary>
    public const byte MODULE_SYSTEM = 13;

    /// <summary>
    /// 圣火模块
    /// </summary>
    public const byte MODULE_FLAME = 14;

    /// <summary>
    /// 每日任务模块
    /// </summary>
    public const byte MODULE_TASK = 15;

    /// <summary>
    /// 好友社交模块
    /// </summary>
    public const byte MODULE_SOCIAL = 16;
    
    /// <summary>
    /// 公会模块 
    /// </summary>
    public const byte MODULE_CORPS = 17;

    /// <summary>
    /// 聊天模块 
    /// </summary>
    public const byte MODULE_CHAT = 18;

    /// <summary>
    /// 兑换商店模块
    /// </summary>
    public const byte MODULE_SHOP = 19;

    /// <summary>
    /// 精英副本模块(众神传)
    /// </summary>
    public const byte MODULE_ELITE_LEVEL = 20;

    /// <summary>
    /// 神器模块
    /// </summary>
    public const byte MODULE_TREASURE = 21;
}

public class RESULT_CODE
{
    public const int SUCCESS        = 0;
    public const int SERVER_ERROR   = -1;   //服务器错误，一般是保持当前界面，或者跳到服务器选择界面
    public const int DB_ERROR       = -2;   //数据库错误，一般是保持当前界面，或者跳到服务器选择界面
    public const int BAD_PARAMETER  = -3;   //参数错误，一般是保持当前界面，或者跳到服务器选择界面
    public const int BAD_REQUEST    = -4;   //请求不被处理
    public const int PARSE_ERROR    = -5;   //解析数据出错，一般用于客户端，但还是写入统一错误码
    public const int CONFIG_ERROR   = -6;   //配置有错
    public const int NOT_EXIST_ERROR = -7;  //找不到操作对象
    public const int NO_ENOUGH_GOLD = -8;  //金币不足
    public const int DIAMOND_INSUFFICIENT = -9;  //钻石不足
    public const int NO_ENOUGH_LEVEL = -10;  //等级不足
}
