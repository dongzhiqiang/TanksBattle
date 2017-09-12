"use strict";

var gameConfig = require("./gameConfig");

class FlameMaterialConfig
{
    constructor() {
        this.id = 0;
        this.exp = 0;
        this.order = 0;
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            exp: {type: Number},
            order: {type: Number},
        };
    }

}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {FlameMaterialConfig}
 */
function getFlameMaterialConfig(key)
{
    return gameConfig.getCsvConfig("flameMaterial", key);
}

exports.FlameMaterialConfig = FlameMaterialConfig;
exports.getFlameMaterialConfig = getFlameMaterialConfig;