"use strict";

var gameConfig = require("./gameConfig");

class LvExpConfig
{
    constructor() {
        this.level = 0;
        this.needExp = 0;
        this.expRatio = 0;
        this.maxStamina = 0;
        this.upgradeStamina = 0;
    }

    static fieldsDesc() {
        return {
            level: {type: Number},
            needExp: {type: Number},
            expRatio: {type: Number},
            maxStamina: {type: Number},
            upgradeStamina: {type: Number}
        };
    }

}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {LvExpConfig}
 */
function getLvExpConfig(key)
{
    return gameConfig.getCsvConfig("lvExp", key);
}

exports.LvExpConfig = LvExpConfig;
exports.getLvExpConfig = getLvExpConfig;