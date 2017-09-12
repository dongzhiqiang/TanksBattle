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
var hadesLevelConfig = require("../../gameConfig/hadesLevelConfig");
var globalDefine = require("../../enumType/globalDefine").globalDefine;
var itemModule = require("../../item/item");
var enSystemId = require("../../enumType/systemDefine").enSystemId;
var systemMgr = require("../../system/systemMgr");

/**
 *
 * @param {Role} role
 * @param {number} mode
 */
function checkEnterHadesLevel(role, mode)
{
    var errObj = {};
    if(!systemMgr.isEnabled(role,enSystemId.hadesLevel,errObj))
    {
        logUtil.info(errObj.errMsg);
        return {code:ResultCodeActivity.ACTIVITY_NOT_START, obj:null};
    }

    var basicCfg = hadesLevelConfig.getHadesLevelBasicCfg();
    var modeCfg = hadesLevelConfig.getHadesLevelModeCfg(mode);

    if (!modeCfg)
        return {code:ResultCode.BAD_PARAMETER, obj:null};

    var actPart = role.getActivityPart();
    var hadesLvlTime = actPart.getNumber(enActProp.hadesLvlTime);
    var hadesLvlCnt = actPart.getNumber(enActProp.hadesLvlCnt);
    var hadesLvlMax = actPart.getNumber(enActProp.hadesLvlMax);


    //如果上次不是今天，那就清0
    if (!dateUtil.isToday(hadesLvlTime))
        hadesLvlCnt = 0;

    //次数检查
    if (hadesLvlCnt >= basicCfg.dayMaxCnt)
        return {code:ResultCodeActivity.DAY_MAX_CNT, obj:null};

    //最大只可以原来打的最大关的后一关
    if (mode > hadesLvlMax + 1)
        return {code:ResultCodeActivity.PRE_MODE_WRONG, obj:null};

    var roleLevel = role.getNumber(enProp.level);
    if (roleLevel < modeCfg.openLevel)
        return {code:ResultCodeActivity.HERO_LVL_WRONG, obj:null};

    var serverData = actPart.getServerData();
    serverData.hadesLevelCurMode = mode;

    return {code:ResultCode.SUCCESS, obj:new activityMessage.EnterHadesLevelResultVo(mode)};
}


/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {EnterHadesLevelVo} reqObj
 */
function enterHadesLevel(session, role, msgObj, reqObj)
{
    var mode = reqObj.mode;
    var retObj = checkEnterHadesLevel(role, mode);
    msgObj.setResponseData(retObj.code, retObj.obj);
    role.send(msgObj);
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_ENTER_HADES_LEVEL, enterHadesLevel, activityMessage.EnterHadesLevelVo, true);

function doEndHadesLevel(role, wave, bossCount)
{
    var actPart = role.getActivityPart();
    var serverData = actPart.getServerData();
    var mode = serverData.hadesLevelCurMode;
    if (!mode)
        return {code:ResultCodeActivity.LVL_NO_START, obj:null};
    delete serverData.hadesLevelCurMode;

    var hadesLvlTime = actPart.getNumber(enActProp.hadesLvlTime);
    var hadesLvlCnt = actPart.getNumber(enActProp.hadesLvlCnt);
    var hadesLvlMax = actPart.getNumber(enActProp.hadesLvlMax);

    var curTime = dateUtil.getTimestamp();
    var isToday = dateUtil.isSameDay(curTime, hadesLvlTime);


    // base reward
    var rewardsMap = {};
    var exp = 0;
    var baseRewardCfg = hadesLevelConfig.getHadesBaseRewardCfg(mode+"_"+wave);
    if(baseRewardCfg)
    {
        exp += baseRewardCfg.exp;
        rewardsMap = role.getItemsPart().addRewards(baseRewardCfg.reward);
    }

    var evaluation;
    var evaluateRewardCfg;
    for(evaluation=hadesLevelConfig.getHadesLevelBasicCfg().maxEvaluation;evaluation>1;evaluation--)
    {
        evaluateRewardCfg = hadesLevelConfig.getHadesEvaluateRewardCfg(mode+"_"+evaluation);
        if(evaluateRewardCfg && evaluateRewardCfg.maxBossCount>=bossCount)
        {
            break;
        }
    }
    evaluateRewardCfg = hadesLevelConfig.getHadesEvaluateRewardCfg(mode+"_"+evaluation);
    if(evaluateRewardCfg)
    {
        exp += evaluateRewardCfg.exp;
        var evalRewards = role.getItemsPart().addRewards(evaluateRewardCfg.reward);
        for(var k in evalRewards)
        {
            rewardsMap[k] = (rewardsMap[k]||0)+evalRewards[k];
        }
    }

    //role.addExp(exp); 策划要求去掉

    actPart.startBatch();
    actPart.setNumber(enActProp.hadesLvlTime, curTime);
    actPart.setNumber(enActProp.hadesLvlCnt, isToday ? hadesLvlCnt + 1 : 1);
    if (mode > hadesLvlMax && evaluation==globalDefine.MAX_EVALUATION)
        actPart.setNumber(enActProp.hadesLvlMax, mode);

    actPart.endBatch();



    var netMsg = new activityMessage.EndHadesLevelResultVo();
    netMsg.evaluation = evaluation;
    netMsg.exp = exp;
    netMsg.wave = wave;
    netMsg.bossCount = bossCount;
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
 * @param {SweepHadesLevelVo} reqObj
 */
function sweepHadesLevel(session, role, msgObj, reqObj)
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

    var retObj = checkEnterHadesLevel(role, mode);
    if (retObj.code !== ResultCode.SUCCESS)
    {
        msgObj.setResponseData(retObj.code, retObj.obj);
        role.send(msgObj);
        return;
    }

    retObj = doEndHadesLevel(role, hadesLevelConfig.getHadesLevelBasicCfg().maxWave, 0);
    msgObj.setResponseData(retObj.code, retObj.obj);
    role.send(msgObj);
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_SWEEP_HADES_LEVEL, sweepHadesLevel, activityMessage.SweepHadesLevelVo, true);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {EndHadesLevelVo} reqObj
 */
function endHadesLevel(session, role, msgObj, reqObj)
{
    var retObj = doEndHadesLevel(role, reqObj.wave, reqObj.bossCount);
    msgObj.setResponseData(retObj.code, retObj.obj);
    role.send(msgObj);
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_END_HADES_LEVEL, endHadesLevel, activityMessage.EndHadesLevelVo, true);