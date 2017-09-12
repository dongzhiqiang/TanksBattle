/**
 * Created by pc20 on 2016/8/26.
 */
"use strict";


var logUtil = require("../../../libs/logUtil");
var ModuleIds = require(".././../netMessage/netMsgConst").ModuleIds;
var handlerMgr = require("../../session/handlerMgr");
var ResultCode = require(".././../netMessage/netMsgConst").ResultCode;
var activityMessage = require("../../netMessage/activityMessage");
var CmdIdsActivity = require(".././../netMessage/activityMessage").CmdIdsActivity;
var ResultCodeActivity = require(".././../netMessage/activityMessage").ResultCodeActivity;
var enProp = require("../../enumType/propDefine").enProp;
var towerConfig = require("../../gameConfig/prophetTowerConfig");
var dateUtil = require("../../../libs/dateUtil");
var valueConfig = require("../../gameConfig/valueConfig");
var towerRewardConfig = require("../../gameConfig/prophetTowerRewardConfig");
var appUtil = require("../../../libs/appUtil");
var enSystemId = require("../../enumType/systemDefine").enSystemId;
var systemMgr = require("../../system/systemMgr");


/***
 * 请求进入关卡处理
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {EnterTowerReq} reqObj
 * @returns {number}
 */
function enterTowerLevel(session, role, msgObj, reqObj)
{
    var type = reqObj.towerType;
    var roomId = "";
    var levelNum = role.getNumber(enProp.towerLevel);
    var cfg = towerConfig.getProphetTowerConfig(levelNum);
    if (cfg == null) {
        return ResultCodeActivity.TOWER_NO_TYPE;
    }

    var errObj={};
    if(!systemMgr.isEnabled(role,enSystemId.prophetTower,errObj))
    {
        //logUtil.info(errObj.errMsg);
        return ResultCodeActivity.SYSTEM_NOT_OPEN;
    }

    //不是同一天 重置数据
    var towerData = role.getActivityPart().getProphetTowerData();
    var time = role.getNumber(enProp.towerEnterTime);
    if(!dateUtil.isSameDay(time, dateUtil.getTimestamp()))
    {
        if (cfg.range[1] - cfg.range[0] < 4) {
            return ResultCode.CONFIG_ERROR;
        }

        var a = {};
        while (Object.getOwnPropertyNames(a).length < 5) {
            var num = appUtil.getRandom(cfg.range[0],cfg.range[1]).toString();
            if (!a.hasOwnProperty(num))
                a[num] = 0;
        }

        towerData.randomId = [];
        for(var key in a) {
            towerData.randomId.push(parseInt(key));
        }
        towerData.randomId = towerData.randomId.sort();
        towerData.getRewardState = [];
        for (var i = 0; i < 5; i++)
        {
            towerData.getRewardState.push(0);
        }

        role.setNumber(enProp.towerEnterNums, 0);
        role.getActivityPart().updateProphetTowerDB();
    }

    //检测进的关卡类型
    if (type == 1) {
        roomId = cfg.roomId;
    }
    else if (type == 2) {
        var openLevel = parseInt(valueConfig.getConfigValueConfig("openTowerRandomLevel")["value"]);
        if (levelNum < openLevel) {
            return ResultCodeActivity.TOWER_CANT_OPEN_RANDOM;
        }

        var time = role.getNumber(enProp.towerEnterTime);
        if(dateUtil.isSameDay(time, dateUtil.getTimestamp()))
        {
            var enterNum = role.getNumber(enProp.towerEnterNums);
            if (enterNum >= parseInt(valueConfig.getConfigValueConfig("towerRandomNum")["value"])) {
                return ResultCodeActivity.DAY_MAX_CNT;
            }
        }

        var num = role.getNumber(enProp.towerEnterNums);
        var tcfg = towerConfig.getProphetTowerConfig(towerData.randomId[num]);
        if (tcfg == null) {
            return ResultCodeActivity.TOWER_NO_TYPE;
        }
        roomId = tcfg.roomId;
    }
    else
    {
        return ResultCodeActivity.TOWER_NO_TYPE;
    }

    role.setNumber(enProp.towerEnterTime, dateUtil.getTimestamp(), false);
    var retBody = new activityMessage.EnterTowerRes();
    retBody.towerType = type;
    retBody.roomId = roomId;
    retBody.towerInfo = role.getActivityPart().getProphetTowerData();
    return retBody;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_ENTER_TOWER, enterTowerLevel, activityMessage.EnterTowerReq, true);

/***
 * 请求结算出来
 * @param session
 * @param role
 * @param msgObj
 * @param reqObj
 * @returns {number}
 */
function endTowerLevel(session, role, msgObj, reqObj)
{
    var levelNum = role.getNumber(enProp.towerLevel);
    var cfg = towerConfig.getProphetTowerConfig(levelNum);
    if (cfg == null) {
        return ResultCodeActivity.TOWER_NO_TYPE;
    }
    var rewardItems;
    if (reqObj.isWin == true) {

        var levelNum = role.getNumber(enProp.towerLevel);
        var cfg = towerConfig.getProphetTowerConfig(levelNum);
        if (cfg == null) {
            return ResultCodeActivity.TOWER_NO_TYPE;
        }
        role.startBatch();
        role.setNumber(enProp.towerWinTime, dateUtil.getTimestamp());
        if (reqObj.towerType == 1) {

            role.setNumber(enProp.towerUseTime, reqObj.useTime);
            role.setNumber(enProp.towerLevel, levelNum+1);
            rewardItems = role.getItemsPart().addRewards(cfg.rewardId);
        }
        else if (reqObj.towerType == 2) {
            var enterNum = role.getNumber(enProp.towerEnterNums);
            if (enterNum >= parseInt(valueConfig.getConfigValueConfig("towerRandomNum")["value"])) {
                return ResultCodeActivity.DAY_MAX_CNT;
            }
            role.setNumber(enProp.towerEnterNums, enterNum+1, false);
        }
        else
        {
            return ResultCodeActivity.TOWER_NO_TYPE;
        }
        role.endBatch();
    }
    role.getActivityPart().updateProphetTowerDB();
    var retBody = new activityMessage.EndTowerRes();
    retBody.towerType = reqObj.towerType;
    retBody.roomId = cfg.roomId;
    retBody.rewards = rewardItems;
    retBody.isWin = reqObj.isWin;
    return retBody;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_END_TOWER, endTowerLevel, activityMessage.EndTowerReq, true);

/***
 * 领取随机挑战的宝箱奖励
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {GetTowerRewardReq} reqObj
 * @returns {number}
 */
function getTowerReward(session, role, msgObj, reqObj)
{
    if (reqObj.idx < 0 || reqObj.idx >= 5)
        return ResultCode.BAD_REQUEST;

    var towerData = role.getActivityPart().getProphetTowerData();
    if (towerData.getRewardState[reqObj.idx] === 1)
        return ResultCode.BAD_REQUEST;

    var levelNum = role.getNumber(enProp.towerLevel);
    var cfg = towerConfig.getProphetTowerConfig(levelNum);
    if (cfg == null) {
        return ResultCodeActivity.TOWER_NO_TYPE;
    }

    var rewardCfg = towerRewardConfig.getProphetTowerRewardConfig(cfg.id);
    if (rewardCfg == null) {
        return ResultCodeActivity.CONFIG_ERROR;
    }

    towerData.getRewardState[reqObj.idx] = 1;
    role.getActivityPart().updateProphetTowerDB();
    role.getItemsPart().addRewards(rewardCfg.rewardId);
    var retBody = new activityMessage.GetTowerRewardRes();
    retBody.idx = reqObj.idx;
    return retBody;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_GET_TOWER_REWARD, getTowerReward, activityMessage.GetTowerRewardReq, true);
