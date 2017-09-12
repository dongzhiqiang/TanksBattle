"use strict";

////////////内置模块////////////
var http = require("http");
var domain = require("domain");

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var appCfg = require("../../config");
var appUtil = require("../libs/appUtil");
var logUtil = require("../libs/logUtil");
var httpUtil = require("../libs/httpUtil");
var router = require("./httpRouter");

////////////模块内变量////////////
var httpSvr = null;

////////////导出函数////////////
function doInit() {
    return new Promise(function (resolve, reject) {
        logUtil.info("HTTP服务开始初始化...");

        //HTTP请求计数
        var reqCounter = 0;

        //HTTP处理入口
        var procFun = function (req, res) {
            //服务器未初始化完？返回一个错误码
            if (!appUtil.isProcessInited())
            {
                httpUtil.response(res, null, 403);
                return;
            }

            //服务器关闭中？那就直接拒绝
            if (appUtil.isProcessExiting())
            {
                httpUtil.response(res, null, 403);
                return;
            }

            //发生错误时，阻止进程退出
            var d = domain.create();
            d.add(req);
            d.add(res);
            d.on("error", function (err) {
                logUtil.error("发生错误", err);
                httpUtil.response(res, null, 500);
            });

            //保存数据块
            var chunks = [];
            var recvLen = 0;

            //不执行setEncoding了，为了得到二进制数据，因为提交的不一定是字符串，是字符串也可能是加密的
            req.on("data", function (d) {
                chunks.push(d);
                recvLen += d.length;
                if (recvLen > appCfg.maxSubmitBodyLen) {
                    logUtil.warn("用户提交数据过多，长度：" + appUtil.formatCapacity(body.length) + "/" + appUtil.formatCapacity(appCfg.maxSubmitBodyLen) + "，对方地址：" + req.connection.remoteAddress);
                    httpUtil.response(res, null, 413);
                    req.removeAllListeners("end");
                }
            });

            req.on("end", function () {
                //计数加一
                ++reqCounter;
                //获取请求的URL
                var url = req.url;
                //把数据块连接成二进制数据缓冲对象
                var body = Buffer.concat(chunks);
                //及时清除接收对象
                chunks = null;

                logUtil.debug("请求[" + reqCounter + "]：" + req.connection.remoteAddress);
                logUtil.debug("地址：" + url);
                logUtil.debug("长度：" + body.length);

                router.doLogic(req, res, body);
            });
        };

        //创建HTTP服务
        httpSvr = http.createServer(procFun);
        httpSvr.listen(appCfg.httpPort, appCfg.httpBindIP, function(){
            logUtil.info("HTTP监听于端口" + appCfg.httpPort);
            logUtil.info("HTTP服务完成初始化");
            resolve();
        });
        httpSvr.on("error", function(err){
            reject(err);
        });
    });
}

function doDestroy()
{
    return new Promise(function (resolve, reject) {
        logUtil.info("HTTP服务开始销毁...");
        if (!httpSvr) {
            logUtil.info("HTTP服务完成销毁");
            resolve();
        }
        else {
            //防止再有错误
            httpSvr.on("error", function(err){
                logUtil.warn("HTTP服务出错", err);
            });
            httpSvr.close(function () {
                httpSvr = null;
                logUtil.info("HTTP服务完成销毁");
                resolve();
            });
        }
    });
}

////////////导出元素////////////
exports.doInit = doInit;
exports.doDestroy = doDestroy;