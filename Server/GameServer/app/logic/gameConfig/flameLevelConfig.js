"use strict";

var map={};

class FlameLevelConfig
{
    constructor() {
        this.level = 0;
        this.id = 0;
        this.fx = "";
        this.attributeId = "";
        this.exp = 0;
        this.power = 0;
        this.powerRate = 0;
    }

    static fieldsDesc() {
        return {
            level: {type: Number},
            id: {type: Number},
            fx: {type: String},
            attributeId: {type: String},
            exp: {type: Number},
            power: {type: Number},
            powerRate: {type: Number},
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
            temp[cfg.level] = cfg;
        }

    }



}

/**
 *
 * @param {number} id
 * @param {number} level
 * @returns {FlameLevelConfig}
 */
function getFlameLevelConfig(id,level)
{
    let temp =map[id];
    return temp&&temp[level];
}

module.exports.FlameLevelConfig = FlameLevelConfig;
module.exports.getFlameLevelConfig = getFlameLevelConfig;
