"use strict";

var gameConfig = require("./gameConfig");


class TreasureConfig
{
    constructor() {
        this.id = 0;
        this.name = "";
        this.mode = "";
        this.pieceId = 0;
        this.skillId = "";
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            name: {type: String},
            mode: {type: String},
            pieceId: {type: Number},
            skillId: {type: String},
        };
    }
}


/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {TreasureConfig}
 */
function getTreasureConfig(key)
{
    return gameConfig.getCsvConfig("treasure", key);
}




var map={};

class TreasureLevelConfig
{
    constructor() {
        this.level = 0;
        this.id = 0;
        this.attributeId = "";
        this.pieceNum = 0;
        this.costGold = 0;
        this.power = 0;
        this.powerRate = 0;
        this.description = "";
    }

    static fieldsDesc() {
        return {
            level: {type: Number},
            id: {type: Number},
            attributeId: {type: String},
            pieceNum: {type: Number},
            costGold: {type: Number},
            power: {type: Number},
            powerRate: {type: Number},
            description: {type: String},
        };
    }

    static afterReadAll(rows)
    {
        let i = rows.length;
        while (i--) {
            let cfg =rows[i];
            let temp = map[cfg.id];
            if(!temp)
                map[cfg.id] =temp = {};
            temp[cfg.level] = cfg;
        }

    }



}

/**
 *
 * @param {number} id
 * @param {number} level
 * @returns {TreasureLevelConfig}
 */
function getTreasureLevelConfig(id,level)
{
    let temp =map[id];
    return temp&&temp[level];
}



exports.TreasureConfig = TreasureConfig;
exports.getTreasureConfig = getTreasureConfig;
exports.TreasureLevelConfig = TreasureLevelConfig;
exports.getTreasureLevelConfig = getTreasureLevelConfig;

