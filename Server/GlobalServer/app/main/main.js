"use strict";

////////////内置模块////////////
var os      = require("os");

////////////外部模块////////////
var cluster = require("cluster");

////////////我的模块////////////
var appCfg  = require("../../config");
var appUtil = require("../libs/appUtil");
var logUtil = require("../libs/logUtil");
var initSys = require("./init");

////////////全局执行////////////
//如果是主进程就创建子进程
if (cluster.isMaster) {
    logUtil.info("应用【" + appCfg.appName + "】开始");

    //一个CPU核心一个工作进程
    var numCPUs = os.cpus().length;
    var workerCnt = Math.max(1, appCfg.maxWorkerCnt <= 0 ? numCPUs : Math.min(appCfg.maxWorkerCnt, numCPUs));
    logUtil.info("将创建" + workerCnt + "个工作进程");

    //广播消息给工作进程
    var broadMsgToWorker = function (msg, exclude)
    {
        for (var i = 0; i < workers.length; ++i)
        {
            var w = workers[i];
            //连接了才能发消息
            if (w !== exclude)
            {
                try {
                    w.send(msg);
                }
                catch (err) {
                }
            }
        }
    };

    //这里直接转发给另的工作进程
    var onRecvWorkerMsg = function(msg)
    {
        broadMsgToWorker(msg, this);
    };

    //创建工作进程
    var workers = [];
    for (var i = 0; i < workerCnt; ++i) {
        var worker = cluster.fork();
        workers.push(worker);

        //接收到工作进程发来的消息的事件
        worker.on("message", onRecvWorkerMsg);
    }

    //disconnect？打个标记，先不删除条目，等确认exit再删除，这样保证子进制有机会处理后续的过程
    cluster.on("disconnect", function (worker) {
        logUtil.warn("工作进程" + worker.id + "与主进程断开联系");
    });
    //exit了？那就删除进程，全部工作进程exit就可以关闭本主进程
    cluster.on("exit", function (worker, code, signal) {
        for (var i = 0; i < workers.length; ++i) {
            var w = workers[i];
            if (w === worker) {
                workers.splice(i, 1);

                logUtil.warn("工作进程" + w.id + "退出");
                logUtil.warn("删除工作进程" + w.id);
                break;
            }
        }
        if (workers.length <= 0) {
            logUtil.warn("工作进程全部退出，主进程也退出！");
            setTimeout(function () {
                process.exit(0);
            }, 1000);
        }
    });

    //处理程序中止消息
    var onMasterProcessCloseSignal = function() {
        logUtil.warn("【收到程序中止请求】");
        var msg = {code:"exit"};
        broadMsgToWorker(msg);
    };
    process.on("SIGHUP", onMasterProcessCloseSignal);
    process.on("SIGINT", onMasterProcessCloseSignal);
    process.on("SIGQUIT", onMasterProcessCloseSignal);
    process.on("SIGABRT", onMasterProcessCloseSignal);
    process.on("SIGTERM", onMasterProcessCloseSignal);
}
else {
    //提示一下
    if (cluster.worker)
        logUtil.info("工作进程" + cluster.worker.id + "启动");
    else
        logUtil.info("应用【" + appCfg.appName + "】开始");

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
        default:
            appUtil.fireProcessEvent(msg.code, msg.cxt, true);
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
            appUtil.fireProcessExiting();
            return;
        }

        //设置初始完成标记
        appUtil.setProcessInited();
        logUtil.info("初始化应用成功");
    });
}