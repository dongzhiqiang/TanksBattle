"use strict";

var map={};

class SkillLvRateConfig
{
    constructor() {
        this.id = "";
        this.lv = 0;
        this.rate = 1;
    }

    static fieldsDesc() {
        return {
            id: {type: String},
            lv: {type: Number},
            rate: {type: Number},

        };
    }

    static afterReadAll(rows)
    {
        let i = rows.length;
        while (i--) {
            let cfg =rows[i];
            let temp = map[cfg.id];
            if(!temp)
                map[cfg.id] =temp = {};
            temp[cfg.lv] = cfg;
        }

    }



}

/**
 *
 * @param {string} id
 * @param {number} lv
 * @returns {SkillLvRateConfig}
 */
function getSkillLvRateConfig(id,lv)
{
    let temp =map[id];
    return temp&&temp[lv];
}

module.exports.SkillLvRateConfig = SkillLvRateConfig;
module.exports.getSkillLvRateConfig = getSkillLvRateConfig;
