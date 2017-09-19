#region Header
/**
 * 名称：
 
 * 日期：2015.9.24
 * 描述：放一些全局的枚举，方便管理
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//系统图标枚举,注意只能在最后加
public enum enSystem
{
    min,
    none,
    other,
    vip,//vip
    task,//任务
    mail,//邮件
    rank,//排行榜
    strong,//如何变强
    chat,//聊天气泡
    stamina,//体力
    gold,//金币
    diamond,//钻石
    sign,//签到
    firstCharge,//首冲
    happyOpen,//开服狂欢
    activity,//战争学院
    scene,//副本
    arena,//竞技场
    hero,//英雄
    weapon,//武器
    opActivity,//运营活动
    flame,//圣火
    venusLevel =50,//维纳斯活动
    hadesLevel,//地狱犬
    goldLevel,//金币活动
    social,//好友
    dailyTask,//每日任务
    guardLevel, //守护家园
    corps,  //公会
    shop,  //商店
    lottery,    //宝藏图标
    lotteryTop, //顶级宝藏的枚举
    warriorTried,  //勇士试炼
    treasureRob, //神器抢夺
    weapon1, //武器-2
    weapon2, //武器-3
    weapon3, //武器-4
    lotteryAdv, //高级宝藏的枚举
    setting,//系统设置
    eliteLevel,//众神传
    treasure,//神器
    prophetTower,//预言者之塔
    element,//元素属性
    max,
}

//系统标志枚举，一般用于红点提示
public enum enSysFlag
{
    min,
    none,
    other,
    equip,//装备红点
    equipLevelUp,//装备升级红点
    equipCompose,//装备合成红点
    max,
}

//系统开启枚举
public enum enSysOpen
{
    min,
    none,
    other,
    equip,//装备是不是开启
    equipCompose,//装备合成是不是开启
    max,
}

//角色部件
public enum enPart
{
    prop,
    tran,
    ani,
    render,
    rsm,
    buff,
    hate,
    dead,
    move,
    combat,
    ai,
    items,
    equips,
    levels,
    talents,
    activity,
    weapon,    
    opActivity,
    systems,
    mail,
    task,
    social,
    flames,
    corps,  //公会
    shop,
    eliteLevels,
    treasure,
    max,
}

//角色部件由谁创建
public enum enPartCreate
{
    role,//角色类创建
    model,//模型类创建
    none,//外部添加的部件，role类不会自动创建，注意要在角色的init之前添加
}

// 通过等级
public enum enLevelEval
{
    c = 1,
    b,
    a,
    s,
    ss,
    sss
}



public class GlobalConst
{
    public const string FLAG_SHOW_BLOOD = "showBloodBar";
    public const string FLAG_REFLESH_WAVE = "refleshWave";
    public const string FLAG_SHOW_FRIENDBLOOD = "showFriendBloodBar";
    public const string FLAG_SHOW_TARGET = "showTarget";
    public const string FLAG_ARENA_BLOOD = "arenaBlood";
}