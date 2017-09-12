"use strict";

var gameConfig = require("./gameConfig");

class EquipRouseRateConfig
{
    constructor() {
        this.id = 0;
        this.baseRate = 0;
        this.lvRate = 0;
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            baseRate: {type: Number},
            lvRate: {type: Number},
        };
    }

}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {EquipRouseRateConfig}
 */
function getEquipRouseRateConfig(key)
{
    return gameConfig.getCsvConfig("equipRouseRate", key);
}

exports.EquipRouseRateConfig = EquipRouseRateConfig;
exports.getEquipRouseRateConfig = getEquipRouseRateConfig;