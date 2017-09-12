
"use strict";
var opActPropDefine = require("../enumType/opActivityPropDefine");
var taskPropDefine = require("../enumType/taskPropDefine");
var TaskRewardConfig=require("../gameConfig/taskRewardConfig");
var dateUtil = require("../../libs/dateUtil");
var enProp = require("../enumType/propDefine").enProp;

////////////////////////////////////////
const enTaskProp = taskPropDefine.taskProp;
const enTaskType = taskPropDefine.taskType;
const enOpActProp = opActPropDefine.enOpActProp;

class VipTask
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
        var taskProp = taskRewardConfig.taskProp;
        var taskRewardTime = enTaskProp[taskRewardConfig.taskRewardTime];
        var currentVipLv = this._role.getNumber(enProp.vipLv);

        var taskPart = this._role.getTaskPart();
        var taskGetTime = taskPart.getNumber(taskRewardTime);
        if(!dateUtil.isToday(taskGetTime) && currentVipLv >= taskProp)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



}

exports.VipTask = VipTask;