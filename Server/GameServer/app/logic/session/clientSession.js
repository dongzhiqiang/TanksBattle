"use strict";

////////////外部模块////////////

////////////我的模块////////////
var appCfg = require("../../../config");
var dateUtil = require("../../libs/dateUtil");
var logUtil = require("../../libs/logUtil");
var Message = require("../../libs/message").Message;
var sessionMgr = require("./sessionMgr");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var ResultCode = require("../netMessage/netMsgConst").ResultCode;
var accountMsg = require("../netMessage/accountMessage");
var CmdIdsAccount = require("../netMessage/accountMessage").CmdIdsAccount;
var handlerMgr = require("./handlerMgr");

////////////导出类////////////
class AccountInfo
{
    constructor(channelId, userId, network, osName, clientVer, lang, deviceModel, root, macAddr, screenWidth, screenHeight)
    {
        this.channelId = channelId;
        this.userId = userId;
        this.network = network;
        this.osName = osName;
        this.clientVer = clientVer;
        this.lang = lang;
        this.deviceModel = deviceModel;
        this.root = root;
        this.macAddr = macAddr;
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;
    }
}

class ClientSession
{
    /**
     *
     * @param {number} sessionId - 会话ID
     * @param {Connection} conn
     */
    constructor(sessionId, conn)
    {
        this._sessionId = sessionId;
        this._conn = conn;
        this._conn.setHandler(this);
        /**
         *
         * @type {Role|null}
         * @private
         */
        this._role = null;
        this._isAuth = false;
        /**
         *
         * @type {AccountInfo}
         * @private
         */
        this._accountInfo = null;
        /**
         * 活跃时间
         * @type {number}
         * @private
         */
        this._activeTime = 0;
        /**
         * 上次服务器推送Ping的时间
         * @type {number}
         * @private
         */
        this._lastSvrPing = 0;
    }

    getSessionId()
    {
        return this._sessionId;
    }

    /**
     *
     * @returns {Role}
     */
    getRole()
    {
        return this._role;
    }

    /**
     *
     * @returns {number}
     */
    getActiveTime()
    {
        return this._activeTime;
    }

    /**
     *
     * @returns {number}
     */
    getLastSvrPing()
    {
        return this._lastSvrPing;
    }

    /**
     *
     * @returns {string}
     */
    getRemoteAddress()
    {
        return this._conn ? this._conn.getRemoteAddress() : "";
    }

    /**
     *
     * @returns {number}
     */
    getRemotePort()
    {
        return this._conn ? this._conn.getRemotePort() : 0;
    }

    /**
     *
     * @returns {string}
     */
    getRemoteFamily()
    {
        return this._conn ? this._conn.getRemoteFamily() : "";
    }

    /**
     *
     * @param {Role} role
     * @param {boolean?} [timerDestroy=false] - 是否要定时删除旧Role
     */
    setRole(role, timerDestroy)
    {
        //如果连接无效了，就不要设置非null的role了
        if (role && !this._conn)
            return;

        //这个连接如果有旧Role，清掉旧Role的连接
        var oldRole = this._role;
        if (oldRole && oldRole !== role)
            oldRole.setSession(null, timerDestroy);
        //要先赋值，因为setSession可能会导致网络出错，然后导致close回调里this._role不能被处理
        this._role = role;
        if (role)
            role.setSession(this);
    }

    /**
     *
     * @returns {boolean}
     */
    isAuth()
    {
        return this._isAuth;
    }

    /**
     *
     * @param {boolean} auth
     */
    setAuth(auth)
    {
        this._isAuth = auth;
    }

    /**
     *
     * @returns {AccountInfo}
     */
    getAccountInfo()
    {
        return this._accountInfo;
    }

    setAccountInfo(channelId, userId, network, osName, clientVer, lang, deviceModel, root, macAddr, screenWidth, screenHeight)
    {
        //本对象已无效？不继续了
        if (!this._conn)
            return;

        this._accountInfo = new AccountInfo(channelId, userId, network, osName, clientVer, lang, deviceModel, root, macAddr, screenWidth, screenHeight);
        sessionMgr.setSessionAccount(channelId, userId, this);
    }

    send(msgObj)
    {
        if (this._conn)
        {
            this._conn.send(msgObj);

            if (appCfg.debug)
            {
                var module = msgObj.getModule();
                var cmd = msgObj.getCommand();
                if (!(module === ModuleIds.MODULE_ACCOUNT && cmd === CmdIdsAccount.PUSH_PING))
                    logUtil.debug("发送数据，模块：" + module + "，命令：" + cmd + "，错误码：" + msgObj.getErrorCode() + "，内容：" + JSON.stringify(msgObj.getBodyObj()));
            }
        }
    }

    sendEx(module, cmd, body)
    {
        if (this._conn)
        {
            var msgObj = Message.newRequest(module, cmd, body);
            this._conn.send(msgObj);

            if (appCfg.debug)
            {
                if (!(module === ModuleIds.MODULE_ACCOUNT && cmd === CmdIdsAccount.PUSH_PING))
                    logUtil.debug("发送数据，模块：" + module + "，命令：" + cmd + "，错误码：" + msgObj.getErrorCode() + "，内容：" + JSON.stringify(msgObj.getBodyObj()));
            }
        }
    }

    /**
     * 如果向多个连接发送同样的数据，就不要重复各种写入、加密、压缩、生成校验码
     * @param {IOBuffer} ioBuf - 经过各种加长度、校验和、加密、压缩的buffer
     * @param {Message} msgObj - 为了能断线重连时重发，还是要带上这个，不过只是简单加入队列
     */
    sendBufDirectly(ioBuf, msgObj)
    {
        if (this._conn)
        {
            this._conn.sendBufDirectly(ioBuf, msgObj);

            if (appCfg.debug)
            {
                var module = msgObj.getModule();
                var cmd = msgObj.getCommand();
                if (!(module === ModuleIds.MODULE_ACCOUNT && cmd === CmdIdsAccount.PUSH_PING))
                    logUtil.debug("发送数据，模块：" + module + "，命令：" + cmd + "，错误码：" + msgObj.getErrorCode() + "，内容：" + JSON.stringify(msgObj.getBodyObj()));
            }
        }
    }

    /**
     * 注意：只给sessionMgr调用
     */
    close()
    {
        if (this._role)
        {
            this._role.release();
            this._role = null;
        }

        if (this._conn)
        {
            this._conn.close();
            this._conn = null;
        }
    }

    /**
     *
     * @param {Message} msgObj
     */
    onRecvData(msgObj)
    {
        if (appCfg.debug)
        {
            var module = msgObj.getModule();
            var cmd = msgObj.getCommand();
            if (!(module === ModuleIds.MODULE_ACCOUNT && cmd === CmdIdsAccount.PUSH_PING))
                logUtil.debug("收到数据，模块：" + module + "，命令：" + cmd + "，内容：" + JSON.stringify(msgObj.getBodyObj()));
        }

        //收到数据包，就记一下活跃时间
        this._activeTime = dateUtil.getTimestamp();

        if (!this._isAuth)
        {
            this.onRecvDataWhenNoAuth(msgObj);
            return;
        }

        if (!this._role)
        {
            this.onRecvDataWhenNoRole(msgObj);
            return;
        }

        this.onRecvDataNormal(msgObj);
    }

    sendThenKick(msgObj)
    {
        if (!this._conn)
            return;

        //先保存一下conn，因为有可能连接已失效，调用sendThenClose后会马上回调onConnClose，造成role没被清除
        var conn = this._conn;

        //让conn脱离与本对象的关联，让它自生自灭
        this._conn.setHandler(null);
        this._conn = null;

        //发送消息
        conn.send(msgObj);
        //发送强制下线消息，发送完关闭连接
        var bodyObj = new accountMsg.ForceLogoutVo(null, accountMsg.ForceLogoutVo.KEEP_CURRENT);
        msgObj = Message.newRequest(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.PUSH_FORCE_LOGOUT, bodyObj);
        conn.sendThenClose(msgObj);

        //这里让Mgr取消本Session的注册，由于conn已脱离关系，连接不会被关闭，而role会被销毁
        sessionMgr.delSession(this._sessionId);
    }

    sendThenKickEx(module, cmd, body)
    {
        var msgObj = Message.newRequest(module, cmd, body);
        this.sendThenKick(msgObj);
    }

    /**
     *
     * @param {string|null} reason
     * @param {boolean?} gotoLogin - 默认是跳到服务器选择界面
     */
    kickSession(reason, gotoLogin)
    {
        if (!this._conn)
            return;

        //先保存一下conn，因为有可能连接已失效，调用sendThenClose后会马上回调onConnClose，造成role没被清除
        var conn = this._conn;

        //让conn脱离与本对象的关联，让它自生自灭
        this._conn.setHandler(null);
        this._conn = null;

        //发送消息，发送完后才关闭连接
        var bodyObj = new accountMsg.ForceLogoutVo(reason, gotoLogin ? accountMsg.ForceLogoutVo.GOTO_LOGIN_UI : accountMsg.ForceLogoutVo.GOTO_SVR_SELECT);
        var msgObj = Message.newRequest(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.PUSH_FORCE_LOGOUT, bodyObj);
        conn.sendThenClose(msgObj);

        //这里让Mgr取消本Session的注册，由于conn已脱离关系，连接不会被关闭，而role会被销毁
        sessionMgr.delSession(this._sessionId);
    }

    sendLoginTipMsg(msg)
    {
        var retObj = new accountMsg.LoginTipMsgVo(msg);
        this.sendEx(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.PUSH_TIP_MSG, retObj);
    }

    /**
     *
     * @param {Message} msgObj
     */
    onRecvDataWhenNoAuth(msgObj)
    {
        if (msgObj.getModule() === ModuleIds.MODULE_ACCOUNT) {
            switch (msgObj.getCommand()) {
                case CmdIdsAccount.CMD_LOGIN:
                case CmdIdsAccount.CMD_RELOGIN:
                case CmdIdsAccount.CMD_PING:
                    handlerMgr.dispatchNetMsg(this, msgObj);
                    break;
                default:
                    msgObj.setResponseData(ResultCode.BAD_REQUEST);
                    this.send(msgObj);
                    break;
            }
        }
        else {
            msgObj.setResponseData(ResultCode.BAD_REQUEST);
            this.send(msgObj);
        }
    }

    /**
     *
     * @param {Message} msgObj
     */
    onRecvDataWhenNoRole(msgObj)
    {
        if (msgObj.getModule() === ModuleIds.MODULE_ACCOUNT) {
            switch (msgObj.getCommand()) {
                case CmdIdsAccount.CMD_DEMO_HERO_DATA:
                case CmdIdsAccount.CMD_CREATE_ROLE:
                case CmdIdsAccount.CMD_ACTIVATE_ROLE:
                case CmdIdsAccount.CMD_LOGOUT:
                case CmdIdsAccount.CMD_SERVER_TIME:
                case CmdIdsAccount.CMD_PING:
                    handlerMgr.dispatchNetMsg(this, msgObj);
                    break;
                default:
                    msgObj.setResponseData(ResultCode.BAD_REQUEST);
                    this.send(msgObj);
                    break;
            }
        }
        else {
            msgObj.setResponseData(ResultCode.BAD_REQUEST);
            this.send(msgObj);
        }
    }

    onRecvDataNormal(msgObj)
    {
        if (msgObj.getModule() === ModuleIds.MODULE_ACCOUNT) {
            switch (msgObj.getCommand()) {
                case CmdIdsAccount.CMD_LOGOUT:
                case CmdIdsAccount.CMD_TEST_ECHO:
                case CmdIdsAccount.CMD_SERVER_TIME:
                case CmdIdsAccount.CMD_PING:
                    handlerMgr.dispatchNetMsg(this, msgObj);
                    break;
                default:
                    msgObj.setResponseData(ResultCode.BAD_REQUEST);
                    this.send(msgObj);
                    break;
            }
        }
        else {
            handlerMgr.dispatchNetMsg(this, msgObj);
        }
    }

    /**
     * 这个回调一般是意外关闭连接的回调
     * 这时考虑把未发送成功的数据包重新发送给客户端
     * 注意这个回调不一定是完全异步的，有可能连接已失效，然后去send数据，结果马上有close回调，所以要小心
     * @param {Message[]} pendingMsgList - 还没发送成功给客户端的数据包
     */
    onConnClose(pendingMsgList)
    {
        //由于是意外关闭，考虑保留role
        if (this._role)
        {
            //把没发送成功的数据包给role保管，好让它交给新的连接
            this._role.addPendingMsgList(pendingMsgList);
            //让Role跟本对象脱离关系，并开启定时删除
            this.setRole(null, true);
        }

        //这里让Mgr取消本Session的注册，由于role已脱离关系，role不会被销毁，而conn会被关闭（其实早关闭了）
        sessionMgr.delSession(this._sessionId);
    }

    /**
     * 把本Session从Role剥离，让Role处于离线状态，不过客户端可以重连上来
     */
    detachSessionFromRole()
    {
        //有Role，就保留它，但会定时删除，也可以重连上来，继续控制这个Role
        if (this._role)
        {
            //把没发送成功的数据包给role保管，好让它交给新的连接
            if (this._conn)
                this._role.addPendingMsgList(this._conn.getPendingList());
            //让Role跟本对象脱离关系，并开启定时删除
            this.setRole(null, true);
        }

        //这里让Mgr取消本Session的注册，由于role已脱离关系，role不会被销毁，而conn会被关闭（其实早关闭了）
        sessionMgr.delSession(this._sessionId);
    }

    syncVirtualTime()
    {
        var curTime = dateUtil.getTimestamp();
        var tzOffset = dateUtil.getTimezoneOffset();
        var syncTime = new accountMsg.SyncServerTime(curTime, tzOffset);
        this.sendEx(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.PUSH_SERVER_TIME, syncTime);
    }

    sendPingPush()
    {
        this._lastSvrPing = dateUtil.getTimestamp();
        this.sendEx(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.PUSH_PING);
    }
}

////////////导出元素////////////
exports.ClientSession = ClientSession;