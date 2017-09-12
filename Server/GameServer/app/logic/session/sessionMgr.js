"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var dateUtil = require("../../libs/dateUtil");
var logUtil = require("../../libs/logUtil");
var NetMsgCodec = require("../../libs/netMsgCodec").NetMsgCodec;
var Message = require("../../libs/message").Message;
var clientSession = require("./clientSession");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var accountMsg = require("../netMessage/accountMessage");
var CmdIdsAccount = require("../netMessage/accountMessage").CmdIdsAccount;

////////////模块内常量////////////
const TIMER_INV_CHECK_SESSION   = 60000;        //单位毫秒，多久检查一次会话活跃
const NEED_SERVER_PING_TIME     = 5 * 60;     //单位秒，多少时间没有活跃，就主动Ping客户端
const MARK_ROLE_OFFLINE_TIME    = 10 * 60;      //单位秒，多少时间没有活跃，就强制关闭连接，但Role还保留一段时间

////////////模块内变量////////////
/**
 *
 * 会话ID种子
 * @type {number}
 */
var clientSessionID = 0;
/**
 * 会话与会话对象的映射，这里用了ES6的Map，数据量多时性能更好一点，而且有size，不用单独维护size
 * @type {Map.<number, ClientSession>}
 */
var clientSessionMap = new Map();
/**
 * 账号与会话对象的映射
 * 结构：channelId userId session
 * @type {Map.<string, Map.<string, ClientSession>>}
 */
var accountSessionMap = new Map();
/**
 * 定时检测会话的定时器
 * @type {object}
 */
var timerIdCheckSession = null;

////////////导出函数////////////
function newSession(conn)
{
    var session = new clientSession.ClientSession(++clientSessionID, conn);
    clientSessionMap.set(session.getSessionId(), session);
}

/**
 *
 * @param {number} sessionId
 */
function delSession(sessionId)
{
    var session = clientSessionMap.get(sessionId);
    if (session)
    {
        var accountInfo = session.getAccountInfo();
        if (accountInfo)
        {
            var userIdSessionMap = accountSessionMap.get(accountInfo.channelId);
            if (userIdSessionMap)
                userIdSessionMap.delete(accountInfo.userId);
        }
        clientSessionMap.delete(sessionId);

        try {
            session.close();
        }
        catch (err) {
            logUtil.error("会话关闭出错", err);
        }

        if (clientSessionMap.size <= 0)
        {
            logUtil.info("用户数为0");
        }
    }
}

function getSessionCount()
{
    return clientSessionMap.size;
}

/**
 *
 * @param channelId
 * @param userId
 * @param {ClientSession} session
 */
function setSessionAccount(channelId, userId, session)
{
    var userIdSessionMap = accountSessionMap.get(channelId);
    if (!userIdSessionMap)
    {
        userIdSessionMap = new Map();
        accountSessionMap.set(channelId, userIdSessionMap);
    }

    userIdSessionMap.set(userId, session);
}

/**
 *
 * @param channelId
 * @param userId
 * @returns {ClientSession}
 */
function getSessionByAccount(channelId, userId)
{
    var userIdSessionMap = accountSessionMap.get(channelId);
    if (!userIdSessionMap)
        return null;
    return userIdSessionMap.get(userId);
}

function syncVirtualTimeToAll()
{
    var bodyObj = new accountMsg.SyncServerTime(dateUtil.getTimestamp(), dateUtil.getTimezoneOffset());
    var msgObj = Message.newRequest(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.PUSH_SERVER_TIME, bodyObj);
    var ioBuf = NetMsgCodec.encodeEx(msgObj);
    for (var session of clientSessionMap.values())
    {
        session.sendBufDirectly(ioBuf, msgObj);
    }
}

function kickAllSession(reason)
{
    for (var session of clientSessionMap.values())
    {
        session.kickSession(reason, false);
    }
}

function onTimerCheckSession()
{
    //把要处理的Session放到列表，免得循环内，Map被修改

    /**
     * @type {ClientSession[]}
     */
    var markOffList = [];
    /**
     * @type {ClientSession[]}
     */
    var pushPingList = [];

    var curTime = dateUtil.getTimestamp();
    for (var session of clientSessionMap.values())
    {
        var activeTime = session.getActiveTime();
        var notActive = curTime - activeTime;
        if (notActive > MARK_ROLE_OFFLINE_TIME)
        {
            markOffList.push(session);
        }
        else if (notActive > NEED_SERVER_PING_TIME)
        {
            //如果上次主动Ping的时间，主动Ping一次客户端
            var lastSvrPing = session.getLastSvrPing();
            if (curTime - lastSvrPing > NEED_SERVER_PING_TIME)
                pushPingList.push(session);
        }
    }

    for (let i = 0; i < markOffList.length; ++i)
    {
        let session = markOffList[i];
        session.detachSessionFromRole();
    }

    for (let i = 0; i < pushPingList.length; ++i)
    {
        let session = pushPingList[i];
        session.sendPingPush();
    }
}

var doInitCoroutine = Promise.coroutine(function * () {
    logUtil.info("会话管理模块开始初始化...");

    //开启定时器
    timerIdCheckSession = setInterval(onTimerCheckSession, TIMER_INV_CHECK_SESSION);

    logUtil.info("会话管理模块完成初始化");
});

function doInit()
{
    return doInitCoroutine();
}

var doDestroyCoroutine = Promise.coroutine(function * () {
    logUtil.info("会话管理模块开始销毁...");

    //关闭定时器
    clearInterval(timerIdCheckSession);
    timerIdCheckSession = null;

    kickAllSession("服务器停机");
    logUtil.info("会话管理模块完成销毁");
});

function doDestroy()
{
    return doDestroyCoroutine();
}


////////////导出元素////////////
exports.newSession = newSession;
exports.delSession = delSession;
exports.getSessionCount = getSessionCount;
exports.setSessionAccount = setSessionAccount;
exports.getSessionByAccount = getSessionByAccount;
exports.syncVirtualTimeToAll = syncVirtualTimeToAll;
exports.doInit = doInit;
exports.doDestroy = doDestroy;