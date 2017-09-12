"use strict";

var ArrayList = require("../arrayList").ArrayList;
var BaseProxy = require("./baseProxy").BaseProxy;
var ProtocolCoder = require("../protocolCoder");

class ListProxy extends BaseProxy
{
    /**
     * 读取数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {number} flag - 类型标记
     * @return {ArrayList} 读取的数据
     * @override
     */
    getValue(ioBuf, flag) {
        //var typeFlag = BaseProxy.getFlagType(flag);
        //if (typeFlag !== BaseProxy.TYPE_FLAG_LIST) {
        //    throw new Error("ListProxy~getValue 必须是ArrayList类型");
        //}

        var coder = ProtocolCoder.instance;

        //这里可能会抛出错误，让上层处理
        var len = BaseProxy.readVarUInt(ioBuf);
        //本来len跟canReadLen()是没法比的，但为了安全，假定至少一个元素一个字节，所以至少剩余len个字节可读，如果连每个元素一字节都不够读，那说明len有问题
        //if (ioBuf.canReadLen() < len) {
        //    throw new Error("ListProxy~getValue 元素个数过多");
        //}

        var arr = new ArrayList(len);

        for (var i = 0; i < len; ++i) {
            arr[i] = coder.decode(ioBuf);
        }
        return arr;
    }

    /**
     * 写入数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {ArrayList} value - 数据
     * @override
     */
    setValue(ioBuf, value)
    {
        //if (!ArrayList.isArrayList(value))
        //{
        //    throw new Error("ListProxy~setValue 必须是ArrayList类型");
        //}

        var coder = ProtocolCoder.instance;

        ioBuf.writeUInt8(BaseProxy.TYPE_FLAG_LIST);
        var len = value.length;
        BaseProxy.writeVarUInt(ioBuf, len);
        for (var i = 0; i < len; ++i) {
            coder.encode(ioBuf, value[i]);
        }
    }
}

exports.ListProxy = ListProxy;