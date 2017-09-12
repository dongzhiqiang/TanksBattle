"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var appUtil = require("../libs/appUtil");
var logUtil = require("../libs/logUtil");
var httpUtil = require("../libs/httpUtil");
var dbUtil = require("../libs/dbUtil");

////////////模块内变量////////////

////////////导出函数////////////
var doLogicCoroutine = Promise.coroutine(function * (req, res, body, pathName){
    var retMsg = {};
    try {
        //转换提交的内容
        //{userId:"1",token:"123456789"}
        var jsonObj = httpUtil.getJsonObjFromTEA(body);
        logUtil.debug(JSON.stringify(jsonObj));

        //检查字段和格式
        if (!jsonObj)
        {
            retMsg.code = 2;
            retMsg.msg = "数据格式不正确";
            return;
        }
        if (!Object.isString(jsonObj.userId))
        {
            retMsg.code = 3;
            retMsg.msg = "用户ID必须是字符串";
            return;
        }
        if (!Object.isString(jsonObj.token))
        {
            retMsg.code = 4;
            retMsg.msg = "令牌必须是字符串";
            return;
        }
        var userId = jsonObj.userId;
        var token  = jsonObj.token;
        if (userId.length < 0)
        {
            retMsg.code = 5;
            retMsg.msg = "必须提供用户ID";
            return;
        }
        if (token.length < 0)
        {
            retMsg.code = 6;
            retMsg.msg = "必须提供令牌";
            return;
        }

        //查询用户数据
        var query = {userId:userId};
        var db = dbUtil.getDB();
        var col = db.collection("accountList");
        var ret = yield col.findOne(query, {token:1});
        //不存在这个用户
        if (!ret)
        {
            retMsg.code = 7;
            retMsg.msg = "账号不存在";
            return;
        }

        //检查令牌
        if (ret.token !== token)
        {
            retMsg.code = 8;
            retMsg.msg = "令牌不正确";
            return;
        }

        //令牌对了，可以清空令牌了
        var update = {$set:{token:null}};
        yield col.updateOne(query, update);

        //下发结果
        retMsg.code = 0;
        retMsg.msg = "注销成功";
    }
    catch (err) {
        retMsg.code = 1;
        retMsg.msg = "服务器异常";
    }
    finally {
        httpUtil.responseTEA(res, retMsg);
    }
});

function doLogic(req, res, body, pathName)
{
    doLogicCoroutine(req, res, body, pathName);
}

////////////导出元素////////////
exports.doLogic = doLogic;
exports.binaryBody = true;