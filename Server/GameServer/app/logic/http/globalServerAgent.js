"use strict";

var Promise = require("bluebird");

var appCfg = require("../../../config");
var logUtil = require("../../libs/logUtil");
var httpUtil = require("../../libs/httpUtil");
var sessionMgr = require("../session/sessionMgr");

var NOTIFY_ALIVE_TIMER_INV = 30 * 1000;
var timer = null;

function onTimerNotifyLive()
{
    var msgObj = {
        area: appCfg.areaName,
        name: appCfg.serverName,
        index: appCfg.showIndex,
        host: appCfg.publicHostName,
        port: appCfg.port,
        lanHost: appCfg.lanHostName,
        lanPort: appCfg.httpPort,
        serverId: appCfg.serverId,
        onlineNum: sessionMgr.getSessionCount(),
        showState: appCfg.showState
    };

    httpUtil.doPost(appCfg.globalServerUrl + "notifySvrAlive?key=" + encodeURIComponent(appCfg.adminKey), msgObj, function(err, data){
        if (err)
        {
            logUtil.error("onTimerNotifyLive", err);
            return;
        }

        if (!data)
        {
            logUtil.error("onTimerNotifyLive，结果为" + data);
        }

        if (data.code !== 0)
        {
            logUtil.error("onTimerNotifyLive，错误码：" + data.code + "，错误消息：" + data.msg);
            return;
        }

        //logUtil.debug("通知全局服存活成功");
    });
}

var notifySvrDownCoroutine = Promise.coroutine(function * () {
    try {
        yield httpUtil.doPostCoroutine(appCfg.globalServerUrl + "notifySvrDown?key=" + encodeURIComponent(appCfg.adminKey), {serverId: appCfg.serverId}, "json");
    }
    catch (err) {
        logUtil.error("通知全局服关服成功", err);
    }
});

/**
 *
 * @param command - 命令，其实就是域名后面那个单词
 * @param body - 提交的内容
 * @param {HttpCallbackWithOkMsgCxt?} callback - 如果填了就用回调形式，否则用协程
 */
function generalPost(command, body, callback)
{
    var url = appCfg.globalServerUrl + command + "?key=" + encodeURIComponent(appCfg.adminKey);
    if (callback)
    {
        httpUtil.doPost(url, body, function (err, data){
            if (err)
            {
                logUtil.error(command, err);
                callback(false, "服务器错误");
                return;
            }

            if (!data)
            {
                logUtil.error(command + "，结果为" + data);
                callback(false, "服务器错误");
                return;
            }

            callback(data.code === 0, data.msg, data.cxt);
        });
    }
    else
    {
        return new Promise(function (resolve, reject){
            httpUtil.doPost(url, body, function (err, data){
                if (err)
                {
                    logUtil.error(command, err);
                    resolve({ok:false, msg:"服务器错误"});
                    return;
                }

                if (!data)
                {
                    logUtil.error(command + "，结果为" + data);
                    resolve({ok:false, msg:"服务器错误"});
                    return;
                }

                resolve({ok:data.code === 0, msg:data.msg, cxt:data.cxt});
            });
        });
    }
}

/**
 *
 * @param channelId
 * @param userId
 * @param token
 * @param {HttpCallbackWithOkMsgCxt?} callback - 如果填了就用回调形式，否则用协程
 */
function checkLogin(channelId, userId, token, callback)
{
    var msgObj = {
        channelId : channelId,
        userId : userId,
        token : token,
        serverId : appCfg.serverId
    };
    return generalPost("checkLogin", msgObj, callback);
}

/**
 *
 * @param channelId
 * @param userId
 * @param guid
 * @param name
 * @param level
 * @param roleId
 * @param heroId
 * @param serverId
 * @param {HttpCallbackWithOkMsgCxt} callback
 */
function updateRoleInfo(channelId, userId, guid, name, level, roleId, heroId, serverId, callback)
{
    var msgObj = {
        channelId : channelId,
        userId : userId,
        guid : guid,
        name:name,
        level:level,
        roleId:roleId,
        heroId:heroId,
        serverId:serverId
    };
    return generalPost("updateRole", msgObj, callback);
}

function requestNewRole(channelId, userId, guid, name, roleId, serverId, level, callback)
{
    var msgObj = {
        channelId : channelId,
        userId : userId,
        guid : guid,
        name : name,
        roleId : roleId,
        serverId : serverId,
        level : level
    };
    return generalPost("requestNewRole", msgObj, callback);
}

function requestDelRole(heroId, callback)
{
    var msgObj = {
        heroId : heroId
    };
    return generalPost("requestDelRole", msgObj, callback);
}

function requestNewCorps(name, serverId, level, callback)
{
    var msgObj = {
        name : name,
        serverId : serverId,
        level : level
    };
    return generalPost("requestNewCorps", msgObj, callback);
}

function requestDelCorps(corpsId, callback)
{
    var msgObj = {
        corpsId : corpsId
    };
    return generalPost("requestDelCorps", msgObj, callback);
}

var doInitCoroutine = Promise.coroutine(function * () {
    logUtil.info("全局服代理模块开始初始化...");
    timer = setInterval(onTimerNotifyLive, NOTIFY_ALIVE_TIMER_INV);
    //马上调用一次
    onTimerNotifyLive.call(timer);
    logUtil.info("全局服代理模块完成初始化");
});

function doInit()
{
    return doInitCoroutine();
}

var doDestroyCoroutine = Promise.coroutine(function * () {
    logUtil.info("全局服代理模块开始销毁...");
    try {
        //删除定时器就行了
        if (timer)
        {
            clearInterval(timer);
            timer = null;
        }
        yield notifySvrDownCoroutine();
        logUtil.info("全局服代理模块完成销毁");
    }
    catch (err) {
        logUtil.warn("全局服代理模块销毁出错", err);
    }
});

function doDestroy()
{
    return doDestroyCoroutine();
}

////////////导出元素////////////
exports.checkLogin = checkLogin;
exports.updateRoleInfo = updateRoleInfo;
exports.requestNewRole = requestNewRole;
exports.requestDelRole = requestDelRole;
exports.requestNewCorps = requestNewCorps;
exports.requestDelCorps = requestDelCorps;
exports.doInit = doInit;
exports.doDestroy = doDestroy;