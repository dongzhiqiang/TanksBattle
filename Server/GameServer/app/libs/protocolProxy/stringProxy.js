"use strict";

var BaseProxy = require("./baseProxy").BaseProxy;

class StringProxy extends BaseProxy
{
    /**
     * 读取数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {number} flag - 类型标记
     * @return {string} 读取的数据
     * @override
     */
    getValue(ioBuf, flag) {
        //var typeFlag = BaseProxy.getFlagType(flag);
        //if (typeFlag !== BaseProxy.TYPE_FLAG_STRING) {
        //    throw new Error("StringProxy~getValue 必须是string类型");
        //}

        //这里可能会抛出错误，让上层处理
        var len = BaseProxy.readVarUInt(ioBuf);

        var str = ioBuf.readOnlyString(len);
        if (str === null)
        {
            throw new Error("StringProxy~getValue 数据字节不足" + len);
        }

        return str;
    }

    /**
     * 写入数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {string} value - 数据
     * @override
     */
    setValue(ioBuf, value)
    {
        //if (!Object.isString(value))
        //{
        //    throw new Error("StringProxy~setValue 必须是string类型");
        //}

        ioBuf.writeUInt8(BaseProxy.TYPE_FLAG_STRING);
        var len = Buffer.byteLength(value);
        BaseProxy.writeVarUInt(ioBuf, len);
        ioBuf.writeOnlyString(value, len);
    }
}

exports.StringProxy = StringProxy;