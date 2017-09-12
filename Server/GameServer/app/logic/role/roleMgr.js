"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var appCfg = require("../../../config");
var logUtil = require("../../libs/logUtil");
var enProp = require("../enumType/propDefine").enProp;
var globalDefine = require("../enumType/globalDefine").globalDefine;
var roleModule = require("./role");
var onlineRoleMgr = require("./onlineRoleMgr");
var offlineRoleMgr = require("./offlineRoleMgr");
var robotRoleMgr = require("./robotRoleMgr");

////////////模块内变量////////////
/**
 *
 * @type {Map.<string, Role>}
 */
var guidMap = new Map();

////////////导出函数////////////
/**
 * 其它模块不要直接调用这个带创建机器人角色
 * @param {object} data
 * @param {boolean?} [noShowDebug=false]
 * @return {Role}
 */
function createRole(data, noShowDebug)
{
    //必须有最基本数据
    if (!isRoleData(data))
    {
        logUtil.error("不完整的角色基本数据");
        return null;
    }

    var guid = data.props.guid;

    //尝试删除旧的
    removeRoleByGUID(guid);

    /**
     * @type {Role}
     */
    var role;
    try {
        if (noShowDebug) {
            //禁掉多余的日志
            var oldDebugFlag = appCfg.debug;
            appCfg.debug = false;
        }

        role = new roleModule.Role(data);

        var heroId = role.getNumber(enProp.heroId);
        logUtil.debug("创建Role对象，guid：" + guid + "，heroId：" + heroId);
    }
    catch (err) {
        logUtil.error("创建角色失败", err);
        return null;
    }
    finally {
        if (noShowDebug) {
            //恢复原来的标记
            appCfg.debug = oldDebugFlag;
        }
    }

    guidMap.set(guid, role);

    //如果是主角，还要后续操作
    if (heroId)
    {
        if (heroId > 0)
        {
            if (role.getNumber(enProp.offline) > 0)
            {
                offlineRoleMgr.addRoleByRoleMgr(heroId, role);
            }
            else
            {
                onlineRoleMgr.addRoleByRoleMgr(heroId, role);
            }
        }
        else
        {
            robotRoleMgr.addRoleByRoleMgr(heroId, role);
        }
    }

    return role;
}

/**
 *
 * @param {string} guid
 * @param {boolean?} [noShowDebug=false]
 * @return {Role}
 */
function removeRoleByGUID(guid, noShowDebug)
{
    var role = guidMap.get(guid);
    if (role)
    {
        var heroId = role.getNumber(enProp.heroId);
        var robotId = role.getNumber(enProp.robotId);
        var offline = role.getNumber(enProp.offline);

        try {
            if (noShowDebug) {
                //禁掉多余的日志
                var oldDebugFlag = appCfg.debug;
                appCfg.debug = false;
            }

            role.destroy();

            logUtil.debug("销毁Role对象，guid：" + guid + "，heroId：" + heroId);
        }
        catch (err) {
            logUtil.error("角色销毁出错", err);
        }
        finally {
            if (noShowDebug) {
                //恢复原来的标记
                appCfg.debug = oldDebugFlag;
            }
        }

        guidMap.delete(guid);
        if (guidMap.size <= 0)
        {
            logUtil.info("角色数量为0");
        }

        //如果是主角，还要后续操作
        if (heroId)
        {
            if (heroId > 0)
            {
                //offline值大于0就表示真实离线
                if (offline > 0)
                {
                    offlineRoleMgr.removeRoleByRoleMgr(heroId, robotId);
                }
                else
                {
                    onlineRoleMgr.removeRoleByRoleMgr(heroId, robotId);
                }
            }
            else
            {
                robotRoleMgr.removeRoleByRoleMgr(heroId, robotId);
            }
        }
    }
}

/**
 *
 * @param guid
 * @returns {Role}
 */
function findRoleByGUID(guid)
{
    return guidMap.get(guid);
}

function removeRoleByHeroId(heroId)
{
    if (heroId < 0) {
        let role = robotRoleMgr.findRoleByHeroId(heroId);
        if (role) {
            robotRoleMgr.removeRoleByHeroId(heroId);
        }
    }
    else {
        let role = onlineRoleMgr.findRoleByHeroId(heroId);
        if (role) {
            onlineRoleMgr.removeRoleByHeroId(heroId);
        }
        else {
            role = offlineRoleMgr.findRoleByHeroId(heroId);
            if (role) {
                offlineRoleMgr.removeRoleByHeroId(heroId);
            }
        }
    }
}

function findRoleByHeroId(heroId)
{
    if (heroId < 0) {
        role = robotRoleMgr.findRoleByHeroId(heroId);
        if (role)
            return role;
    }
    else {
        var role = onlineRoleMgr.findRoleByHeroId(heroId);
        if (role)
            return role;

        role = offlineRoleMgr.findRoleByHeroId(heroId);
        if (role)
            return role;
    }

    return null;
}
/**
 * 根据roleName在onlineRoleMgr和offlineRoleMgr查找role
 * @param {String} heroName
 * @returns {Role|null}
 */
function findRoleByHeroName(heroName)
{
    if(!heroName || heroName == "")
        return null;
    var role = onlineRoleMgr.findRoleByHeroName(heroName);
    if (role)
        return role;

    role = offlineRoleMgr.findRoleByHeroName(heroName);
    if (role)
        return role;
}

var findRoleOrLoadOfflineByHeroIdCoroutine = Promise.coroutine(function * (heroId) {
    if (heroId < 0) {
        role = robotRoleMgr.findRoleByHeroId(heroId);
        if (role)
            return role;
    }
    else {
        var role = onlineRoleMgr.findRoleByHeroId(heroId);
        if (role)
            return role;

        role = yield offlineRoleMgr.findRoleOrLoadOfflineByHeroId(heroId);
        if (role)
            return role;
    }

    return null;
});
function findRoleOrLoadOfflineByHeroId(heroId)
{
    return findRoleOrLoadOfflineByHeroIdCoroutine(heroId);
}

/**
 * 是否有效的角色数据
 * 就是是否包含最基本的角色属性
 * @param data
 * @returns {boolean}
 */
function isRoleData(data)
{
    if (!data || !data.props)
        return false;
    var props = data.props;
    return !!(props.guid && props.name && props.roleId && props.level >= globalDefine.MIN_ROLE_LEVEL && props.level <= globalDefine.MAX_ROLE_LEVEL);
}

var doInitCoroutine = Promise.coroutine(function * () {
    logUtil.info("角色管理模块开始初始化...");

    yield robotRoleMgr.doInit(exports);
    yield onlineRoleMgr.doInit(exports);
    yield offlineRoleMgr.doInit(exports);

    logUtil.info("角色管理模块完成初始化");
});

function doInit()
{
    return doInitCoroutine();
}

var doDestroyCoroutine = Promise.coroutine(function * () {
    logUtil.info("角色管理模块开始销毁...");

    yield offlineRoleMgr.doDestroy();
    yield onlineRoleMgr.doDestroy();
    yield robotRoleMgr.doDestroy();

    guidMap.clear();

    logUtil.info("角色管理模块完成销毁");
});

function doDestroy()
{
    return doDestroyCoroutine();
}

////////////导出元素////////////
exports.createRole = createRole;
exports.removeRoleByGUID = removeRoleByGUID;
exports.findRoleByGUID = findRoleByGUID;
exports.removeRoleByHeroId = removeRoleByHeroId;
exports.findRoleByHeroId = findRoleByHeroId;
exports.findRoleByHeroName = findRoleByHeroName;
exports.findRoleOrLoadOfflineByHeroId = findRoleOrLoadOfflineByHeroId;
exports.isRoleData = isRoleData;
exports.doInit = doInit;
exports.doDestroy = doDestroy;

/**
 * 本模块的定义
 * @typedef {Object} RoleMgr
 * @property {function(object, boolean?)} createRole
 * @property {function(string, boolean?)} removeRoleByGUID
 * @property {function(string)} findRoleByGUID
 * @property {function(number)} removeRoleByHeroId
 * @property {function(number)} findRoleByHeroId
 * @property {function(string)} findRoleByHeroName
 * @property {function(number)} findRoleOrLoadOfflineByHeroId
 * @property {function(object)} isRoleData
 */