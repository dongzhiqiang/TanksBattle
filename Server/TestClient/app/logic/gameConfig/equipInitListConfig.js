"use strict";

var gameConfig = require("./gameConfig");

class EquipInitListConfig
{
    constructor() {
        this.id = "";
        this.equips = [];
    }

    static fieldsDesc() {
        return {
            id: {type: String},
            equips: {type: Array, elemType:Number},
        };
    }

}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {EquipInitListConfig}
 */
function getEquipInitListConfig(key)
{
    return gameConfig.getCsvConfig("equipInitList", key);
}

exports.EquipInitListConfig = EquipInitListConfig;
exports.getEquipInitListConfig = getEquipInitListConfig;