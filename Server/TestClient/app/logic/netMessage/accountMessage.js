"use strict";

var netMsgConst = require("./netMsgConst");

//值不可以为0
const CmdIdsAccount = {
    CMD_LOGIN: 1,           //登录
    CMD_CREATE_ROLE: 2,     //创建角色
    CMD_ACTIVATE_ROLE: 3,   //激活角色
    CMD_LOGOUT: 4,          //主动注销登录，这个不用回复
    CMD_RELOGIN: 5,         //断线自动重新登录
    CMD_TEST_ECHO:6,        //收到包后马上发回，性能测试用
    CMD_SERVER_TIME:7,      //请求服务器的虚拟时间
    CMD_PING: 8,            //客户端PING服务端

    PUSH_FORCE_LOGOUT: -1,  //踢下线
    PUSH_TIP_MSG: -2,       //通用提示消息
    PUSH_SERVER_TIME: -3,   //同步虚拟时间
    PUSH_PING: -4,          //服务端PING客户端
};

const ResultCodeAccount = {
    CHECK_TOKEN_FAIL: 1,    //一般要跳到登录界面，因为token无效的，必须重新登录
    RELOGIN_FAIL: 2,    //一般只要跳回服务器选择界面
    ROLE_DATA_LOST: 3,    //角色数据丢失
};

/////////////////////////////////请求类////////////////////////////
class LoginRequestVo {
    constructor() {
        this.channelId = "";           //渠道Id
        this.userId = "";           //用户Id
        this.token = "";           //登录验证用的token
        this.serverId = 0;            //服务器Id
        this.clientVer = "";           //客户端版本
        this.lang = "";           //客户端语言
        this.deviceModel = "";           //设备型号
        this.osName = "";           //系统名称
        this.root = 0;            //是否越狱
        this.macAddr = "";           //硬件地址
        this.network = "";           //联网类型
        this.screenWidth = 0;            //屏幕宽
        this.screenHeight = 0;            //屏幕高
    }

    static fieldsDesc() {
        return {
            channelId: {type: String, notNull: true},
            userId: {type: String, notNull: true},
            token: {type: String, notNull: true},
            serverId: {type: Number},
            clientVer: {type: String},
            lang: {type: String},
            deviceModel: {type: String},
            osName: {type: String},
            root: {type: Number},
            macAddr: {type: String},
            network: {type: String},
            screenWidth: {type: Number},
            screenHeight: {type: Number}
        };
    }
}

class ReloginRequestVo {
    constructor() {
        this.channelId = "";           //渠道Id
        this.userId = "";           //用户Id
        this.token = "";           //登录验证用的token
        this.heroId = 0;            //主角Id
        this.lastLogin = 0;         //最后登录时间，用于校验内存里的那个Role确实是自己离线时那个Role
        this.clientVer = "";           //客户端版本
        this.lang = "";           //客户端语言
        this.osName = "";           //系统名称
        this.network = "";           //联网类型
    }

    static fieldsDesc() {
        return {
            channelId: {type: String, notNull: true},
            userId: {type: String, notNull: true},
            token: {type: String, notNull: true},
            heroId: {type: Number, notNull: true},
            lastLogin: {type: Number, notNull: true},
            clientVer: {type: String},
            lang: {type: String},
            osName: {type: String},
            network: {type: String}
        };
    }
}

class CreateRoleRequestVo {
    constructor() {
        this.roleId = "";                   //角色类型ID
        this.name = "";                     //角色名
    }

    static fieldsDesc() {
        return {
            roleId: {type: String, notNull: true},
            name: {type: String, notNull: true}
        };
    }
}

class ActivateRoleRequestVo {
    constructor(heroId) {
        this.heroId = heroId;
    }

    static fieldsDesc() {
        return {
            heroId: {type: Number, notNull: true}
        };
    }
}

/////////////////////////////////回复类////////////////////////////
class RoleBriefVo
{
    constructor(guid, name, level, roleId, heroId) {
        this.guid = guid;
        this.name = name;
        this.level = level;
        this.roleId = roleId;
        this.heroId = heroId;
    }

    static fieldsDesc() {
        return {
            guid: {type: String, notNull: true},
            name: {type: String, notNull: true},
            level: {type: Number, notNull: true},
            roleId: {type: String, notNull: true},
            heroId: {type: Number, notNull: true}
        };
    }
}

class RoleListVo {
    constructor(roleList) {
        this.roleList = roleList;
    }

    static fieldsDesc() {
        return {
            roleList: {type: Array, itemType: RoleBriefVo, notNull: true}
        };
    }
}

/////////////////////////////////推送类////////////////////////////
class LoginTipMsgVo {
    constructor(msg) {
        this.msg = msg;
    }
}

class ForceLogoutVo {
    /**
     *
     * @param {string|null} msg
     * @param {number?} type
     */
    constructor(msg, type) {
        this.msg = msg;
        this.type = type;
    }

    static fieldsDesc() {
        return {
            msg: {type: String, notNull: false},
            type: {type: Number, notNull: true}
        };
    }
}
ForceLogoutVo.GOTO_SVR_SELECT = 0;    //客户端收到消息后关闭连接，跳到服务器选择界面
ForceLogoutVo.GOTO_LOGIN_UI = 1;    //客户端收到消息后关闭连接，跳到登录界面
ForceLogoutVo.KEEP_CURRENT = 2;    //客户端收到消息后关闭连接，保持当前界面，界面跳转靠别的消息

class SyncServerTime {
    constructor(time, tzOffset) {
        this.time = time;
        this.tzOffset = tzOffset;
    }
}

/////////////////////////////////全局执行////////////////////////////
var noSendCmdIds = [CmdIdsAccount.PUSH_FORCE_LOGOUT,
                    CmdIdsAccount.PUSH_TIP_MSG,
                    CmdIdsAccount.CMD_SERVER_TIME,
                    CmdIdsAccount.PUSH_SERVER_TIME,
                    CmdIdsAccount.PUSH_PING];
netMsgConst.addServerNoResend(netMsgConst.ModuleIds.MODULE_ACCOUNT, noSendCmdIds);

/////////////////////////////////导出元素////////////////////////////
exports.CmdIdsAccount = CmdIdsAccount;
exports.ResultCodeAccount = ResultCodeAccount;

exports.LoginRequestVo = LoginRequestVo;
exports.ReloginRequestVo = ReloginRequestVo;
exports.CreateRoleRequestVo = CreateRoleRequestVo;
exports.ActivateRoleRequestVo = ActivateRoleRequestVo;

exports.RoleBriefVo = RoleBriefVo;
exports.RoleListVo = RoleListVo;

exports.LoginTipMsgVo = LoginTipMsgVo;
exports.ForceLogoutVo = ForceLogoutVo;
exports.SyncServerTime = SyncServerTime;