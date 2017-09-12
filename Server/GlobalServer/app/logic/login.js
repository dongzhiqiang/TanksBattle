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
        //{username:"user",password:"123456"}
        var jsonObj = httpUtil.getJsonObjFromTEA(body);
        logUtil.debug(JSON.stringify(jsonObj));

        //检查字段和格式
        if (!jsonObj)
        {
            retMsg.code = 2;
            retMsg.msg = "数据格式不正确";
            return;
        }
        if (!Object.isString(jsonObj.username))
        {
            retMsg.code = 3;
            retMsg.msg = "账号必须是字符串";
            return;
        }
        if (!Object.isString(jsonObj.password))
        {
            retMsg.code = 4;
            retMsg.msg = "密码必须是字符串";
            return;
        }
        var username = jsonObj.username.trim().toLowerCase();   //去空格，并转为小写
        var password = jsonObj.password;                        //密码允许空格
        if (username.length < 0)
        {
            retMsg.code = 5;
            retMsg.msg = "必须输入账号";
            return;
        }
        if (password.length < 0)
        {
            retMsg.code = 6;
            retMsg.msg = "必须输入密码";
            return;
        }

        //查询用户数据
        var query = {username:username};
        var db = dbUtil.getDB();
        var col = db.collection("accountList");
        var ret = yield col.findOne(query, {userId:1, password:1});
        //不存在这个用户
        if (!ret)
        {
            retMsg.code = 7;
            retMsg.msg = "账号不存在";
            return;
        }

        //检查密码
        password = appUtil.getMD5(password);
        if (ret.password !== password)
        {
            retMsg.code = 8;
            retMsg.msg = "密码不正确";
            return;
        }

        //密码对了，可以生成和写入token
        var token = appUtil.getRandomStr(16);
        var update = {$set:{token:token}};
        yield col.updateOne(query, update);

        //下发用户ID和令牌
        retMsg.code = 0;
        retMsg.msg = "登录成功";
        retMsg.cxt = {userId:ret.userId, token:token};
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