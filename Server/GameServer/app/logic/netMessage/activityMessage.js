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

    CMD_REQ_WARRIOR_TRIED : 17,   //请求勇士试炼信息
    CMD_REFRESH_WARRIOR : 18,      //刷新勇士试炼
    CMD_ENTER_WARRIOR_LEVEL:  19,   //进入勇士试炼
    CMD_END_WARRIOR_LEVEL:  20,      //结束勇士试炼
    CMD_WARRIOR_REWARD:  21,         //领取勇士试炼关卡奖励

    CMD_REQ_TREASURE_ROB : 22,
    CMD_START_TREASURE_ROB : 23,
    CMD_END_TREASURE_ROB : 24,
    CMD_REFRESH_TREASURE_ROB : 25,

    CMD_ARENA_GET_POS : 26,
    CMD_ARENA_SET_POS : 27,

    CMD_ENTER_TOWER : 28,           //预言者之塔 挑战进入
    CMD_END_TOWER : 29,             //预言者之塔 挑战结束
    CMD_GET_TOWER_REWARD : 30,      //预言者之塔 领取随机奖励

    PUSH_SYNC_PROP       : -1,   //同步属性
    PUSH_TREASURE_ROB_BATTLE_LOG   : -2, //推送神器抢夺战报
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
    SYSTEM_NOT_OPEN: 13,   //系统尚未开启
    LEVEL_HAS_TRIED: 14,     //关卡已经通关
    WARR_TRIED_TIMES_UNABLED: 15,   //勇士试炼次数不足
    WARR_CANNOT_REWARD: 16,    //勇士试炼没有可领取的奖励
    TOWER_NO_TYPE: 17,      //挑战类型没有
    TOWER_NO_CONFIG: 18,    //未找到对应配置
    TOWER_CANT_OPEN_RANDOM :19, //没有达到打开随即挑战的层级
    TREASURE_NOT_CHALLENGER : 20, //要抢夺的角色不在对方列表
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

class ArenaPosVo
{
    constructor(arenaPos) {
        this.arenaPos = arenaPos;
    }

    static fieldsDesc() {
        return {
            arenaPos: {type: String, notNull: true}
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
//请求勇士试炼信息
class WarriorTriedDataReq
{
    constructor() {
    }
}
//刷新勇士试炼关卡
class RefreshWarriorReq
{
    constructor() {
    }
}
//进入勇士试炼
class EnterWarriorReq
{
    constructor() {
        this.index = 0;
    }

    static fieldsDesc() {
        return {
            index: {type: Number, notNull: true},
        };
    }
}
//结束勇士试炼
class EndWarriorReq
{
    constructor() {
        this.index = 0;
        this.isWin = false;
    }

    static fieldsDesc() {
        return {
            index: {type: Number, notNull: true},
            isWin: {type: Boolean, notNull: true},
        };
    }
}
//领取勇士试炼奖励
class GetWarriorRewardReq
{
    constructor() {
        this.index = 0;
    }

    static fieldsDesc() {
        return {
            index: {type: Number, notNull: true},
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
        this.myScoreVal = 0;
        this.myOldScoreVal = 0;
        this.rewards = {};
        this.upgradeRewards = {};
        this.rewardId = 0;
        this.upgradeRewardId = 0;

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
//请求勇士试炼返回
class WarriorTriedDataRes
{
    constructor(triedData, reset) {
        this.triedData = triedData;
        this.reset = reset;
    }
}
//刷新试炼关卡返回
class RefreshWarriorRes
{
    constructor(refresh, trieds) {
        this.refresh = refresh;   //今日已刷新次数
        this.trieds = trieds;    //试炼关卡信息
    }
}
//进入勇士试炼返回
class EnterWarriorRes
{
    constructor(roomId, star, index) {
        this.roomId = roomId;
        this.star = star;
        this.index =  index;
    }
}

//结束勇士试炼返回
class EndWarriorRes
{
    constructor(index, isWin, remainTried) {
        this.index =  index;
        this.isWin = isWin;
        this.remainTried = remainTried;
    }
}
//领取勇士试炼奖励返回
class GetWarriorRewardRes
{
    constructor(index, trieds) {
        this.index =  index;
        this.trieds = trieds;
    }
}

//////////////////////////
class ReqTreasureRobRequestVo
{
    constructor() {
        this.listTime = 0; //对方ID列表更新时间
        this.dataTime = 0; //对手信息更新时间
        this.useGold = false; //是否使用金币强制刷新
    }

    static fieldsDesc() {
        return {
            listTime: {type: Number, notNull: true},
            dataTime: {type: Number, notNull: true},
            useGold: {type: Boolean, notNull: true}
        };
    }
}

class TreasureChallengerVo
{
    constructor() {
        this.heroId = 0;
        this.name = "";
        this.level = 0;
        this.power = 0;
        this.score = 0;
        this.corpsId = 0;
        this.roleId = "";
    }
}

class TreasureRobChallengerVo
{
    constructor() {
        this.rank = -1;
        this.itemId = 0;
        this.itemNum = 0;
        this.info = null;
    }
}

class ReqTreasureRobResultVo
{
    constructor() {
        this.clientNew = false;
        this.listTime = 0;
        this.dataTime = 0;
        this.challengers = [];
        this.battleLogs = [];
    }
}


class StartTreasureRobRequestVo
{
    constructor() {
        this.heroId = 0;    //对方ID
        this.battleLogIndex = -1; //战报index，非复仇-1
    }

    static fieldsDesc() {
        return {
            heroId: {type: Number, notNull: true},
            battleLogIndex: {type: Number, notNull: true}
        };
    }
}

class EndTreasureRobRequestVo
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

class EndTreasureRobResultVo
{
    constructor() {
        this.weWin = false;
        this.itemId = -1;
        this.itemNum = -1;
    }
}


//预言者之塔挑战界面请求进入
class EnterTowerReq
{
    constructor(){
        this.towerType = 0;
    }

    static fieldsDesc() {
        return {
            towerType: {type: Number},
        };
    }
}

//预言者之塔挑战界面请求进入返回
class EnterTowerRes
{
    constructor(){
        this.towerType = 0;
        this.roomId = "";
        this.towerInfo = {}
    }

    static fieldsDesc() {
        return {
            towerType: {type: Number},
            roomId: {type: String},
            towerInfo: {type: Object},
        };
    }
}
//预言者之塔结算请求
class EndTowerReq
{
    constructor(){
        this.towerType;
        this.roomId = "";
        this.isWin;
        this.useTime;
    }

    static fieldsDesc() {
        return {
            towerType: {type: Number},
            roomId: {type: String},
            isWin: {type: Boolean},
            useTime: {type: Number},
        };
    }
}
//预言者之塔结算请求返回
class EndTowerRes
{
    constructor(){
        this.towerType = 0;
        this.roomId = "";
        this.rewards = {};
        this.isWin = false;
    }

    static fieldsDesc() {
        return {
            towerType: {type: Number},
            roomId: {type: String},
            rewards: {type: Object},
            isWin: {type: Boolean},
        };
    }
}
//预言者之塔请求领取随机奖励
class GetTowerRewardReq
{
    constructor(){
        this.idx = 0;
    }

    static fieldsDesc() {
        return {
            idx: {type: Number},
        };
    }
}
//预言者之塔请求领取随机奖励返回
class GetTowerRewardRes
{
    constructor(){
        this.idx = 0;
    }

    static fieldsDesc() {
        return {
            idx: {type: Number},
        };
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
exports.ArenaPosVo = ArenaPosVo;
exports.EnterGuardLevelResultVo = EnterGuardLevelResultVo;
exports.EndGuardLevelResultVo = EndGuardLevelResultVo;

exports.SyncActivityPropVo = SyncActivityPropVo;
exports.WarriorTriedDataReq = WarriorTriedDataReq;
exports.WarriorTriedDataRes = WarriorTriedDataRes;
exports.RefreshWarriorReq = RefreshWarriorReq;
exports.RefreshWarriorRes = RefreshWarriorRes;
exports.EnterWarriorReq = EnterWarriorReq;
exports.EnterWarriorRes = EnterWarriorRes;
exports.EndWarriorReq = EndWarriorReq;
exports.EndWarriorRes = EndWarriorRes;
exports.GetWarriorRewardReq = GetWarriorRewardReq;
exports.GetWarriorRewardRes = GetWarriorRewardRes;
exports.ReqTreasureRobRequestVo = ReqTreasureRobRequestVo;
exports.TreasureChallengerVo = TreasureChallengerVo;
exports.TreasureRobChallengerVo = TreasureRobChallengerVo;
exports.ReqTreasureRobResultVo = ReqTreasureRobResultVo;
exports.StartTreasureRobRequestVo = StartTreasureRobRequestVo;
exports.EndTreasureRobRequestVo = EndTreasureRobRequestVo;
exports.EndTreasureRobResultVo = EndTreasureRobResultVo;

exports.EnterTowerReq = EnterTowerReq;
exports.EnterTowerRes = EnterTowerRes;
exports.EndTowerReq = EndTowerReq;
exports.EndTowerRes = EndTowerRes;
exports.GetTowerRewardReq = GetTowerRewardReq;
exports.GetTowerRewardRes = GetTowerRewardRes;
