#region Header
/**
 * 名称：NumberProxy
 
 * 日期：2015.11.16
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace NetCore
{
    public class ObjectProxy : Proxy
    {
        #region Fields
        
        #endregion


        #region Properties
    
        #endregion


        #region Constructors


        public ObjectProxy()
        {
    
        }
        #endregion


        #region Static Methods
    
        #endregion


        #region Private Methods
    
        #endregion

        public override bool getValue(IoBuffer ioBuffer, System.Type type, byte flag, out object value)
        {
            value = null;

            if (ioBuffer.ReadSize < 4)
            {
                Debuger.LogError("可读字节长度不足" + 4);
                return false;
            }

            int code = ioBuffer.ReadInt32();

            //取键数
            int fieldCount;
            if (!readVarInt32(ioBuffer, out fieldCount))
            {
                return false;
            }

            if (ioBuffer.ReadSize < 4)
            {
                Debuger.LogError("可读字节长度不足" + 4);
                return false;
            }

            var objLen = ioBuffer.ReadInt32();
            //减去objLen自己占用4字节
            objLen -= 4;

            if (ioBuffer.ReadSize < objLen)
            {
                Debuger.LogError("可读字节长度不足" + objLen);
                return false;
            }

            // 类型的获取
            TypeDef def = ProtocolCoder.instance.getTypeDef(code);
            if (def == null)
            {
                Debuger.LogError("类型定义{0}不存在", type.Name);
                return false;
            }
            if (def.type != type)
            {
                Debuger.LogError("类型不匹配。TypeDef:{0} Type:{1}", def.type.Name, type.Name);
                return false;
            }

            value = System.Activator.CreateInstance(type);

            //先保存读取位置，如果后面有多余的字段，就跳过去
            var readPos1 = ioBuffer.ReadPos;

            BeginParentLog(type.Name, fieldCount);
            var fieldCountInCfg = def.fields.Length;
            for (int i = 0; i < fieldCount; i++)
            {
                if (i < fieldCountInCfg)
                {
                    FieldInfo fieldInfo = def.fields[i];

                    AddLogNewLine(fieldInfo.Name + ": ");//加个分隔符
                    object obj;
                    if (!ProtocolCoder.instance.Decode(ioBuffer, fieldInfo.FieldType, out obj))
                    {
                        return false;
                    }
                    fieldInfo.SetValue(value, obj);
                    AddLog(",");//加个分隔符
                }
                else
                {
                    Debuger.Log("实际数据成员数比配置的多，主类型：{0}", type.Name);

                    //居然实现字段比描述的字段还多，看来对方加字段了，这里跳过这个对象剩余的数据吧
                    var readPos2 = ioBuffer.ReadPos;
                    //对象总长度减去已读的长度
                    ioBuffer.Skip(objLen - (readPos2 - readPos1));
                    break;
                }
            }
            EndParentLog();

            return true;
        }

        public override bool setValue(IoBuffer ioBuffer, object value)
        {
		    byte flag = Types.OBJECT;

            TypeDef def = ProtocolCoder.instance.getTypeDef(value.GetType());
            if (def == null)
            {
                if (ProtocolCoder.CanRegister(value.GetType()))
                    ProtocolCoder.instance.Register(value.GetType());
                def = ProtocolCoder.instance.getTypeDef(value.GetType());
                if (def == null)
                {
                    Debuger.LogError("找不到对象的预定义：{0}", value.GetType().Name);
                    return false;
                }
            }

            // 类型标记，#### 0000
            ioBuffer.Write(flag);

            //名字哈希值
            ioBuffer.Write(def.code);

            // 字段数量
            putVarInt32(ioBuffer, (int)def.fields.Length);

            //获取当前要写入的位置
            var writePos1 = ioBuffer.WritePos;
            //对象长度字段先占位
            ioBuffer.Write(0);

            BeginParentLog(value.GetType().Name, def.fields.Length);
            //Debuger.Log("{0} code:{1} 字段数:{2}", value, def.code, def.fields.Length);
            // 遍历属性
            for (int i = 0; i < def.fields.Length; i++)
            {
                AddLogNewLine(def.fields[i].Name + ": ");//加个分隔符
                if (!ProtocolCoder.instance.Encode(ioBuffer, def.fields[i].GetValue(value)))
                    return false;
                AddLog(",");//加个分隔符
            }
            EndParentLog();

            //再次获取当前要写入的位置
            var writePos2 = ioBuffer.WritePos;
            //两个位置相减得到对象的序列化大小
            var objectLen = writePos2 - writePos1;
            //更新序列化大小字段
            ioBuffer.WriteBack(objectLen, writePos1);

            return true;
        }

    

    }
}
