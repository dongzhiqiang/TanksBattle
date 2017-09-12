"use strict";

var BaseProxy = require("./baseProxy").BaseProxy;

class BytesProxy extends BaseProxy
{
    /**
     * 读取数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {number} flag - 类型标记
     * @return {Buffer} 读取的数据
     * @override
     */
    getValue(ioBuf, flag) {
        //var typeFlag = BaseProxy.getFlagType(flag);
        //if (typeFlag !== BaseProxy.TYPE_FLAG_BYTEARRAY) {
        //    throw new Error("BytesProxy~getValue 必须是Buffer类型");
        //}

        //这里可能会抛出错误，让上层处理
        var len = BaseProxy.readVarUInt(ioBuf);

        if (ioBuf.canReadLen() < len) {
            throw new Error("BytesProxy~getValue 数据不足");
        }

        var buf = new Buffer(len);
        ioBuf.readBytes(buf);

        return buf;
    }

    /**
     * 写入数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {Buffer} value - 数据
     * @override
     */
    setValue(ioBuf, value)
    {
        //if (!Buffer.isBuffer(value))
        //{
        //    throw new Error("BytesProxy~setValue 必须是Buffer类型");
        //}

        ioBuf.writeUInt8(BaseProxy.TYPE_FLAG_BYTEARRAY);
        BaseProxy.writeVarUInt(ioBuf, value.length);
        ioBuf.writeBytes(value);
    }
}

exports.BytesProxy = BytesProxy;