"use strict";

var gameConfig = require("./gameConfig");

class FlameConfig
{
    constructor() {
        this.id = 0;
        this.name = "";
        this.icon = "";
        this.mod = "";
        this.needLevel = 0;
        this.attributeLimit = "";
        this.costGold = 0;
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            name: {type: String},
            icon: {type: String},
            mod: {type: String},
            needLevel: {type: Number},
            attributeLimit: {type: String},
            costGold: {type: Number},
        };
    }

}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {FlameConfig}
 */
function getFlameConfig(key)
{
    return gameConfig.getCsvConfig("flame", key);
}

exports.FlameConfig = FlameConfig;
exports.getFlameConfig = getFlameConfig;