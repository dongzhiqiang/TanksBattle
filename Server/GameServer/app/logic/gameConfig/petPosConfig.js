"use strict";

var gameConfig = require("./gameConfig");

class PetPosConfig
{
    constructor() {
        this.id = 0;
        this.desc = "";
        this.level = 0;
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            desc: {type: String},
            level: {type: Number}
        };
    }
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {PetPosConfig}
 */
function getPetPosConfig(key)
{
    return gameConfig.getCsvConfig("petPos", key);
}

exports.PetPosConfig = PetPosConfig;
exports.getPetPosConfig = getPetPosConfig;