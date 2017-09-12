"use strict";

var gameConfig = require("./gameConfig");
var LvValue = require("./lvValue").LvValue;

class TalentConfig
{
    constructor() {
        this.id = "";
        this.name = "";
        this.type = 0;
        this.icon = "";
        this.description = "";
        this.upgradeId = 0;
        this.maxLevel = 0;
        this.stateId = 0;
        this.power = "";
        this.powerRate = "";
    }

    static fieldsDesc() {
        return {
            id: {type: String},
            name: {type: String},
            type: {type: Number},
            icon: {type: String},
            description: {type: String},
            upgradeId: {type: Number},
            maxLevel: {type: Number},
            stateId: {type: Number},
            power: {type: String},
            powerRate: {type: String},
        };
    }

    /** 因为有配置依赖关系只能在使用的时候再构造
     * returns {LvValue}
     */
    getPowerLvValue()
    {
        if(!this._powerLvValue)
        {
            this._powerLvValue = new LvValue(this.power);
        }
        return this._powerLvValue;
    }

    /** 因为有配置依赖关系只能在使用的时候再构造
     * returns {LvValue}
     */
    getPowerRateLvValue()
    {
        if(!this._powerRateLvValue)
        {
            this._powerRateLvValue = new LvValue(this.powerRate);
        }
        return this._powerRateLvValue;
    }
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {TalentConfig}
 */
function getTalentConfig(key)
{
    return gameConfig.getCsvConfig("talent", key);
}

exports.TalentConfig = TalentConfig;
exports.getTalentConfig = getTalentConfig;