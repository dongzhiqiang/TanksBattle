"use strict";

var gameConfig = require("./gameConfig");

class ArenaBasicCfg
{
    constructor() {
        this.openLevel = 0;
        this.limitTime = 0;
        this.freeChance	= 0;
        this.coolDown = 0;
        this.buyChance = 0;
        this.buyChancePrice = 0;
        this.roomId	= "";
        this.movieName = "";
        this.itsHeroPos = [];
        this.itsPet1Pos = [];
        this.itsPet2Pos = [];
        this.myPet1Pos = [];
        this.myPet2Pos = [];
        this.addHateValue = 0;
        this.itsHeroBornType = "";
        this.itsHeroDeadType = "";
        this.itsPetDeadType = "";
        this.myHeroDeadType = "";
        this.myPetDeadType = "";
        this.spartaPropRateId = "";
        this.heroShieldBuff = 0;
        this.maxRankNum	= 0;
        this.showRankNum = 0;
        this.chooseUpwards = 0;
        this.chooseDownwards = 0;
        this.chooseGapMinFactor = 0;
        this.chooseGapMaxFactor = 0;
        this.logNum = 0;
        this.dayRewardTime = 0;
        this.weekRewardDay = 0;
        this.weekRewardTime = 0;
        this.winRewardId = 0;
        this.loseRewardId = 0;
        this.ruleIntro = "";
    }

    static fieldsDesc() {
        return {
            openLevel: {type: Number},
            limitTime: {type: Number},
            freeChance: {type: Number},
            coolDown: {type: Number},
            buyChance: {type: Number},
            buyChancePrice: {type: Number},
            roomId: {type: String},
            movieName: {type: String},
            itsHeroPos: {type: Array, elemType: Number},
            itsPet1Pos: {type: Array, elemType: Number},
            itsPet2Pos: {type: Array, elemType: Number},
            myPet1Pos: {type: Array, elemType: Number},
            myPet2Pos: {type: Array, elemType: Number},
            addHateValue: {type: Number},
            itsHeroBornType: {type: String},
            itsHeroDeadType: {type: String},
            itsPetDeadType: {type: String},
            myHeroDeadType: {type: String},
            myPetDeadType: {type: String},
            spartaPropRateId: {type: String},
            heroShieldBuff: {type: Number},
            maxRankNum: {type: Number},
            showRankNum: {type: Number},
            chooseUpwards: {type: Number},
            chooseDownwards: {type: Number},
            chooseGapMinFactor: {type: Number},
            chooseGapMaxFactor: {type: Number},
            logNum: {type: Number},
            dayRewardTime: {type: Number},
            weekRewardDay: {type: Number},
            weekRewardTime: {type: Number},
            winRewardId: {type: Number},
            loseRewardId: {type: Number},
            ruleIntro: {type: String},
        };
    }
}

class ArenaGradeCfg
{
    constructor() {
        this.grade = 0;
        this.gradeName = "";
        this.iconName = "";
        this.nameImg = "";
        this.minScore = 0;
        this.maxScore = 0;
        this.atkWinScore = 0;
        this.atkCtnWinScore = 0;
        this.dfdLoseScore = 0;
        this.dayRewardId = 0;
        this.upgradeRewardId = 0;
    }

    static fieldsDesc() {
        return {
            grade: {type: Number},
            gradeName: {type: String},
            iconName: {type: String},
            nameImg: {type: String},
            minScore: {type: Number},
            maxScore: {type: Number},
            atkWinScore: {type: Number},
            atkCtnWinScore: {type: Number},
            dfdLoseScore: {type: Number},
            dayRewardId: {type: Number},
            upgradeRewardId: {type: Number},
        };
    }
}

class ArenaRobotCfg
{
    constructor() {
        this.robotId  = 0;
        this.robotNum = 0;
        this.minScore = 0;
        this.maxScore = 0;

    }

    static fieldsDesc() {
        return {
            robotId:  {type: Number},
            robotNum: {type: Number},
            minScore: {type: Number},
            maxScore: {type: Number},
        };
    }
}

class ArenaRankCfg
{
    constructor() {
        this.rank  = 0;
        this.rewardId = 0;

    }

    static fieldsDesc() {
        return {
            rank:  {type: Number},
            rewardId: {type: Number},
        };
    }
}

/**
 *
 * @returns {ArenaBasicCfg}
 */
function getArenaBasicCfg()
{
    return gameConfig.getCsvConfig("arenaBasic")[0];
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {ArenaGradeCfg}
 */
function getArenaGradeCfg(key)
{
    return gameConfig.getCsvConfig("arenaGrade", key);
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {ArenaRobotCfg}
 */
function getArenaRobotCfg(key)
{
    return gameConfig.getCsvConfig("arenaRobot", key);
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {ArenaRankCfg}
 */
function getArenaRankCfg(key)
{
    return gameConfig.getCsvConfig("arenaRank", key);
}


function getGradeByScore(score)
{
    var cfgs = getArenaGradeCfg();
    for (var grade in cfgs) //注意grade是字符串，如果要当整数用，请用parseInt(x,10)转成整数
    {
        var cfg = cfgs[grade];
        if (score >= cfg.minScore && score <= cfg.maxScore)
            return cfg.grade;   //注意grade变量是字符串，而cfg.grade是数字
    }
    return 0;
}

exports.ArenaBasicCfg = ArenaBasicCfg;
exports.ArenaGradeCfg = ArenaGradeCfg;
exports.ArenaRobotCfg = ArenaRobotCfg;
exports.ArenaRankCfg = ArenaRankCfg;
exports.getArenaBasicCfg = getArenaBasicCfg;
exports.getArenaGradeCfg = getArenaGradeCfg;
exports.getArenaRobotCfg = getArenaRobotCfg;
exports.getArenaRankCfg = getArenaRankCfg;
exports.getGradeByScore = getGradeByScore;