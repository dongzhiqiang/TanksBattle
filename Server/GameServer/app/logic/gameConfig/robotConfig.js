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
        this.petMainNumRange = [];
        this.petSub1NumRange = [];
        this.petSub2NumRange = [];
        this.petRoleIds = [];
        this.petLvRange = [];
        this.petAdvLvRange = [];
        this.petStarRange = [];
        this.petEquipLvRange = [];
        this.petEquipAdvLvRange = [];
        this.petEquipStarRange = [];
        this.petSkillLvRange = [];
        this.petTalentLvRange = [];
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
            petMainNumRange: {type: Array, elemType: Number},
            petSub1NumRange: {type: Array, elemType: Number},
            petSub2NumRange: {type: Array, elemType: Number},
            petRoleIds: {type: Array, elemType: String},
            petLvRange: {type: Array, elemType: Number},
            petAdvLvRange: {type: Array, elemType: Number},
            petStarRange: {type: Array, elemType: Number},
            petEquipLvRange: {type: Array, elemType: Number},
            petEquipAdvLvRange: {type: Array, elemType: Number},
            petEquipStarRange: {type: Array, elemType: Number},
            petSkillLvRange: {type: Array, elemType: Number},
            petTalentLvRange: {type: Array, elemType: Number},
        };
    }

    static afterDefReadRow(row)
    {
        var maxNum1 = row.petMainNumRange[row.petMainNumRange.length - 1];
        var maxNum2 = row.petSub1NumRange[row.petSub1NumRange.length - 1];
        var maxNum3 = row.petSub2NumRange[row.petSub2NumRange.length - 1];
        if (row.petRoleIds.length < maxNum1 + maxNum2 + maxNum3)
            throw new Error("robotConfig, 宠物的角色ID必须petMainNumRange最大值 + petSub1NumRange最大值 + petSub2NumRange最大值");
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