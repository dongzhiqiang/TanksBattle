"use strict";

////////////我的模块////////////
var appCfg  = require("../../config");
var appUtil = require("../libs/appUtil");
var logUtil = require("../libs/logUtil");
var initSys = require("./init");

////////////全局执行////////////
//提示一下
logUtil.info("应用【" + appCfg.appName + "】开始");
logUtil.info("============参数============");
logUtil.info("区    名：" + appCfg.areaName);
logUtil.info("服务器名：" + appCfg.serverName);
logUtil.info("服务器ID：" + appCfg.serverId);
logUtil.info("对 外 IP：" + appCfg.publicHostName);
logUtil.info("对外端口：" + appCfg.port);
logUtil.info("展示状态：" + appCfg.showState);
logUtil.info("位置索引：" + appCfg.showIndex);
logUtil.info("============参数============");

//防止未捕获的错误把进程搞挂
process.on("uncaughtException", function(err) {
    logUtil.error("发生未捕获的错误", err);
});

//接收父进程来的消息
process.on("message", function(msg)
{
    switch (msg.code)
    {
        case "exit":
            appUtil.fireProcessExiting(true);
            break;
    }
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

//注册进程准备退出的监听
appUtil.addProcessExitingListener(function() {
    //先执行模块销毁
    initSys.doDestroy(function() {
        logUtil.info("所有模块完成退出过程，可以正式退出了");

        //这里没用exit是因为可能有些写法问题，导致还有对象引用，导致进程不能退出，这里不能隐藏这个问题
        if (process.disconnect)
            process.disconnect();
    });
});

logUtil.info("初始化应用...");
initSys.doInit(function(ok){
    if (!ok) {
        logUtil.fatal("初始化应用失败，进程将退出");
        //设置初始完成标记
        appUtil.setProcessInited();
        appUtil.fireProcessExiting();
        return;
    }

    //设置初始完成标记
    appUtil.setProcessInited();
    logUtil.info("初始化应用成功");
});