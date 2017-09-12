"use strict";

var appUtil = require("../appUtil");

class BaseProxy
{
    /**
     * 从IOBuffer读取正整数，根据读取的标记值，读取字节数会不一样
     * @param {IOBuffer} ioBuf
     * @return {number}
     */
    static readVarUInt(ioBuf)
    {
        var tag = ioBuf.readUInt8();
        if ((tag & BaseProxy.FLAG_0X80) === 0)
        {
            return tag & 0x7F;
        }

        var byteCount = tag & BaseProxy.SUB_FLAG_MASK;
        if (ioBuf.canReadLen() < byteCount)
        {
            throw new Error("BaseProxy~readVarUInt 字节不足，需要：" + byteCount + "，实际：" + ioBuf.canReadLen());
        }

        //这里调用成员函数，这里没有用ioBuf["readUInt" + byteCount]()，是因为这种字符串拼接再调用的模式性能不好
        var  value = null;
        switch (byteCount)
        {
            case 8:
                value = ioBuf.readUInt64();
                break;
            case 7:
                value = ioBuf.readUInt56();
                break;
            case 6:
                value = ioBuf.readUInt48();
                break;
            case 5:
                value = ioBuf.readUInt40();
                break;
            case 4:
                value = ioBuf.readUInt32();
                break;
            case 3:
                value = ioBuf.readUInt24();
                break;
            case 2:
                value = ioBuf.readUInt16();
                break;
            case 1:
                value = ioBuf.readUInt8();
                break;
            default:
                throw new Error("BaseProxy~readVarUInt 字节数范围不对(1~8)：" + byteCount);
                break;
        }
        return value;
    }

    /**
     * 写入正整数，根据大小写入不同的字节数，由于js是double类型，不能写入超过2^53的整数
     * @param {IOBuffer} ioBuf
     * @param {number} value
     */
    static writeVarUInt(ioBuf, value)
    {
        if (value < 0 || value > appUtil.INT_MAX_POSITIVE)
        {
            throw new Error("BaseProxy~writeVarUInt 写入的数必须是0 - 2^53范围内：" + value);
        }

        //这里没用移位来判断数值大小，是因为JS不支持大于2^32的移位
        if (value < BaseProxy.FLAG_0X80)
        {
            ioBuf.writeUInt8(value);
        }
        else if (value < 0x100)
        {
            ioBuf.writeUInt8(BaseProxy.FLAG_0X80 | 1);
            ioBuf.writeUInt8(value);
        }
        else if (value < 0x10000)
        {
            ioBuf.writeUInt8(BaseProxy.FLAG_0X80 | 2);
            ioBuf.writeUInt16(value);
        }
        else if (value < 0x1000000)
        {
            ioBuf.writeUInt8(BaseProxy.FLAG_0X80 | 3);
            ioBuf.writeUInt24(value);
        }
        else if (value < 0x100000000)
        {
            ioBuf.writeUInt8(BaseProxy.FLAG_0X80 | 4);
            ioBuf.writeUInt32(value);
        }
        else if (value < 0x10000000000)
        {
            ioBuf.writeUInt8(BaseProxy.FLAG_0X80 | 5);
            ioBuf.writeUInt40(value);
        }
        else if (value < 0x1000000000000)
        {
            ioBuf.writeUInt8(BaseProxy.FLAG_0X80 | 6);
            ioBuf.writeUInt48(value);
        }
        else if (value < 0x100000000000000)
        {
            ioBuf.writeUInt8(BaseProxy.FLAG_0X80 | 7);
            ioBuf.writeUInt56(value);
        }
        else
        {
            ioBuf.writeUInt8(BaseProxy.FLAG_0X80 | 8);
            ioBuf.writeUInt64(value);
        }
    }

    /**
     * 获取
     * @param {number} flag
     */
    static getFlagType(flag)
    {
        return (flag & BaseProxy.TYPE_MASK) || flag;
    }

    static getSubFlag(flag)
    {
        return flag & BaseProxy.SUB_FLAG_MASK;
    }

    /**
     * 读取数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {number} flag - 类型标记
     * @return {*} 读取的数据
     * @abstract
     */
    getValue(ioBuf, flag)
    {
        throw new Error("BaseProxy~getValue 必须实现基类的该方法");
    }

    /**
     * 写入数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {*} value - 数据
     * @abstract
     */
    setValue(ioBuf, value)
    {
        throw new Error("BaseProxy~setValue 必须实现基类的该方法");
    }
}

BaseProxy.TYPE_MASK             = 0xF0;
BaseProxy.SUB_FLAG_MASK         = 0x07;
BaseProxy.FLAG_0X80             = 0x80;
BaseProxy.FLAG_0X08             = 0x08;

BaseProxy.TYPE_FLAG_OBJECT      = 0xF0;
BaseProxy.TYPE_FLAG_STRING      = 0xE0;
BaseProxy.TYPE_FLAG_ARRAY       = 0xD0;
BaseProxy.TYPE_FLAG_MAP         = 0xC0;
BaseProxy.TYPE_FLAG_BYTEARRAY   = 0xB0;
BaseProxy.TYPE_FLAG_DATETIME    = 0xA0;
BaseProxy.TYPE_FLAG_LIST        = 0x90;
BaseProxy.TYPE_FLAG_ENUM        = 0x50;
BaseProxy.TYPE_FLAG_BOOLEAN     = 0x20;
BaseProxy.TYPE_FLAG_NUMBER      = 0x10;
BaseProxy.TYPE_FLAG_NULL        = 0x01;
BaseProxy.TYPE_FLAG_UNKOWN      = 0x00;

exports.BaseProxy = BaseProxy;