
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
class EliteLvTask
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

        var cnt = 0;
        var eliteLvPart = this._role.getEliteLevelsPart();

        var eliteLvInfo = eliteLvPart.getEliteLevels();

       for(let i=0;i<eliteLvInfo.length;++i)
        {
            if(dateUtil.isToday(eliteLvInfo[i].enterTime))
                cnt += eliteLvInfo[i].count;
        }

        var taskPart=this._role.getTaskPart();
        var taskGetTime = taskPart.getNumber(taskRewardTime);
        if(!dateUtil.isToday(taskGetTime)  && cnt >= taskProp)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



}

exports.EliteLvTask = EliteLvTask;