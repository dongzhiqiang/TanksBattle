"use strict";

var gameConfig = require("./gameConfig");

class PetFormationConfig
{
    constructor() {
        this.id = 0;
        this.desc = "";
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            desc: {type: String},
        };
    }

}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {PetFormationConfig}
 */
function getPetFormationConfig(key)
{
    return gameConfig.getCsvConfig("petFormation", key);
}

exports.PetFormationConfig = PetFormationConfig;
exports.getPetFormationConfig = getPetFormationConfig;