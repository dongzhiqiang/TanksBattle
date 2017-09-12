"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var appCfg  = require("../../config");
var appUtil = require("../libs/appUtil");
var logUtil = require("../libs/logUtil");
var httpUtil= require("../libs/httpUtil");
var initCfgHandler = require("./initGameCfg");
var initNetMsg = require("./initNetMsg");
var playerMgr = require("../logic/player/playerMgr");
var handlerMgr = require("../logic/network/handlerMgr");

////////////全局执行////////////
logUtil.info("应用【" + appCfg.appName + "】开始");

//防止未捕获的错误把进程搞挂
process.on("uncaughtException", function(err) {
    logUtil.error("发生未捕获的错误", err);
});

//注册进程准备退出的监听
appUtil.addProcessExitingListener(function() {
    setInterval(function(){
        //都处理完了？那可以结束了
        //这里<=1是因为还包括自己
        if (appUtil.getExitingListenerCount() <= 1)
        {
            logUtil.info("所有模块完成退出过程，可以正式退出了");
            clearInterval(this);
            //这里没用exit是因为可能有些写法问题，导致还有对象引用，导致进程不能退出，这里不能隐藏这个问题
            if (process.disconnect)
                process.disconnect();
        }
    }, 100);
});

//处理程序中止消息
var onProcessCloseSignal = function() {
    logUtil.warn("【收到程序中止请求】");
    appUtil.fireProcessExiting();
};
process.on("SIGHUP", onProcessCloseSignal);
process.on("SIGINT", onProcessCloseSignal);
process.on("SIGQUIT", onProcessCloseSignal);
process.on("SIGABRT", onProcessCloseSignal);
process.on("SIGTERM", onProcessCloseSignal);

//注意，这里做个表率，就是Promise.coroutine不能频繁调用，效率低，要一次创建出协程，多次使用协程，就如下面的mainCoroutine()。
var mainCoroutine = Promise.coroutine(function * (){
    yield initCfgHandler.doInit();
    yield initNetMsg.doInit();
    yield playerMgr.createPlayer(appCfg.usernameStartNum, appCfg.createPlayerNum);
    yield playerMgr.runGameLogic();
    yield playerMgr.clearPlayer();
    yield initNetMsg.doDestroy();
    yield initCfgHandler.doDestroy();
});
mainCoroutine();