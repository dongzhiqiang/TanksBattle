"use strict";

var appUtil = require("../../libs/appUtil");
var logUtil = require("../../libs/logUtil");
var ProtocolCoder = require("../../libs/protocolCoder");
var ResultCode = require("../netMessage/netMsgConst").ResultCode;

class HandlerInfo{
    constructor(func, bodyType, kickIfBadBody, allowBodyNull)
    {
        this.func = func;
        this.bodyType = bodyType;
        this.kickIfBadBody = kickIfBadBody;
        this.allowBodyNull = allowBodyNull;
    }
}

/**
 *
 * @type {object.<number, object.<number, HandlerInfo>>}
 */
var handlerMap = {};

/**
 *
 * @param {number} module
 * @param {number} cmd
 * @param {function} func
 * @param {function?} [bodyType=null]
 * @param {boolean?} [kickIfBadBody=false] - 仅bodyType不为null时有效，如果body类型验正失败，是否踢出去？
 * @param {boolean?} [allowBodyNull=false] - 仅bodyType不为null时有效，如果body为null，是否接受
 */
function registerHandler(module, cmd, func, bodyType, kickIfBadBody, allowBodyNull)
{
    var cmdFuncMap = handlerMap[module];
    if (!cmdFuncMap)
    {
        cmdFuncMap = {};
        handlerMap[module] = cmdFuncMap;
    }
    cmdFuncMap[cmd] = new HandlerInfo(func, bodyType, kickIfBadBody, allowBodyNull);

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
 * 如果是马上返回的消息，可以用这个简化的接口
 * @param module
 * @param cmd
 * @param func 成功执行则返回一个要发送给客户端的对象，否则返回错误码(Number)
 * @param bodyType
 * @param kickIfBadBody
 * @param allowBodyNull
 */
function registerHandlerEx(module, cmd, func, bodyType, kickIfBadBody, allowBodyNull){
    registerHandler(module, cmd, (session, role, msgObj, req)=>{
        let res = func(session, role, msgObj, req);
        if(res===null || res===undefined)
            throw new Error("没有返回值 module:"+module+" cmd:"+cmd);
        else if(Object.isNumber(res)){//出错了，那么res为错误码
            msgObj.setResponseData(res);
            if (role)
                role.send(msgObj);
            else
                session.send(msgObj);
        }
        else if(Object.isObject(res))
        {
            msgObj.setResponseData(ResultCode.SUCCESS, res);
            if (role)
                role.send(msgObj);
            else
                session.send(msgObj);
        }
        else
            throw new Error("返回值不是错误码或者对象 module:"+module+" cmd:"+cmd);

    }, bodyType, kickIfBadBody, allowBodyNull)
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
 * @param {ClientSession} session
 * @param {Message} msgObj
 */
function dispatchNetMsg(session, msgObj)
{
    var module = msgObj.getModule();
    var cmd = msgObj.getCommand();
    var cmdFuncMap = handlerMap[module];
    if (!cmdFuncMap)
    {
        logUtil.warn("dispatchNetMsg，找不到模块：" + module);
        msgObj.setResponseData(ResultCode.BAD_REQUEST);
        session.send(msgObj);
        return;
    }

    var handlerInfo = cmdFuncMap[cmd];
    if (!handlerInfo)
    {
        logUtil.warn("dispatchNetMsg，找不到命令：" + cmd);
        msgObj.setResponseData(ResultCode.BAD_REQUEST);
        session.send(msgObj);
        return;
    }

    try {
        var role = session.getRole();
        var bodyObj = msgObj.getBodyObj();

        //检查参数，设置了类型才检查参数
        if (handlerInfo.bodyType) {
            var isBadBody = false;
            if (!bodyObj) {
                //如果允许null，那就不算类型不对
                if (!handlerInfo.allowBodyNull)
                    isBadBody = true;
            }
            else {
                //源数据是json时，就按字段验证数据
                if (msgObj.isJsonBody())
                    isBadBody = !appUtil.validateObjectFields(bodyObj, handlerInfo.bodyType);
                //源数据是自定义二进制时，就直接判断最外层类型就行了，里层在反串行化时就验证了
                else
                    isBadBody = bodyObj.constructor !== handlerInfo.bodyType;
            }

            //如果参数不对，那就下发提示
            if (isBadBody) {
                msgObj.setResponseData(ResultCode.BAD_PARAMETER);
                //如果要踢出去，那就发完就踢
                if (handlerInfo.kickIfBadBody)
                    session.kickSession("通讯消息错误", false);
                else
                    session.send(msgObj);
                return;
            }
        }

        handlerInfo.func(session, role, msgObj, bodyObj);
    }
    catch (err) {
        logUtil.error("dispatchNetMsg", err);
        //把请求对象转为回应对象
        msgObj.setResponseData(ResultCode.SERVER_ERROR);
        //一般用role来发，因为有可能role的session失效，这样就可以把消息发到延时队列
        if (role)
            role.send(msgObj);
        else
            session.send(msgObj);
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
exports.registerHandlerEx = registerHandlerEx;