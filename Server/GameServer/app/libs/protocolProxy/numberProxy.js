"use strict";

var appUtil = require("../appUtil");
var BaseProxy = require("./baseProxy").BaseProxy;

const INT32  = 0x01;
const INT64  = 0x02;
const FLOAT  = 0x03;
const DOUBLE = 0x04;

class NumberProxy extends BaseProxy
{
    /**
     * 读取数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {number} flag - 类型标记
     * @return {number} 读取的数据
     * @override
     */
    getValue(ioBuf, flag) {
        //var typeFlag = BaseProxy.getFlagType(flag);
        //if (typeFlag !== BaseProxy.TYPE_FLAG_NUMBER) {
        //    throw new Error("NumberProxy~getValue 必须是number类型");
        //}

        var signal = (flag & BaseProxy.FLAG_0X08) !== 0 ? -1 : 1;
        var subType = BaseProxy.getSubFlag(flag);
        var value = 0;
        switch (subType)
        {
            case INT32:
                value = BaseProxy.readVarUInt(ioBuf) * signal;
                break;
            case INT64:
                value = BaseProxy.readVarUInt(ioBuf) * signal;
                break;
            case FLOAT:
                value = ioBuf.readFloat();
                break;
            case DOUBLE:
                value = ioBuf.readDouble();
                break;
            default:
                throw new Error("NumberProxy~getValue 数字类型不对：" + subType);
                break;
        }
        if (value === null)
        {
            throw new Error("NumberProxy~getValue 数据字节不足");
        }
        return value;
    }

    /**
     * 写入数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {number} value - 数据
     * @override
     */
    setValue(ioBuf, value)
    {
        //if (!Object.isNumber(value))
        //{
        //    throw new Error("NumberProxy~setValue 必须是number类型");
        //}

        var flag = BaseProxy.TYPE_FLAG_NUMBER;
        //如果去除小数点后等于自己，那就说明是整数
        var isInteger = Math.floor(value) === value;
        if (isInteger)
        {
            //符号位存到flag
            flag = flag | (value < 0 ? BaseProxy.FLAG_0X08 : 0) | ((value > appUtil.INT32_MAX_POSITIVE || value < appUtil.INT32_MAX_NEGTIVE) ? INT64 : INT32);
            ioBuf.writeUInt8(flag);
            BaseProxy.writeVarUInt(ioBuf, Math.abs(value));
        }
        //如果数值范围不可以用float装载那就用double
        else if (value > appUtil.FLOAT_MAX || value < -appUtil.FLOAT_MAX)
        {
            flag |= DOUBLE;
            ioBuf.writeUInt8(flag);
            ioBuf.writeDouble(value);
        }
        else
        {
            flag |= FLOAT;
            ioBuf.writeUInt8(flag);
            ioBuf.writeFloat(value);
        }
    }
}

exports.NumberProxy = NumberProxy;