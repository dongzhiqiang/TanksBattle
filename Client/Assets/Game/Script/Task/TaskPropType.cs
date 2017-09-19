using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum enTaskProp
{
    vitality=1,
    levelGet=2,
    goldlvGet=3,
    hadesLvGet=4, 
    venusLvGet=5,
    arenaGet=6,
    SpecialLvGet=7,
    vitalityBox1=8,
    vitalityBox2=9,
    vitalityBox3=10,
    vitalityBox4=11,
    dailyTaskGet=12,
    dailyTaskTotal=13,
    task1=14,    
    task2 = 15,
    task3 = 16,
    task4 = 17,
    task5 = 18,
    task6 = 19,
    task7 = 20,
    task8 = 21,
    task9 = 22,
    task10 = 23,
    task11 = 24,
    task12 = 25,
    task13 = 26,
    task14 = 27,
    task15 = 28,
    task16 = 29,
    task17 = 30,
    task18 = 31,
    task19 = 32,
    task20 = 33,
    task21 = 34,
    task22 = 35,
    task23 = 36,
    task24 = 37,
    task25 = 38,
    task26 = 39,
    task27 = 40,
    task28 = 41,
    task29 = 42,
    task30 = 43,
    task31 = 44,
    task32 = 45,
    task33 = 46,
    task34 = 47,
    task35 = 48,
    task36 = 49,
    task37 = 50,
    task38 = 51,
    task39 = 52,
    task40 = 53,
    task41 = 54,
    task42 = 55,
    task43 = 56,
    task44 = 57,
    task45 = 58,
    task46 = 59,
    task47 = 60,
    task48 = 61,
    task49 = 62,
    task50 = 63,
    task51 = 64,
    task52 = 65,
    task53 = 66,
    task54 = 67,
    task55 = 68,
    task56 = 69,
    task57 = 70,
    task58 = 71,
    task59 = 72,
    task60 = 73,
    task61 = 74,
    task62 = 75,
    task63 = 76,
    task64 = 77,
    task65 = 78,
    task66 = 79,
    task67 = 80,
    task68 = 81,
    task69 = 82,
    task70 = 83,
    task71 = 84,
    task72 = 85,
    task73 = 86,
    task74 = 87,
    task75 = 88,
    task76 = 89,
    task77 = 90,
    task78 = 91,
    task79 = 92,
    task80 = 93,
    task81 = 94,
    task82 = 95,
    task83 = 96,
    task84 = 97,
    task85 = 98,
    task86 = 99,
    task87 = 100,
    task88 = 101,
    task89 = 102,
    task90 = 103,
    task91 = 104,
    task92 = 105,
    task93 = 106,
    task94 = 107,
    task95 = 108,
    task96 = 109,
    task97 = 110,
    task98 = 111,
    task99 = 112,
    task100 = 113,
    guardLvGet = 114,  
    advLtyGet = 115,
    topLtyGet = 116,
    costDiamondGet = 117,
    corpsGet = 118,
    vip1Get = 119,
    vip2Get = 120,
    vip3Get = 121,
    vip4Get = 122,
    vip5Get = 123,
    vip6Get = 124,
    vip7Get = 125,
    vip8Get = 126,
    vip9Get = 127,
    vip10Get = 128,
    vip11Get = 129,
    vip12Get = 130,
    vip13Get = 131,
    vip14Get = 132,
    vip15Get = 133,
    vip16Get = 134,
    vip17Get = 135,
    vip18Get = 136,
    vip19Get = 137,
    vip20Get = 138,
    treasureRobGet = 139,
    upEquipGet = 140,
    eliteLvGet = 142,
    warriorTriedGet = 143,
    buyGoldGet = 144,
    prophetTowerGet = 145,
    shareGet = 146,
    monthCardGet = 147,
    opActivityGet = 148,


}

public enum enTaskType
{
    normalLv = 1, //副本普通难度满星通关
    specialLv = 2, //副本精英难度满星通关
    activity = 3,   //活动
    arena = 4,   //竞技场总胜利任务
    power = 5,  //战力达到**任务
    weaponSkill = 6, //*个武器技能升到*级任务
    equipAdvLv = 7, //*个装备进阶*任务
    equipStar = 8, //*个装备觉醒*星任务
    daily = 12, //每日任务完成总次数任务
    friend = 13, //添加*个好友任务
    corps = 14, //公会任务
    lottery = 17, //开启宝箱任务
    cost = 18 ,//消费任务
    vip = 19, //vip任务
    upGrade = 20 ,//升级装备或神侍任务
    eliteLv = 21,//众神传任务
    warriorTried = 22,//勇士试炼任务
    buyGold = 23,//购买金币任务
    prophetTower = 24,//预言者之塔任务
    share = 25,//分享任务
    monthCard = 26, //月卡返利
    opActivity = 27, //运营返利

}

public enum enRewardState
{
    canGetReward=1,
    hasGetReward=2,
    cantGetReward=3,
}

public class GrowthTask
{
    public string progress;
    public enRewardState taskState;
}