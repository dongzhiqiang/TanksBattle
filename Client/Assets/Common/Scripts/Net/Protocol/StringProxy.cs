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

namespace NetCore
{
    public class StringProxy : Proxy
    {
        #region Fields
        public IoBuffer m_buff= new IoBuffer(128);//注意如果以后做多线程就不能共用同一个buffer
        #endregion


        #region Properties
    
        #endregion


        #region Constructors


        public StringProxy()
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

            int len;
            if (!readVarInt32(ioBuffer, out len))
            {
                return false;
            }

            if (ioBuffer.ReadSize < len)
            {
                Debuger.LogError("可读字节长度不足" + len);
                return false;
            }

            string str = ioBuffer.ReadOnlyStr(len);

            value = str;

            AddLog(value);
            return true;
        }

        public override bool setValue(IoBuffer ioBuffer, object value)
        {
            byte flag = Types.STRING;
            string str = (string)value;

            ioBuffer.Write(flag);
            int length = System.Text.Encoding.UTF8.GetByteCount(str);
            putVarInt32(ioBuffer, length);
            ioBuffer.WriteOnlyStr(str, length);

            AddLog(value);
            return true;
        }
    }
}
