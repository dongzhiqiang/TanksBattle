"use strict";
var gameConfig = require("./gameConfig");
var appUtil = require("../../libs/appUtil");
//刷新消耗封顶，大于它的消耗都一样
const TopRefresh = 8;
//收集关卡配置
//var triedLevels = [];
//收集星级配置
var triedStars = [];


//基础配置
class WarTriedBaseConfig
{
    constructor() {
        this.dailyNum = 0;
        this.taskWeight = "";
        this.freeRefresh = 0;
    }

    static fieldsDesc() {
        return {
            dailyNum: {type: Number},
            taskWeight: {type: String},
            freeRefresh: {type:Number},
        };
    }
}
//刷新价格配置
class TriedRefreshCostConfig
{
    constructor() {
        this.num = 0;
        this.cost = 0;
    }

    static fieldsDesc() {
        return {
            num: {type: Number},
            cost: {type: Number},
        };
    }
}
//星级试炼表配置
class TriedStarConfig
{
    constructor() {
        this.key = 0;
        this.group = 0;
        this.star = 0;
        this.weight = 0;
        this.rewardId = 0;
    }

    static fieldsDesc() {
        return {
            key: {type: Number},
            group: {type: Number},
            star: {type: Number},
            weight: {type: Number},
            rewardId: {type: Number},
        };
    }
}
//试炼关卡配置
class TriedLevelConfig
{
    constructor() {
        this.roomId = "";
        this.passId = 0;
    }

    static fieldsDesc() {
        return {
            roomId: {type: String},
            passId: {type: Number},
        };
    }
}
//试炼等级关卡
class TriedLevelRoomConfig
{
    constructor() {
        this.level = 0;
        this.rooms = "";
        this.weights = "";
    }

    static fieldsDesc() {
        return {
            level: {type: Number},
            rooms: {type: String},
            weights: {type: String},
        };
    }
}
/**
 * 获取试炼通用配置
 * @returns {WarTriedBaseConfig}
 */
function getTriedBaseConfig()
{
    return gameConfig.getCsvConfig("base")[0];
}

/**
 * 获取刷新价格
 * @param {Number} num
 * @returns {Number}
 */
function getRefreshCostConfig(num)
{
    return gameConfig.getCsvConfig("refreshCost", num<=TopRefresh?num:TopRefresh).cost;
}
/**
 * 获取星级配置
 * @param star
 * @returns {TriedStarConfig}
 */
function getStarConfig(star)
{
    return gameConfig.getCsvConfig("starTried", star);
}

/**
 * 取随机星级
 * @param {Number} type
 * @param {Number} getNum
 * @returns {TriedStarConfig[]}
 */
function getRandomStar(type, getNum)
{
    if(triedStars.length == 0)
    {
        var dic = gameConfig.getCsvConfig("starTried");
        for(var key in dic)
            triedStars.push(dic[key]);
    }
    var arr = [];
    if(type == 1)
        arr = triedStars.slice(0, 5);
    else
        arr = triedStars.slice(5, 7);
    return appUtil.getRepeatableRandItems(arr, getNum, 'weight');

}
/**
 * 取随机关卡的配置
 * @param {Number} num
 * @returns {TriedLevelConfig[]}
 */
function getRandomTriedLevel(level, num)
{
    var cfgs = gameConfig.getCsvConfig("levelRoom");
    var cfg = null;
    var len = cfgs.length;
    for(var i = 0; i < len; ++i)
    {
        if(cfgs[i].level > level)
            cfg = cfgs[i-1];
    }
    if(cfg == null)//取最后一个
        cfg = cfgs[len-1];
    var rooms = cfg.rooms.split('|');
    var weights = cfg.weights.split('|');
    var items = [];
    for(var i = 0,count = rooms.length; i < count; ++i)
        items.push({"roomId":rooms[i], "weight":Number(weights[i])});

    return appUtil.getRepeatableRandItems(items, num, 'weight');  //根据权重抽取n个

}


exports.WarTriedBaseConfig = WarTriedBaseConfig;
exports.TriedRefreshCostConfig = TriedRefreshCostConfig;
exports.TriedStarConfig = TriedStarConfig;
exports.TriedLevelConfig = TriedLevelConfig;
exports.getTriedBaseConfig = getTriedBaseConfig;
exports.getRefreshCostConfig = getRefreshCostConfig;
exports.getRandomTriedLevel = getRandomTriedLevel;
exports.getRandomStar = getRandomStar;
exports.getStarConfig = getStarConfig;
exports.TriedLevelRoomConfig = TriedLevelRoomConfig;