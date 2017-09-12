"use strict";

var BaseProxy = require("./baseProxy").BaseProxy;
var ProtocolCoder = require("../protocolCoder");

class MapProxy extends BaseProxy
{
    /**
     * 读取数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {number} flag - 类型标记
     * @return {Map} 读取的数据
     * @override
     */
    getValue(ioBuf, flag) {
        //var typeFlag = BaseProxy.getFlagType(flag);
        //if (typeFlag !== BaseProxy.TYPE_FLAG_MAP) {
        //    throw new Error("MapProxy~getValue 必须是Map类型");
        //}

        var coder = ProtocolCoder.instance;

        //这里可能会抛出错误，让上层处理
        var len = BaseProxy.readVarUInt(ioBuf);
        //本来len跟canReadLen()是没法比的，但为了安全，假定至少一个元素一个字节，所以至少剩余len个字节可读，如果连每个元素一字节都不够读，那说明len有问题
        //这里乘2是因为键值对
        //if (ioBuf.canReadLen() < len * 2) {
        //    throw new Error("MapProxy~getValue 元素个数过多");
        //}

        var map = new Map();

        for (var i = 0; i < len; ++i) {
            var key = coder.decode(ioBuf);
            var val = coder.decode(ioBuf);
            map.set(key, val);
        }
        return map;
    }

    /**
     * 写入数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {Map} value - 数据
     * @override
     */
    setValue(ioBuf, value)
    {
        //if (!Object.isMap(value))
        //{
        //    throw new Error("MapProxy~setValue 必须是Map类型");
        //}

        var coder = ProtocolCoder.instance;

        ioBuf.writeUInt8(BaseProxy.TYPE_FLAG_MAP);
        var len = value.size;
        BaseProxy.writeVarUInt(ioBuf, len);
        for (var entry of value) {
            coder.encode(ioBuf, entry[0]);
            coder.encode(ioBuf, entry[1]);
        }
    }
}

exports.MapProxy = MapProxy;