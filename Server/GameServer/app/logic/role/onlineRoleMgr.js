"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var logUtil = require("../../libs/logUtil");
var enProp = require("../enumType/propDefine").enProp;
var adminServerAgent = require("../http/adminServerAgent");

////////////模块内变量////////////
/**
 * 通用角色管理模块
 * @type {RoleMgr|null}
 */
var roleMgrModule = null;

/**
 * 以主角ID为key的表
 * @type {object.<number, Role>}
 */
var onlineRoleMap = {};

/**
 * 以名字为key的表
 * @type {object.<string, Role>}
 */
var onlineRoleObjs = {};

/**
 * 在线主角数组
 * @type {Role[]}
 */
var onlineRoleList = [];

/**
 * 通知在线定时间隔
 * @type {number}
 */
var NOTIFY_ONLINE_TIMER_INV = 2 * 60 * 1000;

/**
 * 通知在线定时器
 * @type {object}
 */
var timer = null;

////////////导出函数////////////
function addRoleByRoleMgr(heroId, role)
{
    onlineRoleMap[heroId] = role;
    onlineRoleObjs[role.getString(enProp.name)] = role;
  //  onlineRoleList.push(role);

    onlineRoleList.pushIfNotExist(role);

}

function removeRoleByRoleMgr(heroId, robotId)
{
    let role = findRoleByHeroId(heroId);
    delete onlineRoleObjs[role.getString(enProp.name)];
 /*   if (onlineRoleMap.delete(heroId))
    {
        if (onlineRoleMap.size <= 0)
            logUtil.info("在线主角数量为0");
    }*/
    delete onlineRoleMap[heroId];

    for(var i = 0,len = onlineRoleList.length; i < len; ++i)
    {
        if(onlineRoleList[i].getHeroId() == heroId)
        {
            onlineRoleList.splice(i, 1);
            break;
        }
    }
    if (onlineRoleList.length <= 0)
        logUtil.info("在线主角数量为0");

}

function removeRoleByHeroId(heroId)
{
    var role = onlineRoleMap[heroId];
    if (role)
    {
        var guid = role.getString(enProp.guid);
        roleMgrModule.removeRoleByGUID(guid);
    }
}

/**
 *
 * @param heroId
 * @returns {Role}
 */
function findRoleByHeroId(heroId)
{
    return onlineRoleMap[heroId];
}
/**
 * 根据名字查找role
 * @param {String} name
 * @returns {Role|null}
 */
function findRoleByHeroName(name)
{
    return onlineRoleObjs[name];
}

/**
 *
 * @param {number[]} heroIdList
 * @returns {Role[]}
 */
function getHeroByHeroIds(heroIdList)
{
    var tempHeroList = [];
    for (var i = 0; i < heroIdList.length; ++i)
    {
        var role = onlineRoleMap[heroIdList[i]];
        if (role)
            tempHeroList.push(role);
    }
    return tempHeroList;
}

/**
 * 获取在线的角色列表
 * @returns {Role[]}
 */
function getOnlineRoles()
{
    return onlineRoleList;
}


/*
/!**
 * 这里用了Map的遍历，性能不太好，千万要少用
 * @param {function(Role)} func - 要执行的函数
 * @param {boolean} excludeOffline - 离线的，不发送
 *!/
function forEachOfHero(func, excludeOffline)
{
    if (excludeOffline)
    {
         for (let role of onlineRoleMap.values())
         {
         try {
         if (role.getSession())
         func(role);
         }
         catch (err) {
         logUtil.error("forEachOfHero", err);
         }
         }

    }
    else
    {
         for (let role of onlineRoleMap.values())
         {
         try {
         func(role);
         }
         catch (err) {
         logUtil.error("forEachOfHero", err);
         }
         }

    }
}
*/

/**
 * 这里改成了遍历数组
 * @param {function(Role)} func - 要执行的函数
 * @param {boolean} excludeOffline - 离线的，不发送
 */
function forEachOfHero(func, excludeOffline)
{
    if (excludeOffline)
    {
        for(var i = 0,len = onlineRoleList.length; i < len; ++i)
        {
            var role = onlineRoleList[i];
            try {
                if (role.getSession())
                    func(role);
            }
            catch (err) {
                logUtil.error("forEachOfHero", err);
            }
        }
    }
    else
    {
        for(var i = 0,len = onlineRoleList.length; i < len; ++i)
        {
            var role = onlineRoleList[i];
            try {
                func(role);
            }
            catch (err) {
                logUtil.error("forEachOfHero", err);
            }
        }
    }
}

function onTimerNotifyTimer()
{
    adminServerAgent.logOnlineRoleNum(onlineRoleList.length);
}

var doInitCoroutine = Promise.coroutine(function * () {
    logUtil.info("在线角色管理模块开始初始化...");

    timer = setInterval(onTimerNotifyTimer, NOTIFY_ONLINE_TIMER_INV);

    logUtil.info("在线角色管理模块完成初始化");
});

function doInit(roleMgr)
{
    roleMgrModule = roleMgr;
    return doInitCoroutine();
}

var doDestroyCoroutine = Promise.coroutine(function * () {
    logUtil.info("在线角色管理模块开始销毁...");

    if (timer)
    {
        clearInterval(timer);
        timer = null;
    }

    /*for (let heroId of onlineRoleMap.keys())
    {
        try {
            removeRoleByHeroId(heroId);
        }
        catch (err) {
            logUtil.error("doDestroyCoroutine", err);
        }
    }*/
    for(var heroId in onlineRoleMap)
    {
        try {
            removeRoleByHeroId(heroId);
        }
        catch (err) {
            logUtil.error("doDestroyCoroutine", err);
        }
    }

    onlineRoleObjs = {};
    onlineRoleMap = {};
    onlineRoleList = [];
    roleMgrModule = null;

    logUtil.info("在线角色管理模块完成销毁");
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
exports.findRoleByHeroName = findRoleByHeroName;
exports.getOnlineRoles = getOnlineRoles;
exports.getHeroByHeroIds = getHeroByHeroIds;
exports.forEachOfHero = forEachOfHero;

exports.doInit = doInit;
exports.doDestroy = doDestroy;