"use strict";

var LvValue = require("./lvValue").LvValue;

var map={};

class RoleSkillConfig
{
    constructor() {
        this.roleId = "";
        this.id = "";
        this.name = "";
        this.levelCostId = 0;
        this.talent = null;
        this.powerRate = "";
    }

    static fieldsDesc() {
        return {
            roleId: {type: String},
            id: {type: String},
            name: {type: String},
            levelCostId: {type: Number},
            talent: {type: Array, elemType:Number},
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

    static afterReadAll(rows)
    {
        let i = rows.length;
        while (i--) {
            let cfg =rows[i];
            let temp = map[cfg.roleId];
            if(!temp)
                map[cfg.roleId] =temp = {};
            temp[cfg.id] = cfg;
        }

    }

    /**
     *
     * @param {string} roleId
     * @param {string} skillId
     * @returns {RoleSkillConfig}
     */
    static getRoleSkillConfig(roleId,skillId)
    {
        let temp =map[roleId];
        return temp&&temp[skillId];
    }

}

module.exports = RoleSkillConfig;
