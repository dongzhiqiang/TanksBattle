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
    var retMsg = {};
    try {
        //{channelId:"",userId:"",guid:"",name:"",roleId:"kratos",serverId:1,level:0}
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
        if (!Object.isString(jsonObj.roleId))
        {
            retMsg.code = 7;
            retMsg.msg = "roleId必须是字符串";
            return;
        }
        if (!Object.isNumber(jsonObj.serverId))
        {
            retMsg.code = 8;
            retMsg.msg = "serverId必须是数字";
            return;
        }
        if (!Object.isNumber(jsonObj.level))
        {
            retMsg.code = 9;
            retMsg.msg = "level必须是数字";
            return;
        }

        //获取数据集合
        var db = dbUtil.getDB();
        var col1 = db.collection("roleList");

        //检测名字是否重复
        var nameCnt = yield col1.count({name:jsonObj.name});
        if (nameCnt > 0)
        {
            retMsg.code = 10;
            retMsg.msg = "名字已被使用";
            return;
        }

        //申请唯一角色实例ID
        var col2 = db.collection("numberSeed");
        //这里用了upsert，如果不出错，是会返回结果对象
        var ret = yield col2.findOneAndUpdate({_id:0}, {$inc:{heroId:1}}, {projection:{heroId:1}, upsert:true, returnOriginal:false});
        //得到唯一角色实例ID
        var heroId = ret.heroId;
        col2 = null;

        //收集参数
        var updateObj = {};
        updateObj.channelId = jsonObj.channelId;
        updateObj.userId = jsonObj.userId;
        updateObj.guid = jsonObj.guid;
        updateObj.name = jsonObj.name;
        updateObj.level = jsonObj.level;
        updateObj.roleId = jsonObj.roleId;
        updateObj.heroId = heroId;
        updateObj.serverId = jsonObj.serverId;
        updateObj.lastLogin = dateUtil.getTrueTimestamp();

        //插入数据库（这里有极低的概率出现名字重复，有catch）
        yield col1.insertOne(updateObj);

        retMsg.code = 0;
        retMsg.msg = "OK";
        retMsg.cxt = {heroId:heroId}; //返回角色实例ID
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