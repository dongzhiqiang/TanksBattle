"use strict";

var handlerMgr = require("../../session/handlerMgr");
var ModuleIds = require(".././../netMessage/netMsgConst").ModuleIds;
var ResultCode = require(".././../netMessage/netMsgConst").ResultCode;
var activityMessage = require("../../netMessage/activityMessage");
var CmdIdsActivity = require(".././../netMessage/activityMessage").CmdIdsActivity;
var enSystemId = require("../../enumType/systemDefine").enSystemId;
var systemMgr = require("../../system/systemMgr");
var ResultCodeActivity = require(".././../netMessage/activityMessage").ResultCodeActivity;
var dbUtil = require("../../../libs/dbUtil");
var WarTriedBaseConfig = require("../../../logic/gameConfig/warriorTriedConfig");
var rewardConfig = require("../../../logic/gameConfig/rewardConfig");

/**
 * 勇士试炼数据结构
 * @typedef {Object} WarriorTried
 * @property {Number} remainTried 剩余试炼次数
 * @property {Number} refresh 今日已刷新次数
 * @property {Number} uptime 更新时间
 * @property {Object[]} trieds 试炼关卡信息
 */


/**
 * 请求勇士试炼的信息，这里会检查时间是否重置，重置了才返回数据，否则是返回空
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {WarriorTriedDataReq} reqObj
 */
function reqWarriorTried(session, role, msgObj, reqObj)
{
    var actPart = role.getActivityPart();
    var isReset = actPart.checkWarriorTriedReset();
    if(!isReset)
        return new activityMessage.WarriorTriedDataRes(null, isReset);
    else
        return new activityMessage.WarriorTriedDataRes(actPart.getWarriorTriedData(), isReset);
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_REQ_WARRIOR_TRIED, reqWarriorTried, activityMessage.WarriorTriedDataReq);

/**
 * 请求刷新勇士试炼
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {RefreshWarriorReq} reqObj
 */
function refreshWarrior(session, role, msgObj, reqObj)
{
    var actPart = role.getActivityPart();
    var result = actPart.refreshWarrior(false);
    if(result)
    {
        var warrior = actPart.getWarriorTriedData();
        return new activityMessage.RefreshWarriorRes(warrior.refresh, warrior.trieds);
    }
    else
        return ResultCode.DIAMOND_INSUFFICIENT;

}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_REFRESH_WARRIOR, refreshWarrior, activityMessage.RefreshWarriorReq);


/**
 * 请求进入勇士试炼
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {EnterWarriorReq} reqObj
 */
function enterWarrior(session, role, msgObj, reqObj)
{
    var errObj = {};
    if(!systemMgr.isEnabled(role,enSystemId.warriorTried, errObj))   //检测下系统是否开启
        return ResultCodeActivity.SYSTEM_NOT_OPEN;

    var part = role.getActivityPart();
    var wt = part.getWarriorTriedData();
    //状态
    if(wt.trieds[reqObj.index].status != 0)  //关卡已通关
        return ResultCodeActivity.LEVEL_HAS_TRIED;
    //次数检查
    if(wt.remainTried == 0)
        return ResultCodeActivity.WARR_TRIED_TIMES_UNABLED;

    return new activityMessage.EnterWarriorRes(wt.trieds[reqObj.index].room, wt.trieds[reqObj.index].star, reqObj.index);

}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_ENTER_WARRIOR_LEVEL, enterWarrior, activityMessage.EnterWarriorReq);



/**
 * 请求结束勇士试炼
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {EndWarriorReq} reqObj
 */
function endWarrior(session, role, msgObj, reqObj)
{
    //检查一下
    var part = role.getActivityPart();
    var wt = part.getWarriorTriedData();
    if(reqObj.isWin)
    {
        part.doEndWarriorWin(reqObj.index);
    }

    return new activityMessage.EndWarriorRes(reqObj.index, reqObj.isWin, part.getWarriorTriedData().remainTried);
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_END_WARRIOR_LEVEL, endWarrior, activityMessage.EndWarriorReq);

/**
 * 领取勇士试炼关卡奖励
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {GetWarriorRewardReq} reqObj
 */
function getWarriorReward(session, role, msgObj, reqObj)
{
    //检查一下
    var part = role.getActivityPart();
    var wt = part.getWarriorTriedData();
    if(wt.trieds[reqObj.index].status != 1)
        return ResultCodeActivity.WARR_CANNOT_REWARD;


    //发放奖励
    var star = wt.trieds[reqObj.index].star;
    var cfg = WarTriedBaseConfig.getStarConfig(star);
    var rewards = rewardConfig.getRandomReward2(cfg.rewardId);
    var items = {};
    for(var i = 0,len = wt.trieds[reqObj.index].rewards.length; i<len;++i)
    {
        items[wt.trieds[reqObj.index].rewards[i].itemId] = (items[wt.trieds[reqObj.index].rewards[i].itemId] || 0) + wt.trieds[reqObj.index].rewards[i].num;
    }
    //给物品
    role.getItemsPart().addItems(items);
    wt.trieds[reqObj.index].status = 2;
    var db = dbUtil.getDB(role.getUserId());
    var col = db.collection("role");
    col.updateOneNoThrow({"props.heroId":role.getHeroId()}, {"$set":{"warriorTried.trieds":wt.trieds}});

    //如果所有关卡奖励都领完了 可以自动刷新
    var allFinish = true;
    for(var i = 0,len = wt.trieds.length; i<len; ++i)
    {
        if(wt.trieds[i].status != 2)
        {
            allFinish = false;
            break;
        }
    }
    if(allFinish)   //全部完成
    {
        role.getActivityPart().refreshWarrior(true);
        return new activityMessage.GetWarriorRewardRes(reqObj.index, wt.trieds);
    }
    return new activityMessage.GetWarriorRewardRes(reqObj.index, null);

}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_WARRIOR_REWARD, getWarriorReward, activityMessage.GetWarriorRewardReq);