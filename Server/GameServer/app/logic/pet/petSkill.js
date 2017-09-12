"use strict";

var logUtil = require("../../libs/logUtil");
var RoleSkillConfig = require("../gameConfig/roleSkillConfig");
var roleConfig = require("../gameConfig/roleConfig");

class PetSkill
{
    constructor(data)
    {

        this.skillId = data.skillId;
        this.level = data.level;

        /**
         * 角色
         * @type {Role|null}
         */
        Object.defineProperty(this, "_owner", {enumerable: false, writable: true, value: null});

        //logUtil.debug("PetSkill创建，skillId：" + this.skillId + "，level：" + this.level);
    }

    /**
     *
     * @param {Role} v
     */
    setOwner(v)
    {
        this._owner = v;
    }

    /**
     *
     * @returns {Role|null}
     */
    getOwner()
    {
        return this._owner;
    }

    /**
     * 当已在数据库时，存盘
     */
    syncAndSave()
    {
        if (!this._owner)
            return;

        var part = this._owner.getPetSkillsPart();
        if (!part)
            return;

        part.syncAndSavePetSkill(this.skillId);
    }

    release()
    {
        if (this._owner)
            logUtil.debug("PetSkill销毁，角色guid：" + this._owner.getGUID() + "，skillId：" + this.skillId);
        else
            logUtil.debug("PetSkill销毁，skillId：" + this.skillId);
    }
}

/**
 *
 * @param data
 * @returns {boolean}
 */
function isPetSkillData(data)
{
    return !!(data && data.skillId != "" && data.level > 0);
}

/**
 *
 * @param data
 * @returns {PetSkill|null}
 */
function createPetSkill(data)
{
    if (!isPetSkillData(data))
    {
        logUtil.error("PetSkill基本数据不完整或有错，数据：" + JSON.stringify(data));
        return null;
    }

    var systemSkillCfg = RoleSkillConfig.getRoleSkillConfig(data.roleId,data.skillId);
    if (!systemSkillCfg)
    {
        logUtil.error("SkillId无效，skillId：" + data.skillId);
        return null;
    }

    return new PetSkill(data);
}

/**
 *
 * @param {string} roleId
 * @returns {Array}
 */
function getInitPetSkills(roleId)
{
    var result = [];
    var roleCfg = roleConfig.getRoleConfig(roleId);
    result.push({skillId:roleCfg.atkUpSkill, level:1});
    for(var i=0;i<roleCfg.skills.length;i++)
    {
        result.push({skillId:roleCfg.skills[i], level:1});
    }
    return result;
}

/**
 *
 * @param {string} roleId
 * @param {string} skillId
 * @returns {boolean}
 */
function hasPetSkills(roleId, skillId)
{
    var result = [];
    var roleCfg = roleConfig.getRoleConfig(roleId);
    if(roleCfg.atkUpSkill==skillId)
    {
        return true;
    }
    for(var i=0;i<roleCfg.skills.length;i++)
    {
        if(roleCfg.skills[i]==skillId)
        {
            return true;
        }
    }
    return false;
}

exports.isPetSkillData = isPetSkillData;
exports.createPetSkill = createPetSkill;
exports.getInitPetSkills = getInitPetSkills;
exports.hasPetSkills = hasPetSkills;