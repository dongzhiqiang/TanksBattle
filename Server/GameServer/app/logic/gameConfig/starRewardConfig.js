/**
 * Created by pc20 on 2016/8/11.
 */
"use strict";
var gameConfig = require("./gameConfig");

class StarRewardConfig
{
    constructor() {
        this.id = 0;
        this.normalReward = [];
        this.specialReward = [];
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            normalReward: {type: Array, elemType:Number, arrayLayer:2},
            specialReward: {type: Array, elemType:Number, arrayLayer:2},
        };
    }
}


/**
 *
 * @param key
 * @returns {StarRewardConfig}
 */
function getStarRewardConfig(key)
{
    return gameConfig.getCsvConfig("starReward", key);
}

/**
 *
 * @param nodeId｛String｝
 * @param starNum{Number}
 * @param type{Number} 1:普通关卡 2:精英关卡
 * @returns {*|Number}
 */
function getRewardId(nodeId, starNum, type)
{
    var cfg = gameConfig.getCsvConfig("starReward", nodeId);

    //给必得道具
    var rewardItem = type === 1 || type === 2 ? cfg.normalReward : cfg.specialReward;
    for (let i = 0; i < rewardItem.length; ++i)
    {
        var elem = rewardItem[i];
        var id = elem[0];
        var star = elem[1];
        if (star === starNum) {
            return id;
        }
    }
    return 0;
}


exports.StarRewardConfig = StarRewardConfig;
exports.getStarRewardConfig = getStarRewardConfig;
exports.getRewardId = getRewardId;