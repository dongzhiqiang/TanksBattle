"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var appCfg = require("../../../config");
var appUtil = require("../../libs/appUtil");
var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var robotConfig = require("../gameConfig/robotConfig");
var roleConfig = require("../gameConfig/roleConfig");
var enProp = require("../enumType/propDefine").enProp;

var guidGenerator = require("../../libs/guidGenerator");
var equipUtil = require("../equip/equipUtil");
var rankMgr = require("../rank/rankMgr");

////////////模块内变量////////////
/**
 * 通用角色管理模块
 * @type {RoleMgr|null}
 */
var roleMgrModule = null;

/**
 * 机器人对象
 * {主角ID:角色对象}
 * @type {Map.<number, Role>}
 */
var robotRolesMap = new Map();
/**
 * 以名字为key的表
 * @type {object.<string, Role>}
 */
var robotRoleNameObjs = {};

/**
 * 机器人ID对应的英雄ID列表
 * @type {Map.<number,number[]>}
 */
var robotIdHeroIdsMap = new Map();

/**
 * 最大机器人主角ID
 * @type {number}
 */
var maxRobotHeroId = 0;

/**
 * 准备写入数据库的新机器数据数组，因为一次只写入一个，效率不行的
 * @type {object[]}
 */
var tempNewRobotData = [];

////////////内部函数////////////
var readRobotData = Promise.coroutine(function * ()
{
    logUtil.info("开始读取机器人数据...");
    var db = dbUtil.getDB(0);
    var colRobot = db.collection("robot");
    var robotDataArr = yield colRobot.findArray({}, {_id:0});
    robotRolesMap.clear();
    robotRoleNameObjs = {};
    for (var i = 0; i < robotDataArr.length; ++i)
    {
        let robotData = robotDataArr[i];
        let heroId = robotData.props.heroId;
        let heroIdAbs = Math.abs(heroId);

        if (heroIdAbs > maxRobotHeroId)
            maxRobotHeroId = heroIdAbs;

        let robotRole = roleMgrModule.createRole(robotData, true);
        //加入排行
        addRobotToAllRank(robotRole);
    }
    if (maxRobotHeroId === 0)
        maxRobotHeroId = 100000 * appCfg.serverId; //为了保证不同服的机器的heroId不一样
    logUtil.info("结束读取机器人数据");
});

var saveRobotDataList  = Promise.coroutine(function * (robotDataList)
{
    if (!robotDataList || robotDataList.length <= 0)
        return;

    logUtil.info("开始保存新机器人数据...");
    var db = dbUtil.getDB(0);
    var colRobot = db.collection("robot");
    yield colRobot.insertMany(robotDataList);
    logUtil.info("结束保存新机器人数据");
});

/**
 *
 * @param robotId
 * @returns {number[]}
 */
function getRobotHeroIdList(robotId)
{
    var heroIdList = robotIdHeroIdsMap.get(robotId);
    if (!heroIdList)
    {
        heroIdList = [];
        robotIdHeroIdsMap.set(robotId, heroIdList);
    }
    return heroIdList;
}

/**
 *
 * @param {number} robotId
 * @returns {object}
 */
function generateOneRobotData(robotId)
{
    var heroGuid = guidGenerator.generateGUID();
    var heroId = -(++maxRobotHeroId);   //机器人角色ID为负
    var robotCfg = robotConfig.getRobotConfig(robotId);
    var roleIds = robotCfg.roleIds;
    var roleId = roleIds[appUtil.getRandom(0, roleIds.length - 1)];
    var roleCfg = roleConfig.getRoleConfig(roleId);
    var roleName = roleCfg.name + (-heroId);
    var heroLvRange = robotCfg.heroLvRange;
    var equipLvRange = robotCfg.equipLvRange;
    var equipAdvLvRange = robotCfg.equipAdvLvRange;
    var equipStarRange = robotCfg.equipStarRange;
    var skillLvRange = robotCfg.skillLvRange;

    var roleData = {};

    //基本属性
    var roleLevel = appUtil.getRandom(heroLvRange[0], heroLvRange[1]);
    roleData.props = {
        heroId: heroId,
        guid:   heroGuid,
        roleId: roleId,
        name:   roleName,
        level:  roleLevel,
        robotId: robotId,
    };

    //装备
    var heroEquips = roleCfg.initEquips ? equipUtil.getInitEquips(roleId) : [];
    for (let i = 0; i < heroEquips.length; ++i)
    {
        let equipLv = appUtil.getRandom(equipLvRange[0], equipLvRange[1]);
        let equipAdvLv = appUtil.getRandom(equipAdvLvRange[0], equipAdvLvRange[1]);
        let equipStar = appUtil.getRandom(equipStarRange[0], equipStarRange[1]);
        let equip = heroEquips[i];
        if (!equip)
            continue;

        equip.level = equipLv;
        equip.advLv = equipAdvLv;
        let equipIdBak = equip.equipId;
        equip.equipId = equipUtil.getEquipIdByEquipIdAndStar(equip.equipId, equipStar);
        if (!equip.equipId)
            logUtil.warn("generateOneRobotData，找不到星级对应的装备ID，装备ID：" + equipIdBak + "，星级" + equipStar);
    }
    roleData.equips = heroEquips;

    //武器、武器技能、武器铭文、武器属性
    //var heroSkillLv = appUtil.getRandom(skillLvRange[0], skillLvRange[1]);
    roleData.weapons ={curWeapon:0};

    //活动属性
    roleData.actProps = {};

    return roleData;
}

/**
 *
 * @param {Role} role
 */
function addRobotToAllRank(role)
{
    //主角加入
    rankMgr.addToAllRankByRole(role);
}

////////////导出函数////////////
/**
 *
 * @param {number} heroId
 * @param {Role} role
 */
function addRoleByRoleMgr(heroId, role)
{
    robotRolesMap.set(heroId, role);
    robotRoleNameObjs[role.getString(enProp.name)] = role;

    var robotId = role.getNumber(enProp.robotId);
    var heroIdList = getRobotHeroIdList(robotId);
    heroIdList.push(heroId);
}

function removeRoleByRoleMgr(heroId, robotId)
{
    var role = findRoleByHeroId(heroId);
    delete robotRoleNameObjs[role.getString(enProp.name)];
    if (robotRolesMap.delete(heroId))
    {
        var heroIdList = getRobotHeroIdList(robotId);
        heroIdList.removeValue(heroId);

        if (robotRolesMap.size <= 0)
            logUtil.info("机器人主角数量为0");
    }
}

function removeRoleByHeroId(heroId)
{
    var role = robotRolesMap.get(heroId);
    if (role)
    {
        var guid = role.getString(enProp.guid);
        roleMgrModule.removeRoleByGUID(guid, true);
    }
}

/**
 *
 * @param heroId
 * @returns {Role}
 */
function findRoleByHeroId(heroId)
{
    return robotRolesMap.get(heroId);
}
/**
 * 根据名字找到role
 * @param name
 * @returns {Role}
 */
function findRoleByName(name)
{
    return robotRoleNameObjs[name];
}

/**
 *
 * @param robotId
 * @param offset
 * @param {boolean?} noTryAddToRank
 * @returns {Role}
 */
function getRobotRoleOrAddNew(robotId, offset, noTryAddToRank)
{
    /**
     * @type {Role}
     */
    var robotRole;
    var heroIdList = getRobotHeroIdList(robotId);
    if (offset < heroIdList.length) {
        let heroId = heroIdList[offset];
        robotRole = findRoleByHeroId(heroId);
    }
    else if (offset === heroIdList.length) {
        let robotData = generateOneRobotData(robotId);
        robotRole = roleMgrModule.createRole(robotData, true);
        tempNewRobotData.push(robotData);
        //加入排行
        if (!noTryAddToRank)
            addRobotToAllRank(robotRole);
    }
    else {
        throw new Error("getRobotRoleOrAddNew 下标只能 <= heroIdList.length");
    }
    return robotRole;
}

function getRoleNumByRobotId(robotId)
{
    return getRobotHeroIdList(robotId).length;
}

function checkTempNewRobotData()
{
    var temp = tempNewRobotData;
    tempNewRobotData = [];
    return saveRobotDataList(temp);
}

var doInitCoroutine = Promise.coroutine(function * () {
    logUtil.info("机器人角色管理模块开始初始化...");

    //读取机器人数据
    yield readRobotData();

    logUtil.info("机器人角色管理模块完成初始化");
});

function doInit(roleMgr)
{
    roleMgrModule = roleMgr;
    return doInitCoroutine();
}

var doDestroyCoroutine = Promise.coroutine(function * () {
    logUtil.info("机器人角色管理模块开始销毁...");

    for (let heroId of robotRolesMap.keys())
    {
        try {
            removeRoleByHeroId(heroId);
        }
        catch (err) {
            logUtil.error("doDestroyCoroutine", err);
        }
    }

    robotRolesMap.clear();
    robotIdHeroIdsMap.clear();
    maxRobotHeroId = 0;
    roleMgrModule = null;
    robotRoleNameObjs = {};

    logUtil.info("机器人角色管理模块完成销毁");
});

function doDestroy()
{
    return doDestroyCoroutine();
}

////////////导出元素////////////
exports.addRoleByRoleMgr = addRoleByRoleMgr;
exports.removeRoleByRoleMgr = removeRoleByRoleMgr;

exports.removeRoleByHeroId = removeRoleByHeroId;
exports.findRoleByHeroId = findRoleByHeroId;
exports.findRoleByName = findRoleByName;

exports.getRobotRoleOrAddNew = getRobotRoleOrAddNew;
exports.getHeroNumByRobotId = getRoleNumByRobotId;
exports.checkTempNewRobotData = checkTempNewRobotData;
exports.addRobotToAllRank = addRobotToAllRank;

exports.doInit = doInit;
exports.doDestroy = doDestroy;