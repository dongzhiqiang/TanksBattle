
"use strict";
var opActPropDefine = require("../enumType/opActivityPropDefine");
var taskPropDefine = require("../enumType/taskPropDefine");
var TaskRewardConfig=require("../gameConfig/taskRewardConfig");
var dateUtil = require("../../libs/dateUtil");

////////////////////////////////////////
const enTaskProp = taskPropDefine.taskProp;
const enTaskType = taskPropDefine.taskType;
const enOpActProp = opActPropDefine.enOpActProp;

class LotteryTask
{
    constructor(role) {
        /**
         * @type {Role}
         */
        Object.defineProperty(this, "_role", {enumerable: true, value: role});

    }

    /**
     *
     * @param {number}taskId
     * @returns {Boolean}
     */
    canGetReward(taskId)
    {
        var taskRewardConfig = TaskRewardConfig.getTaskRewardConfig(taskId);
        var taskType = taskRewardConfig.taskType;
        var taskFields = taskRewardConfig.taskField;
        var taskProp = taskRewardConfig.taskProp;
        var taskRewardTime = enTaskProp[taskRewardConfig.taskRewardTime];

        var time1,time10;
        var opActivityPart = this._role.getOpActivityPart();

        if(taskFields.length == 2)
        {
            time1 = opActivityPart.getNumber(enOpActProp[taskFields[0]]);
            time10 = opActivityPart.getNumber(enOpActProp[taskFields[1]]);
        }
        else
            return false

        var taskPart = this._role.getTaskPart();
        var taskGetTime = taskPart.getNumber(taskRewardTime);
        if(!dateUtil.isToday(taskGetTime) && (dateUtil.isToday(time1)||dateUtil.isToday(time10)))
        {
            return true;
        }
        else
        {
            return false;
        }
    }



}

exports.LotteryTask = LotteryTask;