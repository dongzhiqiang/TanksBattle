"use strict";
////////////内部模块////////////
var fs      = require("fs");
var pathUtil= require("path");

////////////我的模块////////////
var appCfg  = require("../../config");
var appUtil = require("../libs/appUtil");
var dateUtil = require("../libs/dateUtil");
var logUtil = require("../libs/logUtil");
var httpUtil= require("../libs/httpUtil");
var mime    = require("../libs/mime").types;

////////////模块内变量////////////
var fileCache = null;

////////////私有函数////////////
function initFileCache()
{
    fileCache = {totalsize: 0, files: {}};
}

function addFileCache(pathName, file, contentType)
{
    //程序退出中？不缓存
    if (appUtil.isProcessExiting())
        return;

    //已在缓存中？可能是同时多次回调导致，这里会更新时间值，可以直接返回
    //什么？file和cache.file可能内容不一样？不是有watchFile么。
    var cache = findFileCache(pathName);
    if (cache)
        return;

    fileCache.totalsize += file.length;
    fileCache.files[pathName] = {file:file, time:dateUtil.getTrueTimestamp(), type:contentType};
    logUtil.debug("文件缓存，文件：" + pathName + "，文件大小：" + appUtil.formatCapacity(file.length) +"，缓存共大小："  + appUtil.formatCapacity(fileCache.totalsize));

    fs.watchFile(pathName, function (curr, prev) {
        removeFileCache(pathName);
    });
}

function removeFileCache(pathName)
{
    var files = fileCache.files;
    var item = files[pathName];
    if (!item)
        return;

    fileCache.totalsize -= item.file.length;
    delete files[pathName];
    logUtil.debug("删除文件缓存：" + pathName + "，缓存共大小：" + appUtil.formatCapacity(fileCache.totalsize));

    fs.unwatchFile(pathName);
}

function findFileCache(pathName)
{
    var cache = fileCache.files[pathName];
    if (cache) {
        //更新时间
        cache.time = dateUtil.getTrueTimestamp();
        return cache;
    }
    else {
        return null;
    }
}

////////////导出函数////////////
function doLogic(req, res, body, pathName)
{
    //去除前面的/
    pathName = pathName.substring(1);
    pathName = pathUtil.normalize(pathName);

	if (pathName === appCfg.resDirName || pathName === appCfg.resDirName + pathUtil.sep)
	{
		pathName = pathUtil.join(appCfg.resDirName, "index.html");
	}
    else if (pathName === "favicon.ico")
    {
        pathName = pathUtil.join(appCfg.resDirName, pathName);
    }
    else if (!pathName.startWith(appCfg.resDirName))
    {
        //比如/resource/../../file.txt，它是非法的
        logUtil.warn("访问不能访问的文件：" + pathName);
        httpUtil.response(res, null, 403);
        return;
    }

    logUtil.debug("文件路径：" + pathName);

    var cache = findFileCache(pathName);
    if (cache)
    {
        httpUtil.response(res, cache.file, 200, {"Content-Type": cache.type});
    }
    else
    {
        fs.exists(pathName, function (exists)
        {
            if (!exists)
            {
                httpUtil.response(res, null, 404);
            }
            else
            {
                fs.readFile(pathName, function (err, file)
                {
                    if (err)
                    {
                        logUtil.error("读取文件失败：" + pathName, err);
                        httpUtil.response(res, null, 500);
                    }
                    else
                    {
                        var ext = pathUtil.extname(pathName);
                        ext = ext ? ext.substring(1) : "unknown";
                        var contentType = mime[ext] || mime["unknown"];

                        addFileCache(pathName, file, contentType);

                        httpUtil.response(res, file, 200, {"Content-Type": contentType});
                    }
                });
            }
        });
    }
}

////////////初始化函数////////////
function initCacheChecker()
{
    var timer = setInterval(function()
    {
        var files = fileCache.files;
        var curTime = dateUtil.getTrueTimestamp();
        for (var k in files)
        {
            var item = files[k];
            if (curTime - item.time > appCfg.fileCacheTime)
            {
                fileCache.totalsize -= item.file.length;
                delete files[k];
                logUtil.debug("删除文件缓存：" + k + "，缓存共大小：" + appUtil.formatCapacity(fileCache.totalsize));

                fs.unwatchFile(k);
            }
        }
    }, 5000);

    //注册进程准备退出的监听
    var onProcessExiting = function() {
        logUtil.info("静态文件缓存开始关闭...");
        //清理监视注册
        var files = fileCache.files;
        for (var k in files)
        {
            fs.unwatchFile(k);
        }
        //缓存对象还原
        initFileCache();
        //清定时器
        clearInterval(timer);
        //表示自己已处理完了
        appUtil.removeProcessExitingListener(onProcessExiting);
        logUtil.info("静态文件缓存完成关闭");
    };
    appUtil.addProcessExitingListener(onProcessExiting);
}

////////////全局执行////////////
initFileCache();
initCacheChecker();

////////////导出元素////////////
exports.doLogic = doLogic;