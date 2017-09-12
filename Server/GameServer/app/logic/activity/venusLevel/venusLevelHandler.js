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
var venusLevelConfig = require("../../gameConfig/venusLevelConfig");
var enItemId = require("../../enumType/globalDefine").enItemId;
var enSystemId = require("../../enumType/systemDefine").enSystemId;
var systemMgr = require("../../system/systemMgr");

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {EnterVenusLevelVo} reqObj
 */
function enterVenusLevel(session, role, msgObj, reqObj)
{
    var basicCfg = venusLevelConfig.getVenusLevelBasicCfg();

    var actPart = role.getActivityPart();
    var venusLvlTime = actPart.getNumber(enActProp.venusLvlTime);
    var venusLvlEnt1 = actPart.getNumber(enActProp.venusLvlEntered1);
    var venusLvlEnt2 = actPart.getNumber(enActProp.venusLvlEntered2);

    var errObj = {};
    if(!systemMgr.isEnabled(role,enSystemId.venusLevel,errObj))
    {
        logUtil.info(errObj.errMsg);
        msgObj.setResponseData(ResultCodeActivity.ACTIVITY_NOT_START);
        role.send(msgObj);
        return;
    }

    //如果上次不是今天，那就清0
    if (!dateUtil.isToday(venusLvlTime)) {
        venusLvlEnt1 = 0;
        venusLvlEnt2 = 0;
    }

    //判断当前活动区间
    var index = 0;
    if (dateUtil.isNowBetweenTimeEx(basicCfg.openTime1, 0, basicCfg.closeTime1, 0))
    {
        index = 1;
    }
    else if (dateUtil.isNowBetweenTimeEx(basicCfg.openTime2, 0, basicCfg.closeTime2, 0))
    {
        index = 2;
    }

    if(index == 0)
    {
        msgObj.setResponseData(ResultCodeActivity.ACTIVITY_NOT_START);
        role.send(msgObj);
        return;
    }

    //次数检查
    if ((index == 1 && venusLvlEnt1 > 0) || (index == 2 && venusLvlEnt2 > 0))
    {
        msgObj.setResponseData(ResultCodeActivity.ACTIVITY_ENTERED);
        role.send(msgObj);
        return;
    }

    var serverData = actPart.getServerData();
    serverData.venusLevelCurIdx = index;

    msgObj.setResponseData(ResultCode.SUCCESS);
    role.send(msgObj);
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_ENTER_VENUS_LEVEL, enterVenusLevel, activityMessage.EnterVenusLevelVo, true);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {EndVenusLevelVo} reqObj
 */
function endVenusLevel(session, role, msgObj, reqObj)
{
    var actPart = role.getActivityPart();
    var serverData = actPart.getServerData();

    var curIdx = serverData.venusLevelCurIdx;
    if (!curIdx)
    {
        msgObj.setResponseData(ResultCodeActivity.LVL_NO_START);
        role.send(msgObj);
        return;
    }
    delete serverData.venusLevelCurIdx;

    var venusLvlTime = actPart.getNumber(enActProp.venusLvlTime);
    //var venusLvlEnt1 = actPart.getNumber(enActProp.venusLvlEntered1);
    //var venusLvlEnt2 = actPart.getNumber(enActProp.venusLvlEntered2);

    var curTime = dateUtil.getTimestamp();
    //var isToday = dateUtil.isSameDay(curTime, venusLvlTime);

    var rewardCfg = venusLevelConfig.getVenusLevelRewardCfg(reqObj.evaluation);
    if(rewardCfg.minPercentage>reqObj.percentage)
    {
        msgObj.setResponseData(ResultCodeActivity.LVL_NO_START);
        role.send(msgObj);
        return;
    }

    role.addStamina(rewardCfg.stamina);
    role.getItemsPart().addItem(enItemId.REDSOUL, rewardCfg.soul);

    actPart.startBatch();
    actPart.setNumber(enActProp.venusLvlTime, curTime);
    if(curIdx==1) {
        actPart.setNumber(enActProp.venusLvlEntered1, 1);
    }
    else
    {
        actPart.setNumber(enActProp.venusLvlEntered2, 1);
    }

    actPart.endBatch();



    var netMsg = new activityMessage.EndVenusLevelResultVo();
    netMsg.evaluation = reqObj.evaluation;
    netMsg.percentage = reqObj.percentage;
    netMsg.soul = rewardCfg.soul;
    netMsg.stamina = rewardCfg.stamina;


    msgObj.setResponseData(ResultCode.SUCCESS, netMsg);
    role.send(msgObj);
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_END_VENUS_LEVEL, endVenusLevel, activityMessage.EndVenusLevelVo, true);