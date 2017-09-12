"use strict";

////////////内部模块////////////
var queryUtil = require("querystring");
var urlUtil = require("url");
var http = require("http");
var https = require("https");

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var appUtil = require("./appUtil");
var logUtil = require("./logUtil");
var tea16 = require("./tea16");
var mime = require("./mime").types;

////////////模块内数据////////////
var someHttpErrDesc = {
    200: "<h1>200 OK</h1>",
    400: "<h1>400 Bad Request</h1>",
    403: "<h1>403 Forbidden</h1>",
    404: "<h1>404 Not Found</h1>",
    413: "<h1>413 Request Entity Too Large</h1>",
    500: "<h1>500 Internal Server Error</h1>"
};

////////////私有函数////////////
function isOKHttpStateCode(code)
{
    return Math.floor(code / 100) === 2;
}

////////////导出函数////////////
/**
 * 往HTTP客户端写数据
 * @param {http.ServerResponse} res
 * @param {*?} body
 * @param {number?} statusCode
 * @param {object?} headers - 注意这个值会被直接修改
 * @param {boolean?} teaEnc
 */
function response(res, body, statusCode, headers, teaEnc) {
    statusCode = statusCode || 200;
    headers = headers || {};

    var isBuffer = Buffer.isBuffer(body);
    if (isBuffer) {
        //不加密就这里加上内容类型
        if (!teaEnc && !("Content-Type" in headers))
            headers["Content-Type"] = mime["unknown"];
    }
    else {
        var isString = Object.isString(body);
        if (body === null || body === undefined) {
            body = someHttpErrDesc[statusCode] || "";
            isString = true;
        }
        else if (!isString) {
            body = JSON.stringify(body || {});
        }
        body = new Buffer(body);

        //不加密就这里加上内容类型
        if (!teaEnc)
        {
            if (!("Content-Type" in headers))
                headers["Content-Type"] = isString ? (isOKHttpStateCode(statusCode) ? mime["txt"] : mime["html"]) : mime["json"];
            headers["Content-Type"] += "; charset=utf-8";
        }
    }

    //考虑加密
    if (teaEnc) {
        tea16.encrypt(body);
        headers["Content-Type"] = mime["unknown"];
    }
    //服务器关闭中？让客户端快点关闭连接，不然服务器关不掉
    if (appUtil.isProcessExiting()) {
        headers["Connection"] = "close";
    }

    headers["Content-Length"] = body.length;
    res.writeHeader(statusCode, headers);
    res.end(body);
}

/**
 * 往HTTP客户端写数据
 * @param {http.ServerResponse} res
 * @param {*?} body
 * @param {number?} statusCode
 * @param {object?} headers
 */
function responseTEA(res, body, statusCode, headers) {
    response(res, body, statusCode, headers, true);
}

function getQueryObj(str) {
    return queryUtil.parse(str);
}

function getQueryObjFromTEA(buf) {
    return queryUtil.parse(tea16.decryptString(buf));
}

function getQueryObjFromUrl(urlstr) {
    return urlUtil.parse(urlstr, true).query;    //第二个参数true表示query是对象
}

function queryObjToString(obj) {
    return queryUtil.stringify(obj);
}

function queryObjToTEA(obj) {
    return tea16.encryptString(queryUtil.stringify(obj));
}

function jsonObjToTEA(obj) {
    return tea16.encryptString(JSON.stringify(obj));
}

function getJsonObjFromTEA(buf)
{
    try
    {
        return JSON.parse(tea16.decryptString(buf));
    }
    catch (e)
    {
        return null;
    }
}

/**
 * HTTP请求的回调
 * @callback httpRequestCallback
 * @param {Error} err
 * @param {string|object} res
 */

/**
 * 执行GET请求
 * @param {string} url
 * @param {httpRequestCallback} callback
 * @param {string?} [resultType=json] - 值可以是text(文本)、query(query string转的object)、json(json string转的object)、buffer（可能是二进制数据）
 * @param {object?} headers
 */
function doGet(url, callback, resultType, headers)
{
    var useHttps = url.toLowerCase().indexOf("https") === 0;
    var urlObj = urlUtil.parse(url);
    var options =
    {
        method:     "GET",
        host:       urlObj.host,
        hostname:   urlObj.hostname,
        port:       urlObj.port === undefined ? (useHttps ? 443 : 80) : urlObj.port,
        path:       urlObj.path,
        auth:       urlObj.auth
    };

    var req = (useHttps ? https : http).request(options, function(res)
    {
        //先不转码，直接接二进制
        var chunks = [];

        res.on("data", function (chunk)
        {
            chunks.push(chunk);
        });

        res.on("error", function(err)
        {
            callback(err, null);
        });

        res.on("end", function ()
        {
            var resBody = Buffer.concat(chunks);
            chunks = null;
            resultType = resultType || "json"; //默认为json
            switch (resultType)
            {
                case "json":
                    resBody = appUtil.parseJsonObj(resBody.toString());
                    break;
                case "query":
                    resBody = getQueryObj(resBody.toString());
                    break;
                case "text":
                    resBody = resBody.toString();
                    break;
            }

            if (isOKHttpStateCode(res.statusCode))
                callback(null, resBody);
            else
                callback(new Error("HTTP错误，错误码：" + res.statusCode), resBody);
        });
    });

    req.on("error", function(err)
    {
        //logUtil.warn("HTTP失败", err);
        callback(err, null);
    });

    //设置头
    for (var k in headers)
        req.setHeader(k, headers[k]);

    //开始传输
    req.end();
}

/**
 * 执行GET请求，不使用回调，则使用协程方式
 * @param {string} url
 * @param {string?} [resultType=json] - 值可以是text(文本)、query(query string转的object)、json(json string转的object)
 * @param {object?} headers
 */
function doGetCoroutine(url, resultType, headers)
{
    return new Promise(function (resolve, reject) {
        doGet(url, function (err, res) {
            if (err)
                reject(err);
            else
                resolve(res);
        }, resultType, headers);
    });
}

/**
 * 执行POST请求
 * @param {string} url
 * @param {*} body，如果是Object，默认当作JSON对象，Buffer和String的话，就直接发送，其它类型也转成JSON
 * @param {httpRequestCallback} callback
 * @param {string?} [resultType=json] - 值可以是text(文本)、query(query string转的object)、json(json string转的object)、buffer（可能是二进制数据）
 * @param {object?} headers
 */
function doPost(url, body, callback, resultType, headers)
{
    if(!Object.isString(body) && !Buffer.isBuffer(body))
        body = JSON.stringify(body);

    var useHttps = url.toLowerCase().indexOf("https") === 0;
    var urlObj = urlUtil.parse(url);
    var options =
    {
        method:     "POST",
        host:       urlObj.host,
        hostname:   urlObj.hostname,
        port:       urlObj.port === undefined ? (useHttps ? 443 : 80) : urlObj.port,
        path:       urlObj.path,
        auth:       urlObj.auth
    };

    var req = (useHttps ? https : http).request(options, function(res)
    {
        //先不转码，直接接二进制
        var chunks = [];

        res.on("data", function (chunk)
        {
            chunks.push(chunk);
        });

        res.on("error", function(err)
        {
            callback(err, null);
        });

        res.on("end", function ()
        {
            var resBody = Buffer.concat(chunks);
            chunks = null;
            resultType = resultType || "json"; //默认为json
            switch (resultType)
            {
                case "json":
                    resBody = appUtil.parseJsonObj(resBody.toString());
                    break;
                case "query":
                    resBody = getQueryObj(resBody.toString());
                    break;
                case "text":
                    resBody = resBody.toString();
                    break;
            }

            if (isOKHttpStateCode(res.statusCode))
                callback(null, resBody);
            else
                callback(new Error("HTTP错误，地址：" + url + "，错误码：" + res.statusCode), resBody);
        });
    });

    req.on("error", function(err)
    {
        logUtil.warn("HTTP失败", err);
        callback(err, null);
    });

    //设置头
    headers = headers || {};
    headers["Content-Type"] = mime["query"];
    headers["Content-Length"] = Buffer.isBuffer(body) ? body.length : Buffer.byteLength(body);
    for (var k in headers)
        req.setHeader(k, headers[k]);

    //开始传输
    req.end(body,"utf8");
}

/**
 * 执行POST请求，不使用回调，则使用协程方式
 * @param {string} url
 * @param {*} body
 * @param {string?} [resultType="json"] - 值可以是text(文本)、query(query string转的object)、json(json string转的object)
 * @param {object?} headers
 */
function doPostCoroutine(url, body, resultType, headers)
{
    return new Promise(function (resolve, reject) {
        doPost(url, body, function (err, res) {
            if (err)
                reject(err);
            else
                resolve(res);
        }, resultType, headers);
    });
}

////////////全局执行////////////
//忽略证书无效
process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";

////////////导出元素////////////
exports.response = response;
exports.responseTEA = responseTEA;
exports.getQueryObj = getQueryObj;
exports.getQueryObjFromTEA = getQueryObjFromTEA;
exports.getQueryObjFromUrl = getQueryObjFromUrl;
exports.getJsonObjFromTEA = getJsonObjFromTEA;
exports.queryObjToString = queryObjToString;
exports.queryObjToTEA = queryObjToTEA;
exports.jsonObjToTEA = jsonObjToTEA;
exports.doGet = doGet;
exports.doPost = doPost;
exports.doGetCoroutine = doGetCoroutine;
exports.doPostCoroutine = doPostCoroutine;