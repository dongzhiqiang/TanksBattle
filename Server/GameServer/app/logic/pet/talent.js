"use strict";

var logUtil = require("../../libs/logUtil");
var talentConfig = require("../gameConfig/talentConfig");
var roleConfig = require("../gameConfig/roleConfig");

class Talent
{
    constructor(data)
    {

        this.talentId = data.talentId;
        this.level = data.level;

        /**
         * 角色
         * @type {Role|null}
         */
        Object.defineProperty(this, "_owner", {enumerable: false, writable: true, value: null});

        //logUtil.debug("Talent创建，talentId：" + this.talentId + "，level：" + this.level);
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

        var part = this._owner.getTalentsPart();
        if (!part)
            return;

        part.syncAndSaveTalent(this.talentId);
    }

    release()
    {
        if (this._owner)
            logUtil.debug("Talent销毁，角色guid：" + this._owner.getGUID() + "，talentId：" + this.talentId);
        else
            logUtil.debug("Talent销毁，talentId：" + this.talentId);
    }
}

/**
 *
 * @param data
 * @returns {boolean}
 */
function isTalentData(data)
{
    return !!(data && data.talentId != "" && data.level > 0);
}

/**
 *
 * @param data
 * @returns {Talent|null}
 */
function createTalent(data)
{
    if (!isTalentData(data))
    {
        logUtil.error("Talent基本数据不完整或有错，数据：" + JSON.stringify(data));
        return null;
    }

    var talentCfg = talentConfig.getTalentConfig(data.talentId);
    if (!talentCfg)
    {
        logUtil.error("TalentId无效，talentId：" + data.talentId);
        return null;
    }

    return new Talent(data);
}

/**
 *
 * @param {string} roleId
 * @returns {Array}
 */
function getInitTalents(roleId)
{
    var result = [];
    var roleCfg = roleConfig.getRoleConfig(roleId);
    for(var i=0;i<roleCfg.talents.length;i++)
    {
        result.push({talentId:roleCfg.talents[i], level:1});
    }
    return result;
}

/**
 *
 * @param {string} roleId
 * @param {string} talentId
 * @param {object|null} outPos
 * @returns {boolean}
 */
function hasTalent(roleId, talentId, outPos)
{
    var result = [];
    var roleCfg = roleConfig.getRoleConfig(roleId);
    for(var i=0;i<roleCfg.talents.length;i++)
    {
        if(roleCfg.talents[i]==talentId)
        {
            if(outPos)
            {
                outPos.pos = i;
            }
            return true;
        }
    }
    return false;
}

exports.isTalentData = isTalentData;
exports.createTalent = createTalent;
exports.getInitTalents = getInitTalents;
exports.hasTalent = hasTalent;