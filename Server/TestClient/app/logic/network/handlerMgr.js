"use strict";

var logUtil = require("../../libs/logUtil");
var ProtocolCoder = require("../../libs/protocolCoder");

/**
 *
 * @type {object.<number, object.<number, function>>}
 */
var handlerMap = {};

/**
 *
 * @param {number} module
 * @param {number} cmd
 * @param {function} func
 * @param {function?} [bodyType=null]
 */
function registerHandler(module, cmd, func, bodyType)
{
    var cmdFuncMap = handlerMap[module];
    if (!cmdFuncMap)
    {
        cmdFuncMap = {};
        handlerMap[module] = cmdFuncMap;
    }
    cmdFuncMap[cmd] = func;

    //主要用于自定义的二进制串行化
    if (ProtocolCoder.instance.canRegisterType(bodyType))
    {
        ProtocolCoder.instance.registerType(bodyType);
    }
    else if (ProtocolCoder.instance.canRegisterEnum(bodyType))
    {
        ProtocolCoder.instance.registerEnum(bodyType);
    }
}

/**
 *
 * @param {number} module
 * @param {number} cmd
 */
function unregisterHandler(module, cmd)
{
    var cmdFuncMap = handlerMap[module];
    if (!cmdFuncMap)
        return;

    delete cmdFuncMap[cmd];
}

/**
 *
 * @param {Player} player
 * @param {Message} msgObj
 */
function dispatchNetMsg(player, msgObj)
{
    var module = msgObj.getModule();
    var cmd = msgObj.getCommand();
    var cmdFuncMap = handlerMap[module];
    if (!cmdFuncMap)
    {
        logUtil.warn("dispatchNetMsg，找不到模块：" + module);
        return;
    }

    var func = cmdFuncMap[cmd];
    if (!func)
    {
        logUtil.warn("dispatchNetMsg，模块：" + module + "，找不到命令：" + cmd);
        return;
    }

    var code = msgObj.getErrorCode();
    var errMsg = msgObj.getErrorMsg();
    var body = msgObj.getBodyObj();
    try {
        func(player, code, errMsg, body);
    }
    catch (err) {
        logUtil.error("dispatchNetMsg", err);
    }
}

function clearHandler()
{
    handlerMap = {};
}

exports.registerHandler = registerHandler;
exports.unregisterHandler = unregisterHandler;
exports.dispatchNetMsg = dispatchNetMsg;
exports.clearHandler = clearHandler;