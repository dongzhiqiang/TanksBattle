"use strict";

////////////内置模块////////////

////////////我的模块////////////
var appCfg = require("../../../config");
var logUtil = require("../../libs/logUtil");
var sessionMgr = require("./sessionMgr");
var IOBuffer = require("../../libs/ioBuffer").IOBuffer;
var NetMsgCodec = require("../../libs/netMsgCodec").NetMsgCodec;

////////////导出类////////////
class Connection
{
    /**
     *
     * @param {net.Socket} socket
     */
    constructor(socket)
    {
        /**
         * @type {net.Socket}
         * @private
         */
        this._socket = socket;
        /**
         * @type {NetConnectionHandler|ClientSession}
         * @private
         */
        this._handler = null;
        /**
         * @type {NetMsgCodec}
         * @private
         */
        this._msgCodec = new NetMsgCodec(appCfg.maxMessageLen);
        /**
         * 转换过后的消息对象列表
         * @type {Message[]}
         * @private
         */
        this._msgObjList = [];
        /**
         * 接收数据的Buffer
         * @type {IOBuffer}
         * @private
         */
        this._recvBuffer = new IOBuffer();
        /**
         * 发送数据的Buffer
         * @type {IOBuffer}
         * @private
         */
        this._sendBuffer = new IOBuffer();
        //发送时会引用Buffer，自动不能整理数据覆盖写过的Buffer
        this._sendBuffer.setAutoTidy(false);
        /**
         * 正在发送数据，主要用于重发机制
         * @type {Message[]}
         * @private
         */
        this._pendingList = [];
        /**
         * 是否处于发送包后关闭状态
         * @type {boolean}
         * @private
         */
        this._inSendThenClose = false;
    }

    getHandler()
    {
        return this._handler;
    }

    setHandler(v)
    {
        this._handler = v;
    }

    getRemoteAddress()
    {
        if (this._socket)
            return this._socket.remoteAddress || "";
        else
            return "";
    }

    getRemotePort()
    {
        if (this._socket)
            return this._socket.remotePort || 0;
        else
            return 0;
    }

    /**
     * IP协议族类型
     * @returns {string} IPv4、IPv6
     */
    getRemoteFamily()
    {
        if (this._socket)
            return this._socket.remoteFamily  || "";
        else
            return "";
    }

    getPendingList()
    {
        return this._pendingList;
    }

    start()
    {
        var self = this;

        //提示一下
        logUtil.info("连接开启：" + self._socket.remoteAddress + ":" + self._socket.remotePort);

        //读取数据
        self._socket.on("data", function(buf)
        {
            if (self._handler)
            {
                //处于最后包发送中？不管后面的数据了
                if (self._inSendThenClose)
                    return;

                self._recvBuffer.writeBytes(buf);

                if (!self._msgCodec.decode(self._recvBuffer, self._msgObjList))
                {
                    //数据包有问题？那就强制关闭连接
                    self._doClose();
                    return;
                }

                if (self._msgObjList.length > 0)
                {
                    for (var i = 0; i < self._msgObjList.length; ++i)
                    {
                        //有可能回调时把self._handler清除了，这里每次判断
                       try {
                            if (self._handler)
                                self._handler.onRecvData(self._msgObjList[i]);
                        }
                        catch (err) {
                            logUtil.error("Connection~data", err);
                        }
                    }
                    self._msgObjList = [];
                }
            }
        });

        //关闭事件
        self._socket.on("close", function(had_error)
        {
            if (self._socket)
            {
                if (had_error)
                    logUtil.debug("得到因错误导致连接关闭消息：" + self._socket.remoteAddress + ":" + self._socket.remotePort);
                else
                    logUtil.debug("得到连接关闭消息：" + self._socket.remoteAddress + ":" + self._socket.remotePort);

                self._doClose();
            }
        });

        //错误处理
        self._socket.on("error", function(err)
        {
            if (self._socket)
            {
                //socket出错很正常，debug模式下才显示报错
                if (appCfg.debug)
                    logUtil.error("得到连接错误消息：" + self._socket.remoteAddress + ":" + self._socket.remotePort, err);

                self._doClose();
            }
        });

        //把自己纳入会话管理
        sessionMgr.newSession(self);
    }

    /**
     * 内部用，关闭连接并通知回调
     * @private
     */
    _doClose()
    {
        if (this._socket)
        {
            //提示一下
            logUtil.info("连接关闭：" + this._socket.remoteAddress + ":" + this._socket.remotePort);

            //这里可以不用取消事件监听
            this._socket.destroy();
            this._socket = null;
        }

        if (this._handler)
        {
            this._handler.onConnClose(this._pendingList);
            this._handler = null;
        }
    }

    /**
     * 主动关闭连接，可选择是否要回调
     * @param {boolean} [needCallback=false] - 是否要回调
     */
    close(needCallback)
    {
        if (!needCallback)
            this._handler = null;
        //这里还要把缓存的发送包清除
        this._pendingList = [];
        this._doClose();
    }

    /**
     * 发送数据
     * @param {Message} msgObj
     */
    send(msgObj)
    {
        var self = this;
        if (self._socket && !self._inSendThenClose)
        {
            NetMsgCodec.encode(msgObj, self._sendBuffer);
            self._pendingList.push(msgObj);
            self._socket.write(self._sendBuffer.getReadableRef(), function(){
                if (self._pendingList.length > 0) {
                    self._pendingList.shift();
                    //终于发完了？可以整理发送缓冲了
                    if (self._pendingList.length <= 0)
                        self._sendBuffer.tidy();
                }
                //有可能先send，后sendThenClose，过一会，前面的send回调回来，socket还在，但pendingList为空了
                else if (self._socket && !self._inSendThenClose) {
                    logUtil.warn("回调后，pendingList为空");
                }
            });
            //跳过写入的数据
            self._sendBuffer.skipAll();
        }
    }

    /**
     * 如果向多个连接发送同样的数据，就不要重复各种写入、加密、压缩、生成校验码
     * @param {IOBuffer} ioBuf - 经过各种加长度、校验和、加密、压缩的buffer
     * @param {Message} msgObj - 为了能断线重连时重发，还是要带上这个，不过只是简单加入队列
     */
    sendBufDirectly(ioBuf, msgObj)
    {
        var self = this;
        if (self._socket && !self._inSendThenClose)
        {
            self._sendBuffer.writeBytes(ioBuf);
            self._pendingList.push(msgObj);
            self._socket.write(self._sendBuffer.getReadableRef(), function(){
                if (self._pendingList.length > 0) {
                    self._pendingList.shift();
                    //终于发完了？可以整理发送缓冲了
                    if (self._pendingList.length <= 0)
                        self._sendBuffer.tidy();
                }
                //有可能先send，后sendThenClose，过一会，前面的send回调回来，socket还在，但pendingList为空了
                else if (self._socket && !self._inSendThenClose) {
                    logUtil.warn("回调后，pendingList为空");
                }
            });
            //跳过写入的数据
            self._sendBuffer.skipAll();
        }
    }

    /**
     * 发送完这个数据后，关闭连接
     * 至于接收，如果没有半包，就不允许再接收了，如果有半包，那就处理完半包的完整包就不再处理了
     * @param {Message} msgObj
     */
    sendThenClose(msgObj)
    {
        var self = this;
        if (self._socket && !self._inSendThenClose)
        {
            self._inSendThenClose = true;
            NetMsgCodec.encode(msgObj, self._sendBuffer);
            //有时发送完后，没有关闭连接，这里就强制关闭它吧
            self._socket.end(self._sendBuffer.getReadableRef(), function(){
                self._doClose();
            });
            //清除发送缓存，注意，不能再修改它了
            self._sendBuffer.skipAll();
            //这里还要把缓存的发送包清除
            self._pendingList = [];
        }
    }
}

////////////导出元素////////////
exports.Connection = Connection;