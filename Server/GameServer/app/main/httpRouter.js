"use strict";

////////////我的模块////////////
var logUtil     = require("../libs/logUtil");
var httpUtil    = require("../libs/httpUtil");

////////////我的逻辑模块////////////
var defaultLogic    = require("../logic/http/default");
var adminHandler    = require("../logic/http/admin");

////////////模块内数据////////////
/**
 * HTTP处理函数
 * @callback HttpHandlerFunction
 * @param {http.IncomingMessage} req - 请求对象
 * @param {http.ServerResponse} res - 回应对象
 * @param {Buffer|string} body - 请求内容
 * @param {string} pathName - 请求路径
 */

/**
 * HTTP处理对象
 * @typedef {Object} HttpHandlerObject
 * @property {HttpHandlerFunction} doLogic - 处理函数
 * @property {boolean} binaryBody - 表明body是不是字符串，如果是，那就要转换成字符串再传给doLogic
 */

/**
 * 普通路由
 * @type {object.<string, HttpHandlerObject|HttpHandlerFunction>}
 */
var routerNormalCfg    =
{
    "/"                 : defaultLogic,
    "/admin"            : adminHandler,
    "/urlCounter"       : printCounter
};

/**
 * 正则路由元素类型
 * @typedef {Object} RegexRouterItem
 * @property {RegExp} reg - 区别规则
 * @property {HttpHandlerObject|HttpHandlerFunction} proc - 处理函数
 */

/**
 * 正则路由
 * @type {RegexRouterItem[]}
 */
var routerRegexCfg =
[
];

/**
 * 各URL请求计数
 * @type {object.<string, number>}
 */
var reqCounter = {};

////////////私有函数////////////
//打印请求计数
function printCounter(req, res, body, pathName)
{
    var msg = {};
    msg.ok  = true;
    msg.msg = "success";
    msg.data= reqCounter;
    httpUtil.response(res, msg);
}

////////////导出函数////////////
/**
 * 路由函数
 * @param {http.IncomingMessage} req
 * @param {http.ServerResponse} res
 * @param {Buffer|string} body
 */
function doLogic(req, res, body)
{
    var qmPos = req.url.indexOf("?");
    qmPos = qmPos < 0 ? req.url.length : qmPos;
    var pathName = req.url.substring(0, qmPos);

    logUtil.debug("路径名：" + pathName);

    //普通路由
    var proc = routerNormalCfg[pathName];

    //正则路由
    if (proc === null || proc === undefined)
    {
        for (var i = 0; i < routerRegexCfg.length; ++i)
        {
            var item = routerRegexCfg[i];
            if (item.reg.test(pathName))
            {
                proc = item.proc;
                break;
            }
        }
    }

    //不存在的地址
    if (proc === null || proc === undefined)
    {
        httpUtil.response(res, null, 404);
    }
    else
    {
        //如果不强调要二进制数据，就自动转换为字符串
        if (!proc.binaryBody)
        {
            body = body.toString();
            logUtil.debug("提交内容：" + body);
        }

        //如果是对象，就取函数，如果是函数就直接调试
        if (proc.doLogic)
            proc = proc.doLogic;

        //调用逻辑
        proc(req, res, body, pathName);

        //请求计数
        reqCounter[pathName] = (reqCounter[pathName] || 0) + 1;
    }
}

////////////导出元素////////////
exports.doLogic = doLogic;

////////////说明////////////