"use strict";

var gameConfig = require("./gameConfig");

class RoleLvPropConfig
{
    constructor() {
        this.lv = 0;
        this.rate = 0;
        this.defRateRole = 0;
        this.defRatePet = 0;
        this.defRateMonster = 0;
    }

    static fieldsDesc() {
        return {
            lv: {type: Number},
            rate: {type: Number},
            defRateRole: {type: Number},
            defRatePet: {type: Number},
            defRateMonster: {type: Number}
        };
    }
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {RoleLvPropConfig}
 */
function getRoleLvPropConfig(key)
{
    return gameConfig.getCsvConfig("roleLvProp", key);
}

exports.RoleLvPropConfig = RoleLvPropConfig;
exports.getRoleLvPropConfig = getRoleLvPropConfig;