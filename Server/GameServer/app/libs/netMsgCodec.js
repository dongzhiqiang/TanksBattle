"use strict";

////////////内置模块////////////

////////////我的模块////////////
var logUtil = require("./logUtil");
var tea16 = require("./tea16");
var IOBuffer = require("./ioBuffer").IOBuffer;
var Message = require("./message").Message;


////////////模块内变量////////////
const PACKAGE_INDETIFIER = -1;  //包前导标记
const PACKAGE_PRE_LENGTH = 8;   //包标记和长度总字节数

////////////导出类////////////
/**
 * 格式：
 * [包标志（4）][包总长（4）][消息数据检验和（4）][消息数据（N）]
 */
class NetMsgCodec {

    /**
     *
     * @param {number?} [maxMessageLen=-1] - 包体长度限制，字节，<0 则无限制
     */
    constructor(maxMessageLen) {
        /**
         * 是否在接收消息体
         * 这个状态是指接收获取数据长度后，接收消息体的状态
         * @type {boolean}
         * @private
         */
        this._inReceiving = false;
        /**
         * 等待的包体的长度（包括检验和的4字节）
         * @type {number}
         * @private
         */
        this._waitingLength = 0;
        /**
         * 包体长度限制，字节，<0 则无限制
         * @type {number}
         * @private
         */
        this._maxMessageLen = (maxMessageLen === null || maxMessageLen === undefined) ? -1 : maxMessageLen;
    }

    /**
     *
     * @param {Message} msgObj
     * @param {IOBuffer} outBuf
     */
    static encode(msgObj, outBuf) {
        outBuf.writeInt32(PACKAGE_INDETIFIER);
        var relWPOfLen = outBuf.getRelativeWritePos();
        outBuf.writeInt32(0);   //包长，先占位
        var relWPOfCheck = outBuf.getRelativeWritePos();
        outBuf.writeInt32(0);   //检验和，先点位
        var relWPOfMsg = outBuf.getRelativeWritePos();
        msgObj.writeBytes(outBuf);
        var relWPNow = outBuf.getRelativeWritePos();
        //消息数据的长度
        var msgBufLen = relWPNow - relWPOfMsg;
        //回写消息长度
        outBuf.writeInt32WithRelativePos(msgBufLen + 8, relWPOfLen);  //数据长度字段4字节 + 检验和4字节
        //绝对读取位置，用于根据它求写的绝对位置
        var absRPNow = outBuf.getReadPos();
        //消息写的绝对位置
        var absWritePosOfMsg = absRPNow + relWPOfMsg;
        //对消息数据加密和求检验和
        tea16.encrypt(outBuf.buffer(), absWritePosOfMsg, msgBufLen);
        var checkSum = IOBuffer.calcCheckSum(outBuf, absWritePosOfMsg, msgBufLen);
        //回写消息数据检验和
        outBuf.writeInt32WithRelativePos(checkSum, relWPOfCheck);
    }

    /**
     * IOBuffer内部新建，然后返回
     * @param {Message} msgObj
     */
    static encodeEx(msgObj)
    {
        var buf = new IOBuffer();
        NetMsgCodec.encode(msgObj, buf);
        return buf;
    }

    /**
     *
     * @param {IOBuffer} inBuf
     * @param {Message[]} outObjList
     * @return {boolean} 如果返回false就说明数据包有问题，要踢出这个用户
     */
    decode(inBuf, outObjList) {
        //循环遍历
        while (inBuf.canReadLen() > 0)
        {
            if (!this._inReceiving)
            {
                while (true)
                {
                    //必须收集到包标记和包长度数据
                    if (inBuf.canReadLen() < PACKAGE_PRE_LENGTH)
                        return true;
                    //必须以包标记开头
                    if (inBuf.peekInt32() === PACKAGE_INDETIFIER)
                    {
                        inBuf.skip(4);
                        break;
                    }
                    //否则就跳过这个字节
                    else
                    {
                        inBuf.skip(1);
                    }
                }

                //找到前导字节了，取本包长度
                var packLen = inBuf.readInt32();
                //如果没有数据，那至少应该8字节，数据长度字段4字节 + 检验和4字节
                if (packLen < 8)
                {
                    logUtil.warn("收到的数据包总长小于8， 包总长：" + packLen);
                    continue;
                }
                else if (this._maxMessageLen >= 0 && packLen > this._maxMessageLen)
                {
                    logUtil.warn("收到的数据包总长大于限制值， 包总长：" + packLen + "，限制长：" + this._maxMessageLen);
                    return false;
                }
                this._waitingLength = packLen - 4;   //减去数据长度字段4字节
                this._inReceiving = true;
            }

            if (this._inReceiving)
            {
                if (inBuf.canReadLen() < this._waitingLength)
                    return true;

                var msgLen = this._waitingLength - 4;  //减去检验和4字节
                this._inReceiving = false;
                this._waitingLength = 0;

                //获得检验和
                var checkSum1 = inBuf.readInt32();
                //计算检验和
                var checkSum2 = IOBuffer.calcCheckSum(inBuf, inBuf.getReadPos(), msgLen);
                //检验和不对？
                if (checkSum1 !== checkSum2)
                {
                    //跳过这个包
                    inBuf.skip(msgLen);
                    logUtil.warn("数据包检验和不正确， checkSum1：" + checkSum1 + "，checkSum2：" + checkSum2);
                    continue;
                }

                //解密数据
                tea16.decrypt(inBuf.buffer(), inBuf.getReadPos(), msgLen);
                var msgObj = Message.fromIOBuffer(inBuf, msgLen);
                //加入消息列表
                outObjList.push(msgObj);
            }
        }
        return true;
    }
}

////////////导出元素////////////
exports.NetMsgCodec = NetMsgCodec;