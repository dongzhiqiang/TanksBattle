"use strict";

var gameConfig = require("./gameConfig");

class PetAdvLvPropRateConfig
{
    constructor() {
        this.advLv = 0;
        this.needLv = 0;
        this.quality = 0;
        this.qualityLevel = 0;
        this.maxTalentLv = 0;
        this.baseRate = 0;
        this.lvRate = 0;
    }

    static fieldsDesc() {
        return {
            advLv: {type: Number},
            needLv: {type: Number},
            quality: {type: Number},
            qualityLevel: {type: Number},
            maxTalentLv: {type: Number},
            baseRate: {type: Number},
            lvRate: {type: Number},
        };
    }

}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {PetAdvLvPropRateConfig}
 */
function getPetAdvLvPropRateConfig(key)
{
    return gameConfig.getCsvConfig("petAdvLvPropRate", key);
}

exports.PetAdvLvPropRateConfig = PetAdvLvPropRateConfig;
exports.getPetAdvLvPropRateConfig = getPetAdvLvPropRateConfig;