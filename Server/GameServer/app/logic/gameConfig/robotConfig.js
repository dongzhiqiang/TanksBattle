"use strict";

var gameConfig = require("./gameConfig");

class RobotConfig
{
    constructor() {
        this.robotId = 0;
        this.roleIds = [];
        this.heroLvRange = [];
        this.equipLvRange = [];
        this.equipAdvLvRange = [];
        this.equipStarRange = [];
        this.skillLvRange = [];
    }

    static fieldsDesc() {
        return {
            robotId: {type: Number},
            roleIds: {type: Array, elemType: String},
            heroLvRange: {type: Array, elemType: Number},
            equipLvRange: {type: Array, elemType: Number},
            equipAdvLvRange: {type: Array, elemType: Number},
            equipStarRange: {type: Array, elemType: Number},
            skillLvRange: {type: Array, elemType: Number},
        };
    }

    static afterDefReadRow(row)
    {
    }
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {RobotConfig}
 */
function getRobotConfig(key)
{
    return gameConfig.getCsvConfig("robot", key);
}

exports.RobotConfig = RobotConfig;
exports.getRobotConfig = getRobotConfig;