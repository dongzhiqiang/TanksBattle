"use strict";

////////////我的模块////////////
var appUtil = require("./appUtil");
var logUtil = require("./logUtil");
var snappy = require("../libs/snappy");
var IOBuffer = require("./ioBuffer").IOBuffer;
var ProtocolCoder = require("../libs/protocolCoder");

////////////模块内变量////////////
const USE_JSON_FORMAT   = true;         //是否使用JSON
const ENABLE_COMPRESS   = false;        //TODO 正式发布开启压缩
const JSON_COMPRESS_LEN = 1024;         //JSON多少字符才要压缩，如果不想压缩，就设置一个很大的值，比如appUtil.INT_MAX_POSITIVE

const MSG_FLAG_JSON     = 0x00000001;   //是否JSON，否则是自定义的二进制
const MSG_FLAG_COMPRESS = 0x00000002;   //是否压缩过（只对JSON有效）
const MSG_FLAG_RESPONSE = 0x00000004;   //是否为回应类消息对象，如果是，则消息体前面有错误码，还可能有错误消息
const MSG_FLAG_ERR_MSG  = 0x00000008;   //是否带有错误消息字符串，一般只用于回应类消息

////////////导出类////////////
class Message
{
    constructor()
    {
        /**
         * 模块号
         * @type {number}
         * @private
         */
        this._module = 0;
        /**
         * 命令号
         * @type {number}
         * @private
         */
        this._command = 0;
        /**
         * 消息标记
         * @type {number}
         * @private
         */
        this._flag = 0;
        /**
         * 回应的错误码
         * @type {number}
         * @private
         */
        this._code = 0;
        /**
         * 回应的错误消息
         * @type {string|null}
         * @private
         */
        this._msg = null;
        /**
         * 消息数据
         * @type {IOBuffer|null}
         * @private
         */
        this._body = null;
    }

    /**
     * 创建非回应类对象
     * @param {number} module
     * @param {number} command
     * @param {*?} body
     * @return {Message}
     */
    static newRequest(module, command, body)
    {
        var msg = new Message();
        msg._module = module;
        msg._command = command;
        msg.setRequestBodyObj(body);
        return msg;
    }

    /**
     * 创建回应类消息
     * @param {number} module
     * @param {number} command
     * @param {number} code - 错误码
     * @param {(string|null)?} errMsg - 错误消息，如果没有错误消息，尽量使用null
     * @param {*?} body - 消息体
     * @return {Message}
     */
    static newResponse(module, command, code, errMsg, body)
    {
        var msg = new Message();
        msg._module = module;
        msg._command = command;
        msg.setResponseWithMsg(code, errMsg, body);
        return msg;
    }

    /**
     * 创建回应类消息
     * @param {Message} reqMsg
     * @param {number} code - 错误码
     * @param {(string|null)?} errMsg - 错误消息
     * @param {*?} body - 消息体
     */
    static newResponseWithRequest(reqMsg, code, errMsg, body)
    {
        var msg = new Message();
        msg._module = reqMsg.getModule();
        msg._command = reqMsg.getCommand();
        msg.setResponseWithMsg(code, errMsg, body);
        return msg;
    }

    /**
     * 从二进制里读取数据，暂时这里不解压、不把body转成对象
     * @param {IOBuffer} inBuf - 输入缓冲，注意inBuf里可能不只一个消息数据
     * @param {number} dataLen - 本消息数据的长度，上层分包时知道本包大小
     * @return {Message}
     */
    static fromIOBuffer(inBuf, dataLen)
    {
        do {
            var msg = new Message();
            if (dataLen < 12)
                break;
            else
                dataLen -= 12;
            msg._module = inBuf.readInt32();
            msg._command = inBuf.readInt32();
            msg._flag = inBuf.readInt32();
            //如果是回应消息，就要提取错误码
            if ((msg._flag & MSG_FLAG_RESPONSE) !== 0)
            {
                if (dataLen < 4)
                    break;
                else
                    dataLen -= 4;
                msg._code = inBuf.readInt32();
            }
            else
            {
                msg._code = 0;
            }
            //如果有错误消息，就要提取错误消息
            if ((msg._flag & MSG_FLAG_ERR_MSG) !== 0)
            {
                if (dataLen < 4)
                    break;
                else
                    dataLen -= 4;
                var len = inBuf.readUInt32();
                if (dataLen < len)
                    break;
                else
                    dataLen -= len;
                msg._msg = inBuf.readOnlyString(len);
            }
            else
            {
                msg._msg = null;
            }
            //有消息体？
            if (dataLen > 0)
            {
                var bodyBuf = new IOBuffer(dataLen);
                inBuf.readBytes(bodyBuf);
                msg._body = bodyBuf;
            }
            else
            {
                msg._body = null;
            }
            return msg;
        }
        while (false);

        //能到这里来？那说明前面遇到break，说明数据不够长
        logUtil.error("Message~fromIOBuffer数据不够长");
        //跳过还未读的
        inBuf.skip(dataLen);
        //返回一个空消息
        return new Message();
    }

    /**
     * 写入数据到输出Buffer，这里也不涉及对象转二进制和压缩
     * @param {IOBuffer} outBuf
     */
    writeBytes(outBuf)
    {
        outBuf.writeInt32(this._module);
        outBuf.writeInt32(this._command);
        outBuf.writeInt32(this._flag);
        //如果是回应消息，就要写入错误码
        if ((this._flag & MSG_FLAG_RESPONSE) !== 0)
        {
            outBuf.writeInt32(this._code);
        }
        //如果有错误消息，就要写入错误消息
        if ((this._flag & MSG_FLAG_ERR_MSG) !== 0)
        {
            var msg = this._msg;
            if (msg === null || msg === undefined)
                this._msg = msg = "";
            var len = Buffer.byteLength(msg);
            //这里写成无符号整数
            outBuf.writeUInt32(len);
            outBuf.writeOnlyString(msg, len);
        }
        //如果有消息体就写入消息体
        if (this._body)
        {
            outBuf.writeBytes(this._body);
        }
    }

    /**
     *
     * @returns {number}
     */
    getModule()
    {
        return this._module;
    }

    /**
     *
     * @param {number} v
     */
    setModule(v)
    {
        this._module = v;
    }

    /**
     *
     * @returns {number}
     */
    getCommand ()
    {
        return this._command;
    }

    /**
     *
     * @param {number} v
     */
    setCommand (v)
    {
        this._command = v;
    }

    /**
     * flag不让直接修改
     * @returns {number}
     */
    getFlag()
    {
        return this._flag;
    }

    //body是否json格式
    isJsonBody()
    {
        return (this._flag & MSG_FLAG_JSON) !== 0;
    }

    /**
     * 是否回应类消息
     * @returns {boolean}
     */
    isResponse()
    {
        return (this._flag & MSG_FLAG_RESPONSE) !== 0;
    }

    /**
     * code不让直接修改
     * @returns {number}
     */
    getErrorCode()
    {
        return this._code;
    }

    /**
     * msg不让直接修改
     * @returns {string|null}
     */
    getErrorMsg()
    {
        return this._msg;
    }

    /**
     * body不让直接修改
     * @returns {IOBuffer}
     */
    getBody ()
    {
        return this._body;
    }

    /**
     * 消耗大，不要频繁调用，调用一次把对象保存起来
     * @return {*} 如果解析数据失败，返回null
     */
    getBodyObj() {
        try
        {
            var body = this._body;
            if (body === null || body === undefined)
                return null;
            else {
                var bodyObj;
                //不是JSON？反串行化吧
                if ((this._flag & MSG_FLAG_JSON) === 0)
                {
                    body.resetRead();
                    bodyObj = ProtocolCoder.instance.decode(body);
                    body.resetRead();
                }
                else
                {
                    //没压缩过，就直接把数据转成字符串，并解析成对象
                    if ((this._flag & MSG_FLAG_COMPRESS) === 0)
                        bodyObj = JSON.parse(body.getReadableRef().toString());
                    //压缩过？解压吧，默认直接解压成字符串，并解析成对象
                    else
                        bodyObj = JSON.parse(snappy.uncompress(body.getReadableRef()));
                }
                return bodyObj;
            }
        }
        catch (err) {
            logUtil.error("Message~getBodyObj发生错误", err);
            return null;
        }
    }

    /**
     * 把请求数据写入到IOBuffer
     * 如果是回应类消息，这里会转为请求类消息
     * @param {*} obj
     * @return {boolean} 如果写入错误，返回false
     */
    setRequestBodyObj(obj)
    {
        try {
            //如果是null或undefined，那就直接body为null
            if (obj === null || obj === undefined)
            {
                this._body = null;
                this._msg = null;
                this._code = 0;
                this._flag = 0;
                return true;
            }

            if (USE_JSON_FORMAT)
            {
                var jsonStr = JSON.stringify(obj);
                //如果要压缩，且达到要压缩的字符数，那就压缩，compress可以直接得到Buffer，再直接转为IOBuffer
                if (jsonStr.length > JSON_COMPRESS_LEN && ENABLE_COMPRESS)
                {
                    //压缩并直接转为IOBuffer
                    this._body = new IOBuffer(snappy.compress(jsonStr));
                    //错误消息为空
                    this._msg = null;
                    //错误码为空
                    this._code = 0;
                    //加上标记
                    this._flag = MSG_FLAG_JSON | MSG_FLAG_COMPRESS;
                }
                //否则那就把字符串转为IOBuffer吧
                else
                {
                    //把字符串转成IOBuffer
                    this._body = new IOBuffer(jsonStr);
                    //错误消息为空
                    this._msg = null;
                    //错误码为空
                    this._code = 0;
                    //加上标记
                    this._flag = MSG_FLAG_JSON;
                }
            }
            else
            {
                if (this._body === null || this._body === undefined)
                    this._body = new IOBuffer();
                else
                    this._body.resetWrite();

                //错误消息为空
                this._msg = null;
                //错误码为空
                this._code = 0;
                //设置标记
                this._flag = 0;
                ProtocolCoder.instance.encode(this._body, obj);
            }

            return true;
        }
        catch (err) {
            logUtil.error("Message~setRequestBodyObj发生错误", err);
            this._body = null;
            this._msg = null;
            this._code = 0;
            this._flag = 0;
            return false;
        }
    }

    /**
     * 把回应数据写入到IOBuffer
     * 如果是请求类消息，这里会转为回应类消息
     * @param {number} code
     * @param {*?} obj - 不填就没有Body
     * @return {boolean} 如果写入错误，返回false
     */
    setResponseData(code, obj)
    {
        return this.setResponseWithMsg(code, null, obj);
    }


    /**
     * 把回应数据写入到IOBuffer
     * 如果是请求类消息，这里会转为回应类消息
     * @param {number} code
     * @param {(string|null)?} errMsg - 如果没有错误消息，最好使用null
     * @param {*?} obj - 不填就没有Body
     * @return {boolean} 如果写入错误，返回false
     */
    setResponseWithMsg(code, errMsg, obj)
    {
        try {
            var errMsgFlag = (errMsg === null || errMsg === undefined) ? 0 : MSG_FLAG_ERR_MSG;
            //如果是null或undefined，那就直接body为null
            if (obj === null || obj === undefined)
            {
                this._body = null;
                this._msg = errMsg;
                this._code = code;
                this._flag = MSG_FLAG_RESPONSE | errMsgFlag;
                return true;
            }

            if (USE_JSON_FORMAT)
            {
                var jsonStr = JSON.stringify(obj);
                //如果要压缩，且达到要压缩的字符数，那就压缩，compress可以直接得到Buffer，再直接转为IOBuffer
                if (jsonStr.length > JSON_COMPRESS_LEN && ENABLE_COMPRESS)
                {
                    //压缩并直接转为IOBuffer
                    this._body = new IOBuffer(snappy.compress(jsonStr));
                    //添加错误消息
                    this._msg = errMsg;
                    //添加错误码
                    this._code = code;
                    //加上标记
                    this._flag = MSG_FLAG_RESPONSE | MSG_FLAG_JSON | MSG_FLAG_COMPRESS | errMsgFlag;
                }
                //否则那就把字符串转为IOBuffer吧
                else
                {
                    //把字符串转成IOBuffer
                    this._body = new IOBuffer(jsonStr);
                    //添加错误消息
                    this._msg = errMsg;
                    //添加错误码
                    this._code = code;
                    //加上标记
                    this._flag = MSG_FLAG_RESPONSE | MSG_FLAG_JSON | errMsgFlag;
                }
            }
            else
            {
                if (this._body === null || this._body === undefined)
                    this._body = new IOBuffer();
                else
                    this._body.resetWrite();

                //添加错误消息
                this._msg = errMsg;
                //添加错误码
                this._code = code;
                //设置标记
                this._flag = MSG_FLAG_RESPONSE | errMsgFlag;
                ProtocolCoder.instance.encode(this._body, obj);
            }

            return true;
        }
        catch (err) {
            logUtil.error("Message~setResponseWithMsg发生错误", err);
            this._body = null;
            this._msg = null;
            this._code = 0;
            this._flag = 0;
            return false;
        }
    }

    toString()
    {
        if (this.isResponse())
            return "{module:" + this._module + ", command:" + this._command + ", code:" + this._code + ", body:" + JSON.stringify(this._body) + "}";
        else
            return "{module:" + this._module + ", command:" + this._command + ", body:" + JSON.stringify(this._body) + "}";
    }
}

////////////导出元素////////////
exports.Message = Message;
exports.USE_JSON_FORMAT = USE_JSON_FORMAT;
exports.ENABLE_COMPRESS = ENABLE_COMPRESS;
exports.JSON_COMPRESS_LEN = JSON_COMPRESS_LEN;