"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var logUtil = require("../../libs/logUtil");

////////////模块内变量////////////

////////////内部函数////////////

////////////导出函数////////////
var doInitCoroutine = Promise.coroutine(function * () {
    logUtil.info("活动模块开始初始化...");

    //初始化竞技场模块，由于会互相引用，这个require不能放到顶上
    var arenaMgr = require("./arena/arenaMgr");
    yield arenaMgr.doInit();
    //初始化神器抢夺机器人模块
    var treasureRobMgr = require("./treasureRob/treasureRobMgr");
    yield treasureRobMgr.doInit();

    logUtil.info("活动模块完成初始化");
});

function doInit()
{
    return doInitCoroutine();
}

var doDestroyCoroutine = Promise.coroutine(function * () {
    logUtil.info("活动模块开始销毁...");

    //清理竞技场模块，由于会互相引用，这个require不能放到顶上
    var arenaMgr = require("./arena/arenaMgr");
    yield arenaMgr.doDestroy();

    var treasureRobMgr = require("./treasureRob/treasureRobMgr");
    yield treasureRobMgr.doDestroy();

    logUtil.info("活动模块完成销毁");
});

function doDestroy()
{
    return doDestroyCoroutine();
}

exports.doInit = doInit;
exports.doDestroy = doDestroy;