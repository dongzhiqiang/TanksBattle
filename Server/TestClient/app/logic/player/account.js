"use strict";

////////////内置模块////////////
var Path = require("path");

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var appCfg  = require("../../../config");
var appUtil = require("../../libs/appUtil");
var logUtil = require("../../libs/logUtil");
var httpUtil= require("../../libs/httpUtil");

////////////导出函数////////////
var defaultPageCoroutine = Promise.coroutine(function * (){
    var result = "";
    try
    {
        result = yield httpUtil.doGetCoroutine(appCfg.globalURL, "text");
        logUtil.debug(result);
    }
    catch (err)
    {
        logUtil.error("", err);
    }
    return result;
});

function defaultPage()
{
    return defaultPageCoroutine();
}

var registerCoroutine = Promise.coroutine(function * (username, password){
    var obj = null;
    try
    {
        var buf = httpUtil.jsonObjToTEA({username:username, password:password});
        var result = yield httpUtil.doPostCoroutine(appCfg.globalURL + "register", buf, "buffer");
        obj = httpUtil.getJsonObjFromTEA(result);
        logUtil.debug(JSON.stringify(obj));
    }
    catch (err)
    {
        logUtil.error("", err);
    }
    return obj;
});

function register(username, password)
{
    return registerCoroutine(username, password);
}

var loginCoroutine = Promise.coroutine(function * (username, password){
    var obj = null;
    try
    {
        var buf = httpUtil.jsonObjToTEA({username:username, password:password});
        var result = yield httpUtil.doPostCoroutine(appCfg.globalURL + "login", buf, "buffer");
        obj = httpUtil.getJsonObjFromTEA(result);
        logUtil.debug(JSON.stringify(obj));
    }
    catch (err)
    {
        logUtil.error("", err);
    }
    return obj;
});

function login(username, password)
{
    return loginCoroutine(username, password);
}

var logoutCoroutine = Promise.coroutine(function * (userId, token){
    var obj = null;
    try
    {
        var buf = httpUtil.jsonObjToTEA({userId:userId, token:token});
        var result = yield httpUtil.doPostCoroutine(appCfg.globalURL + "logout", buf, "buffer");
        obj = httpUtil.getJsonObjFromTEA(result);
        logUtil.debug(JSON.stringify(obj));
    }
    catch (err)
    {
        logUtil.error("", err);
    }
    return obj;
});

function logout(userId, token)
{
    return logoutCoroutine(userId, token);
}

var modifyPasswordCoroutine = Promise.coroutine(function * (username, password, newPassword, clearToken){
    var obj = null;
    try
    {
        var buf = httpUtil.jsonObjToTEA({username:username, password:password, newPassword:newPassword, clearToken:clearToken});
        var result = yield httpUtil.doPostCoroutine(appCfg.globalURL + "modifyPassword", buf, "buffer");
        obj = httpUtil.getJsonObjFromTEA(result);
        logUtil.debug(JSON.stringify(obj));
    }
    catch (err)
    {
        logUtil.error("", err);
    }
    return obj;
});

function modifyPassword(username, password, newPassword, clearToken)
{
    return modifyPasswordCoroutine(username, password, newPassword, clearToken);
}

var getServersCoroutine = Promise.coroutine(function * (channelId, userId){
    var obj = null;
    try
    {
        var buf = httpUtil.jsonObjToTEA({channelId:channelId,userId:userId});
        var result = yield httpUtil.doPostCoroutine(appCfg.globalURL + "getServers", buf, "buffer");
        obj = httpUtil.getJsonObjFromTEA(result);
        logUtil.debug(JSON.stringify(obj));
    }
    catch (err)
    {
        logUtil.error("", err);
    }
    return obj;
});

function getServers(channelId, userId)
{
    return getServersCoroutine(channelId, userId);
}

////////////导出元素////////////
exports.defaultPage = defaultPage;
exports.register = register;
exports.login = login;
exports.logout = logout;
exports.modifyPassword = modifyPassword;
exports.getServers = getServers;