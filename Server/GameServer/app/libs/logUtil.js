"use strict";

////////////内部模块////////////
var fs          = require("fs");
var pathUtil    = require("path");

////////////外部模块////////////
var Promise     = require("bluebird");

////////////我的模块////////////
var appCfg      = require("../../config");
var appUtil     = require("./appUtil");
var dateUtil    = require("./dateUtil");

////////////模块内数据////////////
var logStream   = null;
var logFileTime = 0;
var logFileName = "";
var timerCheck  = null;

const LOG_FATAL = 5;
const LOG_ERROR = 4;
const LOG_WARN  = 3;
const LOG_INFO  = 2;
const LOG_DEBUG = 1;

var logLevelMap = {};
logLevelMap[LOG_FATAL] = "FATAL";
logLevelMap[LOG_ERROR] = "ERROR";
logLevelMap[LOG_WARN]  = "WARN";
logLevelMap[LOG_INFO]  = "INFO";
logLevelMap[LOG_DEBUG] = "DEBUG";

var logConFunMap = {};
logConFunMap[LOG_FATAL] = console.error;
logConFunMap[LOG_ERROR] = console.error;
logConFunMap[LOG_WARN]  = console.warn;
logConFunMap[LOG_INFO]  = console.info;
logConFunMap[LOG_DEBUG] = console.log;

////////////私有函数////////////
function getFileNameTimeString()
{
    var date = dateUtil.getTrueDate();
    var ts = "" ;
    ts += date.getFullYear()    + "_";
    ts += (date.getMonth()+1)   + "_";
    ts += date.getDate()        + "_";
    ts += date.getHours()       + "_";
    ts += date.getMinutes()     + "_";
    ts += date.getSeconds();
    return ts;
}

/**
 * 写日志
 * @param {number} loglvl - 值可以是1-5
 * @param {string|object} msgobj - 日志对象
 * @param {Error?} errobj - 如果有错误对象，就传错误对象过来，以便打印调用栈
 */
function writeLog(loglvl, msgobj, errobj)
{
    //日志级别检测
    if (!appCfg.debug && loglvl <= LOG_DEBUG)
        return;

    var work   = "[" + appUtil.getWorkerID() + "]";
    var head   = "[" + dateUtil.getTrueDateString() + "][" + logLevelMap[loglvl]  +"] ";
    var msgstr = Object.isString(msgobj) ? msgobj : JSON.stringify(msgobj);
    var logstr = head + msgstr;
    var errstr = errobj && (errobj.stack || errobj);

    if (appCfg.consoleLog)
    {
        var logConFun = logConFunMap[loglvl];
        logConFun(work + logstr);
        if (errstr)
            logConFun(errstr);
    }

    if (logStream)
    {
        logStream.write(logstr + "\r\n");
        if (errstr)
        {
            logStream.write(errstr + "\r\n");
        }
    }
}

function ensureDir(dirpath)
{
    dirpath = dirpath.trim();
    var arr = dirpath.split("/");
    if (arr.length > 0)
    {
        var path = dirpath.startWith("/") ? "/" : "";
        for (var i = 0; i < arr.length; ++i)
        {
            var part = arr[i].trim();
            path = pathUtil.join(path, part);
            if (!fs.existsSync(path))
            {
                fs.mkdirSync(path);
            }
            else
            {
                var stats = fs.lstatSync(path);
                if (!stats.isDirectory())
                {
                    fs.unlinkSync(path);
                    fs.mkdirSync(path);
                }
            }
        }
    }
}

////////////导出函数////////////
/**
 *
 * @param {string|object} msgobj
 */
function debug(msgobj)
{
    writeLog(LOG_DEBUG, msgobj);
}

/**
 *
 * @param {string|object} msgobj
 */
function info(msgobj)
{
    writeLog(LOG_INFO, msgobj);
}

/**
 *
 * @param {string|object} msgobj
 * @param {Error?} errobj
 */
function warn(msgobj, errobj)
{
    writeLog(LOG_WARN, msgobj, errobj);
}

/**
 *
 * @param {string|object} msgobj
 * @param {Error?} errobj
 */
function error(msgobj, errobj)
{
    writeLog(LOG_ERROR, msgobj, errobj || new Error());
}

/**
 *
 * @param {string|object} msgobj
 * @param {Error?} errobj
 */
function fatal(msgobj, errobj)
{
    writeLog(LOG_FATAL, msgobj, errobj || new Error());
}

////////////日志处理////////////
function getLogPostFix()
{
    return "_[" + appUtil.getWorkerID() + "].log";
}

function checkLogOnTimer()
{
    var curTime = dateUtil.getTrueTimestamp();
    var postFix = getLogPostFix();

    if (curTime - logFileTime > appCfg.logFileInv)
    {
        logFileTime = curTime;
        logFileName = getFileNameTimeString() + postFix;
        if (logStream)
        {
            logStream.end();
            logStream = null;
        }
        logStream = fs.createWriteStream(pathUtil.join(appCfg.logDir, logFileName));
    }

    fs.readdir(appCfg.logDir, function(err, files)
    {
        if(err)
        {
            error("读取日志目录失败" + err);
            return;
        }

        for (var i = 0; i < files.length; ++i)
        {
            var fileName = files[i];
            if (fileName.endWith(postFix) && fileName !== logFileName)
            {
                var filePath = pathUtil.join(appCfg.logDir, fileName);
                fs.stat(filePath, function(err, stats)
                {
                    if(err)
                    {
                        error("获取日志文件属性失败， 文件：" + filePath + "，错误描述：" + err);
                        return;
                    }

                    var modifiedTime = dateUtil.getTimestampFromDate(stats.mtime);
                    if (curTime - modifiedTime > appCfg.logFileTime)
                    {
                        info("尝试删除日志文件：" + filePath);
                        fs.unlink(filePath, function(err)
                        {
                            if(err)
                            {
                                error("删除日志文件失败， 文件：" + filePath + "，错误描述：" + err);
                                return;
                            }

                            info("删除日志完成：" + filePath);
                        });
                    }
                });
            }
        }
    });
}

function initLog()
{
    //保证日志目录存在
    ensureDir(appCfg.logDir);

    //初始化日志文件
    checkLogOnTimer();
}

var doInitCoroutine = Promise.coroutine(function * () {
    info("日志模块开始初始化...");
    //开启定时器，定时删除旧日志文件
    timerCheck = setInterval(checkLogOnTimer, 1000 * 60 * 10);
    info("日志模块完成初始化");
});

function doInit()
{
    return doInitCoroutine();
}

var doDestroyCoroutine = Promise.coroutine(function * () {
    info("日志模块开始销毁...");
    if (timerCheck)
    {
        clearInterval(timerCheck);
        timerCheck = null;
    }
    info("日志模块完成销毁");
});

function doDestroy()
{
    return doDestroyCoroutine();
}

////////////全局执行////////////
initLog();

////////////导出元素////////////
exports.debug = debug;
exports.info  = info;
exports.warn  = warn;
exports.error = error;
exports.fatal = fatal;
exports.doInit = doInit;
exports.doDestroy = doDestroy;