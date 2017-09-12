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
var guardLevelConfig = require("../../gameConfig/guardLevelConfig");
var globalDefine = require("../../enumType/globalDefine").globalDefine;
var itemModule = require("../../item/item");
var enSystemId = require("../../enumType/systemDefine").enSystemId;
var systemMgr = require("../../system/systemMgr");

/**
 *
 * @param {Role} role
 * @param {number} mode
 */
function checkEnterGuardLevel(role, mode)
{
    var errObj = {};
    if(!systemMgr.isEnabled(role,enSystemId.guardLevel,errObj))
    {
        logUtil.info(errObj.errMsg);
        return {code:ResultCodeActivity.ACTIVITY_NOT_START, obj:null};
    }

    var basicCfg = guardLevelConfig.getGuardLevelBasicCfg();
    var modeCfg = guardLevelConfig.getGuardLevelModeCfg(mode);

    if (!modeCfg)
        return {code:ResultCode.BAD_PARAMETER, obj:null};

    var actPart = role.getActivityPart();
    var guardLvlTime = actPart.getNumber(enActProp.guardLvlTime);
    var guardLvlCnt = actPart.getNumber(enActProp.guardLvlCnt);
    var guardLvlMax = actPart.getNumber(enActProp.guardLvlMax);

    var curTime = dateUtil.getTimestamp();
    if (Math.abs(curTime - guardLvlTime) < basicCfg.coolDown)
        return {code:ResultCodeActivity.IN_COOL_DOWN, obj:null};

    //如果上次不是今天，那就清0
    if (!dateUtil.isToday(guardLvlTime))
        guardLvlCnt = 0;

    //次数检查
    if (guardLvlCnt >= basicCfg.dayMaxCnt)
        return {code:ResultCodeActivity.DAY_MAX_CNT, obj:null};

    //最大只可以原来打的最大关的后一关
    if (mode > guardLvlMax + 1)
        return {code:ResultCodeActivity.PRE_MODE_WRONG, obj:null};

    var roleLevel = role.getNumber(enProp.level);
    if (roleLevel < modeCfg.openLevel)
        return {code:ResultCodeActivity.HERO_LVL_WRONG, obj:null};

    var serverData = actPart.getServerData();
    serverData.guardLevelCurMode = mode;

    return {code:ResultCode.SUCCESS, obj:new activityMessage.EnterGuardLevelResultVo(mode)};
}


/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {EnterGuardLevelVo} reqObj
 */
function enterGuardLevel(session, role, msgObj, reqObj)
{
    var mode = reqObj.mode;
    var retObj = checkEnterGuardLevel(role, mode);
    msgObj.setResponseData(retObj.code, retObj.obj);
    role.send(msgObj);
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_ENTER_GUARD_LEVEL, enterGuardLevel, activityMessage.EnterGuardLevelVo, true);

function getRankByValue(value, ranks)
{
    for(var i=ranks.length-1;i>=0;i--)
    {
        if(value>=ranks[i])
        {
            return ranks[i];
        }
    }
    return ranks[0];
}

function doEndGuardLevel(role, wave, point)
{
    var actPart = role.getActivityPart();
    var serverData = actPart.getServerData();
    var mode = serverData.guardLevelCurMode;
    if (!mode)
        return {code:ResultCodeActivity.LVL_NO_START, obj:null};
    delete serverData.guardLevelCurMode;

    var guardLvlTime = actPart.getNumber(enActProp.guardLvlTime);
    var guardLvlCnt = actPart.getNumber(enActProp.guardLvlCnt);
    var guardLvlMax = actPart.getNumber(enActProp.guardLvlMax);

    var curTime = dateUtil.getTimestamp();
    var isToday = dateUtil.isSameDay(curTime, guardLvlTime);


    // base reward
    var rewardsMap = {};
    var exp = 0;
    /*
    var baseRewardCfg = guardLevelConfig.getGuardBaseRewardCfg(mode+"_"+wave);
    if(baseRewardCfg)
    {
        exp += baseRewardCfg.exp;
        rewardsMap = role.getItemsPart().addRewards(baseRewardCfg.reward);
    }

    var roleHpRank = getRankByValue(roleHp, guardLevelConfig.getRoleHpRanks());
    var familyHpRank = getRankByValue(familyHp, guardLevelConfig.getFamilyHpRanks());
    var evaluation = guardLevelConfig.getGuardEvaluateCfg(roleHpRank, familyHpRank).evaluate;
    */
    var evaluation = 1;
    while(evaluation+1<=guardLevelConfig.getGuardLevelBasicCfg().maxEvaluation&&guardLevelConfig.getGuardEvaluateCfg(evaluation+1).point<=point)
    {
        evaluation++;
    }
    var evaluateRewardCfg = guardLevelConfig.getGuardEvaluateRewardCfg(mode+"_"+evaluation);
    if(evaluateRewardCfg)
    {
        exp += evaluateRewardCfg.exp;
        var evalRewards = role.getItemsPart().addRewards(evaluateRewardCfg.reward);
        for(var k in evalRewards)
        {
            rewardsMap[k] = (rewardsMap[k]||0)+evalRewards[k];
        }
    }

    role.addExp(exp);

    actPart.startBatch();
    actPart.setNumber(enActProp.guardLvlTime, curTime);
    actPart.setNumber(enActProp.guardLvlCnt, isToday ? guardLvlCnt + 1 : 1);
    if (mode > guardLvlMax && evaluation==globalDefine.MAX_EVALUATION)
        actPart.setNumber(enActProp.guardLvlMax, mode);

    actPart.endBatch();



    var netMsg = new activityMessage.EndGuardLevelResultVo();
    netMsg.evaluation = evaluation;
    netMsg.exp = exp;
    netMsg.wave = wave;
    netMsg.point = point;
    for(k in rewardsMap)
    {
        netMsg.itemList.push(itemModule.createItem({itemId:parseInt(k),num:rewardsMap[k]}));
    }

    return {code:ResultCode.SUCCESS, obj:netMsg};
}

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {SweepGuardLevelVo} reqObj
 */
function sweepGuardLevel(session, role, msgObj, reqObj)
{
    var mode = reqObj.mode;

    var actPart = role.getActivityPart();
    var hadesLvlMax = actPart.getNumber(enActProp.hadesLvlMax);
    if (hadesLvlMax !== mode)
    {
        msgObj.setResponseData(ResultCodeActivity.CAN_NOT_SWEEP);
        role.send(msgObj);
        return;
    }

    var retObj = checkEnterGuardLevel(role, mode);
    if (retObj.code !== ResultCode.SUCCESS)
    {
        msgObj.setResponseData(retObj.code, retObj.obj);
        role.send(msgObj);
        return;
    }

    retObj = doEndGuardLevel(role, guardLevelConfig.getGuardLevelBasicCfg().maxWave, 2401);
    msgObj.setResponseData(retObj.code, retObj.obj);
    role.send(msgObj);
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_SWEEP_GUARD_LEVEL, sweepGuardLevel, activityMessage.SweepGuardLevelVo, true);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {EndGuardLevelVo} reqObj
 */
function endGuardLevel(session, role, msgObj, reqObj)
{
    var retObj = doEndGuardLevel(role, reqObj.wave, reqObj.point);
    msgObj.setResponseData(retObj.code, retObj.obj);
    role.send(msgObj);
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_END_GUARD_LEVEL, endGuardLevel, activityMessage.EndGuardLevelVo, true);