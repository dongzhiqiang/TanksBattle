"use strict";

const CmdIdsActivity = {
    CMD_ENTER_GOLD_LEVEL : 1,
    CMD_SWEEP_GOLD_LEVEL : 2,
    CMD_END_GOLD_LEVEL   : 3,

    CMD_ENTER_HADES_LEVEL : 4,
    CMD_SWEEP_HADES_LEVEL  : 5,
    CMD_END_HADES_LEVEL  : 6,

    CMD_ENTER_VENUS_LEVEL : 7,
    CMD_END_VENUS_LEVEL  : 8,

    CMD_REQ_CHALLENGERS : 9,
    CMD_START_CHALLENGE : 10,
    CMD_END_ARENA_COMBAT : 11,
    CMD_ARENA_BUY_CHANCE : 12,
    CMD_ARENA_COMBAT_LOG : 13,

    CMD_ENTER_GUARD_LEVEL : 14,
    CMD_END_GUARD_LEVEL  : 15,
    CMD_SWEEP_GUARD_LEVEL  : 16,

    PUSH_SYNC_PROP       : -1,   //同步属性
};

const ResultCodeActivity = {
    DAY_MAX_CNT : 1, //今日次数已用完
    IN_COOL_DOWN : 2, //还在冷却中
    PRE_MODE_WRONG : 3, //请通过前一个难度
    HERO_LVL_WRONG : 4, //您的等级还不够
    LVL_NO_START : 5, //还没有开始
    CAN_NOT_SWEEP : 6, //本难度暂不可以扫荡
    ACTIVITY_NOT_START : 7, //活动尚未开启
    ROLE_NOT_EXISTS : 8, //角色不存在
    ARENA_NOT_CHALLENGER : 9, //要挑战的角色不在挑战对方列表
    DIAMOND_INSUFFICIENT : 10, //钻石不足
    BUY_CHANCE_MAX_COUNT : 11, //今日购买次数达到上限
    ACTIVITY_ENTERED: 12, //已挑战过该活动
};

//////////////////////////////
class EnterGoldLevelVo
{
    constructor() {
        this.mode = 0; //难度模式
    }

    static fieldsDesc() {
        return {
            mode: {type: Number, notNull: true}
        };
    }
}

class SweepGoldLevelVo
{
    constructor() {
        this.mode = 0; //难度模式
        this.hpMax = 0; //总生命值
    }

    static fieldsDesc() {
        return {
            mode: {type: Number, notNull: true},
            hpMax: {type: Number, notNull: true}
        };
    }
}

class EndGoldLevelVo
{
    constructor() {
        this.hp = 0; //剩余生命值
        this.hpMax = 0; //总生命值
    }

    static fieldsDesc() {
        return {
            hp: {type: Number, notNull: true},
            hpMax: {type: Number, notNull: true}
        };
    }
}

class EnterHadesLevelVo
{
    constructor() {
        this.mode = 0; //难度模式
    }

    static fieldsDesc() {
        return {
            mode: {type: Number, notNull: true}
        };
    }
}

class EndHadesLevelVo
{
    constructor() {
        this.wave = 0; //波次
        this.bossCount = 0; //变身boss数
    }

    static fieldsDesc() {
        return {
            wave: {type: Number, notNull: true},
            bossCount: {type: Number, notNull: true}
        };
    }
}

class EndVenusLevelVo
{
    constructor() {
        this.evaluation = 0; //评价
        this.percentage = 0; //百分比
    }

    static fieldsDesc() {
        return {
            evaluation: {type: Number, notNull: true},
            percentage: {type: Number, notNull: true}
        };
    }
}

class SweepHadesLevelVo
{
    constructor() {
        this.mode = 0; //难度模式
    }

    static fieldsDesc() {
        return {
            mode: {type: Number, notNull: true}
        };
    }
}

class EnterVenusLevelVo
{
    constructor() {
    }

    static fieldsDesc() {
        return {};
    }
}

class ReqArenaChallengersVo
{
    constructor() {
        this.listTime = 0; //对方ID列表更新时间
        this.dataTime = 0; //对手信息更新时间
    }

    static fieldsDesc() {
        return {
            listTime: {type: Number, notNull: true},
            dataTime: {type: Number, notNull: true}
        };
    }
}

class ReqStartChallengeVo
{
    constructor() {
        this.heroId = 0;    //对方ID
    }

    static fieldsDesc() {
        return {
            heroId: {type: Number, notNull: true}
        };
    }
}

class ReqEndArenaCombatVo
{
    constructor() {
        this.weWin = false; //是否我方赢了
    }

    static fieldsDesc() {
        return {
            weWin: {type: Boolean, notNull: true}
        };
    }
}

class ReqArenaLogVo
{
    constructor() {
        this.lastTime = 0;  //最近一条记录的时间
    }

    static fieldsDesc() {
        return {
            lastTime: {type: Number, notNull: true}
        };
    }
}

class EnterGuardLevelVo
{
    constructor() {
        this.mode = 0; //难度模式
    }

    static fieldsDesc() {
        return {
            mode: {type: Number, notNull: true}
        };
    }
}

class EndGuardLevelVo
{
    constructor() {
        this.wave = 0; //波次
        this.point = 0;
    }

    static fieldsDesc() {
        return {
            wave: {type: Number, notNull: true},
            point: {type: Number, notNull: true},
        };
    }
}

class SweepGuardLevelVo
{
    constructor() {
        this.mode = 0; //难度模式
    }

    static fieldsDesc() {
        return {
            mode: {type: Number, notNull: true}
        };
    }
}
//////////////////////////////
class EnterGoldLevelResultVo
{
    /**
     *
     * @param {number} mode
     */
    constructor(mode) {
        this.mode = mode;
    }
}

class EndGoldLevelResultVo
{
    constructor() {
        this.rate = "C";
        this.gold = 0;
        this.score = 0;
        this.damage = 0;
        this.rewards = {};
    }
}

class EnterHadesLevelResultVo
{
    /**
     *
     * @param {number} mode
     */
    constructor(mode) {
        this.mode = mode;
    }
}

class EndHadesLevelResultVo
{
    constructor() {
        this.evaluation = 0;
        this.exp = 0;
        this.wave = 0;
        this.bossCount = 0;
        this.itemList = [];
    }
}

class EndVenusLevelResultVo
{
    constructor() {
        this.evaluation = 0; //评价
        this.percentage = 0; //百分比
        this.soul = 0;
        this.stamina = 0;
    }


}

class ArenaChallengerVo
{
    constructor() {
        this.heroId = 0;
        this.name = "";
        this.level = 0;
        this.power = 0;
        this.score = 0;
        this.corpsId = 0;
        this.roleId = "";
        this.pet1Guid = "";
        this.pet1RoleId = "";
        this.pet2Guid = "";
        this.pet2RoleId = "";
    }
}

class ArenaChallengerWithRankVo
{
    constructor() {
        this.rank = -1;
        this.info = null;
    }
}

class ReqArenaChallengersResultVo
{
    constructor() {
        this.clientNew = false;
        this.listTime = 0;
        this.dataTime = 0;
        this.myRankVal = -1;
        this.challengers = [];
    }
}

class ArenaLogItemVo
{
    constructor() {
        this.win = false;
        this.oldRank = -1;
        this.rank = -1;
        this.opHeroId = 0;
        this.opRoleId = "";
        this.opName = "";
        this.opOldScore = 0;
        this.time = 0;
    }
}

class ReqArenaLogResultVo
{
    constructor() {
        this.clientNew = false;
        /**
         *
         * @type {ArenaLogItemVo[]}
         */
        this.logs = [];
    }
}

class ReqEndArenaCombatResultVo
{
    constructor() {
        this.weWin = false;
        this.myRankVal = -1;
        this.myOldRankVal = -1;
        this.rewards = {};
    }
}

class EnterGuardLevelResultVo
{
    /**
     *
     * @param {number} mode
     */
    constructor(mode) {
        this.mode = mode;
    }
}

class EndGuardLevelResultVo
{
    constructor() {
        this.evaluation = 0;
        this.exp = 0;
        this.wave = 0;
        this.point = 0;
        this.itemList = [];
    }
}
//////////////////////////////
class SyncActivityPropVo {
    /**
     *
     * @param {object.<string, *>} props
     */
    constructor(props) {
        this.props = props;
    }
}

//////////////////////////////
exports.CmdIdsActivity = CmdIdsActivity;
exports.ResultCodeActivity = ResultCodeActivity;

exports.EnterGoldLevelVo = EnterGoldLevelVo;
exports.SweepGoldLevelVo = SweepGoldLevelVo;
exports.EndGoldLevelVo = EndGoldLevelVo;
exports.EnterHadesLevelVo = EnterHadesLevelVo;
exports.SweepHadesLevelVo = SweepHadesLevelVo;
exports.EndHadesLevelVo = EndHadesLevelVo;
exports.EnterVenusLevelVo = EnterVenusLevelVo;
exports.EndVenusLevelVo = EndVenusLevelVo;
exports.ReqArenaChallengersVo = ReqArenaChallengersVo;
exports.ReqStartChallengeVo = ReqStartChallengeVo;
exports.ReqEndArenaCombatVo = ReqEndArenaCombatVo;
exports.ReqArenaLogVo = ReqArenaLogVo;
exports.EnterGuardLevelVo = EnterGuardLevelVo;
exports.EndGuardLevelVo = EndGuardLevelVo;
exports.SweepGuardLevelVo = SweepGuardLevelVo;

exports.EnterGoldLevelResultVo = EnterGoldLevelResultVo;
exports.EndGoldLevelResultVo = EndGoldLevelResultVo;
exports.EnterHadesLevelResultVo = EnterHadesLevelResultVo;
exports.EndHadesLevelResultVo = EndHadesLevelResultVo;
exports.EndVenusLevelResultVo = EndVenusLevelResultVo;
exports.ArenaChallengerVo = ArenaChallengerVo;
exports.ArenaChallengerWithRankVo = ArenaChallengerWithRankVo;
exports.ReqArenaChallengersResultVo = ReqArenaChallengersResultVo;
exports.ArenaLogItemVo = ArenaLogItemVo;
exports.ReqArenaLogResultVo = ReqArenaLogResultVo;
exports.ReqEndArenaCombatResultVo = ReqEndArenaCombatResultVo;
exports.EnterGuardLevelResultVo = EnterGuardLevelResultVo;
exports.EndGuardLevelResultVo = EndGuardLevelResultVo;

exports.SyncActivityPropVo = SyncActivityPropVo;