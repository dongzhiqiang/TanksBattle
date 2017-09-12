"use strict";

var gameConfig = require("./gameConfig");

class PetUpgradeCostConfig
{
    constructor() {
        this.id = "";
        this.exp = 0;
    }

    static fieldsDesc() {
        return {
            id: {type: String},
            exp: {type: Number},
        };
    }

}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {PetUpgradeCostConfig}
 */
function getPetUpgradeCostConfig(key)
{
    return gameConfig.getCsvConfig("petUpgradeCost", key);
}

exports.PetUpgradeCostConfig = PetUpgradeCostConfig;
exports.getPetUpgradeCostConfig = getPetUpgradeCostConfig;