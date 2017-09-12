"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var appCfg      = require("../../../config");
var appUtil     = require("../../libs/appUtil");
var logUtil     = require("../../libs/logUtil");
var Message     = require("../../libs/message").Message;
var accoutUtil  = require("./account");
var Connection  = require("../network/connection").Connection;
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var CmdIdsAccount = require("../netMessage/accountMessage").CmdIdsAccount;
var handlerMgr = require("../network/handlerMgr");
var equipConfig = require("../gameConfig/equipConfig");

////////////模块内变量////////////
const RECONNECT_DELAY   = 1000;

////////////导出类////////////
class Player
{
    constructor(username, password)
    {
        this._username      = username;
        this._password      = password;
        this._userInfo      = null;
        this._gameSvrInfo   = null;
        this._roleInfo      = null;
        /**
         *
         * @type {Connection}
         * @private
         */
        this._netConn       = null;
        this._needReconn    = true;
        this._pendingMsgList= [];
    }

    logDebug(msg)
    {
        logUtil.debug(this._username + ": " + msg);
    }

    logInfo(msg)
    {
        logUtil.info(this._username + ": " + msg);
    }

    logWarn(msg, err)
    {
        logUtil.warn(this._username + ": " + msg, err);
    }

    logError(msg, err)
    {
        logUtil.error(this._username + ": " + msg, err);
    }

    getUsername()
    {
        return this._username;
    }

    setUsername(v)
    {
        this._username = v;
    }

    getRoleInfo()
    {
        return this._roleInfo;
    }

    setRoleInfo(v)
    {
        this._roleInfo = v;
    }

    syncRoleProp(guid, syncProps)
    {
        if (!this._roleInfo)
            return false;
        var props = this._roleInfo.props;
        if (!props)
            return false;
        var guidHero = props.guid;
        if (guidHero === guid)
        {
            for (let k in syncProps)
                props[k] = syncProps[k];
            return true;
        }
        else
        {
            var pets = this._roleInfo.pets;
            if (!pets)
                return false;
            for (var i = 0; i < pets.length; ++i)
            {
                var pet = pets[i];
                if (pet.props && pet.props.guid === guid)
                {
                    props = pet.props;
                    for (let k in syncProps)
                        props[k] = syncProps[k];
                    return true;
                }
            }
            return false;
        }
    }

    getRoleProp(propName)
    {
        if (!this._roleInfo)
            return null;
        var props = this._roleInfo.props;
        if (!props)
            return null;
        return props[propName];
    }

    getPetProp(guid, propName)
    {
        if (!this._roleInfo)
            return null;
        var pets = this._roleInfo.pets;
        if (!pets)
            return null;
        for (var i = 0; i < pets.length; ++i)
        {
            var pet = pets[i];
            if (pet.props && pet.props.guid === guid)
            {
                return pet.props[propName];
            }
        }
        return null;
    }

    onRemovePet(guid)
    {
        if (!this._roleInfo)
            return false;
        var pets = this._roleInfo.pets;
        if (!pets)
            return false;
        for (var i = 0; i < pets.length; ++i)
        {
            var pet = pets[i];
            if (pet.props && pet.props.guid === guid)
            {
                pets.splice(i, 1);
                return true;
            }
        }
        return false;
    }

    onAddPet(data)
    {
        if (!data || !data.props || !data.props.guid)
            return false;
        if (!this._roleInfo)
            return false;
        var guid = data.props.guid;
        var pets = this._roleInfo.pets;
        if (!pets)
        {
            pets = [];
            this._roleInfo.pets = pets;
        }
        for (var i = 0; i < pets.length; ++i)
        {
            var pet = pets[i];
            if (pet.props && pet.props.guid === guid)
            {
                return false;
            }
        }
        pets.push(data);
        return true;
    }

    onAddOrUpdateItem(data)
    {
        if (!data || !data.itemId)
            return false;
        if (!this._roleInfo)
            return false;
        var itemId = data.itemId;
        var items = this._roleInfo.items;
        if (!items)
        {
            items = [];
            this._roleInfo.items = items;
        }
        for (var i = 0; i < items.length; ++i)
        {
            var item = items[i];
            if (item.itemId === itemId)
            {
                items[i] = data;
                return true;
            }
        }
        items.push(data);
        return true;
    }

    onRemoveItem(itemId)
    {
        if (!this._roleInfo)
            return false;
        var items = this._roleInfo.items;
        if (!items)
            return false;
        for (var i = 0; i < items.length; ++i)
        {
            var item = items[i];
            if (item.itemId === itemId)
            {
                items.splice(i, 1);
                return true;
            }
        }
        return false;
    }

    onAddOrUpdateEquip(guidOwner, data)
    {
        if (!data || !data.equipId)
            return false;
        if (!this._roleInfo)
            return false;
        var equipId = data.equipId;
        var equipCfg = equipConfig.getEquipConfig(equipId);
        var posIndex = equipCfg.posIndex;

        if (this._roleInfo.props.guid === guidOwner) {
            let equips = this._roleInfo.equips;
            if (!equips)
            {
                equips = [];
                this._roleInfo.equips = equips;
            }
            equips[posIndex] = data;
            return true;
        }
        else {
            var pets = this._roleInfo.pets;
            if (!pets)
                return false;
            for (let i = 0; i < pets.length; ++i)
            {
                var pet = pets[i];
                if (pet.props.guid !== guidOwner)
                    continue;
                let equips = pet.equips;
                if (!equips)
                {
                    equips = [];
                    pet.equips = equips;
                }
                equips[posIndex] = data;
                return true;
            }
            return false;
        }
    }

    onRemoveEquip(guidOwner, index)
    {
        if (!this._roleInfo)
            return false;
        if (this._roleInfo.props.guid === guidOwner) {
            let equips = this._roleInfo.equips;
            if (!equips)
                return false;
            equips[index] = null;
            return true;
        }
        else {
            var pets = this._roleInfo.pets;
            if (!pets)
                return false;
            for (let i = 0; i < pets.length; ++i)
            {
                var pet = pets[i];
                if (pet.props.guid !== guidOwner)
                    continue;
                let equips = pet.equips;
                if (!equips)
                    continue;
                equips[index] = null;
                return true;
            }
            return false;
        }
    }

    syncActivityProp(syncProps)
    {
        if (!this._roleInfo)
            return false;
        var props = this._roleInfo.actProps;
        if (!props)
            return false;

        for (let k in syncProps)
            props[k] = syncProps[k];
        return true;
    }

    /**
     *
     * @returns {boolean}
     */
    isNeedReconn()
    {
        return this._needReconn;
    }

    setNeedReconn(v)
    {
        this._needReconn = v;
    }

    /**
     *
     * @returns {Connection}
     */
    getNetConn()
    {
        return this._netConn;
    }

    setNetConn(v)
    {
        this._netConn = v;
    }

    loginAccount()
    {
        return Player._loginAccountCoroutine(this);
    }

    loginGameServer()
    {
        return Player._loginGameServerCoroutine(this);
    }

    send(msgObj)
    {
        var self = this;

        if (self._netConn)
        {
            self._netConn.send(msgObj);
        }
        else
        {
            //不用再重连不添加到延迟队列
            if(!self._needReconn)
                return;
            self._pendingMsgList.push(msgObj);
        }
    }

    sendEx(module, cmd, body)
    {
        var msgObj = Message.newRequest(module, cmd, body);
        this.send(msgObj);
    }

    /**
     * 如果向多个连接发送同样的数据，就不要重复各种写入、加密、压缩、生成校验码
     * @param {IOBuffer} ioBuf - 经过各种加长度、校验和、加密、压缩的buffer
     * @param {Message} msgObj - 为了能断线重连时重发，还是要带上这个，不过只是简单加入队列
     */
    sendBufDirectly(ioBuf, msgObj)
    {
        var self = this;

        if (self._netConn)
        {
            self._netConn.sendBufDirectly(ioBuf, msgObj);
        }
        else
        {
            //不用再重连不添加到延迟队列
            if(!self._needReconn)
                return;
            self._pendingMsgList.push(msgObj);
        }
    }

    sendLogout()
    {
        var self = this;

        //不要重连了
        self._needReconn = false;
        //清除延迟队列
        self._pendingMsgList = [];

        if (!self._netConn)
            return;

        self.logInfo("尝试断开与游戏服的连接");

        //发送注销消息
        var msgObj = Message.newRequest(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_LOGOUT, null);
        self._netConn.sendThenClose(msgObj);
        //让Conn脱离与本对象的关联
        self._netConn.setHandler(null);
        self._netConn = null;
    }

    sendLoginMsg()
    {
        var self = this;

        var reqObj;
        //要自动重新登录？
        if (self._roleInfo)
        {
            /**
             *
             * @type {ReloginRequestVo}
             */
            reqObj = {};
            reqObj.channelId = self._userInfo.channelId;
            reqObj.userId = self._userInfo.userId;
            reqObj.token = self._userInfo.token;
            reqObj.heroId = self._roleInfo.props.heroId;
            reqObj.lastLogin = self._roleInfo.props.lastLogin;
            reqObj.network = "WIFI";
            reqObj.osName = "Windows";
            reqObj.clientVer = "1.0";
            reqObj.lang = "zh-CN";
            self.sendEx(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_RELOGIN, reqObj);
            self.logInfo("发送重新登录消息");
        }
        else
        {
            //如果要全新登录，就清除延迟队列
            self._pendingMsgList = [];
            /**
             *
             * @type {LoginVo}
             */
            reqObj = {};
            reqObj.channelId = self._userInfo.channelId;
            reqObj.userId = self._userInfo.userId;
            reqObj.token = self._userInfo.token;
            reqObj.network = "WIFI";
            reqObj.osName = "Windows";
            reqObj.clientVer = "1.0";
            reqObj.lang = "zh-CN";
            self.sendEx(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_LOGIN, reqObj);
            self.logInfo("发送登录消息");
        }
    }

    /**
     *
     * @param {Message[]} msgList
     */
    addPendingMsgList(msgList)
    {
        //使用连接的方式
        if (msgList && msgList.length > 0)
            this._pendingMsgList = this._pendingMsgList.concat(msgList);
    }

    sendPendingMsgList()
    {
        var self = this;

        //自动重连成功了？有延迟队列？那就发送吧
        var pending = self._pendingMsgList;
        var pendLen = pending.length;
        if (pendLen > 0)
        {
            //有可能重发过程中会连接失效，这时用send可以把旧的包再次加入重发队列
            //另外，先清空pendingMsgList，因为重发过程中如果失败会concat
            self._pendingMsgList = [];
            for (var i = 0; i < pendLen; ++i)
            {
                var msgObj = pending[i];
                self.send(msgObj);
            }
        }
    }

    /**
     * @param {Message[]} pendingMsgList - 未成功发送给对方的数据包
     */
    onConnClose(pendingMsgList) {
        var self = this;

        //不用重连就不重连
        if (!self._needReconn)
            return;

        //把未发送成功的消息添加到延迟队列
        self.addPendingMsgList(pendingMsgList);
        //一段时间后重连
        self.logInfo((RECONNECT_DELAY / 1000) + "秒后尝试重新连接游戏服");
        setTimeout(function() {
            //有可能timeout后就不用重连
            if (self._needReconn)
                self.loginGameServer();
        }, RECONNECT_DELAY);
    }

    onConnOK()
    {
        var self = this;

        if (self._needReconn)
        {
            self.logInfo("连接游戏服成功");
            self.sendLoginMsg();
        }
    }

    /**
     *
     * @param {Message} msgObj
     */
    onRecvData(msgObj)
    {
        var self = this;

        var module = msgObj.getModule();
        var command = msgObj.getCommand();
        var code = msgObj.getErrorCode();
        var errMsg = msgObj.getErrorMsg();
        var body = msgObj.getBodyObj();

        if (appCfg.debug)
        {
            if (msgObj.isResponse())
                self.logDebug("模块：" + module + "，命令：" + command + "，错误码：" + code + "，错误消息：" + errMsg + "，内容：" + JSON.stringify(body));
            else
                self.logDebug("模块：" + module + "，命令：" + command + "，内容：" + JSON.stringify(body));
        }

        handlerMgr.dispatchNetMsg(this, msgObj);
    }

    logoutGameServer()
    {
        return Player._logoutGameServerCoroutine(this);
    }

    logoutAccount()
    {
        return Player._logoutAccountCoroutine(this);
    }
}

Player._loginAccountCoroutine = Promise.coroutine(function * (self) {
    self._gameSvrInfo = null;
    self._userInfo = null;
    //尝试登录
    self.logInfo("尝试登录账号");
    var retMsg = yield accoutUtil.login(self._username, self._password);
    if (!retMsg)
        throw Error("访问登录服务器失败");
    //账号不存在？
    if (retMsg.code == 7) {
        //尝试注册
        self.logInfo("尝试注册账号");
        retMsg = yield accoutUtil.register(self._username, self._password);
        if (!retMsg)
            throw Error("访问登录服务器失败");
        if (retMsg.code != 0)
            throw Error("尝试注册失败");
        //重新登录
        self.logInfo("尝试登录账号");
        retMsg = yield accoutUtil.login(self._username, self._password);
        if (!retMsg)
            throw Error("访问登录服务器失败");
    }
    if (retMsg.code != 0)
        throw Error("尝试登录失败");
    self._userInfo = {channelId:appCfg.channelId, userId:retMsg.cxt.userId, token:retMsg.cxt.token};
    self.logInfo("获取服务器列表");
    retMsg = yield accoutUtil.getServers(self._userInfo.channelId, self._userInfo.userId);
    if (!retMsg)
        throw Error("访问登录服务器失败");
    if (retMsg.code != 0)
        throw Error("获取服务器列表失败");
    if (!retMsg.cxt || !retMsg.cxt.serverList || retMsg.cxt.serverList.length <= 0)
        throw Error("服务器列表为空");
    var serverList = retMsg.cxt.serverList;
    for (var i = 0; i < serverList.length; ++i)
    {
        var item = serverList[i];
        if (item.serverId == appCfg.gameSvrIdToLogin)
        {
            self._gameSvrInfo = {host:item.host, port:item.port};
            break;
        }
    }
    if (self._gameSvrInfo == null)
        throw Error("没找到要登录的服务器");
});

Player._loginGameServerCoroutine = Promise.coroutine(function * (self) {
    if (self._gameSvrInfo == null)
        throw Error("没找到要登录的服务器");
    if (self._netConn != null)
    {
        self._netConn.close();
        self._netConn = null;
    }
    self._netConn = new Connection();

    yield new Promise(function (resolve, reject) {
        self._netConn.setHandler({
            onRecvData: function (msgObj) {
                self.onRecvData(msgObj);
            },
            /**
             * @param {Message[]} pendingMsgList - 未成功发送给对方的数据包
             */
            onConnClose: function (pendingMsgList) {
                //清连接对象引用
                self._netConn = null;
                //如果是在连接中，就直接延时重连
                self.onConnClose(pendingMsgList);
                //连接失败也用resolve
                resolve();
            },
            onConnOK: function () {
                //回调
                self.onConnOK();
                //让协程继续
                resolve();
            }
        });
        self.logInfo("尝试连接游戏服");
        self._netConn.connect(self._gameSvrInfo.host, self._gameSvrInfo.port);
    });
});

Player._logoutGameServerCoroutine = Promise.coroutine(function * (self) {
    self.sendLogout();
});

Player._logoutAccountCoroutine = Promise.coroutine(function * (self) {
    try {
        self.logInfo("尝试注销登录");
        yield accoutUtil.logout(self._userInfo.userId, self._userInfo.token);
    }
    catch (err)
    {
        self.logError(err.message);
    }
    self._userInfo = null;
});

////////////导出元素////////////
exports.Player = Player;
