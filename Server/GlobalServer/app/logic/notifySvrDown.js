"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var appCfg = require("../../config");
var appUtil = require("../libs/appUtil");
var logUtil = require("../libs/logUtil");
var httpUtil = require("../libs/httpUtil");
var dbUtil = require("../libs/dbUtil");

////////////导出函数////////////
var doLogicCoroutine = Promise.coroutine(function * (req, res, body, pathName) {
    var retMsg = {code:0, msg:"OK"};
    try {
        //转换数据
        var jsonObj = appUtil.parseJsonObj(body);
        logUtil.debug(JSON.stringify(jsonObj));

        //检查参数
        if (!jsonObj)
        {
            retMsg.code = 2;
            retMsg.msg = "数据格式不正确";
            return;
        }
        if (!Object.isNumber(jsonObj.serverId))
        {
            retMsg.code = 3;
            retMsg.msg = "serverId必须是数字";
            return;
        }

        //更新数据库
        var db = dbUtil.getDB();
        var col = db.collection("serverList");
        var query = {serverId:jsonObj.serverId};
        var updateObj = {liveTime:0};
        yield col.updateOne(query, {$set:updateObj});

        //通知刷新缓存
        appUtil.fireProcessEvent("refreshSvrList");
    }
    catch (err) {
        retMsg.code = 1;
        retMsg.msg = "服务器异常";
    }
    finally {
        httpUtil.response(res, retMsg);
    }
});

function doLogic(req, res, body, pathName)
{
    var query = httpUtil.getQueryObjFromUrl(req.url);
    if (!query.key || query.key !== appCfg.adminKey)
    {
        httpUtil.response(res, null, 403);
        return;
    }

    doLogicCoroutine(req, res, body, pathName);
}

////////////导出元素////////////
exports.doLogic = doLogic;