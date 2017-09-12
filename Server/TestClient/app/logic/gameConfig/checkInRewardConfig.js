"use strict";

var gameConfig = require("./gameConfig");
var cfgsWithKey = {};

class CheckInRewardConfig
{
    constructor() {
        this.id = 0;
        this.month = 0;
        this.checkInNums = 0;
        this.itemId = 0;
        this.itemNums = 0;
        this.vipLevel = 0;
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            month: {type: Number},
            checkInNums: {type: Number},
            itemId: {type: Number},
            itemNums: {type: Number},
            vipLevel: {type: Number},
        };
    }

    /**
     * 重新处理配置
     * @param {CheckInRewardConfig[]} rows
     */
    static afterReadAll(rows)
    {
        cfgsWithKey = {};
        for (var i = 0; i < rows.length; ++i)
        {
            var item = rows[i];
            var key = item.month * 100 + item.checkInNums;
            cfgsWithKey[key] = item;
        }
    }
}

/**
 * 通过月份和签到次数找到配置
 * @param {number} month
 * @param {number} num
 * @returns {CheckInRewardConfig|undefined}
 */
function getCheckInRewardConfig(month, num)
{
    return cfgsWithKey[month * 100 + num];
}

exports.CheckInRewardConfig = CheckInRewardConfig;
exports.getCheckInRewardConfig = getCheckInRewardConfig;