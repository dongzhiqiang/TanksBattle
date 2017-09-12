"use strict";

var appUtil = require("../../libs/appUtil");
var logUtil = require("../../libs/logUtil");
var dateUtil = require("../../libs/dateUtil");
var enProp = require("../enumType/propDefine").enProp;
var handlerMgr = require("../session/handlerMgr");
var ModuleIds = require("./../netMessage/netMsgConst").ModuleIds;
var ResultCode = require("./../netMessage/netMsgConst").ResultCode;
var chatMessage = require("../netMessage/chatMessage");
var CmdIdsChat = require("./../netMessage/chatMessage").CmdIdsChat;
var ResultCodeChat = require("./../netMessage/chatMessage").ResultCodeChat;
var corpsMgr = require("../corps/corpsMgr");
var onlineRoleMgr = require("../role/onlineRoleMgr");
var NetMsgCodec = require("../../libs/netMsgCodec").NetMsgCodec;
var Message = require("../../libs/message").Message;
var chatChannelTypes = require("./../enumType/chatDefine").chatChannelTypes;
var valueConfig = require("../gameConfig/valueConfig");

///////////////////////////////////
/**
 * 世界频道需要的等级
 * @type {number}
 */
const WORLD_CHANNEL_ROLE_LEVEL = 10;

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {SendChatMsgReq} reqObj
 */
function sendChatMsg(session, role, msgObj, reqObj)
{
    var targetRole = null;
    var corpsData = null;
    var channel = reqObj.channel;
    switch (channel)
    {
        case chatChannelTypes.whisper:
            {
                targetRole = onlineRoleMgr.findRoleByHeroId(reqObj.target);
                if (targetRole == null || targetRole.isOfflineSeemingly())
                {
                    msgObj.setResponseData(ResultCodeChat.TARGET_NOT_ONLINE, new chatMessage.SendChatMsgRes(reqObj.target));
                    role.send(msgObj);
                    return;
                }
            }
            break;
        case chatChannelTypes.world:
            {
                var roleLv = role.getNumber(enProp.level);
                if (roleLv < valueConfig.getWorldChatLevel())
                {
                    msgObj.setResponseData(ResultCodeChat.ROLE_LEVEL_LOW);
                    role.send(msgObj);
                    return;
                }

                var hornNum = role.getNumber(enProp.hornNum);
                if (hornNum <= 0)
                {
                    msgObj.setResponseData(ResultCodeChat.HORN_NUM_LACK);
                    role.send(msgObj);
                    return;
                }
            }
            break;
        case chatChannelTypes.corps:
            {
                var corpsId = role.getNumber(enProp.corpsId);
                if (corpsId == 0)
                {
                    msgObj.setResponseData(ResultCodeChat.MUST_JOIN_CORPS);
                    role.send(msgObj);
                    return;
                }

                corpsData = corpsMgr.getCorpsData(corpsId);
                if (corpsData == null)
                {
                    msgObj.setResponseData(ResultCodeChat.MUST_JOIN_CORPS);
                    role.send(msgObj);
                    return;
                }
            }
            break;
        case chatChannelTypes.team:
            {
                //暂时没有队伍系统，直接返回报错
                msgObj.setResponseData(ResultCodeChat.MUST_JOIN_TEAM);
                role.send(msgObj);
                return;
            }
            break;
    }

    if (reqObj.content == null || reqObj.content.trim().length <= 0)
    {
        //暂时没有队伍系统，直接返回报错
        msgObj.setResponseData(ResultCodeChat.MSG_CANNOT_EMPTY);
        role.send(msgObj);
        return;
    }

    let pushObj = new chatMessage.RecvChatMsgRes();
    let chatMsgObj = new chatMessage.ChatMsgItem();
    pushObj.channel = reqObj.channel;
    pushObj.target = reqObj.target;
    pushObj.msg = chatMsgObj;
    chatMsgObj.heroId = role.getHeroId();
    chatMsgObj.msg = reqObj.content;
    chatMsgObj.name = role.getString(enProp.name);
    chatMsgObj.time = dateUtil.getTimestamp();
    chatMsgObj.roleId = role.getString(enProp.roleId);
    chatMsgObj.roleLv = role.getNumber(enProp.level);
    chatMsgObj.vipLv = role.getNumber(enProp.vipLv);
    var netMsgObj = Message.newRequest(ModuleIds.MODULE_CHAT, CmdIdsChat.PUSH_RECV_CHAT_MSG, pushObj);
    var netMsgBuf = NetMsgCodec.encodeEx(netMsgObj);

    //回发给自己
    role.sendBufDirectly(netMsgBuf, netMsgObj);

    //发给别人
    switch (channel)
    {
        case chatChannelTypes.whisper:
            {
                if (targetRole != role)
                    targetRole.sendBufDirectly(netMsgBuf, netMsgObj);
            }
            break;
        case chatChannelTypes.world:
            {
                //扣道具
                role.addNumber(enProp.hornNum, -1);
                onlineRoleMgr.forEachOfHero(function(targetRole) {
                    if (targetRole != role)
                        targetRole.sendBufDirectly(netMsgBuf, netMsgObj);
                }, true);
            }
            break;
        case chatChannelTypes.corps:
            {
                var members = corpsData.members;
                for (var i = 0, len = members.length; i < len; ++i)
                {
                    var heroId = members[i].heroId;
                    targetRole = onlineRoleMgr.findRoleByHeroId(heroId);
                    if (targetRole != null && targetRole != role)
                        targetRole.sendBufDirectly(netMsgBuf, netMsgObj);
                }
            }
            break;
    }

    msgObj.setResponseData(ResultCode.SUCCESS);
    role.send(msgObj);
}
handlerMgr.registerHandler(ModuleIds.MODULE_CHAT, CmdIdsChat.CMD_SEND_CHAT_MSG, sendChatMsg, chatMessage.SendChatMsgReq, true);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {CreateWhisperReq} reqObj
 */
function createWhisper(session, role, msgObj, reqObj)
{
    var targetRole = onlineRoleMgr.findRoleByHeroId(reqObj.heroId);
    if (targetRole == null || targetRole.isOfflineSeemingly())
        return ResultCodeChat.TARGET_NOT_ONLINE;

    var roleInfo = new chatMessage.WhisperRoleInfo();
    roleInfo.heroId = targetRole.getHeroId();
    roleInfo.name = targetRole.getString(enProp.name);
    roleInfo.roleId = targetRole.getString(enProp.roleId);
    roleInfo.rolelv = targetRole.getNumber(enProp.rolelv);
    roleInfo.viplv = targetRole.getNumber(enProp.vipLv);
    roleInfo.online = true;
    return roleInfo;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_CHAT, CmdIdsChat.CMD_CREATE_WHISPER, createWhisper, chatMessage.CreateWhisperReq, true);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {RoleOnlineStateReq} reqObj
 */
function reqRoleOnline(session, role, msgObj, reqObj)
{
    var retObj = new chatMessage.RoleOnlineStateRes();
    retObj.states = {};

    var heroIds = reqObj.heroIds;
    for (var i = 0, len = heroIds.length; i < len; ++i)
    {
        var heroId = heroIds[i];
        var targetRole = onlineRoleMgr.findRoleByHeroId(heroId);
        var isOffline = targetRole == null || targetRole.isOfflineSeemingly();
        retObj.states[heroId] = isOffline ? 0 : 1;
    }

    return retObj;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_CHAT, CmdIdsChat.CMD_REQ_ROLE_ONLINE, reqRoleOnline, chatMessage.RoleOnlineStateReq, true);