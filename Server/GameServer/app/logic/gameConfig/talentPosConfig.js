"use strict";

var gameConfig = require("./gameConfig");

class TalentPosConfig
{
    constructor() {
        this.id = "";
        this.needAdvLv = 0;
    }

    static fieldsDesc() {
        return {
            id: {type: String},
            needAdvLv: {type: Number}
        };
    }
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {TalentPosConfig}
 */
function getTalentPosConfig(key)
{
    return gameConfig.getCsvConfig("talentPos", key);
}

exports.TalentPosConfig = TalentPosConfig;
exports.getTalentPosConfig = getTalentPosConfig;