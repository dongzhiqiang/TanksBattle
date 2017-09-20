"use strict";

var gameConfig = require("./gameConfig");

class PropBasicConfig
{
    constructor() {
        this.damage = 0;
        this.damageCritical = 0;
        this.elementC = 0;
        this.elementA = 0;
        this.defRateC = 0;
        this.defRateB = 0;
        this.defRateA = 0;
        this.equipPoint = 0;
        this.powerRate = 0;
    }

    static fieldsDesc() {
        return {
            damage: {type: Number},
            damageCritical: {type: Number},
            elementC: {type: Number},
            elementA: {type: Number},
            defRateC: {type: Number},
            defRateB: {type: Number},
            defRateA: {type: Number},
            equipPoint: {type: Number},
            powerRate: {type: Number},
        };
    }
}


/**
 *
 * @returns {PropBasicConfig}
 */
function getPropBasicConfig()
{
    return gameConfig.getCsvConfig("propBasic")[0];
}


exports.PropBasicConfig = PropBasicConfig;
exports.getPropBasicConfig = getPropBasicConfig;
