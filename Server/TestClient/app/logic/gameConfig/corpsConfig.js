"use strict";

var gameConfig = require("./gameConfig");
//公会等级配置
class CorpsLevelConfig
{
    constructor() {
        this.level = 0;
        this.maxMember = 0;
        this.upValue = 0;
    }

    static fieldsDesc() {
        return {
            level: {type: Number},
            maxMember: {type: Number},
            upValue: {type: Number},

        };
    }
}
//公会通用配置
class CorpsBaseConfig
{
    constructor() {
        this.createCost = 0;   //创建公会消耗
        this.maxName = 0;   //公会名字长度上限
        this.logMax = 0;    //日志保存上限
        this.maxReq = 0;   //入会申请人数上限
        this.quitCorpsCd = 0;   //退出公会冷却时间
        this.CDROfftime = 0;   //不上线弹劾
        this.impContribute = 0;   //弹劾者所需的贡献
        this.supportContribute = 0;   //支持弹劾者所需的贡献
        this.supportNum = 0;    //支持人数
        this.impTime = 0;   //弹劾持续时间
        this.openLevel = 0;  //公会开启等级
        this.declareLimit = 0;   //公会宣言最大字数
        this.buildLogMax = 0;   //公会建设日志最大
    }

    static fieldsDesc() {
        return {
            createCost: {type: Number},
            maxName: {type: Number},
            logMax: {type: Number},
            maxReq: {type: Number},
            quitCorpsCd: {type: Number},
            CDROfftime: {type: Number},
            impContribute: {type: Number},
            supportContribute: {type: Number},
            supportNum: {type: Number},
            impTime: {type: Number},
            openLevel: {type: Number},
            declareLimit: {type: Number},
            buildLogMax: {type: Number},
        };
    }
}
//公会职位权限配置
class CorpsPosFuncConfig
{
    constructor() {
        this.posLevel = 0;
        this.posName = "";
        this.posFunc = "";
    }

    static fieldsDesc() {
        return {
            posLevel: {type: Number},
            posName: {type: String},
            posFunc: {type: String},
        };
    }
}
//公会初始宣言配置
class CorpsDeclareConfig
{
    constructor() {
        this.id = 0;   //创建公会消耗
        this.declare = "";   //宣言
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            declare: {type: String},
        };
    }
}
//公户建设配置
class CorpsBuildConfig
{
    constructor() {
        this.id = 0;   //id
        this.name = "";   //建设名称
        this.openVipLv = 0;  //开启的vip等级
        this.dailyNum = 0;  //每日个人次数
        this.cost = "";  //花费
        this.contri = 0;  //个人可得贡献
        this.corpsConstr = 0;  //公会可得建设度
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            name: {type: String},
            openVipLv: {type: Number},
            dailyNum: {type: Number},
            cost: {type: String},
            contri: {type: Number},
            corpsConstr: {type: Number},
        };
    }
}
/**
 * 获得公会等级配置
 * @returns {CorpsLevelConfig}
 */
function getCorpsLevelCfg(level)
{
    return gameConfig.getCsvConfig("corps", level);
}

/**
 * 获得公会通用配置
 * @returns {CorpsBaseConfig}
 */
function getCorpsBaseCfg()
{
    return gameConfig.getCsvConfig("corpsBase")[0];
}

/**
 * 获得权限数组
 * @returns {Number[]}
 */
function getCorpsPosFuncCfg(pos)
{
    return gameConfig.getCsvConfig("posFunc", pos).posFunc.split('|');
}
/**
 * 判断某个职位是否有该职能
 * @param pos 职位
 * @param funcId 职能
 * @returns {boolean}
 */
function checkHasFunc(pos, funcId)
{
    var arr = gameConfig.getCsvConfig("posFunc", pos).posFunc.split('|');
    for(let i = 0,len = arr.length; i<len; i++)
    {
        if(funcId == arr[i])
            return true;
    }
    return false;
}

/**
 * 获得公会初始宣言配置
 * @returns {CorpsDeclareConfig}
 */
function getCorpsDecalreCfg(id)
{
    return gameConfig.getCsvConfig("initDeclare")[id];
}
/**
 * 获取公会建设配置
 * @param id
 * @returns {CorpsBuildConfig}
 */
function getCorpsBuildCfg(id)
{
    return gameConfig.getCsvConfig("corpsBuild")[id];
}

exports.CorpsLevelConfig = CorpsLevelConfig;
exports.getCorpsLevelCfg = getCorpsLevelCfg;
exports.CorpsBaseConfig = CorpsBaseConfig;
exports.getCorpsBaseCfg = getCorpsBaseCfg;
exports.CorpsPosFuncConfig = CorpsPosFuncConfig;
exports.getCorpsPosFuncCfg = getCorpsPosFuncCfg;
exports.checkHasFunc = checkHasFunc;
exports.CorpsDeclareConfig = CorpsDeclareConfig;
exports.getCorpsDecalreCfg = getCorpsDecalreCfg;
exports.CorpsBuildConfig = CorpsBuildConfig;
exports.getCorpsBuildCfg = getCorpsBuildCfg;