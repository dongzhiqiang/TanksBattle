/**
 * Created by pc20 on 2016/6/15.
 */
"use strict";

var gameConfig = require("./gameConfig");

class VipConfig
{
    constructor() {
        this.level = 0;
        this.totalRecharge = 0;
        this.staminaBuyNum = 0;
        this.specialLvlResetNum = 0;
        this.arenaBuyNum = 0;
        this.sweepLvlTimes = 0;
        this.warriorSweep = 0;
        this.arenaFreezeTime = 0;
    }

    static fieldsDesc() {
        return {
            level: {type: Number},
            totalRecharge: {type: Number},
            staminaBuyNum: {type: Number},
            specialLvlResetNum: {type: Number},
            arenaBuyNum: {type: Number},
            sweepLvlTimes: {type: Number},
            warriorSweep: {type: Number},
            arenaFreezeTime: {type: Number},
        };
    }
}

/**
 *
 * @param key
 * @returns {VipConfig}
 */
function getVipConfig(key)
{
    return gameConfig.getCsvConfig("vip", key);
}


exports.VipConfig = VipConfig;
exports.getVipConfig = getVipConfig;
