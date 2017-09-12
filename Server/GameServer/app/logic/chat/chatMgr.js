"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var logUtil = require("../../libs/logUtil");
var dateUtil = require("../../libs/dateUtil");
var Message = require("../../libs/message").Message;
var ModuleIds = require("./../netMessage/netMsgConst").ModuleIds;
var chatMessage = require("../netMessage/chatMessage");
var CmdIdsChat = require("./../netMessage/chatMessage").CmdIdsChat;
var NetMsgCodec = require("../../libs/netMsgCodec").NetMsgCodec;
var chatChannelTypes = require("./../enumType/chatDefine").chatChannelTypes;
var onlineRoleMgr = require("../role/onlineRoleMgr");

////////////模块内变量////////////

////////////内部函数////////////

////////////导出函数////////////
/**
 * 
 * @param {string} content - 内容
 * @param {number|Role} [target=0] - 目标主角ID，如果是0，就发送给所有在线人
 */
function sendSystemChatMsg(content, target)
{
    var role = null;
    if (Object.isNumber(target) && target != 0)
    {
        role = onlineRoleMgr.findRoleByHeroId(target);
        if (role == null)
            return;
    }
    else if (Object.isObject(target))
    {
        role = target;
    }

    let pushObj = new chatMessage.RecvChatMsgRes();
    let chatMsgObj = new chatMessage.ChatMsgItem();
    pushObj.channel = chatChannelTypes.system;
    pushObj.target = 0;
    pushObj.msg = chatMsgObj;
    chatMsgObj.heroId = 0;
    chatMsgObj.msg = content;
    chatMsgObj.name = "系统";
    chatMsgObj.time = dateUtil.getTimestamp();
    chatMsgObj.roleId = "";
    chatMsgObj.roleLv = 0;
    chatMsgObj.vipLv = 0;
    var netMsgObj = Message.newRequest(ModuleIds.MODULE_CHAT, CmdIdsChat.PUSH_RECV_CHAT_MSG, pushObj);
    var netMsgBuf = NetMsgCodec.encodeEx(netMsgObj);

    if (role != null)
    {
        role.sendBufDirectly(netMsgBuf, netMsgObj);
    }
    else
    {
        onlineRoleMgr.forEachOfHero(function(role) {
            role.sendBufDirectly(netMsgBuf, netMsgObj);
        }, true);
    }
}

var doInitCoroutine = Promise.coroutine(function * () {
    logUtil.info("聊天模块开始初始化...");

    logUtil.info("聊天模块完成初始化");
});

function doInit()
{
    return doInitCoroutine();
}

var doDestroyCoroutine = Promise.coroutine(function * () {
    logUtil.info("聊天模块开始销毁...");

    logUtil.info("聊天模块完成销毁");
});

function doDestroy()
{
    return doDestroyCoroutine();
}

exports.sendSystemChatMsg = sendSystemChatMsg;
exports.doInit = doInit;
exports.doDestroy = doDestroy;