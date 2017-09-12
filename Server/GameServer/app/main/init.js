"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var appUtil     = require("../libs/appUtil");
var logUtil     = require("../libs/logUtil");

////////////我的逻辑模块////////////
var initCfgHandler = require("./initGameCfg");
var initDBHandler = require("./initDB");
var initSvrAgentHandler = require("./initServerAgent");
var initHttpHandler = require("./initHttp");
var initNetMsgHandler = require("./initNetMsg");
var initSocketHandler = require("./initSocket");
var roleMgr = require("../logic/role/roleMgr");
var sessionMgr = require("../logic/session/sessionMgr");
var rankMgr = require("../logic/rank/rankMgr");
var activityMgr = require("../logic/activity/activityMgr");
var socialMgr = require("../logic/social/socialMgr");
var corpsMgr = require("../logic/corps/corpsMgr");
var chatMgr = require("../logic/chat/chatMgr");

////////////导出函数////////////
/**
 * 初始化各模块
 * 初始化失败后，建议模块自己马上做清理操作
 * @param {CallbackOnlyBooleanParam} callback
 */
var doInitCoroutine = Promise.coroutine(function * (callback){
    try {
        yield logUtil.doInit();
        yield initCfgHandler.doInit();
        yield initDBHandler.doInit();
        yield initSvrAgentHandler.doInit();
        yield initHttpHandler.doInit();
        yield initNetMsgHandler.doInit();
        yield initSocketHandler.doInit();
        yield rankMgr.doInit(); //rankMgr必须在roleMgr之前初始化，因为机器人也要入榜
        yield roleMgr.doInit();
        yield sessionMgr.doInit();
        yield activityMgr.doInit(); //activityMgr必须在rankMgr、robotMgr之后初始化
        yield socialMgr.doInit();
        yield corpsMgr.doInit();
        yield chatMgr.doInit();
        callback(true);
    }
    catch (err) {
        logUtil.error("初始化失败", err);
        callback(false);
    }
});

function doInit(callback)
{
    doInitCoroutine(callback);
}

/**
 * 销毁各模块
 * 注意，各模块不允许返回异常，出现异常自行处理
 * 各个模块要允许未初始化成功、已销毁也能调用
 * @param {CallbackWithNoParam} callback
 */
var doDestroyCoroutine = Promise.coroutine(function * (callback){
    try {
        //如果正在初始化，还是初始化再关闭
        while(!appUtil.isProcessInited())
            yield Promise.delay(10);

        yield activityMgr.doDestroy();
        yield rankMgr.doDestroy();
        yield socialMgr.doDestroy();
        yield sessionMgr.doDestroy();
        yield roleMgr.doDestroy();  //注意顺序，roleMgr要放在sessionMgr之后
        yield initSocketHandler.doDestroy();
        yield initNetMsgHandler.doDestroy();
        yield initHttpHandler.doDestroy();
        yield initSvrAgentHandler.doDestroy();
        yield initDBHandler.doDestroy();
        yield initCfgHandler.doDestroy();
        yield logUtil.doDestroy();
        yield corpsMgr.doDestroy();
    }
    catch (err) {
        logUtil.error("销毁模块出错", err);
    }
    finally {
        callback();
    }
});

function doDestroy(callback)
{
    doDestroyCoroutine(callback);
}

////////////导出元素////////////
exports.doInit = doInit;
exports.doDestroy = doDestroy;