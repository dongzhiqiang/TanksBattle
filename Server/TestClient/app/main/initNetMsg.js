"use strict";

////////////内置模块////////////

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var appUtil = require("../libs/appUtil");
var logUtil = require("../libs/logUtil");
var handlerMgr = require("../logic/network/handlerMgr");

////////////导出函数////////////
var doInitCoroutine = Promise.coroutine(function * () {
    logUtil.info("消息处理开始初始化...");

    //其实没啥事，引用了就会自动执行
    var playerHandler = require("../logic/player/playerHandler");
    var testHandler = require("../logic/player/doTest");

    logUtil.info("消息处理完成初始化");
});

function doInit()
{
    return doInitCoroutine();
}

var doDestroyCoroutine = Promise.coroutine(function * () {
    try {
        logUtil.info("消息处理开始销毁...");
        handlerMgr.clearHandler();
        logUtil.info("消息处理完成销毁");
    }
    catch (err) {
        logUtil.warn("消息处理销毁出错", err);
    }
});

function doDestroy()
{
    return doDestroyCoroutine();
}

////////////导出元素////////////
exports.doInit = doInit;
exports.doDestroy = doDestroy;