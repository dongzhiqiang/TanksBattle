"use strict";

var gameConfig = require("./gameConfig");
var LvValue = require("./lvValue").LvValue;

class HeroTalentConfig
{
    constructor() {
        this.id = 0;
        this.name = "";
        this.levelCostId = 0;
        this.powerRate = "";
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            name: {type: String},
            levelCostId: {type: Number},
            powerRate: {type: String},
        };
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

    static getTalentConfig(key)
    {
        return gameConfig.getCsvConfig("heroTalent", key);
    }

}

module.exports = HeroTalentConfig;
