#region Header
/**
 * 名称：NumberProxy
 
 * 日期：2015.11.16
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NetCore
{
    public class ArrayProxy : Proxy
    {
        #region Fields

        #endregion


        #region Properties

        #endregion


        #region Constructors


        public ArrayProxy()
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
            if (!type.IsArray)
            {
                Debuger.LogError("不是数组类型");
                return false;
            }

            int len;
            if (!readVarInt32(ioBuffer, out len))
            {
                return false;
            }

            Type elemType = type.GetElementType();
            Array arr = Array.CreateInstance(elemType, len);

            BeginParentLog("数组", len);
            bool needNewLine = elemType.IsClass && typeof(string) != elemType.GetElementType();

            for (int i = 0; i < len; i++)
            {
                if (needNewLine) AddLogNewLine("");
                object e;
                if (!ProtocolCoder.instance.Decode(ioBuffer, elemType, out e))
                    return false;
                arr.SetValue(e, i);
                AddLog(",");//加个分隔符
            }
            value = arr;
            EndParentLog();

            return true;
        }

        public override bool setValue(IoBuffer ioBuffer, object value)
        {
            byte flag = Types.ARRAY;

            // #### 0000
            ioBuffer.Write(flag);
            System.Array arr = (System.Array)value;
            if (arr == null)
            {
                Debuger.LogError("不能转为array类型：{0}", value.GetType().Name);
                return false;
            }

            BeginParentLog("数组", arr.Length);
            bool needNewLine = value.GetType().GetElementType().IsClass && typeof(string) != value.GetType().GetElementType();
            putVarInt32(ioBuffer, arr.Length);
            for (int i = 0; i < arr.Length; i++)
            {
                if (needNewLine) AddLogNewLine("");
                ProtocolCoder.instance.Encode(ioBuffer, arr.GetValue(i));
                AddLog(",");//加个分隔符
            }
            EndParentLog();

            return true;
        }
    }
}
