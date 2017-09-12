"use strict";

var BaseProxy = require("./baseProxy").BaseProxy;

class BooleanProxy extends BaseProxy
{
    /**
     * 读取数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {number} flag - 类型标记
     * @return {boolean} 读取的数据
     * @override
     */
    getValue(ioBuf, flag)
    {
        //var typeFlag = BaseProxy.getFlagType(flag);
        //if (typeFlag !== BaseProxy.TYPE_FLAG_BOOLEAN)
        //{
        //    throw new Error("BooleanProxy~getValue 必须是boolean类型");
        //}

        return BaseProxy.getSubFlag(flag) !== 0x00;
    }

    /**
     * 写入数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {boolean} value - 数据
     * @override
     */
    setValue(ioBuf, value)
    {
        //if (!Object.isBoolean(value))
        //{
        //    throw new Error("BooleanProxy~setValue 必须是boolean类型");
        //}

        ioBuf.writeUInt8(BaseProxy.TYPE_FLAG_BOOLEAN | (value ? 0x01 : 0x00));
    }
}

exports.BooleanProxy = BooleanProxy;