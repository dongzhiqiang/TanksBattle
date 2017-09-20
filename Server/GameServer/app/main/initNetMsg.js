"use strict";

////////////内置模块////////////

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var logUtil = require("../libs/logUtil");
var handlerMgr = require("../logic/session/handlerMgr");

////////////导出函数////////////
var doInitCoroutine = Promise.coroutine(function * () {
    logUtil.info("消息处理开始初始化...");

    //其实没啥事，引用了就会自动执行
    var accountHandler = require("../logic/session/accountHandler");
    var roleHandler = require("../logic/role/roleHandler");
    var equipHandler = require("../logic/equip/equipHandler");
    var gmHandler = require("../logic/gm/gmHandler");
    var levelHandler = require("../logic/level/levelHandler");
    var activityHandler = require("../logic/activity/activityHandler");
    var rankHandler = require("../logic/rank/rankHandler");
    var weaponHandler = require("../logic/weapon/weaponHandler");
    var opActivityHandler=require("../logic/opActivity/opActivityHandler");
    var mailHandler = require("../logic/mail/mailHandler");
    var taskHandler = require("../logic/task/taskHandler");
    var socialHandler = require("../logic/social/socialHandler");
    var flameHandler = require("../logic/flame/flameHandler");
    var systemHandler = require("../logic/system/systemHandler");
    var corpsHandler = require("../logic/corps/corpsHandler");
    var chatHandler = require("../logic/chat/chatHandler");
    var shopHandler = require("../logic/exchangeShop/shopHandler");
    var eliteLevelHandler = require("../logic/eliteLevel/eliteLevelHandler");
    var treasureHandler = require("../logic/treasure/treasureHandler");

    logUtil.info("消息处理完成初始化");
});

function doInit() {
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

function doDestroy() {
    return doDestroyCoroutine();
}

////////////导出元素////////////
exports.doInit = doInit;
exports.doDestroy = doDestroy;