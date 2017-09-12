"use strict";

var gameConfig = require("./gameConfig");

class EquipAdvanceRateConfig
{
    constructor() {
        this.id = 0;
        this.needLv = 0;
        this.maxLv = 0;
        this.quality = 0;
        this.qualityLv = 0;
        this.baseRate = 0;
        this.lvRate = 0;
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            needLv: {type: Number},
            maxLv: {type: Number},
            quality: {type: Number},
            qualityLv: {type: Number},
            baseRate: {type: Number},
            lvRate: {type: Number},
        };
    }

}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {EquipAdvanceRateConfig}
 */
function getEquipAdvanceRateConfig(key)
{
    return gameConfig.getCsvConfig("equipAdvanceRate", key);
}

exports.EquipAdvanceRateConfig = EquipAdvanceRateConfig;
exports.getEquipAdvanceRateConfig = getEquipAdvanceRateConfig;