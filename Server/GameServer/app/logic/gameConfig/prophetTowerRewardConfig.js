/**
 * Created by pc20 on 2016/9/1.
 */
"use strict";

var gameConfig = require("./gameConfig");

class ProphetTowerRewardConfig
{
    constructor() {
        this.id = 0;
        this.rewardId;
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            rewardId: {type: Number},
        };
    }
}


/**
 *
 * @returns {ProphetTowerRewardConfig}
 */
function getProphetTowerRewardConfig(key)
{
    return gameConfig.getCsvConfig("prophetReward", key);
}


exports.ProphetTowerRewardConfig = ProphetTowerRewardConfig;
exports.getProphetTowerRewardConfig = getProphetTowerRewardConfig;
