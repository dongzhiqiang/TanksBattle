"use strict";

const CmdIdsEliteLevel = {
    CMD_ENTER_ELITE_LEVEL: 1, // 进入精英关卡
    CMD_END_ELITE_LEVEL: 2, // 结算精英关卡
    CMD_SWEEP_ELITE_LEVEL: 3, // 扫荡精英关卡
    CMD_GET_FIRST_REWARD: 4, // 获取首杀奖励
    CMD_RESET_ELITE_LEVEL: 5, // 重置精英关卡

    PUSH_ADD_OR_UPDATE_ELITE_LEVEL: -1, // 推送精英关卡修改
};

const ResultCodeEliteLevel = {
    ELITE_LEVEL_NOT_EXISTS: 1, //精英关卡不存在
    ELITE_LEVEL_NO_STAMINA: 2, // 体力不足，不能进关卡
    ELITE_LEVEL_NO_NUM: 3, // 次数用完
    ELITE_LEVEL_NO_RECORD: 4, // 结算时没有进入关卡的记录
    ELITE_LEVEL_MUST_PASS: 5, //未通关不能扫荡
    ELITE_LEVEL_SWEEP_COND: 6, //未满足扫荡条件
    ELITE_LEVEL_REWARDED: 7, //已领取过首杀奖励
    ELITE_LEVEL_NO_RESET_NUM: 8, //重置次数已用完
    ELITE_LEVEL_NEED_PASS_LEVEL: 9, //需要通过前置关卡
};

/////////////////////////////////请求类////////////////////////////
class EnterEliteLevelRequestVo {
    constructor() {
        this.levelId = 0;
    }

    static fieldsDesc() {
        return {
            levelId: {type: Number},
        };
    }
}

class EndEliteLevelRequestVo {
    constructor() {
        this.levelId = 0;
        this.time = 0;
        this.isWin = false;
        this.starsInfo = {};
        this.monsterItems = [];
        this.specialItems = [];
        this.bossItems = [];
        this.boxItems = [];
    }

    static fieldsDesc() {
        return {
            levelId : {type: Number},
            time : {type: Number},
            isWin : {type: Boolean},
            starsInfo : {type: Object},
            monsterItems : {type: Array},
            specialItems : {type: Array},
            bossItems : {type: Array},
            boxItems : {type: Array}
        };
    }
}

class SweepEliteLevelRequestVo
{
    constructor() {
        this.levelId = 0;
        this.multiple = false;
    }

    static fieldsDesc() {
        return {
            levelId : {type: Number, notNull: true},
            multiple : {type: Boolean, notNull: true},
        };
    }
}

class GetFirstRewardRequestVo {
    constructor() {
        this.levelId = 0;
    }

    static fieldsDesc() {
        return {
            levelId: {type: Number},
        };
    }
}

class ResetEliteLevelRequestVo {
    constructor() {
        this.levelId = 0;
    }

    static fieldsDesc() {
        return {
            levelId: {type: Number},
        };
    }
}

/////////////////////////////////回复类////////////////////////////

class EnterEliteLevelResultVo{
    constructor(levelId, dropItems) {
        this.levelId = levelId;
        this.dropItems = dropItems;
    }

    static fieldsDesc() {
        return {
            levelId: {type: Number, notNull: true},
            dropItems: {type: Object, notNull: true}
        }
    }
}


class EndEliteLevelResultVo {
    constructor(){
        this.levelId = 0;
        this.isWin = false;
        this.heroExp = 0;
        this.pet1Exp = 0;
        this.pet2Exp = 0;
    }

    static fieldsDesc() {
        return {
            levelId: {type: Number},
            isWin: {type: Boolean},
            heroExp: {type: Number},
            pet1Exp: {type: Number},
            pet2Exp: {type: Number},
        };
    }
}

class SweepEliteLevelResultVo
{
    constructor() {
        this.levelId = 0;

        /**
         *
         * @type {SweepLevelRewardInfo[]}
         */
        this.rewards = [];
    }
}

class GetFirstRewardResultVo
{
    constructor() {
        this.levelId = 0;
    }
}

class ResetEliteLevelResultVo
{
    constructor() {
        this.levelId = 0;
    }
}
/////////////////////////////////推送类////////////////////////////

class AddOrUpdateEliteLevelVo {
    /**
     * @param {boolean} isAdd - 否则是update
     * @param {EliteLevel} eliteLevel
     */
    constructor(isAdd, eliteLevel) {
        this.isAdd = isAdd;
        this.eliteLevel = eliteLevel;
    }
}


exports.CmdIdsEliteLevel = CmdIdsEliteLevel;
exports.ResultCodeEliteLevel = ResultCodeEliteLevel;
exports.EnterEliteLevelRequestVo = EnterEliteLevelRequestVo;
exports.EndEliteLevelRequestVo = EndEliteLevelRequestVo;
exports.SweepEliteLevelRequestVo = SweepEliteLevelRequestVo;
exports.GetFirstRewardRequestVo = GetFirstRewardRequestVo;
exports.ResetEliteLevelRequestVo = ResetEliteLevelRequestVo;
exports.EnterEliteLevelResultVo = EnterEliteLevelResultVo;
exports.EndEliteLevelResultVo = EndEliteLevelResultVo;
exports.SweepEliteLevelResultVo = SweepEliteLevelResultVo;
exports.GetFirstRewardResultVo = GetFirstRewardResultVo;
exports.ResetEliteLevelResultVo = ResetEliteLevelResultVo;
exports.AddOrUpdateEliteLevelVo = AddOrUpdateEliteLevelVo;