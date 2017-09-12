"use strict";

var gameConfig = require("./gameConfig");

class VenusLevelBasicCfg
{
    constructor() {
        this.openTime1 = 0;
        this.closeTime1 = 0;
        this.openTime2 = 0;
        this.closeTime2 = 0;
        this.roomId = "";
    }

    static fieldsDesc() {
        return {
            openTime1: {type: Number},
            closeTime1: {type: Number},
            openTime2: {type: Number},
            closeTime2: {type: Number},
            roomId: {type: String},
        };
    }
}

class VenusLevelRewardCfg
{
    constructor() {
        this.evaluate = 0;
        this.minPercentage = 0;
        this.soul = 0;
        this.stamina = 0;
    }

    static fieldsDesc() {
        return {
            evaluate: {type: Number},
            minPercentage: {type: Number},
            soul: {type: Number},
            stamina: {type: Number},
        };
    }
}



/**
 *
 * @returns {VenusLevelBasicCfg}
 */
function getVenusLevelBasicCfg()
{
    return gameConfig.getCsvConfig("venusLevelBasic")[0];
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {VenusLevelRewardCfg}
 */
function getVenusLevelRewardCfg(key)
{
    return gameConfig.getCsvConfig("venusLevelReward", key);
}


exports.VenusLevelBasicCfg = VenusLevelBasicCfg;
exports.getVenusLevelBasicCfg = getVenusLevelBasicCfg;
exports.VenusLevelRewardCfg = VenusLevelRewardCfg;
exports.getVenusLevelRewardCfg = getVenusLevelRewardCfg;