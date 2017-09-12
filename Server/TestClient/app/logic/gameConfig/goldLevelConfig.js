"use strict";

var gameConfig = require("./gameConfig");

class GoldLevelBasicCfg
{
    constructor() {
        this.dayMaxCnt = 0;
        this.coolDown = 0;
        this.flyGoldGap = 0;
        this.flyGoldShowNum = 0;
        this.rateBHP = 0;
        this.rateAHP = 0;
        this.rateSHP = 0;
        this.rateSSHP = 0;
        this.rateSSSHP = 0;
        this.rateCBuf = 0;
        this.rateBBuf = 0;
        this.rateABuf = 0;
        this.rateSBuf = 0;
        this.rateSSBuf = 0;
        this.rateSSSBuf = 0;
    }

    static fieldsDesc() {
        return {
            dayMaxCnt: {type: Number},
            coolDown: {type: Number},
            flyGoldGap: {type: Number},
            flyGoldShowNum: {type: Number},
            rateBHP: {type: Number},
            rateAHP: {type: Number},
            rateSHP: {type: Number},
            rateSSHP: {type: Number},
            rateSSSHP: {type: Number},
            rateCBuf: {type: Number},
            rateBBuf: {type: Number},
            rateABuf: {type: Number},
            rateSBuf: {type: Number},
            rateSSBuf: {type: Number},
            rateSSSBuf: {type: Number},
        };
    }
}

class GoldLevelModeCfg
{
    constructor() {
        this.mode = 0;
        this.roomId = "";
        this.monsterId = "";
        this.openLevel = 0;
        this.maxGold = 0;
        this.basicGold = 0;
        this.goldFactor = 0.0;
        this.limitTime = 0;
        this.rateBItemID = 0;
        this.rateBItemCount = 0;
        this.rateAItemID = 0;
        this.rateAItemCount = 0;
        this.rateSItemID = 0;
        this.rateSItemCount = 0;
        this.rateSSItemID = 0;
        this.rateSSItemCount = 0;
        this.rateSSSItemID = 0;
        this.rateSSSItemCount = 0;
    }

    static fieldsDesc() {
        return {
            mode: {type: Number},
            roomId: {type: String},
            monsterId: {type: String},
            openLevel: {type: Number},
            maxGold: {type: Number},
            basicGold: {type: Number},
            goldFactor: {type: Number},
            limitTime: {type: Number},
            rateBItemID: {type: Number},
            rateBItemCount: {type: Number},
            rateAItemID: {type: Number},
            rateAItemCount: {type: Number},
            rateSItemID: {type: Number},
            rateSItemCount: {type: Number},
            rateSSItemID: {type: Number},
            rateSSItemCount: {type: Number},
            rateSSSItemID: {type: Number},
            rateSSSItemCount: {type: Number},
        };
    }
}

/**
 *
 * @returns {GoldLevelBasicCfg}
 */
function getGoldLevelBasicCfg()
{
    return gameConfig.getCsvConfig("goldLevelBasic")[0];
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {GoldLevelModeCfg}
 */
function getGoldLevelModeCfg(key)
{
    return gameConfig.getCsvConfig("goldLevelMode", key);
}
exports.GoldLevelBasicCfg = GoldLevelBasicCfg;
exports.GoldLevelModeCfg = GoldLevelModeCfg;
exports.getGoldLevelBasicCfg = getGoldLevelBasicCfg;
exports.getGoldLevelModeCfg = getGoldLevelModeCfg;