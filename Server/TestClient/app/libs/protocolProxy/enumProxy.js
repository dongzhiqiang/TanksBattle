"use strict";

var BaseProxy = require("./baseProxy").BaseProxy;
var EnumType = require("../enumType").EnumType;
var ProtocolCoder = require("../protocolCoder");

class EnumProxy extends BaseProxy
{
    /**
     * 读取数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {number} flag - 类型标记
     * @return {EnumType} 读取的数据
     * @override
     */
    getValue(ioBuf, flag)
    {
        //var typeFlag = BaseProxy.getFlagType(flag);
        //if (typeFlag !== BaseProxy.TYPE_FLAG_ENUM)
        //{
        //    throw new Error("EnumProxy~getValue 必须是EnumType");
        //}

        var code = ioBuf.readInt32();
        if (code === null)
        {
            throw new Error("EnumProxy~getValue 数据字节不足" + 4);
        }

        var objDesc = ProtocolCoder.instance.getEnumDef(code);
        if (!objDesc)
        {
            throw new Error("EnumProxy~getValue 未知枚举类型HashCode："  + code);
        }

        //这里可能会抛出错误，让上层处理
        var value = BaseProxy.readVarUInt(ioBuf);

        var enumItem = objDesc.type.getByValue(value);
        if (!enumItem)
        {
            //Function.name暂时是非ES标准
            throw new Error("EnumProxy~getValue 找不到枚举值，类型："  + objDesc.name + "，值：" + value);
        }

        return enumItem;
    }

    /**
     * 写入数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {EnumType} value - 数据
     * @override
     */
    setValue(ioBuf, value)
    {
        //if (!EnumType.isEnumType(value))
        //{
        //    throw new Error("EnumProxy~setValue 必须是EnumType");
        //}

        var type = value.constructor;
        var enumDef = ProtocolCoder.instance.getEnumDef(type);
        if (!enumDef)
        {
            enumDef = ProtocolCoder.instance.registerEnum(type);
            if (!enumDef)
            {
                throw new Error("EnumProxy~setValue 注册失败：" + type);
            }
        }

        ioBuf.writeUInt8(BaseProxy.TYPE_FLAG_ENUM);
        ioBuf.writeInt32(enumDef.code);
        BaseProxy.writeVarUInt(ioBuf, value.val);
    }
}

exports.EnumProxy = EnumProxy;