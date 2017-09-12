/**
 * Created by pc20 on 2016/2/16.
 */
"use strict";

var gameConfig = require("./gameConfig");

class LevelConfig
{
    constructor() {
        this.id = "0";            //房间ID
        this.roomName = "";    //房间名
        this.preLevelId = "0";  //前置关卡ID
        this.roomNodeId = "0";  //房间所属地图
        this.roomType = 0;    //关卡类型
        this.roomStory = "";   //房间描述
        this.sceneFileName = "";   //房间资源文件名
        this.roomSprite = "";  //关卡图标
        this.sceneId = "";     //场景名
        this.levelProp = "";//副本固定属性
        this.levelPropRate = "";//副本属性分配比例
        this.levelLv = "";//副本怪物等级
        this.maxChallengeNum = 0; //每日最大挑战次数
        this.powerNum = 99999; //推荐战斗力
        this.staminaCost = 0;  //消耗体力
        this.petNum = 2;      //限制携带侍宠
        this.expReward = 0;   //奖励经验
        this.petExp = 0;      //宠物经验奖励
        this.taskId = "";    //三星条件列表
        this.dropId = 0;    //结算时掉落Id
        this.time = 0;      //通关时间
        this.monsterDrop = [];  //关卡内小怪掉落
        this.specialDrop = [];  //关卡精英怪掉落
        this.bossDrop = [];     //关卡boss掉落
        this.boxDrop = [];     //宝箱掉落
    }

    static fieldsDesc() {
        return {
            id: {type: String},
            roomName: {type: String},
            preLevelId: {type: String},
            roomNodeId: {type: String},
            roomType: {type: Number},
            roomStory: {type: String},
            sceneFileName: {type: String},
            roomSprite: {type: String},
            sceneId: {type: String},
            levelProp: {type: String},
            levelPropRate: {type: String},
            levelLv: {type: String},
            maxChallengeNum: {type: Number},
            powerNum: {type: Number},
            staminaCost: {type: Number},
            petNum: {type: Number},
            expReward: {type: Number},
            petExp: {type: Number},
            taskId: {type: String},
            dropId: {type: Number},
            monsterDrop: {type: Array, elemType:Number, arrayLayer:2},
            specialDrop: {type: Array, elemType:Number, arrayLayer:2},
            bossDrop: {type: Array, elemType:Number, arrayLayer:2},
            boxDrop: {type: Array, elemType:Number, arrayLayer:2}
        };
    }
}

class SweepLevelCfg
{
    constructor() {
        this.type = 0;  //0，单次，1，多次
        this.stars = 0;
        this.vipLv = 0;
        this.condOp = 0;//0，前两个条件按或运算，1，前两个条件按与运算
        this.tip = "";
    }

    static fieldsDesc() {
        return {
            type: {type: Number},
            stars: {type: Number},
            vipLv: {type: Number},
            condOp: {type: Number},
            tip: {type: String},
        };
    }
}

/**
 *
 * @param key
 * @returns {LevelConfig|Object.<string,LevelConfig>}
 */
function getLevelConfig(key)
{
    return gameConfig.getCsvConfig("room", key);
}

/**
 *
 * @param key
 * @returns {SweepLevelCfg|Object.<number,SweepLevelCfg>}
 */
function getSweepLevelCfg(key)
{
    return gameConfig.getCsvConfig("sweepLevelCfg", key);
}

exports.LevelConfig = LevelConfig;
exports.SweepLevelCfg = SweepLevelCfg;
exports.getLevelConfig = getLevelConfig;
exports.getSweepLevelCfg = getSweepLevelCfg;

