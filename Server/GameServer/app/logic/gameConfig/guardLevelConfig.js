"use strict";

var gameConfig = require("./gameConfig");

class GuardLevelBasicCfg
{
    constructor() {
        this.dayMaxCnt = 0;
        this.maxEvaluation = 0;
        this.limitTime = 0;
        this.coolDown = 0;
    }

    static fieldsDesc() {
        return {
            dayMaxCnt: {type: Number},
            maxEvaluation: {type: Number},
            limitTime: {type: Number},
            coolDown: {type: Number},
        };
    }
}

class GuardLevelModeCfg
{
    constructor() {
        this.mode = 0;
        this.roomId = "";
        this.openLevel = 0;
    }

    static fieldsDesc() {
        return {
            mode: {type: Number},
            roomId: {type: String},
            openLevel: {type: Number},
        };
    }
}

class GuardBaseRewardCfg
{
    constructor() {
        this.id = "";
        this.exp = 0;
        this.reward = 0;
    }

    static fieldsDesc() {
        return {
            id: {type: String},
            exp: {type: Number},
            reward: {type: Number},
        };
    }
}

class GuardEvaluateRewardCfg
{
    constructor() {
        this.id = "";
        this.exp = 0;
        this.reward = 0;
    }

    static fieldsDesc() {
        return {
            id: {type: String},
            exp: {type: Number},
            reward: {type: Number},
        };
    }
}

class GuardEvaluateCfg
{
    constructor() {
        this.evaluate = 0;
        this.point = 0;
    }

    static fieldsDesc() {
        return {
            evaluate: {type: Number},
            point: {type: Number},
        };
    }
}

/**
 *
 * @returns {GuardLevelBasicCfg}
 */
function getGuardLevelBasicCfg()
{
    return gameConfig.getCsvConfig("guardLevelBasic")[0];
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {GuardLevelModeCfg}
 */
function getGuardLevelModeCfg(key)
{
    return gameConfig.getCsvConfig("guardLevelMode", key);
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {GuardBaseRewardCfg}
 */
function getGuardBaseRewardCfg(key)
{
    return gameConfig.getCsvConfig("guardBaseReward", key);
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {GuardEvaluateRewardCfg}
 */
function getGuardEvaluateRewardCfg(key)
{
    return gameConfig.getCsvConfig("guardEvaluateReward", key);
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {GuardEvaluateCfg}
 */
function getGuardEvaluateCfg(key)
{
    return gameConfig.getCsvConfig("guardEvaluate", key);
}



exports.GuardLevelBasicCfg = GuardLevelBasicCfg;
exports.GuardLevelModeCfg = GuardLevelModeCfg;
exports.GuardBaseRewardCfg = GuardBaseRewardCfg;
exports.GuardEvaluateRewardCfg = GuardEvaluateRewardCfg;
exports.GuardEvaluateCfg = GuardEvaluateCfg;
exports.getGuardLevelBasicCfg = getGuardLevelBasicCfg;
exports.getGuardLevelModeCfg = getGuardLevelModeCfg;
exports.getGuardBaseRewardCfg = getGuardBaseRewardCfg;
exports.getGuardEvaluateRewardCfg = getGuardEvaluateRewardCfg;
exports.getGuardEvaluateCfg = getGuardEvaluateCfg;
