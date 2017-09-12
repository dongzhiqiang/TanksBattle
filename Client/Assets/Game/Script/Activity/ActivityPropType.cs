using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum enActProp
{
    goldLvlTime = 1,
    goldLvlCnt = 2,
    goldLvlMax = 3, //打过的最高难度
    goldLvTodayMaxScore = 4,
    hadesLvlTime = 5,
    hadesLvlCnt = 6,
    hadesLvlMax = 7, //打过的最高难度
    venusLvlTime = 8,
    venusLvlEntered1 = 9,
    venusLvlEntered2 = 10,
    arenaScore = 11,
    arenaTime = 12,  //上次打竞技场时间
    arenaCnt = 13,  //上次所在日打竞技场的次数
    arenaBuyCntTime = 14,   //上次买竞技场挑战次数时间
    arenaBuyCnt = 15,   //上次买竞技场挑战次数
    arenaMaxScore = 16,  //曾经的最高积分
    arenaConWin = 17,   //连胜次数
    mainLvlTime = 18, //最后通关副本时间
    mainNormalLvlCnt = 19,   //每天普通副本通关次数
    mainSpecialLvlCnt = 20,   //每天精英副本通关次数
    guardLvlTime = 21,
    guardLvlCnt = 22,
    guardLvlMax = 23,
    arenaTotalWin = 24, //竞技场总胜利次数
    arenaPos = 25,//竞技场阵型
    treasureLstTime = 26,
    treasureTime = 27,
    treasureCnt = 28,
    treasureRobedCnt = 29,
    treasureRobedMax = 30,
    treasureRobedTime = 31,
    arenaRank = 32, //用于下发自己的竞技场排行给别人，不存盘
}