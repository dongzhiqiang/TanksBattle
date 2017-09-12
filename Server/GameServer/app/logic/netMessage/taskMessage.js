"use strict";

const CmdIdsTask=
{
    CMD_GET_DAILY_TASK_REWARD:1,
    CMD_GET_VITALITY_REWARD : 2,
    CMD_GET_GROWTH_TASK_REWARD : 3,

    PUSH_SYNC_PROP: -1 //同步属性
};

const ResultCodeTask=
{
    GET_DAILY_TASK_REWARD_FAILED : 1,
    GET_VITALITY_REWARD_FAILED : 2,
    GET_GROWTH_TASK_REWARD_FAILED : 3,
}


/////////////////////////////////请求类////////////////////////////

class GetTaskRewardReq {
    /**
     * @param {Number} taskId
     */
    constructor(taskId) {
        this.taskId = taskId;
    }

    static fieldsDesc() {
        return {
            taskId: {type: Number, notNull: true},
        };
    }
}

class GetVitalityRewardReq {
    /**
     * @param {Number} vitalityId
     */
    constructor(vitalityId) {
        this.vitalityId = vitalityId;
    }

    static fieldsDesc() {
        return {
            vitalityId: {type: Number, notNull: true},
        };
    }
}

/////////////////////////////////回复类////////////////////////////

class GetTaskRewardRes
{
    /**
     * @param {Number} taskId
     */
    constructor(taskId) {
        this.taskId = taskId;
    }
}

class GetVitalityRewardRes
{
    /**
     * @param {Number} vitalityId
     */
    constructor(vitalityId)
    {
        this.vitalityId = vitalityId;
    }
}


//////////////////////////////
class SyncTaskPropVo {
    /**
     *
     * @param {object.<string, *>} props
     */
    constructor(props) {
        this.props = props;
    }
}
exports.CmdIdsTask=CmdIdsTask;
exports.ResultCodeTask=ResultCodeTask;
exports.GetTaskRewardReq=GetTaskRewardReq;
exports.GetTaskRewardRes=GetTaskRewardRes;
exports.GetVitalityRewardReq=GetVitalityRewardReq;
exports.GetVitalityRewardRes=GetVitalityRewardRes;
exports.SyncTaskPropVo=SyncTaskPropVo;
