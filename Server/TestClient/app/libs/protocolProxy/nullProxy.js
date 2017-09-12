"use strict";

var BaseProxy = require("./baseProxy").BaseProxy;

class NullProxy extends BaseProxy
{
    /**
     * 读取数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {number} flag - 类型标记
     * @return {null} 读取的数据
     * @override
     */
    getValue(ioBuf, flag)
    {
        //var typeFlag = BaseProxy.getFlagType(flag);
        //if (typeFlag !== BaseProxy.TYPE_FLAG_NULL)
        //{
        //    throw new Error("NullProxy~getValue 必须是null类型");
        //}

        return null;
    }

    /**
     * 写入数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {null|undefined} value - 数据
     * @override
     */
    setValue(ioBuf, value)
    {
        //if (value !== null && value !== undefined)
        //{
        //    throw new Error("NullProxy~setValue 必须是null或undefined");
        //}

        ioBuf.writeUInt8(BaseProxy.TYPE_FLAG_NULL);
    }
}

exports.NullProxy = NullProxy;