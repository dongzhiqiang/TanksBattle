"use strict";

var logUtil = require("../logUtil");
var BaseProxy = require("./baseProxy").BaseProxy;
var ProtocolCoder = require("../protocolCoder");

class ObjectProxy extends BaseProxy
{
    /**
     * 读取数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {number} flag - 类型标记
     * @return {object} 读取的数据
     * @override
     */
    getValue(ioBuf, flag) {
        //var typeFlag = BaseProxy.getFlagType(flag);
        //if (typeFlag !== BaseProxy.TYPE_FLAG_OBJECT) {
        //    throw new Error("ObjectProxy~getValue 必须是Object类型");
        //}

        var coder = ProtocolCoder.instance;

        var code = ioBuf.readInt32();
        if (code === null) {
            throw new Error("ObjectProxy~getValue 数据字节不足" + 4);
        }

        //这里可能会抛出错误，让上层处理
        var fieldCount = BaseProxy.readVarUInt(ioBuf);

        var objLen = ioBuf.readInt32();
        if (objLen === null) {
            throw new Error("ObjectProxy~getValue 数据字节不足" + 4);
        }
        //减去objLen自己占用4字节
        objLen -= 4;

        if (ioBuf.canReadLen() < objLen) {
            throw new Error("ObjectProxy~getValue 数据字节不足" + objLen);
        }

        var objDesc = coder.getTypeDef(code);
        if (!objDesc) {
            throw new Error("ObjectProxy~getValue 未知类型HashCode：" + code);
        }

        //创建实例
        var obj = new objDesc.type();

        //先保存读取位置，如果后面有多余的字段，就跳过去
        var readPos1 = ioBuf.getReadPos();

        for (var i = 0; i < fieldCount; ++i) {
            var fieldDesc = objDesc.fields[i];
            if (fieldDesc) {
                var fieldVal = coder.decode(ioBuf);
                //如果是null，得看看允不允许null
                if (fieldVal === null || fieldVal === undefined) {
                    if (fieldDesc.notNull)
                        //Function.name暂时是非ES标准
                        throw new Error("ObjectProxy~getValue 成员不能为null，主类型：" + objDesc.name + "，成员名：" + fieldDesc.name);
                }
                //如果不是null，得用对类型
                else {
                    if (fieldVal.constructor !== fieldDesc.type)
                        //Function.name暂时是非ES标准
                        throw new Error("ObjectProxy~getValue 成员数据类型不对，主类型：" + objDesc.name + "，成员名：" + fieldDesc.name + "，实际类型：" + fieldVal.constructor.name + "，期望类型：" + fieldDesc.type.name);
                }

                obj[fieldDesc.name] = fieldVal;
            }
            else {
                logUtil.warn("ObjectProxy~getValue 实际数据成员数比配置的多，主类型：" + objDesc.name);

                //居然实现字段比描述的字段还多，看来对方加字段了，这里跳过这个对象剩余的数据吧
                var readPos2 = ioBuf.getReadPos();
                //对象总长度减去已读的长度
                ioBuf.skip(objLen - (readPos2 - readPos1));
                break;
            }
        }

        return obj;
    }

    /**
     * 写入数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {object} value - 数据
     * @override
     */
    setValue(ioBuf, value)
    {
        //if (!Object.isObject(value))
        //{
        //    throw new Error("ObjectProxy~setValue 必须是Object");
        //}

        var coder = ProtocolCoder.instance;
        var type = value.constructor;
        if (!coder.canRegisterType(type))
        {
            throw new Error("ObjectProxy~setValue 类型必须带上字段描述");
        }

        var typeDef = coder.getTypeDef(type);
        if (!typeDef) {
            typeDef = coder.registerType(type);
            if (!typeDef) {
                throw new Error("ObjectProxy~setValue 注册失败：" + type);
            }
        }

        ioBuf.writeUInt8(BaseProxy.TYPE_FLAG_OBJECT);
        ioBuf.writeInt32(typeDef.code);

        var fields = typeDef.fields;
        var fieldCount = fields.length;
        BaseProxy.writeVarUInt(ioBuf, fieldCount);

        //获取当前要写入的位置
        var relWritePos1 = ioBuf.getRelativeWritePos();
        //对象长度字段先占位
        ioBuf.writeInt32(0);

        for (var i = 0; i < fieldCount; ++i) {
            var fieldDesc = fields[i];
            var fieldVal = value[fieldDesc.name];
            //如果是null，得看看允不允许null
            if (fieldVal === null || fieldVal === undefined) {
                if (fieldDesc.notNull)
                    //Function.name暂时是非ES标准
                    throw new Error("ObjectProxy~getValue 成员不能为null，主类型：" + type.name + "，成员名：" + fieldDesc.name);
            }
            //如果不是null，得用对类型
            else {
                if (fieldVal.constructor !== fieldDesc.type)
                    //Function.name暂时是非ES标准
                    throw new Error("ObjectProxy~getValue 成员数据类型不对，主类型：" + type.name + "，成员名：" + fieldDesc.name + "，实际类型：" + fieldVal.constructor.name + "，期望类型：" + fieldDesc.type.name);
            }

            coder.encode(ioBuf, fieldVal);
        }

        //再次获取当前要写入的位置
        var relWritePos2 = ioBuf.getRelativeWritePos();
        //两个位置相减得到对象的序列化大小
        var objectLen = relWritePos2 - relWritePos1;
        //更新序列化大小字段
        ioBuf.writeInt32WithRelativePos(objectLen, relWritePos1);
    }
}

exports.ObjectProxy = ObjectProxy;