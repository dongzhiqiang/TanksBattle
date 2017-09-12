"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var appUtil     = require("../libs/appUtil");
var logUtil     = require("../libs/logUtil");

////////////我的逻辑模块////////////
var initCfgHandler = require("./initGameCfg");
var initDBHandler = require("./initDB");
var initHttpHandler = require("./initHttp");

////////////导出函数////////////
var doInitCoroutine = Promise.coroutine(function * (callback){
    try {
        yield logUtil.doInit();
        yield initCfgHandler.doInit();
        yield initDBHandler.doInit();
        yield initHttpHandler.doInit();
        callback(true);
    }
    catch (err) {
        logUtil.error("初始化失败", err);
        callback(false);
    }
});
/**
 * 初始化各模块
 * 初始化失败后，建议模块自己马上做清理操作
 * @param {CallbackOnlyBooleanParam} callback
 */
function doInit(callback)
{
    return doInitCoroutine(callback);
}

var doDestroyCoroutine = Promise.coroutine(function * (callback){
    try {
        yield initHttpHandler.doDestroy();
        yield initDBHandler.doDestroy();
        yield initCfgHandler.doDestroy();
        yield logUtil.doDestroy();
    }
    catch (err) {
        logUtil.error("销毁模块出错", err);
    }
    finally {
        callback();
    }
});

/**
 * 销毁各模块
 * 注意，各模块不允许返回异常，出现异常自行处理
 * 各个模块要允许未初始化成功、已销毁也能调用
 * @function
 * @param {CallbackWithNoParam} callback
 */
function doDestroy(callback)
{
    return doDestroyCoroutine(callback);
}

////////////导出元素////////////
exports.doInit = doInit;
exports.doDestroy = doDestroy;