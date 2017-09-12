"use strict";

var gameConfig = require("./gameConfig");

class TreasureRobBasicCfg
{
    constructor() {
        this.dayMaxCnt = 0;
        this.dayMaxRob = 0;
        this.fleshGold = 0;
        this.minPowerRate = 0;
        this.maxPowerRate = 0;
    }

    static fieldsDesc() {
        return {
            dayMaxCnt: {type: Number},
            dayMaxRob: {type: Number},
            fleshGold: {type: Number},
            minPowerRate: {type: Number},
            maxPowerRate: {type: Number},
        };
    }
}

class TreasureRobPieceCfg
{
    constructor() {
        this.count = 0;
        this.rate = 0;
    }

    static fieldsDesc() {
        return {
            count: {type: Number},
            rate: {type: Number},
        };
    }
}

/**
 *
 * @returns {TreasureRobBasicCfg}
 */
function getTreasureRobBasicCfg()
{
    return gameConfig.getCsvConfig("treasureRobBasic")[0];
}

/**
 *
 * @returns {array.<TreasureRobPieceCfg>}
 */
function getTreasureRobPieceCfg()
{
    return gameConfig.getCsvConfig("treasureRobPiece");
}

exports.TreasureRobBasicCfg = TreasureRobBasicCfg;
exports.TreasureRobPieceCfg = TreasureRobPieceCfg;
exports.getTreasureRobBasicCfg = getTreasureRobBasicCfg;
exports.getTreasureRobPieceCfg = getTreasureRobPieceCfg;