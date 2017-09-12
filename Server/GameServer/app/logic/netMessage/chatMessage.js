"use strict";

var netMsgConst = require("./netMsgConst");

//值不可以为0
const CmdIdsChat = {
    CMD_SEND_CHAT_MSG: 1,       //发送聊天给别人
    CMD_CREATE_WHISPER: 2,      //主动建立私聊
    CMD_REQ_ROLE_ONLINE: 3,     //批量获取聊天对象是否在线
    PUSH_RECV_CHAT_MSG: -1,     //聊天消息发给客户端
};

const ResultCodeChat = {
    TARGET_NOT_ONLINE: 1,       //对方不在线
    HORN_NUM_LACK: 2,           //喇叭数不足
    MUST_JOIN_CORPS: 3,         //必须加入一个公会
    MSG_CANNOT_EMPTY: 4,        //聊天内容不能为空
    ROLE_LEVEL_LOW: 5,          //角色等级不够
    MUST_JOIN_TEAM: 6,          //必须加入一个队伍
};

/////////////////////////////////请求类////////////////////////////
class SendChatMsgReq
{
    constructor() {
        this.channel = 0;
        this.target = 0;
        this.content = "";
    }

    static fieldsDesc() {
        return {
            channel: {type: Number, notNull: true},
            target: {type: Number, notNull: true},
            content: {type: String, notNull: true},
        };
    }
}

class CreateWhisperReq
{
    constructor() {
        this.heroId = 0;
    }

    static fieldsDesc() {
        return {
            heroId: {type: Number, notNull: true},
        };
    }
}

class RoleOnlineStateReq
{
    constructor() {
        /**
         *
         * @type {number[]}
         */
        this.heroIds = [];
    }

    static fieldsDesc() {
        return {
            heroIds: {type: Array, itemType: Number, notNull: true},
        };
    }
}

/////////////////////////////////回复类////////////////////////////
class SendChatMsgRes
{
    constructor(cxt) {
        /**
         * 上下文，字符串形式
         * @type {string}
         */
        this.cxt = cxt == null ? "" : cxt.toString();
    }
}

class WhisperRoleInfo
{
    constructor() {
        this.heroId = 0;
        this.name = "";
        this.roleId = "";
        this.rolelv = 0;
        this.viplv = 0;
        this.online = false;
    }
}

/////////////////////////////////推送类////////////////////////////
class ChatMsgItem
{
    constructor() {
        this.heroId = 0;
        this.msg = "";
        this.name = "";
        this.time = 0;
        this.roleId = "";
        this.roleLv = 0;
        this.vipLv = 0;
    }
}

class RecvChatMsgRes
{
    constructor() {
        this.channel = 0;
        this.target = 0;
        /**
         *
         * @type {ChatMsgItem}
         */
        this.msg = null;
    }
}

class RoleOnlineStateRes
{
    constructor() {
        /**
         *
         * @type {object.<number,number>}
         */
        this.states = {};
    }
}

/////////////////////////////////全局执行////////////////////////////
var noSendCmdIds = [
    CmdIdsChat.PUSH_RECV_CHAT_MSG
];
netMsgConst.addServerNoResend(netMsgConst.ModuleIds.MODULE_CHAT, noSendCmdIds);

/////////////////////////////////导出元素////////////////////////////
exports.CmdIdsChat = CmdIdsChat;
exports.ResultCodeChat = ResultCodeChat;

exports.SendChatMsgReq = SendChatMsgReq;
exports.CreateWhisperReq = CreateWhisperReq;
exports.RoleOnlineStateReq = RoleOnlineStateReq;

exports.SendChatMsgRes = SendChatMsgRes;
exports.WhisperRoleInfo = WhisperRoleInfo;

exports.ChatMsgItem = ChatMsgItem;
exports.RecvChatMsgRes = RecvChatMsgRes;
exports.RoleOnlineStateRes = RoleOnlineStateRes;