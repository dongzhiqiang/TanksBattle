"use strict";
var handlerMgr = require("../session/handlerMgr");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var actPropDefine = require("../enumType/activityPropDefine");
var taskPropDefine = require("../enumType/taskPropDefine");
var TaskRewardConfig=require("../gameConfig/taskRewardConfig");
var CmdIdsTask = require("../netMessage/taskMessage").CmdIdsTask;
var ResultCodeTask = require("../netMessage/taskMessage").ResultCodeTask;
var GetTaskRewardReq = require("../netMessage/taskMessage").GetTaskRewardReq;
var GetTaskRewardRes = require("../netMessage/taskMessage").GetTaskRewardRes;
var GetVitalityRewardReq = require("../netMessage/taskMessage").GetVitalityRewardReq;
var GetVitalityRewardRes = require("../netMessage/taskMessage").GetVitalityRewardRes;
var dateUtil = require("../../libs/dateUtil");

////////////////////////////////////////
const enTaskProp = taskPropDefine.taskProp;
const enTaskType = taskPropDefine.taskType;
const enActProp = actPropDefine.enActProp;


/**
 * 领取每日任务奖励
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 *  @param {GetTaskRewardReq} reqObj
 * @return {GetTaskRewardRes|Number}
 */
function getDailyTaskReward(session, role, msgObj,reqObj) {

    var taskId = reqObj.taskId;
    var taskPart=role.getTaskPart();
    if(taskPart.getDailyTaskReward(taskId))
        return new GetTaskRewardRes(taskId);
    else
        return ResultCodeTask.GET_DAILY_TASK_REWARD_FAILED;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_TASK, CmdIdsTask.CMD_GET_DAILY_TASK_REWARD, getDailyTaskReward, GetTaskRewardReq);

/**
 * 领取活跃度奖励
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 *  @param {GetVitalityRewardReq} reqObj
 * @return {GetVitalityRewardRes|Number}
 */
function getVitalityReward(session, role, msgObj,reqObj) {

    var vitalityId = reqObj.vitalityId;
    var taskPart=role.getTaskPart();
    if(taskPart.getVitalityReward(vitalityId))
        return new GetVitalityRewardRes(vitalityId);
    else
        return ResultCodeTask.GET_VITALITY_REWARD_FAILED;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_TASK, CmdIdsTask.CMD_GET_VITALITY_REWARD, getVitalityReward, GetVitalityRewardReq);

/**
 * 领取成长任务奖励
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 *  @param {GetTaskRewardReq} reqObj
 * @return {GetTaskRewardRes|Number}
 */
function getGrowthTaskReward(session, role, msgObj,reqObj) {

    var taskId = reqObj.taskId;
    var taskPart=role.getTaskPart();
    if(taskPart.getGrowthTaskReward(taskId))
        return new GetTaskRewardRes(taskId);
    else
        return ResultCodeTask.GET_GROWTH_TASK_REWARD_FAILED;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_TASK, CmdIdsTask.CMD_GET_GROWTH_TASK_REWARD, getGrowthTaskReward, GetTaskRewardReq);

