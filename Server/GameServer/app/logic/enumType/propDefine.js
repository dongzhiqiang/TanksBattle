"use strict";

const PF_PROTECT = 1;                   //发给离线战斗、查看他人详情
const PF_PUBLIC  = 2 | PF_PROTECT;      //发给全部
const PF_SAVEDB  = 4;                   //要存盘

//属性枚举
//注意：在能表清意思的前提下，名字一定要越短越好
var enProp = {
    channelId       : 40,   //字符串，主角才有，渠道ID
    userId          : 41,   //字符串，主角才有，用户ID
    lastLogin       : 42,   //整数，主角才有，最近登录时间
    loginCount      : 43,   //整数，主角才有，登录次数
    heroId          : 44,   //整数，主角才有，主角ID，必须大于0
    gmFlag          : 45,   //整数，主角才有，如果带有这个标记，是GM
    guid            : 46,   //字符串，必须有
    createTime      : 47,   //整数，创建时间
    roleId          : 48,   //字符串，角色类型ID
    name            : 49,   //字符串，名字
    level           : 50,   //整数，等级
    exp             : 51,   //整数，经验值
    //curWeapon     : 52,   //整数，当前武器索引,弃用，放到武器部件里了
    stamina         : 53,   //整数，体力值
    advLv           : 54,   //整数，等阶
    star            : 55,   //整数，星级
    camp            : 56,   //整数，阵营
    gold            : 57,   //整数，金币
    staminaTime     : 64,   //整数，给体力的时间
    power           : 65,   //整数，战斗力
    corpsId         : 66,   //整数，军团ID
    arenaCoin       : 67,   //整数，竞技场兑换币
    diamond         : 70,   //整数，钻石
    robotId         : 71,   //整数，如果是机器人有机器人配置ID
    offline         : 72,   //整数，主角才有，如果是真的离线角色（就是不是掉线的），这个值是正整数，如果是掉线的角色，这个值是负整数
    lastLogout      : 73,   //整数，上次登出时间，这个登出不包括掉线、离线加载后的Role销毁，只是在线加载后的Role销毁
    staminaBuyNum   : 74,   //整数，体力购买次数
    staminaBuyTime  : 75,   //整数，体力购买时间
    vipLv           : 76,   //整数，vip等级
    hornNum         : 77,   //整数，世界频道聊天的喇叭数
    powerTotal      : 78,   //整数，总战力(包括出战宠物)
    corpsName       : 79,   //字符串，公会名字
    lastRankLike    : 82,   //整数，排行榜上次点赞时间
    rankLikeLog     : 83,   //字符串，排行榜点赞记录，是json数据，格式：{排行类型1:[点赞对象ID1,点赞对象ID2]}
    towerLevel      : 84,   //整数，预言者之塔挑战层
    towerWinTime    : 85,   //整数，预言者之塔最后一次胜利时间
    towerUseTime    : 86,   //整数，预言者之塔挑战用时
    towerEnterNums  : 87,   //整数，预言者之塔挑进入次数
    towerEnterTime  : 88,   //整数，预言者之塔挑最后一次进入时间
    upEquipTime     : 89,   //整数，上次升级装备时间
    upEquipNum      : 90,   //整数，每日升级装备次数
};

var enPropFight = {
    minFightProp   : 0,
    hpMax          : 1,
    atk            : 2,
    def            : 3,
    damageDef      : 4,
    damage         : 5,
    critical       : 6,
    criticalDef    : 7,
    criticalDamage : 8,
    fire           : 9,
    ice            : 10,
    thunder        : 11,
    dark           : 12,
    fireDef        : 13,
    iceDef         : 14,
    thunderDef     : 15,
    darkDef        : 16,
    hpCut          : 17,
    damageReflect  : 18,
    mpMax          : 19,
    speed          : 20,
    cdCut          : 21,
    shieldMax      : 22,
    maxFightProp   : 23,
};

//属性标记，默认就是私有，只发给自己
var propFlags = {};
propFlags[enProp.channelId] = PF_SAVEDB;
propFlags[enProp.userId]    = PF_SAVEDB;
propFlags[enProp.lastLogin] = PF_SAVEDB;
propFlags[enProp.loginCount]= PF_SAVEDB;
propFlags[enProp.heroId]    = PF_PUBLIC|PF_SAVEDB;
propFlags[enProp.gmFlag]    = PF_SAVEDB;
propFlags[enProp.guid]      = PF_PUBLIC|PF_SAVEDB;
propFlags[enProp.createTime]= PF_SAVEDB;
propFlags[enProp.roleId]    = PF_PUBLIC|PF_SAVEDB;
propFlags[enProp.name]      = PF_PUBLIC|PF_SAVEDB;
propFlags[enProp.level]     = PF_PUBLIC|PF_SAVEDB;
propFlags[enProp.exp]       = PF_SAVEDB;
//propFlags[enProp.curWeapon] = PF_PUBLIC|PF_SAVEDB;
propFlags[enProp.stamina]   = PF_SAVEDB;
propFlags[enProp.advLv]     = PF_PROTECT|PF_SAVEDB;
propFlags[enProp.star]      = PF_PROTECT|PF_SAVEDB;
propFlags[enProp.camp]      = PF_PROTECT|PF_SAVEDB;
propFlags[enProp.gold]      = PF_SAVEDB;
propFlags[enProp.staminaTime]   = PF_SAVEDB;
propFlags[enProp.power]         = PF_PROTECT|PF_SAVEDB;
propFlags[enProp.corpsId]       = PF_PROTECT|PF_SAVEDB;
propFlags[enProp.arenaCoin]     = PF_SAVEDB;
propFlags[enProp.diamond]       = PF_SAVEDB;
propFlags[enProp.robotId]       = PF_SAVEDB;
propFlags[enProp.offline]       = PF_PROTECT;
propFlags[enProp.lastLogout]    = PF_SAVEDB;
propFlags[enProp.staminaBuyNum] = PF_SAVEDB;
propFlags[enProp.staminaBuyTime]= PF_SAVEDB;
propFlags[enProp.vipLv]         = PF_PROTECT|PF_SAVEDB;
propFlags[enProp.hornNum]       = PF_SAVEDB;
propFlags[enProp.powerTotal]    = PF_PROTECT|PF_SAVEDB;
propFlags[enProp.lastRankLike]  = PF_SAVEDB;
propFlags[enProp.rankLikeLog]   = PF_SAVEDB;
propFlags[enProp.towerLevel]    = PF_SAVEDB;
propFlags[enProp.towerWinTime]  = PF_SAVEDB;
propFlags[enProp.towerUseTime]  = PF_SAVEDB;
propFlags[enProp.towerEnterNums]= PF_SAVEDB;
propFlags[enProp.towerEnterTime]= PF_SAVEDB;
propFlags[enProp.upEquipTime]   = PF_SAVEDB;
propFlags[enProp.upEquipNum]    = PF_SAVEDB;

/**
 * 带protect标记的属性（其实包括公有）
 * @type {object.<string, boolean>}
 */
var protectPropNames = {};

/**
 * 带public标记的属性
 * @type {object.<string, boolean>}
 */
var publicPropNames = {};

/**
 * 带savedb标记的属性
 * @type {object.<string, boolean>}
 */
var saveDBPropNames = {};

/**
 * 为了让getDBData快速，如果全部属性都可以保存，那就筛选了
 * @type {boolean}
 */
var allPropNeedSave = true;

/**
 * 属性描述
 * @typedef {Object} PropertyDescription
 * @property {string} name
 * @property {number} flag
 */

/**
 *
 * @type {object.<number, PropertyDescription>}
 */
var propConfigMap = {};

////////////////////////
for (var propName in enProp)
{
    var propId = enProp[propName];
    var propFlag = propFlags[propId] || 0;
    propConfigMap[propId] = {name:propName, flag:propFlag};
    protectPropNames[propName] = (propFlag & PF_PROTECT) !== 0; //为了检索效率，全部赋值
    publicPropNames[propName] = (propFlag & PF_PUBLIC) !== 0; //为了检索效率，全部赋值
    saveDBPropNames[propName] = (propFlag & PF_SAVEDB) !== 0; //为了检索效率，全部赋值
    if (!saveDBPropNames[propName])
        allPropNeedSave = false;
}
////////////////////////

////////////////////////////////////
exports.PF_PROTECT = PF_PROTECT;
exports.PF_PUBLIC = PF_PUBLIC;
exports.PF_SAVEDB = PF_SAVEDB;
exports.enProp = enProp;
exports.enPropFight = enPropFight;
exports.propFlags = propFlags;
exports.protectPropNames = protectPropNames
exports.publicPropNames = publicPropNames;
exports.saveDBPropNames = saveDBPropNames;
exports.allPropNeedSave = allPropNeedSave;
exports.propConfigMap = propConfigMap;