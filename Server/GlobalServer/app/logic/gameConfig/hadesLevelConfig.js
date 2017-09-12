"use strict";

var gameConfig = require("./gameConfig");

class HadesLevelBasicCfg
{
    constructor() {
        this.dayMaxCnt = 0;
        this.maxWave = 0;
        this.maxEvaluation = 0;
        this.limitTime = 0;
    }

    static fieldsDesc() {
        return {
            dayMaxCnt: {type: Number},
            maxWave: {type: Number},
            maxEvaluation: {type: Number},
            limitTime: {type: Number},
        };
    }
}

class HadesLevelModeCfg
{
    constructor() {
        this.mode = 0;
        this.roomId = "";
        this.openLevel = 0;
        this.bossId = "";
        this.waveFlag = "";
    }

    static fieldsDesc() {
        return {
            mode: {type: Number},
            roomId: {type: String},
            openLevel: {type: Number},
            bossId: {type: String},
            waveFlag: {type: String},
        };
    }
}

class HadesBaseRewardCfg
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

class HadesEvaluateRewardCfg
{
    constructor() {
        this.id = "";
        this.maxBossCount = 0;
        this.exp = 0;
        this.reward = 0;
    }

    static fieldsDesc() {
        return {
            id: {type: String},
            maxBossCount: {type: Number},
            exp: {type: Number},
            reward: {type: Number},
        };
    }
}

/**
 *
 * @returns {HadesLevelBasicCfg}
 */
function getHadesLevelBasicCfg()
{
    return gameConfig.getCsvConfig("hadesLevelBasic")[0];
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {HadesLevelModeCfg}
 */
function getHadesLevelModeCfg(key)
{
    return gameConfig.getCsvConfig("hadesLevelMode", key);
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {HadesBaseRewardCfg}
 */
function getHadesBaseRewardCfg(key)
{
    return gameConfig.getCsvConfig("hadesBaseReward", key);
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {HadesEvaluateRewardCfg}
 */
function getHadesEvaluateRewardCfg(key)
{
    return gameConfig.getCsvConfig("hadesEvaluateReward", key);
}

exports.HadesLevelBasicCfg = HadesLevelBasicCfg;
exports.HadesLevelModeCfg = HadesLevelModeCfg;
exports.HadesBaseRewardCfg = HadesBaseRewardCfg;
exports.HadesEvaluateRewardCfg = HadesEvaluateRewardCfg;
exports.getHadesLevelBasicCfg = getHadesLevelBasicCfg;
exports.getHadesLevelModeCfg = getHadesLevelModeCfg;
exports.getHadesBaseRewardCfg = getHadesBaseRewardCfg;
exports.getHadesEvaluateRewardCfg = getHadesEvaluateRewardCfg;