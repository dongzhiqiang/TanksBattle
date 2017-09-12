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
        if (!Object.isString(jsonObj.channelId))
        {
            retMsg.code = 3;
            retMsg.msg = "channelId必须是字符串";
            return;
        }
        if (!Object.isString(jsonObj.userId))
        {
            retMsg.code = 4;
            retMsg.msg = "userId必须是字符串";
            return;
        }
        if (!Object.isString(jsonObj.guid))
        {
            retMsg.code = 5;
            retMsg.msg = "guid必须是字符串";
            return;
        }
        if (!Object.isString(jsonObj.name))
        {
            retMsg.code = 6;
            retMsg.msg = "name必须是字符串";
            return;
        }
        if (!Object.isNumber(jsonObj.level))
        {
            retMsg.code = 7;
            retMsg.msg = "level必须是数字";
            return;
        }
        if (!Object.isString(jsonObj.roleId))
        {
            retMsg.code = 8;
            retMsg.msg = "roleId必须是字符串";
            return;
        }
        if (!Object.isNumber(jsonObj.heroId))
        {
            retMsg.code = 9;
            retMsg.msg = "heroId必须是数字";
            return;
        }
        if (!Object.isNumber(jsonObj.serverId))
        {
            retMsg.code = 10;
            retMsg.msg = "serverId必须是数字";
            return;
        }

        //收集参数
        var updateObj = {};
        updateObj.channelId = jsonObj.channelId;
        updateObj.userId = jsonObj.userId;
        updateObj.guid = jsonObj.guid;
        updateObj.name = jsonObj.name;
        updateObj.level = jsonObj.level;
        updateObj.roleId = jsonObj.roleId;
        updateObj.heroId = jsonObj.heroId;
        updateObj.serverId = jsonObj.serverId;
        updateObj.lastLogin = dateUtil.getTrueTimestamp();

        //更新数据库
        var db = dbUtil.getDB();
        var col = db.collection("roleList");
        var query = {heroId:updateObj.heroId};
        yield col.updateOne(query, {$set:updateObj}, {upsert:true});
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