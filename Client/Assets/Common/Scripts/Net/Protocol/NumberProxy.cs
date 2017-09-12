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
    public class NumberProxy : Proxy
    {
        #region Fields
        public const byte INT32 = 0x01;
        public const byte INT64 = 0x02;
        public const byte FLOAT = 0x03;
        public const byte DOUBLE = 0x04;
        #endregion


        #region Properties
    
        #endregion


        #region Constructors


        public NumberProxy()
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

		    byte flagType = getFlagTypes(flag);
		    if (flagType != Types.NUMBER) {
			    Debuger.LogError("Types.NUMBER 类型解析出错 {0}",flag);
                
                return false;
		    }

		    // 0000 #000
		    bool nevigate = ((flag & FLAG_0X08) != 0); 
		
		    // 0000 0###
		    byte signal = getFlagSignal(flag);
		    if(signal == INT32) {
                int i;
                if(!readVarInt32(ioBuffer,out i))
                {                    
                    return false;
                }

                if (type == typeof(short))
			        value = (short)(nevigate ? -i : i);
                else if (type == typeof(byte))
                    value = (byte)(nevigate ? -i : i);
                else
                    value = nevigate ? -i : i;
                    
		    } else if(signal == INT64) {
			    long l;
                if(!readVarInt64(ioBuffer,out l))
                {                    
                    return false;
                }
                value = nevigate ? -l : l;
		    } else if(signal == FLOAT) {
                if (ioBuffer.ReadSize < 4)
                {
                    Debuger.LogError("可读字节长度不足" + 4);
                    return false;
                }
			    value = ioBuffer.ReadFloat();
		    } else if(signal == DOUBLE) {
                if (ioBuffer.ReadSize < 8)
                {
                    Debuger.LogError("可读字节长度不足" + 8);
                    return false;
                }
                value = ioBuffer.ReadDouble();
		    }
            AddLog(value);
            return true;
        }

        public override bool setValue(IoBuffer ioBuffer, object value)
        {
            byte flag = Types.NUMBER;
            if (value is int || value is byte || value is short)
            {
                int v = System.Convert.ToInt32(value);
                if (v < 0)
                {
                    flag |= FLAG_0X08 | INT32;
                    v = -v;
                }
                else
                {
                    flag |= INT32;
                }
                ioBuffer.Write(flag);
                putVarInt32(ioBuffer, v);
            }
            else if (value is long)
            {
                long v = (long)value;
                if(v < 0) {
				    flag |= FLAG_0X08 | INT64;
				    v = -v;
			    } else {
				    flag |= INT64;
			    }
			    ioBuffer.Write(flag);
                putVarInt64(ioBuffer, v);
            }
            else if(value is float){
                flag |= FLOAT;
                ioBuffer.Write(flag);
                ioBuffer.Write((float)value);
            }
            else if (value is double)
            {
                flag |= DOUBLE;
                ioBuffer.Write(flag);
                ioBuffer.Write((double)value);
            }
            else
            {
                Debuger.LogError("未知的类型：{0}", value);
                return false;
            }
            AddLog(value);
            return true;
        }

    

    }
}
