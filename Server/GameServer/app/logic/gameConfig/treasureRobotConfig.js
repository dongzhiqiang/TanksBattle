/**
 * Created by Administrator on 2016/9/12.
 */
"use strict";

var gameConfig = require("./gameConfig");

class TreasureRobotConfig
{
    constructor() {
        this.robotId=0;
        this.robotNum=0;
    }

    static fieldsDesc() {
        return {
            robotId: {type: Number},
            robotNum: {type: Number},
        };
    }
}

/**
 *
 * @param key
 * @returns {TreasureRobotConfig}
 */
function getTreasureRobotConfig(key)
{
    return gameConfig.getCsvConfig("treasureRobot", key);
}


exports.TreasureRobotConfig = TreasureRobotConfig;
exports.getTreasureRobotConfig = getTreasureRobotConfig;
