
"use strict";
var opActPropDefine = require("../enumType/opActivityPropDefine");
var taskPropDefine = require("../enumType/taskPropDefine");
var TaskRewardConfig=require("../gameConfig/taskRewardConfig");
var dateUtil = require("../../libs/dateUtil");

////////////////////////////////////////
const enTaskProp = taskPropDefine.taskProp;
const enTaskType = taskPropDefine.taskType;
const enOpActProp = opActPropDefine.enOpActProp;

class CostTask
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
        var taskFields = taskRewardConfig.taskField;
        var taskProp = taskRewardConfig.taskProp;
        var taskRewardTime = enTaskProp[taskRewardConfig.taskRewardTime];

        var totalCost;
        var opActivityPart = this._role.getOpActivityPart();

        if(taskFields.length == 1)
        {
            totalCost =  dateUtil.isToday(opActivityPart.getNumber(enOpActProp.lastCostDiamond))? opActivityPart.getNumber(enOpActProp[taskFields[0]]) : 0;
        }
        else
            return false

        var taskPart = this._role.getTaskPart();
        var taskGetTime = taskPart.getNumber(taskRewardTime);
        if(!dateUtil.isToday(taskGetTime) && totalCost >= taskProp)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



}

exports.CostTask = CostTask;