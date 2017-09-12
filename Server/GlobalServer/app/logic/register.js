"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var appUtil = require("../libs/appUtil");
var logUtil = require("../libs/logUtil");
var httpUtil = require("../libs/httpUtil");
var dbUtil = require("../libs/dbUtil");

////////////模块内变量////////////
var regExpUsername = /^[a-z0-9]+$/;

////////////导出函数////////////
var doLogicCoroutine = Promise.coroutine(function * (req, res, body, pathName){
    var retMsg = {code:0, msg:"注册成功"};
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
        if (username.length < 2 || username.length > 25)
        {
            retMsg.code = 5;
            retMsg.msg = "账号必须在2-25个字之间";
            return;
        }
        if (!regExpUsername.test(username))
        {
            retMsg.code = 6;
            retMsg.msg = "账号只能由字母、数字组成";
            return;
        }
        if (password.length < 6)
        {
            retMsg.code = 7;
            retMsg.msg = "密码至少6个字";
            return;
        }

        //检查username是否已存在，以免浪费userId种子
        var db = dbUtil.getDB();
        var col = db.collection("accountList");
        var ret = yield col.findOne({username:username}, {userId:1});
        //已存在账号？
        if (ret)
        {
            retMsg.code = 8;
            retMsg.msg = "账号已存在";
            return;
        }

        //分配userId
        var col2 = db.collection("numberSeed");
        //这里用了upsert，如果不出错，是会返回结果对象
        ret = yield col2.findOneAndUpdate({_id:0}, {$inc:{userId:1}}, {projection:{userId:1}, upsert:true, returnOriginal:false});
        var userId = ret.userId.toString(); //这里统一为字符串
        col2 = null;

        //有userId了，可以插入数据了，这里如果有错误，下面会catch
        password = appUtil.getMD5(password);
        yield col.insertOne({userId:userId, username:username, password:password, token:null});
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