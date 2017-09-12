
"use strict";
var opActPropDefine = require("../enumType/opActivityPropDefine");
var taskPropDefine = require("../enumType/taskPropDefine");
var TaskRewardConfig=require("../gameConfig/taskRewardConfig");
var dateUtil = require("../../libs/dateUtil");

////////////////////////////////////////
const enTaskProp = taskPropDefine.taskProp;
const enTaskType = taskPropDefine.taskType;
const enOpActProp = opActPropDefine.enOpActProp;

class CorpsTask
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
        var taskRewardTime = enTaskProp[taskRewardConfig.taskRewardTime];

        var taskPart = this._role.getTaskPart();
        var taskGetTime = taskPart.getNumber(taskRewardTime);
        if(!dateUtil.isToday(taskGetTime) && this._role.getCorpsPart().getOwnBuildStateAndCheckUpdate())
        {
            return true;
        }
        else
        {
            return false;
        }
    }



}

exports.CorpsTask = CorpsTask;