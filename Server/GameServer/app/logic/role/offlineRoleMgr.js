"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var logUtil = require("../../libs/logUtil");
var dateUtil = require("../../libs/dateUtil");
var dbUtil = require("../../libs/dbUtil");
var enProp = require("../enumType/propDefine").enProp;

////////////模块内变量////////////
/**
 * 通用角色管理模块
 * @type {RoleMgr|null}
 */
var roleMgrModule = null;

/**
 *
 * @type {Map.<number, Role>}
 */
var offlineRoleMap = new Map();
/**
 * 以名字为key的表
 * @type {object.<string, Role>}
 */
var offlineRoleObjs = {};

////////////导出函数////////////
function addRoleByRoleMgr(heroId, role)
{
    offlineRoleMap.set(heroId, role);
    offlineRoleObjs[role.name] = role;
}

function removeRoleByRoleMgr(heroId, robotId)
{
    let role = findRoleByHeroId(heroId);
    delete offlineRoleObjs[role.name];
    if (offlineRoleMap.delete(heroId))
    {
        if (offlineRoleMap.size <= 0)
            logUtil.info("离线主角数量为0");
    }

}

function removeRoleByHeroId(heroId)
{
    var role = offlineRoleMap.get(heroId);
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
    return offlineRoleMap.get(heroId);
}
/**
 * 根据名字查找role
 * @param {String} name
 * @returns {Role|null}
 */
function findRoleByHeroName(name)
{
    return offlineRoleObjs[name];
}

var findRoleOrLoadOfflineByHeroIdCoroutine = Promise.coroutine(function * (heroId) {
    //注意，这里没用userId来选连接
    var db = dbUtil.getDB();
    var col = db.collection("role");
    var roleData = yield col.findOne({"props.heroId":heroId});
    if (!roleData || !roleData.props)
        return null;
    //打上离线标记，值就是开始成为离线角色的时间
    //因为createRole里要用到offline属性，所以这里要先添加
    roleData.props.offline = dateUtil.getTimestamp();
    /** @type {Role} */
    var role = roleMgrModule.createRole(roleData);
    //如果创建成功了，那就给它开启超时删除机制，暂时就用setSession
    if (role)
    {
        role.startTimerDestroy();
        //前面设置了属性，这里不用设置了
        //role.setOfflineTime(true);
    }
    return role;
});

function findRoleOrLoadOfflineByHeroId(heroId)
{
    return findRoleOrLoadOfflineByHeroIdCoroutine(heroId);
}

var doInitCoroutine = Promise.coroutine(function * () {
    logUtil.info("离线角色管理模块开始初始化...");
    //没啥事做
    logUtil.info("离线角色管理模块完成初始化");
});

function doInit(roleMgr)
{
    roleMgrModule = roleMgr;
    return doInitCoroutine();
}

var doDestroyCoroutine = Promise.coroutine(function * () {
    logUtil.info("离线角色管理模块开始销毁...");
    for (let heroId of offlineRoleMap.keys())
    {
        try {
            removeRoleByHeroId(heroId);
        }
        catch (err) {
            logUtil.error("doDestroyCoroutine", err);
        }
    }

    offlineRoleObjs = {};
    offlineRoleMap.clear();
    roleMgrModule = null;

    logUtil.info("离线角色管理模块完成销毁");
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
exports.findRoleOrLoadOfflineByHeroId = findRoleOrLoadOfflineByHeroId;
exports.findRoleByHeroName = findRoleByHeroName;
exports.doInit = doInit;
exports.doDestroy = doDestroy;