"use strict";

////////////我的模块////////////
var appCfg = require("../../../config");
var appUtil = require("../../libs/appUtil");
var httpUtil = require("../../libs/httpUtil");

////////////导出函数////////////
function doLogic(req, res, body, pathName)
{
    var query = httpUtil.getQueryObjFromUrl(req.url);
    if (!query.key || query.key !== appCfg.adminKey || !query.action)
    {
        httpUtil.response(res, null, 403);
        return;
    }
    switch (query.action)
    {
    case "exit":
        {
            //这里一定要放在前面，因为后面的response发现关闭中时，会用Connection: close来让客户端不要保持连接了
            appUtil.fireProcessExiting();
            httpUtil.response(res, {code:0, msg:"OK"});
        }
        break;
    }
}

////////////导出元素////////////
exports.doLogic = doLogic;