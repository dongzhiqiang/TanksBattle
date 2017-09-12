"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var appCfg = require("../../config");
var appUtil = require("../libs/appUtil");
var logUtil = require("../libs/logUtil");
var httpUtil = require("../libs/httpUtil");
var dbUtil = require("../libs/dbUtil");

////////////模块内变量////////////
//default渠道就是默认自带的渠道
var channelCheckHandlerMap = {
    "default":doCheckLogin
};

////////////私有函数////////////
//默认渠道就在这里验证
var doCheckLoginCoroutine = Promise.coroutine(function * (res, userId, token){
    var retMsg = {code:0, msg:"OK"};
    try {
        //查询用户数据
        var query = {userId:userId};
        var db = dbUtil.getDB();
        var col = db.collection("accountList");
        var ret = yield col.findOne(query, {token:1});
        //不存在这个用户
        if (!ret)
        {
            retMsg.code = 2;
            retMsg.msg = "账号不存在";
            return;
        }

        //检查密码
        if (ret.token !== token)
        {
            retMsg.code = 3;
            retMsg.msg = "登录状态无效";
            return;
        }
    }
    catch (err) {
        retMsg.code = 1;
        retMsg.msg = "服务器异常";
    }
    finally {
        httpUtil.response(res, retMsg);
    }
});

function doCheckLogin(res, userId, token)
{
    doCheckLoginCoroutine(res, userId, token);
}

////////////导出函数////////////
function doLogic(req, res, body, pathName)
{
    //权限检查
    var query = httpUtil.getQueryObjFromUrl(req.url);
    if (!query.key || query.key !== appCfg.adminKey)
    {
        httpUtil.response(res, null, 403);
        return;
    }

    //转换数据
    //{channelId:"default",userId:"1",token:"123456",serverId:1}
    var jsonObj = appUtil.parseJsonObj(body);
    logUtil.debug(JSON.stringify(jsonObj));

    //检查字段和格式
    if (!jsonObj)
    {
        httpUtil.response(res, {code:1, msg:"数据格式不正确"});
        return;
    }
    if (!Object.isString(jsonObj.channelId))
    {
        httpUtil.response(res, {code:2, msg:"渠道ID必须是字符串"});
        return;
    }
    if (!Object.isString(jsonObj.userId))
    {
        httpUtil.response(res, {code:3, msg:"用户ID必须是字符串"});
        return;
    }
    if (!Object.isString(jsonObj.token))
    {
        httpUtil.response(res, {code:4, msg:"令牌必须是字符串"});
        return;
    }

    var handler = channelCheckHandlerMap[jsonObj.channelId];
    if (!handler || !Object.isFunction(handler))
    {
        httpUtil.response(res, {code:5, msg:"渠道ID未知"});
        return;
    }

    //调用函数
    handler(res, jsonObj.userId, jsonObj.token);
}

////////////导出元素////////////
exports.doLogic = doLogic;