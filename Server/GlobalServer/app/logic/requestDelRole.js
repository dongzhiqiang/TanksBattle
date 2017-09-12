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
    var retMsg = {};
    try {
        //{heroId:1}
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
        if (!Object.isNumber(jsonObj.heroId))
        {
            retMsg.code = 3;
            retMsg.msg = "heroId必须是数字";
            return;
        }

        //获取数据集合
        var db = dbUtil.getDB();
        var col1 = db.collection("roleList");

        //删除数据
        yield col1.deleteOne({heroId:jsonObj.heroId});

        retMsg.code = 0;
        retMsg.msg = "OK";
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