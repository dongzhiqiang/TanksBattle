"use strict";

var gameConfig = require("./gameConfig");

class PetStarPropRateConfig
{
    constructor() {
        this.star = 0;
        this.baseRate = 0;
        this.lvRate = 0;
    }

    static fieldsDesc() {
        return {
            star: {type: Number},
            baseRate: {type: Number},
            lvRate: {type: Number},
        };
    }

}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {PetStarPropRateConfig}
 */
function getPetStarPropRateConfig(key)
{
    return gameConfig.getCsvConfig("petStarPropRate", key);
}

exports.PetStarPropRateConfig = PetStarPropRateConfig;
exports.getPetStarPropRateConfig = getPetStarPropRateConfig;