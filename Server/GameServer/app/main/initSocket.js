"use strict";

////////////内置模块////////////
var net = require("net");

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var appCfg = require("../../config");
var appUtil = require("../libs/appUtil");
var logUtil = require("../libs/logUtil");
var Connection = require("../logic/session/connection").Connection;

////////////模块内变量////////////
var netSvr = null;

////////////导出函数////////////
function doInit() {
    return new Promise(function (resolve, reject) {
        logUtil.info("Socket服务开始初始化...");
        /**
         * 处理入口
         * @param {net.Socket} sock
         */
        var procFun = function (sock) {
            //服务器初始化完成才可以接收请求
            if (!appUtil.isProcessInited())
            {
                sock.destroy();
                return;
            }

            //服务器关闭中？那就直接拒绝
            if (appUtil.isProcessExiting())
            {
                sock.destroy();
                return;
            }

            //连接来了，就新建一个连接对象，不用保存这个对象到某个表里，内部会自己处理
            var conn = new Connection(sock);
            conn.start();
        };

        //创建服务
        netSvr = net.createServer(procFun);
        netSvr.listen(appCfg.port, appCfg.bindIP, function(){
            logUtil.info("Socket监听于端口" + appCfg.port);
            logUtil.info("Socket服务完成初始化");
            resolve();
        });
        netSvr.on("error", function(err){
            reject(err);
        });
    });
}

function doDestroy()
{
    return new Promise(function (resolve, reject) {
        logUtil.info("Socket服务开始销毁...");
        if (!netSvr) {
            logUtil.info("Socket服务完成销毁");
            resolve();
        }
        else {
            //防止再有错误
            netSvr.on("error", function(err){
                logUtil.warn("Socket服务出错", err);
            });
            netSvr.close(function () {
                netSvr = null;
                logUtil.info("Socket服务完成销毁");
                resolve();
            });
        }
    });
}

////////////导出元素////////////
exports.doInit = doInit;
exports.doDestroy = doDestroy;