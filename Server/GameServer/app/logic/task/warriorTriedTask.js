
"use strict";
var actPropDefine = require("../enumType/activityPropDefine");
var taskPropDefine = require("../enumType/taskPropDefine");
var TaskRewardConfig=require("../gameConfig/taskRewardConfig");
var warriorTriedConfig =  require("../gameConfig/warriorTriedConfig");
var dateUtil = require("../../libs/dateUtil");

////////////////////////////////////////
const enTaskProp = taskPropDefine.taskProp;
const enTaskType = taskPropDefine.taskType;
const enActProp = actPropDefine.enActProp;
class WarriorTriedTask
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
        var taskRewardConfig=TaskRewardConfig.getTaskRewardConfig(taskId);
        var taskFields = taskRewardConfig.taskField;
        var taskProp = taskRewardConfig.taskProp;
        var taskRewardTime = enTaskProp[taskRewardConfig.taskRewardTime];

        var time,cnt;
        var activityPart = this._role.getActivityPart();

        var warriorTriedInfo = activityPart.getWarriorTriedData();
        var warriotTriedCfg = warriorTriedConfig.getTriedBaseConfig()
        time = warriorTriedInfo.uptime;
        cnt = warriotTriedCfg.dailyNum - warriorTriedInfo.remainTried;

        var taskPart=this._role.getTaskPart();
        var taskGetTime=taskPart.getNumber(taskRewardTime);
        if(!dateUtil.isToday(taskGetTime) && dateUtil.isToday(time) && cnt >= taskProp)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



}

exports.WarriorTriedTask = WarriorTriedTask;