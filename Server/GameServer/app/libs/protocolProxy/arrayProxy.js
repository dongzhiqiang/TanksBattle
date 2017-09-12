"use strict";

var BaseProxy = require("./baseProxy").BaseProxy;
var ProtocolCoder = require("../protocolCoder");

class ArrayProxy extends BaseProxy
{
    /**
     * 读取数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {number} flag - 类型标记
     * @return {Array} 读取的数据
     * @override
     */
    getValue(ioBuf, flag) {
        //var typeFlag = BaseProxy.getFlagType(flag);
        //if (typeFlag !== BaseProxy.TYPE_FLAG_ARRAY) {
        //    throw new Error("ArrayProxy~getValue 必须是Array类型");
        //}

        var coder = ProtocolCoder.instance;

        //这里可能会抛出错误，让上层处理
        var len = BaseProxy.readVarUInt(ioBuf);

        var arr = new Array(len);

        for (var i = 0; i < len; ++i) {
            arr[i] = coder.decode(ioBuf);
        }

        return arr;
    }

    /**
     * 写入数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {Array} value - 数据
     * @override
     */
    setValue(ioBuf, value)
    {
        //if (!Object.isArray(value))
        //{
        //    throw new Error("ArrayProxy~setValue 必须是Array类型");
        //}

        var coder = ProtocolCoder.instance;

        ioBuf.writeUInt8(BaseProxy.TYPE_FLAG_ARRAY);
        var len = value.length;
        BaseProxy.writeVarUInt(ioBuf, len);
        for (var i = 0; i < len; ++i) {
            coder.encode(ioBuf, value[i]);
        }
    }
}

exports.ArrayProxy = ArrayProxy;