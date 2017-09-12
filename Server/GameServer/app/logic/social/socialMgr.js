"use strict";

var Promise = require("bluebird");
var roleMgr = require("../role/roleMgr");
var eventMgr = require("../../libs/eventMgr");
var dateUtil = require("../../libs/dateUtil");
var eventNames = require("../enumType/eventDefine").eventNames;

/**
 * 角色登录监听
 * @param {string} eventName
 * @param {*} context
 * @param {Role} notifier
 */
function onRoleLogin(eventName, context, notifier)
{
    var part = notifier.getSocialPart();
    part.onPropChange({"lastLogout":0});
}
/**
 * 角色断开连接监听
 * @param {string} eventName
 * @param {*} context
 * @param {Role} notifier
 */
function onRoleLostConn(eventName, context, notifier)
{
    var part = notifier.getSocialPart();
    part.onPropChange({"lastLogout":dateUtil.getTimestamp()});
}
/**
 * 角色重连监听
 * @param {string} eventName
 * @param {*} context
 * @param {Role} notifier
 */
function onRoleRelogin(eventName, context, notifier)
{
    var part = notifier.getSocialPart();
    part.onPropChange({"lastLogout":0});
}
/**
 * 角色登出监听
 * @param {string} eventName
 * @param {*} context
 * @param {Role} notifier
 */
function onRoleLogout(eventName, context, notifier)
{
    var part = notifier.getSocialPart();
    part.onPropChange({"lastLogout":dateUtil.getTimestamp()});
}

var doInitCoroutine = Promise.coroutine(function * () {

    //订阅角色登录事件
    eventMgr.addGlobalListener(onRoleLogin, eventNames.ROLE_LOGIN);
    eventMgr.addGlobalListener(onRoleLogout, eventNames.ROLE_LOGOUT);
    eventMgr.addGlobalListener(onRoleLostConn, eventNames.ROLE_LOSTCONN);
    eventMgr.addGlobalListener(onRoleRelogin, eventNames.ROLE_RELOGIN);

});

var doDestroyCoroutine = Promise.coroutine(function * () {

    //取消订阅角色登录事件
    eventMgr.removeGlobalListener(onRoleLogin, eventNames.ROLE_LOGIN);
    eventMgr.removeGlobalListener(onRoleLogout, eventNames.ROLE_LOGOUT);
    eventMgr.removeGlobalListener(onRoleLostConn, eventNames.ROLE_LOSTCONN);
    eventMgr.removeGlobalListener(onRoleRelogin, eventNames.ROLE_RELOGIN);
});

function doInit()
{
    return doInitCoroutine();
}

function doDestroy()
{
    return doDestroyCoroutine();
}


exports.doInit = doInit;
exports.doDestroy = doDestroy;