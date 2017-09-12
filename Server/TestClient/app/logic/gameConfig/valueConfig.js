/**
 * Created by pc20 on 2016/2/19.
 */
"use strict";

var gameConfig = require("./gameConfig");
var dateUtil = require("../../libs/dateUtil");

var maxMailNum;  //最大邮件数
var mailKeepTime;  //无附件的邮件保留时间
var worldChatLevel;  //世界频道聊天需要的等级

class ValueConfig
{
    constructor() {
        this.name = "";
        this.desc = "";
        this.value = undefined;
        this.type = "";
    }

    static fieldsDesc() {
        return {
            name: {type: String},
            desc: {type: String},
            value: {type: Object},
            type: {type: String}
        };
    }


    static afterReadAll(rows)
    {
        maxMailNum = parseInt(rows.maxMailNum.value);
        mailKeepTime = parseInt(rows.mailKeepTime.value);
        worldChatLevel = parseInt(rows.worldChatLevel.value);

        //设置时间参数
        var sundayFirst = rows.sundayFirst.value;
        var dayBreakPoint = rows.dayBreakPoint.value;
        dateUtil.setParameter(sundayFirst, dayBreakPoint);

    }
}

class TestItemConfig {
    constructor() {
        this.itemId = 0;
        this.itemNum = 0;
    }

    static fieldsDesc() {
        return {
            itemId: {type: Number},
            itemNum: {type: Number},
        };
    }
}

/**
 *
 * @param key
 * @returns {ValueConfig}
 */
function getValueConfig(key)
{
    return gameConfig.getCsvConfig("configValue", key);
}

/**
 *
 * @param key
 * @returns {TestItemConfig}
 */
function getTestItemConfig(key)
{
    return gameConfig.getCsvConfig("testItem", key);
}

/**
 *
 * @param {string} key
 * @returns {number}
 */
function getNumber(key)
{
    return parseInt(gameConfig.getCsvConfig("configValue", key).value);
}

/**
 *
 * @param {string} key
 * @returns {string}
 */
function getString(key)
{
    return gameConfig.getCsvConfig("configValue", key).value;
}

function getMaxMaillNum()
{
    return maxMailNum;
}
function getMailKeepTime()
{
    return mailKeepTime;
}
function getWorldChatLevel()
{
    return worldChatLevel;
}

exports.ConfigValueConfig = ValueConfig;
exports.TestItemConfig = TestItemConfig;
exports.getConfigValueConfig = getValueConfig;
exports.getTestItemConfig = getTestItemConfig;
exports.getNumber = getNumber;
exports.getString = getString;
exports.getMaxMaillNum = getMaxMaillNum;
exports.getMailKeepTime = getMailKeepTime;
exports.getWorldChatLevel = getWorldChatLevel;