"use strict";

var gameConfig = require("./gameConfig");

class SystemSkillConfig
{
    constructor() {
        this.id = "";
        this.name = "";
        this.type = "";
        this.icon = "";
        this.description = "";
        this.damageRate = 0;
        this.petSkillUpgradeId = 0;
        this.maxLevel = 0;
        this.needPetStar = 0;
    }

    static fieldsDesc() {
        return {
            id: {type: String},
            name: {type: String},
            type: {type: String},
            icon: {type: String},
            description: {type: String},
            damageRate: {type: Number},
            petSkillUpgradeId: {type: Number},
            maxLevel: {type: Number},
            needPetStar: {type: Number}
        };
    }
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {SystemSkillConfig}
 */
function getSystemSkillConfig(key)
{
    return gameConfig.getCsvConfig("systemSkill", key);
}

exports.SystemSkillConfig = SystemSkillConfig;
exports.getSystemSkillConfig = getSystemSkillConfig;