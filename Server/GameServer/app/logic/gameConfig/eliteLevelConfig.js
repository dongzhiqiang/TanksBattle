"use strict";

var gameConfig = require("./gameConfig");

class EliteLevelBasicCfg
{
    constructor() {
        this.dayMaxCnt = 0;
        this.costStamina = 0;
        this.resetCost = 0;
    }

    static fieldsDesc() {
        return {
            dayMaxCnt: {type: Number},
            costStamina: {type: Number},
            resetCost: {type: Number},
        };
    }
}

class EliteLevelCfg
{
    constructor() {
        this.id = 0;
        this.name = "";
        this.subname = "";
        this.roomId = "";
        this.openLevel = 0;
        this.firstReward = 0;
        this.reward = "";
        this.openPassLvl = "";
        this.openPassEltLvl = 0;
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            name: {type: String},
            subname: {type: String},
            roomId: {type: String},
            openLevel: {type: Number},
            firstReward: {type: Number},
            reward: {type: String},
            openPassLvl: {type: String},
            openPassEltLvl: {type: Number},
        };
    }
}

class EliteLevelResetCfg
{
    constructor() {
        this.count = 0;
        this.costDiamond = 0;
    }

    static fieldsDesc() {
        return {
            count: {type: Number},
            costDiamond: {type: Number},
        };
    }
}

/**
 *
 * @returns {EliteLevelBasicCfg}
 */
function getEliteLevelBasicCfg()
{
    return gameConfig.getCsvConfig("eliteLevelBasic")[0];
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {EliteLevelCfg}
 */
function getEliteLevelCfg(key)
{
    return gameConfig.getCsvConfig("eliteLevel", key);
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {EliteLevelResetCfg}
 */
function getEliteLevelResetCfg(key)
{
    return gameConfig.getCsvConfig("eliteLevelReset", key);
}

exports.EliteLevelBasicCfg = EliteLevelBasicCfg;
exports.EliteLevelCfg = EliteLevelCfg;
exports.EliteLevelResetCfg = EliteLevelResetCfg;
exports.getEliteLevelBasicCfg = getEliteLevelBasicCfg;
exports.getEliteLevelCfg = getEliteLevelCfg;
exports.getEliteLevelResetCfg = getEliteLevelResetCfg;