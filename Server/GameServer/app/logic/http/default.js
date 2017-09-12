"use strict";

////////////我的模块////////////
var httpUtil = require("../../libs/httpUtil");

////////////导出函数////////////
function doLogic(req, res, body, pathName)
{
    httpUtil.response(res, "你好，世界！");
}

////////////导出元素////////////
exports.doLogic = doLogic;