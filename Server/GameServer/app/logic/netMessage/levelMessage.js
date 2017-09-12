/**
 * Created by pc20 on 2016/2/18.
 */
"use strict";

const CmdIdsLevel = {
    CMD_ENTER: 1,   //进入关卡
    CMD_END: 2,     //关卡结算
    CMD_SWEEP: 3,   //扫荡关卡
    CMD_STAR: 4     //评星奖励
};

const ResultCodeLevel = {
    LEVEL_NO_NUM: 1,        //次数没了
    LEVEL_NO_STAMINA: 2,    //体力没了
    LEVEL_NO_RECORD: 3,     //挑战结束的关卡服务端并没有记录
    LEVEL_CANNOT_USE_AI: 4, //不可挂机
    LEVEL_NOT_EXSTIS: 5,    //关卡不存在
    MUST_PASS_FIRST: 6,     //必须先通过此关
    SWEEP_COND_NOT_MATCH: 7,//不符合扫荡条件
    LEVEL_STAR_CANT_GET: 8, //不符合领取星级奖励条件
    LEVEL_CANNT_AUTOBATTLE: 9,//不允许使用自动战斗
    LEVEL_MUST_AUTOBATTLE: 10,  //只能使用自动战斗
};

/////////////////////////////////请求类////////////////////////////
class LevelEnterVo {
    constructor() {
        this.roomId = "";
        this.nodeId = "";
    }

    static fieldsDesc() {
        return {
            roomId: {type: String},
            nodeId: {type: String}
        };
    }
}

class LevelEndVo {
    constructor() {
        this.roomId = "";
        this.time = 1000;
        this.isWin = false;
        this.starts = 0;
    }

    static fieldsDesc() {
        return {
            roomId : {type: String},
            time : {type: Number},
            isWin : {type: Boolean},
            starts : {type: Number},
            monsterItems : {type: Array},
            specialItems : {type: Array},
            bossItems : {type: Array},
            boxItems : {type: Array}
        };
    }
}

class LevelStarRewardReq {
    constructor() {
        this.nodeId = "";
        this.starNum = 0;
    }

    static fieldsDesc() {
        return {
            nodeId : {type: String},
            starNum : {type: Number}
        };
    }
}

class SweepLevelReq
{
    constructor() {
        this.roomId = "";
        this.multiple = false;
    }

    static fieldsDesc() {
        return {
            roomId : {type: String, notNull: true},
            multiple : {type: Boolean, notNull: true},
        };
    }
}

/////////////////////////////////回复类////////////////////////////
class LevelVo{
    constructor(){
        this.isWin = false;
        this.roomId = "";
        this.nodeId = "";
        this.starsInfo = {};
        this.enterNum = 0;
        this.lastEnter = 0;
    }

    static fieldsDesc() {
        return {
            isWin: {type: Boolean},
            roomId: {type: String},
            nodeId: {type: String},
            starsInfo: {type: Object},
            enterNum: {type: Number},
            lastEnter: {type: Number}
        };
    }
}

class LevelUpdateVo{
    constructor(curNode, curLevel, level, dropItems) {
        this.level = level;
        this.curNode = curNode;
        this.curLevel = curLevel;
        this.dropItems = dropItems;
    }

    static fieldsDesc() {
        return {
            curNode: {type: String, notNull: true},
            curLevel: {type: String, notNull: true},
            level: {type: Object, notNull: true},
            dropItems: {type: Object, notNull: true}
        }
    }
}

class LevelRewardVo {
    constructor(){
        this.roomId = "";
        this.isWin = false;
        this.heroExp = 0;
        this.pet1Exp = 0;
        this.pet2Exp = 0;
        //this.itemList = [];
        this.updateInfo = null;
    }

    static fieldsDesc() {
        return {
            roomId: {type: String},
            isWin: {type: Boolean},
            heroExp: {type: Number},
            pet1Exp: {type: Number},
            pet2Exp: {type: Number},
            updateInfo : {type: Object, notNull: true}
        };
    }
}

class LevelStarRewardVo{
    constructor() {
        this.nodeId = "";
        this.starNum = 0;
    }

    static fieldsDesc() {
        return {
            nodeId: {type: String},
            starNum: {type: Number}
        };
    }
}

class SweepLevelRewardInfo
{
    constructor() {
        this.heroExp = 0;
        /**
         * 神侍GUID和给的经验的映射
         * @type {object.<string, number>}
         */
        this.petExps = {};
        /**
         * 物品ID和给的数量的映射
         * @type {object.<number, number>}
         */
        this.items = {};
    }
}

class SweepLevelRes
{
    constructor() {
        this.roomId = "";
        this.curNode = "";
        this.curLevel = "";
        /**
         *
         * @type {LevelVo}
         */
        this.levelInfo = {};
        /**
         *
         * @type {SweepLevelRewardInfo[]}
         */
        this.rewards = [];
    }
}

/////////////////////////////////推送类////////////////////////////


/////////////////////////////////导出元素////////////////////////////
exports.CmdIdsLevel = CmdIdsLevel;
exports.ResultCodeLevel = ResultCodeLevel;

exports.LevelEnterVo = LevelEnterVo;
exports.LevelEndVo = LevelEndVo;
exports.LevelStarRewardReq = LevelStarRewardReq;
exports.SweepLevelReq = SweepLevelReq;

exports.LevelVo = LevelVo;
exports.LevelRewardVo = LevelRewardVo;
exports.LevelUpdateVo = LevelUpdateVo;
exports.LevelStarRewardVo = LevelStarRewardVo;
exports.SweepLevelRewardInfo = SweepLevelRewardInfo;
exports.SweepLevelRes = SweepLevelRes;