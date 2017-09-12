"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var appCfg = require("../../config");
var appUtil = require("../libs/appUtil");
var dateUtil = require("../libs/dateUtil");
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
        if (!Object.isString(jsonObj.area))
        {
            retMsg.code = 3;
            retMsg.msg = "area必须是字符串";
            return;
        }
        if (!Object.isString(jsonObj.name))
        {
            retMsg.code = 4;
            retMsg.msg = "name必须是字符串";
            return;
        }
        if (!Object.isNumber(jsonObj.index))
        {
            retMsg.code = 5;
            retMsg.msg = "index必须是数字";
            return;
        }
        if (!Object.isString(jsonObj.host))
        {
            retMsg.code = 6;
            retMsg.msg = "host必须是字符串";
            return;
        }
        if (!Object.isNumber(jsonObj.port))
        {
            retMsg.code = 7;
            retMsg.msg = "port必须是数字";
            return;
        }
        if (!Object.isString(jsonObj.lanHost))
        {
            retMsg.code = 8;
            retMsg.msg = "lanHost必须是字符串";
            return;
        }
        if (!Object.isNumber(jsonObj.lanPort))
        {
            retMsg.code = 9;
            retMsg.msg = "lanPort必须是数字";
            return;
        }
        if (!Object.isNumber(jsonObj.serverId))
        {
            retMsg.code = 10;
            retMsg.msg = "serverId必须是数字";
            return;
        }
        if (!Object.isNumber(jsonObj.onlineNum))
        {
            retMsg.code = 11;
            retMsg.msg = "onlineNum必须是数字";
            return;
        }
        if (!Object.isString(jsonObj.showState))
        {
            retMsg.code = 12;
            retMsg.msg = "showState必须是字符串";
            return;
        }

        //收集参数
        var updateObj = {};
        updateObj.area = jsonObj.area;
        updateObj.name = jsonObj.name;
        updateObj.index = jsonObj.index;
        updateObj.host = jsonObj.host;
        updateObj.port = jsonObj.port;
        updateObj.lanHost = jsonObj.lanHost;
        updateObj.lanPort = jsonObj.lanPort;
        updateObj.serverId = jsonObj.serverId;
        updateObj.liveTime = dateUtil.getTrueTimestamp();
        updateObj.loadState = jsonObj.onlineNum > 1 ? "busy" : "idle";
        updateObj.showState = jsonObj.showState;

        //更新数据库
        var db = dbUtil.getDB();
        var col = db.collection("serverList");
        var query = {serverId:updateObj.serverId};
        yield col.updateOne(query, {$set:updateObj}, {upsert:true});

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