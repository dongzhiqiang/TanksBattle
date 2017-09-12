"use strict";

var logUtil = require("../../../libs/logUtil");
var dateUtil = require("../../../libs/dateUtil");
var handlerMgr = require("../../session/handlerMgr");
var ModuleIds = require(".././../netMessage/netMsgConst").ModuleIds;
var ResultCode = require(".././../netMessage/netMsgConst").ResultCode;
var activityMessage = require("../../netMessage/activityMessage");
var CmdIdsActivity = require(".././../netMessage/activityMessage").CmdIdsActivity;
var ResultCodeActivity = require(".././../netMessage/activityMessage").ResultCodeActivity;
var enProp = require("../../enumType/propDefine").enProp;
var enActProp = require("../../enumType/activityPropDefine").enActProp;
var goldLevelConfig = require("../../gameConfig/goldLevelConfig");
var rankTypes = require("../../enumType/rankDefine").rankTypes;
var rankMgr = require("../../rank/rankMgr");
var enSystemId = require("../../enumType/systemDefine").enSystemId;
var systemMgr = require("../../system/systemMgr");

/**
 *
 * @param {Role} role
 * @param {number} mode
 */
function checkEnterGoldLevel(role, mode)
{
    var errObj = {};
    if(!systemMgr.isEnabled(role,enSystemId.goldLevel,errObj))
    {
        logUtil.info(errObj.errMsg);
        return {code:ResultCodeActivity.ACTIVITY_NOT_START, obj:null};
    }

    var basicCfg = goldLevelConfig.getGoldLevelBasicCfg();
    var modeCfg = goldLevelConfig.getGoldLevelModeCfg(mode);

    if (!modeCfg)
        return {code:ResultCode.BAD_PARAMETER, obj:null};

    var actPart = role.getActivityPart();
    var goldLvlTime = actPart.getNumber(enActProp.goldLvlTime);
    var goldLvlCnt = actPart.getNumber(enActProp.goldLvlCnt);
    var goldLvlMax = actPart.getNumber(enActProp.goldLvlMax);

    //如果上次不是今天，那就清0
    if (!dateUtil.isToday(goldLvlTime))
        goldLvlCnt = 0;

    //次数检查
    if (goldLvlCnt >= basicCfg.dayMaxCnt)
        return {code:ResultCodeActivity.DAY_MAX_CNT, obj:null};

    var curTime = dateUtil.getTimestamp();
    if (Math.abs(curTime - goldLvlTime) < basicCfg.coolDown)
        return {code:ResultCodeActivity.IN_COOL_DOWN, obj:null};

    //最大只可以原来打的最大关的后一关
    if (mode > goldLvlMax + 1)
        return {code:ResultCodeActivity.PRE_MODE_WRONG, obj:null};

    var roleLevel = role.getNumber(enProp.level);
    if (roleLevel < modeCfg.openLevel)
        return {code:ResultCodeActivity.HERO_LVL_WRONG, obj:null};

    var serverData = actPart.getServerData();
    serverData.goldLevelCurMode = mode;

    return {code:ResultCode.SUCCESS, obj:new activityMessage.EnterGoldLevelResultVo(mode)};
}

/**
 *
 * @param {number} damageHPRatio
 * @param {GoldLevelBasicCfg} basicCfg
 */
function getScoreLevel(damageHPRatio, basicCfg)
{
    damageHPRatio = Math.round(damageHPRatio * 100);
    if (damageHPRatio < basicCfg.rateBHP)
        return "C";
    else if (damageHPRatio < basicCfg.rateAHP)
        return "B";
    else if (damageHPRatio < basicCfg.rateSHP)
        return "A";
    else if (damageHPRatio < basicCfg.rateSSHP)
        return "S";
    else if (damageHPRatio < basicCfg.rateSSSHP)
        return "SS";
    else
        return "SSS";
}

/**
 *
 * @param {Role} role
 * @param hpLeft
 * @param hpMax
 * @returns {*}
 */
function doEndGoldLevel(role, hpLeft, hpMax)
{
    var actPart = role.getActivityPart();
    var serverData = actPart.getServerData();
    var mode = serverData.goldLevelCurMode;
    if (!mode)
        return {code:ResultCodeActivity.LVL_NO_START, obj:null};
    delete serverData.goldLevelCurMode;

    var goldLvlTime = actPart.getNumber(enActProp.goldLvlTime);
    var goldLvlCnt = actPart.getNumber(enActProp.goldLvlCnt);
    var goldLvlMax = actPart.getNumber(enActProp.goldLvlMax);
    var goldLvTodayMaxScore = actPart.getNumber(enActProp.goldLvTodayMaxScore);
    var curTime = dateUtil.getTimestamp();
    var isToday = dateUtil.isSameDay(curTime, goldLvlTime);

    hpMax = Math.max(hpMax, 1);
    hpLeft = Math.clamp(hpLeft, 0, hpMax);
    var hpDamage = hpMax - hpLeft;
    var damageHPRatio = hpDamage / hpMax;
    var leftHPRatio = hpLeft / hpMax;

    var basicCfg = goldLevelConfig.getGoldLevelBasicCfg();
    var modeCfg = goldLevelConfig.getGoldLevelModeCfg(mode);

    var gold = Math.floor(modeCfg.basicGold + modeCfg.maxGold * (1 - Math.pow(leftHPRatio, modeCfg.goldFactor)));
    var rate = getScoreLevel(damageHPRatio, basicCfg);
    var score = Math.ceil(hpDamage / 10);
    var todayMaxScore = isToday ? score : Math.max(goldLvTodayMaxScore, score);

    actPart.startBatch();
    actPart.setNumber(enActProp.goldLvlTime, curTime);
    actPart.setNumber(enActProp.goldLvlCnt, isToday ? goldLvlCnt + 1 : 1);
    if (mode > goldLvlMax && hpLeft <= 0)   //本关打死了怪才算通关
        actPart.setNumber(enActProp.goldLvlMax, mode);
    if (todayMaxScore !== goldLvTodayMaxScore)
        actPart.setNumber(enActProp.goldLvTodayMaxScore, todayMaxScore);
    actPart.endBatch();

    var itemPart = role.getItemsPart();
    itemPart.addGold(gold);

    //计算要给多少物品
    var rewardItems = {};
    // switch (rate)
    // {
    // case "SSS":
    //     rewardItems[modeCfg.rateSSSItemID] = (rewardItems[modeCfg.rateSSSItemID] || 0) + modeCfg.rateSSSItemCount;
    // case "SS":
    //     rewardItems[modeCfg.rateSSItemID] = (rewardItems[modeCfg.rateSSItemID] || 0) + modeCfg.rateSSItemCount;
    // case "S":
    //     rewardItems[modeCfg.rateSItemID] = (rewardItems[modeCfg.rateSItemID] || 0) + modeCfg.rateSItemCount;
    // case "A":
    //     rewardItems[modeCfg.rateAItemID] = (rewardItems[modeCfg.rateAItemID] || 0) + modeCfg.rateAItemCount;
    // case "B":
    //     rewardItems[modeCfg.rateBItemID] = (rewardItems[modeCfg.rateBItemID] || 0) + modeCfg.rateBItemCount;
    // }
    // itemPart.addItems(rewardItems);

    //如果跨天了，或者有新的好成绩，就加入排行榜
    // if (!isToday || todayMaxScore !== goldLvTodayMaxScore)
    // {
    //     rankMgr.addToRankByRole(rankTypes.goldLevel, role);
    // }

    var netMsg = new activityMessage.EndGoldLevelResultVo();
    netMsg.rate = rate;
    netMsg.gold = gold;
    netMsg.score = score;
    netMsg.damage = Math.round(damageHPRatio * 100);
    netMsg.rewards = rewardItems;
    return {code:ResultCode.SUCCESS, obj:netMsg};
}

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {EnterGoldLevelVo} reqObj
 */
function enterGoldLevel(session, role, msgObj, reqObj)
{
    var mode = reqObj.mode;
    var retObj = checkEnterGoldLevel(role, mode);
    msgObj.setResponseData(retObj.code, retObj.obj);
    role.send(msgObj);
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_ENTER_GOLD_LEVEL, enterGoldLevel, activityMessage.EnterGoldLevelVo, true);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {SweepGoldLevelVo} reqObj
 */
function sweepGoldLevel(session, role, msgObj, reqObj)
{
    var mode = reqObj.mode;
    var hpMax = reqObj.hpMax;

    var actPart = role.getActivityPart();
    var goldLvlMax = actPart.getNumber(enActProp.goldLvlMax);
    if (goldLvlMax !== mode)
    {
        msgObj.setResponseData(ResultCodeActivity.CAN_NOT_SWEEP);
        role.send(msgObj);
        return;
    }

    var retObj = checkEnterGoldLevel(role, mode);
    if (retObj.code !== ResultCode.SUCCESS)
    {
        msgObj.setResponseData(retObj.code, retObj.obj);
        role.send(msgObj);
        return;
    }

    retObj = doEndGoldLevel(role, 0, hpMax);
    msgObj.setResponseData(retObj.code, retObj.obj);
    role.send(msgObj);
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_SWEEP_GOLD_LEVEL, sweepGoldLevel, activityMessage.SweepGoldLevelVo, true);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {EndGoldLevelVo} reqObj
 */
function endGoldLevel(session, role, msgObj, reqObj)
{
    var retObj = doEndGoldLevel(role, reqObj.hp, reqObj.hpMax);
    msgObj.setResponseData(retObj.code, retObj.obj);
    role.send(msgObj);
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_END_GOLD_LEVEL, endGoldLevel, activityMessage.EndGoldLevelVo, true);