"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var dateUtil = require("../../libs/dateUtil");
var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var eventMgr = require("../../libs/eventMgr");
var eventNames = require("../enumType/eventDefine").eventNames;
var rankTypes = require("../enumType/rankDefine").rankTypes;
var enProp = require("../enumType/propDefine").enProp;
var enActProp = require("../enumType/activityPropDefine").enActProp;
var propPart = require("../role/propPart");
var corpsMgr = require("../corps/corpsMgr");
var rankConfig = require("../gameConfig/rankConfig");
var mailMgr = require("../mail/mailMgr");

////////////模块内变量////////////
/**
 * 客户端网络连接处理函数
 * @typedef {Object} RankDataType
 * @property {string} type - 排行类型
 * @property {number} upTime - 更新时间
 * @property {number} clearTime - 上次清除排行的时间
 * @property {number} clearLikeTime - 上次清除点赞的时间
 * @property {object[]} data - 排行数据
 * @property {object} extra - 额外数据
 * @property {object} svrExtra - 服务端额外数据，不下发的字段
 */

/**
 * 格式：{类型名:{type:类型名, upTime:更新时间, clearTime:上次清除排行的时间, clearLikeTime:上次清除点赞的时间, data:排行数据数组, extra:额外数据, svrExtra:服务端额外数据}}
 * @type {object.<string, RankDataType>}
 */
var rankDataMap = {};

/**
 * 格式：{类型名:{键值:名次}}
 * @type {object.<string, object.<number|string, number>>}
 */
var keyRankValMap = {};

/**
 * 定时器的ID
 * @type {*}
 */
var timer = null;

/**
 * 定时任务的间隔
 * @type {number}
 */
const TIMER_TASK_INV = 1000 * 120;

/**
 * 排行数据类型
 * @type {object}
 */
const rankDataTypes = {
    HERO : 0,
    PET : 1,
    CORPS : 2,
};

/**
 * 排行清理方式
 * @type {object}
 */
const clearTypes = {
    CLEAR_TYPE_NONE : 0,
    CLEAR_TYPE_DAY  : 1,
    CLEAR_TYPE_WEEK : 2,
};

/////////////排行配置///////////
/**
 *
 * @type {object.<string, {rankDataType: number, clearType: number, rankCount: number, hasLike: boolean, robotCanAdd: boolean, heroIdField: string, cmpFunc: function, canAddToRank: function, getSrcObjFromRole: function, defValFunc: function, refreshFields: function, moniterRoleProps:string[]}>}
 * rankDataType：键类型
 * clearType：定时清的方式，可以选择不清
 * rankCount：最多排行多少个
 * hasLike：是否有点赞功能，注意，如果有点赞功能，务必把点赞计数字段命名为like
 * robotCanAdd：机器人是否可以加入排行
 * heroIdField：代表主角的字段名，如果没有对应的主角ID字段，就用空串
 * cmpFunc：排序比较函数，注意Array sort的自定义比较函数，返回-1表示要往前排
 * canAddToRank：是否可以加入排行榜，主要用于不在排行榜时，检测上榜用
 * getSrcObjFromRole：从Role获取真正用于排行的对象，可能是神侍，可能是公会信息对象等
 * defValFunc：如果不在排行榜内，如果客户端要看没上榜的排行榜字段，用这个函数生成
 * refreshFields：刷新排行数据，如果返回0，说明没变化，如果返回1，说明只要修改更新时间和保存数据库，如果返回2，说明还要重新排序，修改更新时间，保存数据库
 * moniterRoleProps：要监视的角色属性名列表
 */
var rankTypeInfos = {
    // goldLevel : {
    //     rankDataType: rankDataTypes.HERO,
    //     clearType:clearTypes.CLEAR_TYPE_DAY,
    //     rankCount:100,
    //     hasLike:false,
    //     robotCanAdd:false,
    //     heroIdField:"key",
    //     cmpFunc:function(a, b) { return a.score > b.score ? -1 : (a.score < b.score ? 1 : (a.power > b.power ? -1 : (a.power < b.power ? 1 : 0))); },
    //     canAddToRank:function(item) { return item.score > 0; },
    //     getSrcObjFromRole:function(role) { return role.isHero() ? role : null; },
    //     defValFunc:function(role) {
    //         return {
    //             key : role.getHeroId(),
    //             name : role.getString(enProp.name),
    //             level : role.getNumber(enProp.level),
    //             power : role.getNumber(enProp.powerTotal),
    //             score : dateUtil.isToday(role.getActivityPart().getNumber(enActProp.goldLvlTime)) ? role.getActivityPart().getNumber(enActProp.goldLvTodayMaxScore) : 0,
    //             corpsName : role.getNumber(enProp.corpsName)
    //         };
    //     },
    //     refreshFields: function(rankItem, role)
    //     {
    //         var name = role.getString(enProp.name);
    //         var level = role.getNumber(enProp.level);
    //         var power = role.getNumber(enProp.powerTotal);
    //         var corpsName = role.getNumber(enProp.corpsName);
    //         var flag1 = 0, flag2 = 0;
    //         if (rankItem.name !== name) {rankItem.name = name; flag1 = 1;}
    //         if (rankItem.level !== level) {rankItem.level = level; flag1 = 1;}
    //         if (rankItem.corpsName !== corpsName) {rankItem.corpsName = corpsName; flag1 = 1;}
    //         if (rankItem.power !== power) {rankItem.power = power; flag2 = 2;}
    //         return Math.max(flag1, flag2);
    //     },
    //     moniterRoleProps:["name", "level", "powerTotal", "corpsName"],
    // },
    arena : {
        rankDataType: rankDataTypes.HERO,
        clearType:clearTypes.CLEAR_TYPE_NONE,
        rankCount:10000,    //在doInit里会修正为配置里的
        hasLike:false,
        heroIdField:"key",
        robotCanAdd:true,
        cmpFunc:function(a, b) { return a.score > b.score ? -1 : (a.score < b.score ? 1 : (a.power > b.power ? -1 : (a.power < b.power ? 1 : 0))); },
        canAddToRank:function(item) { return item.score > 0; },
        getSrcObjFromRole:function(role) { return role.isHero() ? role : null; },
        defValFunc:function(role) {
            return {
                key : role.getHeroId(),
                name : role.getString(enProp.name),
                level : role.getNumber(enProp.level),
                power : role.getNumber(enProp.powerTotal),
                score : role.getActivityPart().getNumber(enActProp.arenaScore),
                roleId: role.getString(enProp.roleId),
                pet1Guid: role.getString(enProp.pet1Main),
                pet1RoleId: role.getString(enProp.pet1MRId),
                pet2Guid: role.getString(enProp.pet2Main),
                pet2RoleId: role.getString(enProp.pet2MRId),
            };
        },
        refreshFields: function(rankItem, role)
        {
            var name = role.getString(enProp.name);
            var level = role.getNumber(enProp.level);
            var power = role.getNumber(enProp.powerTotal);
            var pet1Guid = role.getString(enProp.pet1Main);
            var pet1RoleId = role.getString(enProp.pet1MRId);
            var pet2Guid = role.getString(enProp.pet2Main);
            var pet2RoleId = role.getString(enProp.pet2MRId);
            var flag1 = 0, flag2 = 0;
            if (rankItem.name !== name) {rankItem.name = name; flag1 = 1;}
            if (rankItem.level !== level) {rankItem.level = level; flag1 = 1;}
            if (rankItem.pet1Guid !== pet1Guid) {rankItem.pet1Guid = pet1Guid; flag1 = 1;}
            if (rankItem.pet1RoleId !== pet1RoleId) {rankItem.pet1RoleId = pet1RoleId; flag1 = 1;}
            if (rankItem.pet2Guid !== pet2Guid) {rankItem.pet2Guid = pet2Guid; flag1 = 1;}
            if (rankItem.pet2RoleId !== pet2RoleId) {rankItem.pet2RoleId = pet2RoleId; flag1 = 1;}
            if (rankItem.power !== power) {rankItem.power = power; flag2 = 2;}
            return Math.max(flag1, flag2);
        },
        moniterRoleProps:["name", "level", "powerTotal", "pet1Main", "pet1MRId", "pet2Main", "pet2MRId"],
    },
    fullPower : {
        rankDataType: rankDataTypes.HERO,
        clearType:clearTypes.CLEAR_TYPE_NONE,
        rankCount:100,
        hasLike:true,
        robotCanAdd:false,
        heroIdField:"key",
        cmpFunc:function(a, b) { return a.power > b.power ? -1 : (a.power < b.power ? 1 : 0); },
        canAddToRank:function(item) { return item.power > 0; },
        getSrcObjFromRole:function(role) { return role.isHero() ? role : null; },
        defValFunc:function(role) {
            return {
                key : role.getHeroId(),
                name : role.getString(enProp.name),
                level : role.getNumber(enProp.level),
                power : role.getNumber(enProp.power) + role.getNumber(enProp.powerPets),
                like : 0,
            };
        },
        refreshFields: function(rankItem, role)
        {
            var name = role.getString(enProp.name);
            var level = role.getNumber(enProp.level);
            var power = role.getNumber(enProp.power) + role.getNumber(enProp.powerPets);
            var flag1 = 0, flag2 = 0;
            if (rankItem.name !== name) {rankItem.name = name; flag1 = 1;}
            if (rankItem.level !== level) {rankItem.level = level; flag1 = 1;}
            if (rankItem.power !== power) {rankItem.power = power; flag2 = 2;}
            return Math.max(flag1, flag2);
        },
        moniterRoleProps:["name", "level", "power", "powerPets"],
    },
    realPower : {
        rankDataType: rankDataTypes.HERO,
        clearType:clearTypes.CLEAR_TYPE_NONE,
        rankCount:10000,
        hasLike:true,
        robotCanAdd:true,
        heroIdField:"key",
        cmpFunc:function(a, b) { return a.power > b.power ? -1 : (a.power < b.power ? 1 : 0); },
        canAddToRank:function(item) { return item.power > 0; },
        getSrcObjFromRole:function(role) { return role.isHero() ? role : null; },
        defValFunc:function(role) {
            return {
                key : role.getHeroId(),
                name : role.getString(enProp.name),
                level : role.getNumber(enProp.level),
                power : role.getNumber(enProp.powerTotal),
                like : 0,
            };
        },
        refreshFields: function(rankItem, role)
        {
            var name = role.getString(enProp.name);
            var level = role.getNumber(enProp.level);
            var power = role.getNumber(enProp.powerTotal);
            var flag1 = 0, flag2 = 0;
            if (rankItem.name !== name) {rankItem.name = name; flag1 = 1;}
            if (rankItem.level !== level) {rankItem.level = level; flag1 = 1;}
            if (rankItem.power !== power) {rankItem.power = power; flag2 = 2;}
            return Math.max(flag1, flag2);
        },
        moniterRoleProps:["name", "level", "power", "powerTotal"],
    },
    allPetPower : {
        rankDataType: rankDataTypes.HERO,
        clearType:clearTypes.CLEAR_TYPE_NONE,
        rankCount:100,
        hasLike:false,
        robotCanAdd:false,
        heroIdField:"key",
        cmpFunc:function(a, b) { return a.power > b.power ? -1 : (a.power < b.power ? 1 : 0); },
        canAddToRank:function(item) { return item.power > 0; },
        getSrcObjFromRole:function(role) { return role.isHero() ? role : null; },
        defValFunc:function(role) {
            return {
                key : role.getHeroId(),
                name : role.getString(enProp.name),
                level : role.getNumber(enProp.level),
                petNum : role.getPetsPart().getPetCount(),
                power : role.getNumber(enProp.powerPets),
            };
        },
        refreshFields: function(rankItem, role)
        {
            var name = role.getString(enProp.name);
            var level = role.getNumber(enProp.level);
            var petNum = role.getPetsPart().getPetCount();
            var power = role.getNumber(enProp.powerPets);
            var flag1 = 0, flag2 = 0;
            if (rankItem.name !== name) {rankItem.name = name; flag1 = 1;}
            if (rankItem.level !== level) {rankItem.level = level; flag1 = 1;}
            if (rankItem.petNum !== petNum) {rankItem.petNum = petNum; flag1 = 1;}
            if (rankItem.power !== power) {rankItem.power = power; flag2 = 2;}
            return Math.max(flag1, flag2);
        },
        moniterRoleProps:["name", "level", "powerPets"],
    },
    corps : {
        rankDataType: rankDataTypes.CORPS,
        clearType:clearTypes.CLEAR_TYPE_NONE,
        rankCount:50,
        hasLike:false,
        robotCanAdd:false,
        heroIdField:"",
        cmpFunc:function(a, b) { return a.power > b.power ? -1 : (a.power < b.power ? 1 : 0); },
        canAddToRank:function(item) { return item.power > 0; },
        getSrcObjFromRole:function(role) { return role.isHero() ? corpsMgr.getCorpsData(role.getNumber(enProp.corpsId)) : null; },
        defValFunc:function(corpsData) {
            return {
                key : corpsData.props.corpsId,
                name : corpsData.props.name,
                level : corpsData.props.level,
                president : corpsData.props.president,
                power : corpsMgr.getCorpsPower(corpsData.props.corpsId),
            };
        },
        refreshFields: function(rankItem, corpsData)
        {
            var name = corpsData.props.name;
            var level = corpsData.props.level;
            var president = corpsData.props.president;
            var power = corpsMgr.getCorpsPower(corpsData.props.corpsId);
            var flag1 = 0, flag2 = 0;
            if (rankItem.name !== name) {rankItem.name = name; flag1 = 1;}
            if (rankItem.level !== level) {rankItem.level = level; flag1 = 1;}
            if (rankItem.president !== president) {rankItem.president = president; flag1 = 1;}
            if (rankItem.power !== power) {rankItem.power = power; flag2 = 2;}
            return Math.max(flag1, flag2);
        },
        moniterRoleProps:[],
    },
    predictor : {
        rankDataType: rankDataTypes.HERO,
        clearType:clearTypes.CLEAR_TYPE_NONE,
        rankCount:100,
        hasLike:false,
        robotCanAdd:false,
        heroIdField:"key",
        cmpFunc:function(a, b) { return a.maxLayer > b.maxLayer ? -1 : (a.maxLayer < b.maxLayer ? 1 : (a.passTime < b.passTime ? -1 : (a.passTime > b.passTime ? 1 : 0))); }, //最大层按从大到小，过关时间按从少到多
        canAddToRank:function(item) { return item.maxLayer > 0; },
        getSrcObjFromRole:function(role) { return role.isHero() ? role : null; },
        defValFunc:function(role) {
            return {
                key : role.getHeroId(),
                name : role.getString(enProp.name),
                level : role.getNumber(enProp.level),
                maxLayer : role.getNumber(enProp.towerLevel),
                passTime : role.getNumber(enProp.towerUseTime),
            };
        },
        refreshFields: function(rankItem, role)
        {
            var name = role.getString(enProp.name);
            var level = role.getNumber(enProp.level);
            var layer = role.getNumber(enProp.towerLevel);
            var useTime = role.getNumber(enProp.towerUseTime);
            var flag1 = 0, flag2 = 0;
            if (rankItem.name !== name) {rankItem.name = name; flag1 = 1;}
            if (rankItem.level !== level) {rankItem.level = level; flag1 = 1;}
            if (rankItem.maxLayer !== layer) {rankItem.maxLayer = layer; flag2 = 2;}
            if (rankItem.passTime !== useTime) {rankItem.passTime = useTime; flag2 = 2;}
            return Math.max(flag1, flag2);
        },
        moniterRoleProps:["name", "level", "towerLevel", "towerUseTime"],
    },
    levelStar : {
        rankDataType: rankDataTypes.HERO,
        clearType:clearTypes.CLEAR_TYPE_NONE,
        rankCount:100,
        hasLike:false,
        robotCanAdd:false,
        heroIdField:"key",
        cmpFunc:function(a, b) { return a.starNum > b.starNum ? -1 : (a.starNum < b.starNum ? 1 : 0); },
        canAddToRank:function(item) { return item.starNum > 0; },
        getSrcObjFromRole:function(role) { return role.isHero() ? role : null; },
        defValFunc:function(role) {
            return {
                key : role.getHeroId(),
                name : role.getString(enProp.name),
                level : role.getNumber(enProp.level),
                starNum : role.getLevelsPart().getAllStars(),
            };
        },
        refreshFields: function(rankItem, role)
        {
            var name = role.getString(enProp.name);
            var level = role.getNumber(enProp.level);
            var starNum = role.getLevelsPart().getAllStars();
            var flag1 = 0, flag2 = 0;
            if (rankItem.name !== name) {rankItem.name = name; flag1 = 1;}
            if (rankItem.level !== level) {rankItem.level = level; flag1 = 1;}
            if (rankItem.starNum !== starNum) {rankItem.starNum = starNum; flag2 = 2;}
            return Math.max(flag1, flag2);
        },
        moniterRoleProps:["name", "level"],
    },
    petPower : {
        rankDataType: rankDataTypes.PET,
        clearType:clearTypes.CLEAR_TYPE_NONE,
        rankCount:100,
        hasLike:true,
        robotCanAdd:false,
        heroIdField:"heroId",
        cmpFunc:function(a, b) { return a.power > b.power ? -1 : (a.power < b.power ? 1 : 0); },
        canAddToRank:function(item) { return item.power > 0; },
        getSrcObjFromRole:function(role) {  if (role.isHero()) role = role.getPetsPart().getPetByGUID(role.getNumber(enProp.maxPowerPet)); return role; },
        defValFunc:function(role) {
            var owner = role.getOwner();
            return {
                key : role.getGUID(),
                heroId : owner.getHeroId(),
                petName : role.getString(enProp.name),
                heroName : owner.getString(enProp.name),
                power : role.getNumber(enProp.power),
                like : 0,
            };
        },
        refreshFields: function(rankItem, role)
        {
            var owner = role.getOwner();
            var heroId = owner.getHeroId();
            var petName = role.getString(enProp.name);
            var heroName = owner.getString(enProp.name);
            var power = role.getNumber(enProp.power);
            var flag1 = 0, flag2 = 0;
            if (rankItem.heroId !== heroId) {rankItem.heroId = heroId; flag1 = 1;}
            if (rankItem.petName !== petName) {rankItem.petName = petName; flag1 = 1;}
            if (rankItem.heroName !== heroName) {rankItem.heroName = heroName; flag1 = 1;}
            if (rankItem.power !== power) {rankItem.power = power; flag2 = 2;}
            return Math.max(flag1, flag2);
        },
        moniterRoleProps:["name", "power"],
    },
};

////////////排行类型归类////////
/**
 * 主角属性ID修改了要更新的排行榜类型名列表
 * @type {object.<string,string[]>}
 */
var moniterHeroPropsMap = {};

/**
 * 神侍属性ID修改了要更新的排行榜类型名列表
 * @type {object.<string,string[]>}
 */
var moniterPetPropsMap = {};

/**
 * 机器人主角属性ID修改了要更新的排行榜类型名列表
 * @type {object.<string,string[]>}
 */
var robotMoniterHeroPropsMap = {};

/**
 * 机器人神侍属性ID修改了要更新的排行榜类型名列表
 * @type {object.<string,string[]>}
 */
var robotMoniterPetPropsMap = {};

/**
 * 用于主角的排行类型名
 * @type {string[]}
 */
var heroRankTypes = [];

/**
 * 用于神侍的排行类型名
 * @type {string[]}
 */
var petRankTypes = [];

/**
 * 机器人主角可加入的排行类型名
 * @type {string[]}
 */
var robotHeroRankTypes = [];

/**
 * 机器人神侍可加入的排行类型名
 * @type {string[]}
 */
var robotPetRankTypes = [];

/**
 * 用于公会的排行类型名
 * @type {string[]}
 */
var corpsRankTypes = [];

/**
 * 有点赞功能的排行类型名
 * @type {string[]}
 */
var hasLikeRankTypes = [];

////////////全局执行//////////////
for (var rankType in rankTypeInfos)
{
    var info = rankTypeInfos[rankType];
    var rankDataType = info.rankDataType;
    var robotCanAdd = info.robotCanAdd;

    if (info.hasLike)
        hasLikeRankTypes.push(rankType);

    switch (rankDataType)
    {
        case rankDataTypes.PET:
            petRankTypes.push(rankType);
            if (robotCanAdd)
                robotPetRankTypes.push(rankType);
            break;
        case rankDataTypes.CORPS:
            corpsRankTypes.push(rankType);
            continue;
        default:
            heroRankTypes.push(rankType);
            if (robotCanAdd)
                robotHeroRankTypes.push(rankType);
            break;
    }

    var propsMap = rankDataType === rankDataTypes.PET ? moniterPetPropsMap : moniterHeroPropsMap;
    var petPropsMap = rankDataType === rankDataTypes.PET ? robotMoniterHeroPropsMap : robotMoniterPetPropsMap;

    var props = info.moniterRoleProps;
    for (var i = 0; i < props.length; ++i)
    {
        let propName = props[i];

        let typeArr = propsMap[propName];
        if (!typeArr)
            propsMap[propName] = typeArr = [];
        typeArr.pushIfNotExist(rankType);

        if (robotCanAdd)
        {
            let typeArr = petPropsMap[propName];
            if (!typeArr)
                petPropsMap[propName] = typeArr = [];
            typeArr.pushIfNotExist(rankType);
        }
    }
}
//为了提高效率，没有加入监视列表的属性名，也填充一个null值
for (let propName in enProp)
{
    if (!moniterHeroPropsMap[propName])
        moniterHeroPropsMap[propName] = null;
    if (!moniterPetPropsMap[propName])
        moniterPetPropsMap[propName] = null;
    if (!robotMoniterHeroPropsMap[propName])
        robotMoniterHeroPropsMap[propName] = null;
    if (!robotMoniterPetPropsMap[propName])
        robotMoniterPetPropsMap[propName] = null;
}

////////////函数集////////////
/**
 *
 * @returns {MyCollection}
 */
function getDBCollection()
{
    var db = dbUtil.getDB(0);
    return db.collection("rank");
}

/**
 *
 * @param {string} type
 * @returns {RankDataType}
 */
function getRankData(type)
{
    return rankDataMap[type];
}

/**
 *
 * @param {number} rankDataType
 * @param {Role|CorpsInfo} srcObj
 * @returns {number|string}
 */
function getKeyFromSrcObj(rankDataType, srcObj)
{
    switch (rankDataType)
    {
        case rankDataTypes.PET:
            return srcObj.getGUID();
        case rankDataTypes.CORPS:
            return srcObj.getString ? srcObj.getString(enProp.corpsId) : srcObj.props.corpsId;
        default:
            return srcObj.getHeroId();
    }
}

/**
 *
 * @param {string} type
 * @param {Role} role
 * @returns {number} 排名，0是第一名，-1表示没进入排名
 */
function getRankValueByRole(type, role)
{
    var info = rankTypeInfos[type];
    if (!info)
        return -1;
    var rankValMap = keyRankValMap[type];
    if (!rankValMap)
        return -1;
    var srcObj = info.getSrcObjFromRole(role);
    if (!srcObj)
        return -1;
    var key = getKeyFromSrcObj(info.rankDataType, srcObj);
    var rankVal = rankValMap[key];
    return rankVal == undefined ? -1 : rankVal;
}

/**
 *
 * @param {string} type
 * @param {number} key
 * @returns {number} 排名，0是第一名，-1表示没进入排名
 */
function getRankValueByKey(type, key)
{
    var rankValMap = keyRankValMap[type];
    if (!rankValMap)
        return -1;
    var rankVal = rankValMap[key];
    return rankVal == undefined ? -1 : rankVal;
}

/**
 *
 * @param {string} type
 * @param {Role} role
 * @returns {object}
 */
function getRankDataByRole(type, role)
{
    var info = rankTypeInfos[type];
    if (!info)
        return {rank:-1, data:{}};

    var srcObj = info.getSrcObjFromRole(role);
    if (!srcObj)
        return {rank:-1, data:{}};

    var rankData = rankDataMap[type];
    var rankValMap = keyRankValMap[type];
    var rowKey = getKeyFromSrcObj(info.rankDataType, srcObj);
    var rankVal = rankValMap ? rankValMap[rowKey] : undefined;

    if (rankData == undefined || rankVal == undefined)
        return {rank:-1, data:info.defValFunc(srcObj)};
    else
        return {rank:rankVal, data:rankData.data[rankVal]};
}

/**
 *
 * @param {string} type
 * @param {number|string} key
 * @returns {object|null}
 */
function getRankDataByKey(type, key)
{
    var rankData = rankDataMap[type];
    var rankValMap = keyRankValMap[type];
    var rankVal = rankValMap ? rankValMap[key] : undefined;

    if (rankData == undefined || rankVal == undefined)
    {
        return null;
    }
    else
    {
        return {rank:rankVal, data:rankData.data[rankVal]};
    }
}

function fastFindInsertPos(dataArr, newItem, cmpFunc, oldRankVal, leftFirst)
{
    var len = dataArr.length;

    if (leftFirst) {
        let index = oldRankVal - 1;
        for (; index >= 0; --index) {
            let d = dataArr[index];
            let cmpVal = cmpFunc(d, newItem);
            if (cmpVal <= 0)
                break;
        }

        if (index < oldRankVal - 1 || index === len - 1)
            return index + 1;

        index = oldRankVal + 1;
        for (; index < len; ++index) {
            let d = dataArr[index];
            let cmpVal = cmpFunc(d, newItem);
            if (cmpVal >= 0)
                break;
        }

        return index - 1;
    }
    else {
        let index = oldRankVal + 1;
        for (; index < len; ++index) {
            let d = dataArr[index];
            let cmpVal = cmpFunc(d, newItem);
            if (cmpVal >= 0)
                break;
        }

        if (index > oldRankVal + 1 || index === 1)
            return index - 1;

        index = oldRankVal - 1;
        for (; index >= 0; --index) {
            let d = dataArr[index];
            let cmpVal = cmpFunc(d, newItem);
            if (cmpVal <= 0)
                break;
        }

        return index + 1;
    }
}

function moveRankItem(dataArr, newItem, oldRankVal, newRankVal)
{
    if (oldRankVal > newRankVal)
    {
        for (let i = oldRankVal; i > newRankVal; --i)
            dataArr[i] = dataArr[i - 1];
    }
    else
    {
        for (let i = oldRankVal; i < newRankVal; ++i)
            dataArr[i] = dataArr[i + 1];
    }
    dataArr[newRankVal] = newItem;
}

function addToRankByRole(type, role)
{
    var info = rankTypeInfos[type];
    if (!info) {
        logUtil.error("addToRankByRole 排行缺少相关信息：" + type);
        return;
    }

    //如果是机器人或机器人的神侍，要判断是否可以加入机器人
    if (role.isRobot() && !info.robotCanAdd)
        return;

    if ((info.rankDataType === rankDataTypes.HERO && role.isHero())
        ||
        (info.rankDataType === rankDataTypes.PET && role.isPet()))
    {
        var newItem = info.defValFunc(role);
        addToRankByItem(type, newItem);
    }
}

/**
 *
 * @param {Role} role
 */
function addToAllRankByRole(role)
{
    var usedRankTypes = role.isPet() ? (role.isRobot() ? robotPetRankTypes : petRankTypes) : (role.isRobot() ? robotHeroRankTypes : heroRankTypes);
    for (var i = 0; i < usedRankTypes.length; ++i)
    {
        var rankType = usedRankTypes[i];
        var rankInfo = rankTypeInfos[rankType];
        var newItem = rankInfo.defValFunc(role);
        addToRankByItem(rankType, newItem);
    }
}

/**
 *
 * @param {string} type
 * @param {CorpsInfo} corpsInfo
 */
function addToRankByCorpsData(type, corpsInfo)
{
    var info = rankTypeInfos[type];
    if (!info) {
        logUtil.error("addToRankByCorpsData 排行缺少相关信息：" + type);
        return;
    }

    if (info.rankDataType !== rankDataTypes.CORPS)
        return;

    var newItem = info.defValFunc(corpsInfo);
    addToRankByItem(type, newItem);
}

/**
 *
 * @param {CorpsInfo} corpsInfo
 */
function addToAllRankByCorpsData(corpsInfo)
{
    for (var i = 0; i < corpsRankTypes.length; ++i)
    {
        var rankType = corpsRankTypes[i];
        var rankInfo = rankTypeInfos[rankType];
        var newItem = rankInfo.defValFunc(corpsInfo);
        addToRankByItem(rankType, newItem);
    }
}

/**
 *
 * @param {string} type
 * @param {object} newItem
 */
function addToRankByItem(type, newItem)
{
    var key = newItem.key;
    if (!key)
    {
        logUtil.error("addToRankByItem 排行数据请提供key，排行类型：" + type);
        return;
    }

    var rankData = rankDataMap[type];
    if (!rankData)
    {
        logUtil.error("addToRankByItem 不存在的排行类型：" + type);
        return;
    }

    var info = rankTypeInfos[type];
    if (!info)
    {
        logUtil.error("addToRankByItem 排行缺少相关信息：" + type);
        return;
    }

    var rankArr = rankData.data;
    var curTime = dateUtil.getTimestamp();
    var col = getDBCollection();

    //看看这个人是不是原来就在榜里
    var rankValMap = keyRankValMap[type];
    var rankVal = rankValMap[key];
    //原来就在排行里？
    if (rankVal >= 0)
    {
        var oldRankItem = rankArr[rankVal];
        //新旧值比较一下
        var cmpVal = info.cmpFunc(newItem, oldRankItem);
        //值没变？直接不更新了
        if (cmpVal === 0)
            return;
        //如果有like字段，那就复制这个字段到新对象里
        if (info.hasLike)
            newItem.like = oldRankItem.like;
        //找到新的插入点
        let newPos = fastFindInsertPos(rankArr, newItem, info.cmpFunc, rankVal, cmpVal < 0);
        //移动数据到新位置
        moveRankItem(rankArr, newItem, rankVal, newPos);
        //修改更新时间
        rankData.upTime = curTime;
        //排名没变？直接更新数据库数据就行了
        if (newPos === rankVal)
        {
            col.updateOneNoThrow({type:type}, {$set:{upTime:curTime,["data." + rankVal]:newItem}});
        }
        else
        {
            //生成排序映射
            fastGenerateKeyRankValMap(type, rankVal, newPos);
            //存盘
            col.updateOneNoThrow({type:type}, {$set:{upTime:curTime},$pull:{data:{key:key}}});
            col.updateOneNoThrow({type:type}, {$set:{upTime:curTime},$push:{data:{$each:[newItem],$slice:info.rankCount,$position:newPos}}});
        }
    }
    else
    {
        //不在排行榜里？看看够不够资格上榜
        if (!info.canAddToRank(newItem))
            return;

        //找到新的插入点
        let newPos = fastFindInsertPos(rankArr, newItem, info.cmpFunc, rankArr.length, true);
        //排不进前N？不用插入数据了
        if (newPos >= info.rankCount)
            return;
        //插入到新位置
        rankArr.splice(newPos, 0, newItem);
        //插入后，如果超出限制了，删除过量的数据
        if (rankArr.length > info.rankCount)
            rankArr.splice(info.rankCount);
        //修改更新时间
        rankData.upTime = curTime;
        //生成排序映射
        fastGenerateKeyRankValMap(type, rankArr.length - 1, newPos);
        //存盘
        col.updateOneNoThrow({type:type}, {$set:{upTime:curTime},$push:{data:{$each:[newItem],$slice:info.rankCount,$position:newPos}}});
    }
}

/**
 *
 * @param {string} type
 * @param {Role} role
 */
function removeFromRankByRole(type, role)
{
    var info = rankTypeInfos[type];
    var rankDataType = info.rankDataType;

    if (role.isRobot() && !info.robotCanAdd)
        return;

    if ((rankDataType === rankDataTypes.HERO && role.isHero())
        ||
        (rankDataType === rankDataTypes.PET && role.isPet()))
    {
        var key = getKeyFromSrcObj(rankDataType, role);
        removeFromRankByKey(type, key);
    }
}

function removeFromAllRankByRole(role)
{
    var key = getKeyFromSrcObj(role.isPet() ? rankDataTypes.PET : rankDataTypes.HERO, role);
    var usedRankTypes = role.isPet() ? (role.isRobot() ? robotPetRankTypes : petRankTypes) : (role.isRobot() ? robotHeroRankTypes : heroRankTypes);
    for (var i = 0; i < usedRankTypes.length; ++i)
    {
        var rankType = usedRankTypes[i];
        removeFromRankByKey(rankType, key);
    }
}

/**
 *
 * @param {string} type
 * @param {number|string} key
 */
function removeFromRankByKey(type, key)
{
    var rankData = rankDataMap[type];
    var rankValMap = keyRankValMap[type];
    var rankArr = rankData ? rankData.data : undefined;
    var rankVal = rankValMap ? rankValMap[key] : undefined;

    if (rankArr == undefined || rankVal == undefined)
        return;

    var curTime = dateUtil.getTimestamp();
    var col = getDBCollection();

    //删除数据项
    rankArr.splice(rankVal, 1);
    //修改更新时间
    rankData.upTime = curTime;
    //生成排序映射
    fastGenerateKeyRankValMap(type, rankVal, rankArr.length - 1);
    //存盘
    col.updateOneNoThrow({type:type}, {$set:{upTime:curTime},$pull:{data:{key:key}}});
}

function removeFromAllRankByCorpsId(corpsId)
{
    for (var i = 0; i < corpsRankTypes.length; ++i)
    {
        var rankType = corpsRankTypes[i];
        removeFromRankByKey(rankType, corpsId);
    }
}

/**
 *
 * @param {string} type
 * @param {string} key
 * @returns {number} 小于0表示点赞失败，大于等于0，表示新的赞数
 */
function doLikeRankItem(type, key)
{
    var rankInfo = rankTypeInfos[type];
    if (!rankInfo)
        return -1;

    if (!rankInfo.hasLike)
        return -1;

    var rankData = rankDataMap[type];
    var rankValMap = keyRankValMap[type];
    var rankVal = rankValMap ? rankValMap[key] : undefined;

    if (rankData == undefined || rankVal == undefined)
        return -1;

    var rankLimit = rankConfig.getRankBasicConfig().likeRankLimit;
    //rankVal是0~N，rankLimit是1~N，所以这里用等于
    if (rankVal >= rankLimit)
        return -1;

    var curTime = dateUtil.getTimestamp();

    //点赞数加一
    var rankItem = rankData.data[rankVal];
    rankItem.like = (rankItem.like || 0) + 1;

    //修改更新时间
    rankData.upTime = curTime;

    //存盘
    var col = getDBCollection();
    col.updateOneNoThrow({type:type}, {$set:{upTime:curTime,["data." + rankVal + ".like"]:rankItem.like}});

    return rankItem.like;
}

var fillRankItemsCoroutine = Promise.coroutine(function * (type, items){
    var rankData = rankDataMap[type];
    if (!rankData)
    {
        throw new Error("fillRankItems 不存在的排行类型：" + type);
    }

    var info = rankTypeInfos[type];
    if (!info)
    {
        throw new Error("fillRankItems 排行缺少相关信息：" + type);
    }

    if (items.length > info.rankCount)
    {
        logUtil.warn("fillRankItems 填充的项多于规定的，删除多余的：" + type);
        items.splice(info.rankCount, items.length - info.rankCount);
    }

    var curTime = dateUtil.getTimestamp();
    var col = getDBCollection();

    rankData.data = items;
    //修改更新时间
    rankData.upTime = curTime;
    rankData.clearTime = curTime;
    rankData.clearLikeTime = curTime;
    //生成排序映射
    generateKeyRankValMap(type);
    //存盘
    yield col.updateOne({type:type}, {$set:{upTime:curTime, clearTime:curTime, clearLikeTime:curTime, data:items}});
});

/**
 *
 * @param {string} type
 * @param {object[]} items
 * @returns {Promise}
 */
function fillRankItems(type, items) {
    return fillRankItemsCoroutine(type, items);
}

var doCustomDBUpdateCoroutine = Promise.coroutine(function * (type, updateObj, modifyUpTime, subQuery) {
    //直接获取数据，不检查了
    var rankData = rankDataMap[type];
    var col = getDBCollection();
    var queryObj = {type: type};

    //合并查询条件
    if (subQuery)
    {
        for (var k in subQuery)
            queryObj[k] = subQuery[k];
    }

    //看看要不要修改更新时间
    if (modifyUpTime)
    {
        rankData.upTime = dateUtil.getTimestamp();
        if (updateObj.$set)
            updateObj.$set.upTime = rankData.upTime;
        else
            updateObj.$set = {upTime : rankData.upTime};
    }

    //存盘
    yield col.updateOne(queryObj, updateObj);
});

/**
 * 自定义的操作，这里只允许修改extra、svrExtra的数据，extra、svrExtra的内存数据在外部修改，这里不修改extra、svrExtra的内存数据了
 * @param {string} type
 * @param {object} updateObj
 * @param {number?} modifyUpTime -可选，默认不修改upTime
 * @param {object?} subQuery - 可选的子查询条件，会合并到最终的查询，跟type的查询并列，比如{type:type,"svrExtra.tags.id":1}
 */
function doCustomDBUpdate(type, updateObj, modifyUpTime, subQuery) {
    return doCustomDBUpdateCoroutine(type, updateObj, modifyUpTime, subQuery);
}

/**
 *
 * @param {string} type
 */
function generateKeyRankValMap(type)
{
    var rankData = rankDataMap[type];
    if (!rankData)
        return;

    var rankValMap = {};
    keyRankValMap[type] = rankValMap;

    var arr = rankData.data;
    for (var i = 0; i < arr.length; ++i)
    {
        var row = arr[i];
        rankValMap[row.key] = i;
    }
}

/**
 *
 * @param {string} type
 * @param {number} oldRankVal
 * @param {number} newRankVal
 */
function fastGenerateKeyRankValMap(type, oldRankVal, newRankVal)
{
    var rankData = rankDataMap[type];
    var rankValMap = keyRankValMap[type];
    var rankArr = rankData.data;

    if (rankArr.length <= 0)
    {
        keyRankValMap[type] = {};
        return;
    }

    //相等也要处理，因为有可能是新的正好插入最后一个位置，
    //原来的最后位置的超出了，被删除了，oldRankVal是length-1，new和old一样了，
    //这时还是要更新key对应的index
    if (oldRankVal >= newRankVal)
    {
        for (let i = newRankVal; i <= oldRankVal; ++i)
        {
            let row = rankArr[i];
            rankValMap[row.key] = i;
        }
    }
    else if (oldRankVal < newRankVal)
    {
        for (let i = oldRankVal; i <= newRankVal; ++i)
        {
            let row = rankArr[i];
            rankValMap[row.key] = i;
        }
    }
}

/**
 *
 * @param type
 * @returns {RankDataType|null}
 */
function checkRankClearTime(type)
{
    var rankData = rankDataMap[type];
    if (!rankData)
        return null;

    var info = rankTypeInfos[type];
    if (!info)
        return null;

    var curTime = dateUtil.getTimestamp();

    if ((info.clearType == clearTypes.CLEAR_TYPE_DAY && !dateUtil.isSameDay(curTime, rankData.clearTime))
        ||
        (info.clearType == clearTypes.CLEAR_TYPE_WEEK && !dateUtil.isSameWeek(curTime, rankData.clearTime)))
    {
        //修改更新时间
        rankData.upTime = curTime;
        rankData.clearTime = curTime;
        rankData.clearLikeTime = curTime;
        //生成排序映射
        generateKeyRankValMap(type);
        //存盘
        var col = getDBCollection();
        col.updateOneNoThrow({type:type}, {$set:{data:[], upTime:curTime, clearTime:curTime, clearLikeTime:curTime}});

        if (info.clearType == clearTypes.CLEAR_TYPE_DAY)
            logUtil.debug("每天清除排行数据" + type);
        else if (info.clearType == clearTypes.CLEAR_TYPE_WEEK)
            logUtil.debug("每周清除排行数据" + type);
    }

    return rankData;
}

function checkCorpsRank()
{
    var corpsMap = corpsMgr.getCorpsMap();
    for (var i = 0; i < corpsRankTypes.length; ++i)
    {
        var rankType = corpsRankTypes[i];

        for (var corpsId in corpsMap)
        {
            var corpsInfo = corpsMap[corpsId];
            refreshRankDataForCorps(corpsInfo, rankType);
        }
    }
}

function checkLikeData()
{
    var curTime = dateUtil.getTimestamp();

    var heroLikeMap = {};
    var rankTypeArr = [];

    for (let i = 0; i < hasLikeRankTypes.length; ++i)
    {
        let rankType = hasLikeRankTypes[i];
        let rankData = rankDataMap[rankType];
        if (dateUtil.isSameDay(curTime, rankData.clearLikeTime))
            continue;

        rankTypeArr.push(rankType);

        let rankInfo = rankTypeInfos[rankType];
        let heroIdField = rankInfo.heroIdField;
        let rankArr = rankData.data;
        for (let j = 0; j < rankArr.length; ++j)
        {
            let rankItem = rankArr[j];
            if (rankItem.like > 0)
            {
                let heroId = rankItem[heroIdField];
                heroLikeMap[heroId] = (heroLikeMap[heroId] || 0) + rankItem.like;
            }
        }
    }

    var heroLikeArr = [];
    for (var key in heroLikeMap)
    {
        let heroId = parseInt(key);
        let likeNum = heroLikeMap[key];
        heroLikeArr.push({id:heroId, num:likeNum});
    }
    if (heroLikeArr.length <= 0)
        return;

    heroLikeArr.sort(function(a,b){return b.num - a.num;});

    var maxCnt = rankConfig.RankLikeReward.RowsCount;
    if (heroLikeArr.length > maxCnt)
        heroLikeArr.splice(maxCnt);

    var rewardCfgs = rankConfig.getRankLikeReward();
    for (let i = 0; i < heroLikeArr.length; ++i)
    {
        let item = heroLikeArr[i];
        let rankVal = i + 1;
        let heroId = item.id;
        let likeCnt = item.num;
        let items = rewardCfgs[rankVal].reward;
        //创建邮件
        let mail = mailMgr.createMail("排行榜点赞奖励", "系统", "恭喜，您今日在排行榜上总共被点赞" + likeCnt + "次，排名第" + rankVal + "位，根据排名您可获得的奖励已发送，请及时领取。", items);
        //发送
        mailMgr.sendMailToMultiRole([heroId], mail);
    }

    var col = getDBCollection();

    for (let i = 0; i < rankTypeArr.length; ++i)
    {
        let rankType = rankTypeArr[i];
        let rankData = rankDataMap[rankType];

        let rankArr = rankData.data;
        let dbOpObj = {};
        for (let j = 0; j < rankArr.length; ++j)
        {
            let rankItem = rankArr[j];
            if (rankItem.like !== 0)
            {
                rankItem.like = 0;
                dbOpObj["data." + j + ".like"] = 0;
            }
        }

        //修改更新时间
        rankData.upTime = curTime;
        rankData.clearLikeTime = curTime;
        dbOpObj["upTime"] = curTime;
        dbOpObj["clearLikeTime"] = curTime;

        //存盘
        col.updateOneNoThrow({type:rankType}, {$set:dbOpObj});
    }

    logUtil.debug("完成收到点赞数排行给奖励");
}

function onTimerTask()
{
    //先统计点赞排行，不然后面清理数据了，就没法发奖励了
    checkLikeData();

    //看情况清排行
    for (var type in rankTypeInfos)
    {
        checkRankClearTime(type);
    }

    //统计公会相关的排行
    checkCorpsRank();
}

/**
 *
 * @param type
 */
function checkRankTime(type)
{
    var rankData = rankDataMap[type];
    if (!rankData)
        return;

    var info = rankTypeInfos[type];
    if (!info)
        return;

    var curTime = dateUtil.getTimestamp();

    if (info.hasLike && !dateUtil.isSameDay(curTime, rankData.clearLikeTime))
        checkLikeData();

    if ((info.clearType === clearTypes.CLEAR_TYPE_DAY && !dateUtil.isSameDay(curTime, rankData.clearTime)) || (info.clearType === clearTypes.CLEAR_TYPE_WEEK && !dateUtil.isSameWeek(curTime, rankData.clearTime)))
        checkRankClearTime(type);
}

function refreshRankData(srcObj, key, type)
{
    var curTime = dateUtil.getTimestamp();
    var col = getDBCollection();
    var info = rankTypeInfos[type];
    var rankData = rankDataMap[type];
    var rankValMap = keyRankValMap[type];
    var rankVal = rankValMap[key];
    var rankArr = rankData.data;
    var rankItem;
    if (rankVal != undefined)
    {
        rankItem = rankArr[rankVal];
        if (rankItem)
        {
            var ret = info.refreshFields(rankItem, srcObj);
            switch (ret)
            {
                //不用重排？
                case 1:
                    rankData.upTime = curTime;
                    col.updateOneNoThrow({type:type}, {$set:{upTime:curTime,["data." + rankVal]:rankItem}});
                    break;
                case 2:
                    //找到新的插入点
                    let newPos = fastFindInsertPos(rankArr, rankItem, info.cmpFunc, rankVal, true);
                    //移动数据到新位置
                    moveRankItem(rankArr, rankItem, rankVal, newPos);
                    //修改更新时间
                    rankData.upTime = curTime;
                    //排名没变？直接更新数据库数据就行了
                    if (newPos === rankVal)
                    {
                        col.updateOneNoThrow({type:type}, {$set:{upTime:curTime,["data." + rankVal]:rankItem}});
                    }
                    else
                    {
                        //生成排序映射
                        fastGenerateKeyRankValMap(type, rankVal, newPos);
                        //存盘
                        col.updateOneNoThrow({type:type}, {$set:{upTime:curTime},$pull:{data:{key:key}}});
                        col.updateOneNoThrow({type:type}, {$set:{upTime:curTime},$push:{data:{$each:[rankItem],$slice:info.rankCount,$position:newPos}}});
                    }
                    break;
            }
        }
    }
    else
    {
        //生成排序数据项
        rankItem = info.defValFunc(srcObj);

        //不在排行榜里？看看够不够资格上榜
        if (!info.canAddToRank(rankItem))
            return;

        //找到新的插入点
        let newPos = fastFindInsertPos(rankArr, rankItem, info.cmpFunc, rankArr.length, true);
        //排不进前N？不用插入数据了
        if (newPos >= info.rankCount)
            return;
        //插入到新位置
        rankArr.splice(newPos, 0, rankItem);
        //插入后，如果超出限制了，删除过量的数据
        if (rankArr.length > info.rankCount)
            rankArr.splice(info.rankCount);
        //修改更新时间
        rankData.upTime = curTime;
        //生成排序映射
        fastGenerateKeyRankValMap(type, rankArr.length - 1, newPos);
        //存盘
        col.updateOneNoThrow({type:type}, {$set:{upTime:curTime},$push:{data:{$each:[rankItem],$slice:info.rankCount,$position:newPos}}});
    }
}

/**
 *
 * @param {Role} role
 * @param {string} rankType
 */
function refreshRankDataForRole(role, rankType)
{
    //注意，这里没有判断role是否机器人、是否可以加入排行，用这个函数的地方要先判断
    var info = rankTypeInfos[rankType];
    var key = getKeyFromSrcObj(info.rankDataType, role);
    refreshRankData(role, key, rankType);
}

/**
 *
 * @param {CorpsInfo} corpsInfo
 * @param {string} rankType
 */
function refreshRankDataForCorps(corpsInfo, rankType)
{
    var key = corpsInfo.props.corpsId;
    refreshRankData(corpsInfo, key, rankType);
}

/**
 *
 * @param {string} eventName
 * @param {*} context
 * @param {Role} notifier
 */
function onRoleLogin(eventName, context, notifier)
{
    var role = notifier;
    var petsPart = role.getPetsPart();

    logUtil.debug("角色登录：" + role.getString(enProp.name));

    var heroRT = role.isRobot() ? robotHeroRankTypes : heroRankTypes;
    for (let i = 0; i < heroRT.length; ++i)
    {
        let rankType = heroRT[i];
        refreshRankDataForRole(role, rankType);
    }

    var petRT = role.isRobot() ? robotPetRankTypes : petRankTypes;
    for (let i = 0; i < petRT.length; ++i)
    {
        let rankType = petRT[i];
        for (let j = 0, len = petsPart.getPetCount(); j < len; ++j)
        {
            var pet = petsPart.getPetByIndex(j);
            refreshRankDataForRole(pet, rankType);
        }
    }
}

/**
 * 根据是否神侍、机器人，获取监听属性类型
 * @param role
 * @returns {Object.<string, string[]>}
 */
function getMonitorPropsMap(role)
{
    return role.isPet() ? (role.isRobot() ? robotMoniterPetPropsMap : moniterPetPropsMap) : (role.isRobot() ? robotMoniterHeroPropsMap : moniterHeroPropsMap);
}

/**
 *
 * @param {Role} role
 * @param {string} propName
 * @param {number|string} propVal
 */
function updateRankFuncByOneProp(role, propName, propVal)
{
    var propsMap = getMonitorPropsMap(role);
    var typeArr = propsMap[propName];
    if (!typeArr)
        return;
    for (var i = 0; i < typeArr.length; ++i)
    {
        var rankType = typeArr[i];
        refreshRankDataForRole(role, rankType);
    }
}

/**
 *
 * @param {Role} role
 * @param {object.<string, number|string>} propNames
 */
function updateRankFuncByProps(role, propNames)
{
    var propsMap = getMonitorPropsMap(role);
    var rankTypeSet = {};
    for (let i = 0, lenI = propNames.length; i < lenI; ++i)
    {
        var typeArr = propsMap[propNames[i]];
        if (!typeArr)
            continue;
        for (let j = 0, lenJ = typeArr.length; j < lenJ; ++j)
        {
            var rankType = typeArr[j];
            if (rankTypeSet[rankType])
                continue;
            rankTypeSet[rankType] = true;
            refreshRankDataForRole(role, rankType);
        }
    }
}

var doInitCoroutine = Promise.coroutine(function * () {
    logUtil.info("排行榜模块开始初始化...");

    //修正竞技场排行数量
    var arenaConfig = require("../gameConfig/arenaConfig");
    rankTypeInfos.arena.rankCount = arenaConfig.getArenaBasicCfg().maxRankNum;

    var curTime = dateUtil.getTimestamp();
    //把排行数据库一次读出来
    var col = getDBCollection();
    var arr = yield col.findArray({});
    for (var i = 0; i < arr.length; ++i)
    {
        /**
         * @type {RankDataType}
         */
        var item = arr[i];
        let type = item.type;
        let arrLen = item.data.length;

        logUtil.info("读取排行：" + type + "，行数：" + arrLen + "，更新时间：" + dateUtil.getStringFromTimestamp(item.upTime));

        if (!rankTypes[type])
            logUtil.warn("排行类型" + type + "不在预定义中");

        var info = rankTypeInfos[type];
        if (!info)
            logUtil.warn("排行类型" + type + "没有定义信息");
        else
        {
            //数据库的数据太多了？删除多余的
            if (arrLen > info.rankCount)
            {
                item.data = item.data.slice(0, info.rankCount);
                yield col.updateOne({type:type}, {$set:{upTime:curTime},$push:{data:{$each:[],$slice:info.rankCount}}});

                logUtil.warn("排行" + type + "的数据量比规定的多，从" + arrLen + "删成" + info.rankCount + "行");
            }
        }

        rankDataMap[type] = item;
        generateKeyRankValMap(type);
    }
    for(let type in rankTypes)
    {
        //不存在这个排行数据？填上空数据
        if (!rankDataMap[type])
        {
            var data = {type:type, upTime:curTime, clearTime:curTime, clearLikeTime:curTime, data:[], extra:{}, svrExtra:{}};
            rankDataMap[type] = data;
            generateKeyRankValMap(type);
            yield col.insertOne(data);
            logUtil.warn("排行" + type + "没有数据，填充空数据");
        }
    }

    //创建定时器
    timer = setInterval(onTimerTask, TIMER_TASK_INV);
    //马上调用一次
    onTimerTask.call(timer);

    //订阅角色登录事件
    eventMgr.addGlobalListener(onRoleLogin, eventNames.ROLE_LOGIN);

    //设置用于角色属性修改时更新排行榜的函数
    propPart.setUpdateRankFunc(updateRankFuncByOneProp, updateRankFuncByProps);

    logUtil.info("排行榜模块完成初始化");
});

function doInit()
{
    return doInitCoroutine();
}

var doDestroyCoroutine = Promise.coroutine(function * () {
    logUtil.info("排行榜模块开始销毁...");

    //设置用于角色属性修改时更新排行榜的函数
    propPart.setUpdateRankFunc(null, null);

    //取消订阅角色登录事件
    eventMgr.removeGlobalListener(onRoleLogin, eventNames.ROLE_LOGIN);

    //清数据
    rankDataMap = {};
    //删除定时器就行了
    if (timer)
    {
        clearInterval(timer);
        timer = null;
    }
    logUtil.info("排行榜模块完成销毁");
});

function doDestroy()
{
    return doDestroyCoroutine();
}

exports.getRankData = getRankData;
exports.getRankValueByRole = getRankValueByRole;
exports.getRankValueByKey = getRankValueByKey;
exports.getRankDataByRole = getRankDataByRole;
exports.getRankDataByKey = getRankDataByKey;
exports.addToRankByRole = addToRankByRole;
exports.addToAllRankByRole = addToAllRankByRole;
exports.addToRankByCorpsData = addToRankByCorpsData;
exports.addToAllRankByCorpsData = addToAllRankByCorpsData;
exports.addToRankByItem = addToRankByItem;
exports.removeFromRankByRole = removeFromRankByRole;
exports.removeFromAllRankByRole = removeFromAllRankByRole;
exports.removeFromRankByKey = removeFromRankByKey;
exports.removeFromAllRankByCorpsId = removeFromAllRankByCorpsId;
exports.doLikeRankItem = doLikeRankItem;
exports.checkRankTime = checkRankTime;
exports.fillRankItems = fillRankItems;
exports.doCustomDBUpdate = doCustomDBUpdate;
exports.doInit = doInit;
exports.doDestroy = doDestroy;