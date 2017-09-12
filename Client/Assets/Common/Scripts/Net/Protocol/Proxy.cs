#region Header
/**
 * 名称：类模板
 
 * 日期：201x.xx.xx
 * 描述：新建类的时候建议用这个模板
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NetCore
{
    public abstract class Proxy
    {
        #region Fields
        // 1111 0000 
        public const byte TYPE_MASK = (byte)0xF0;

        // 0000 0111
        public const byte SIGNAL_MASK = (byte)0x07;

        // 1000 0000
        public const int FLAG_0X80 = 0x80;

        // 0000 1000
	    public const int FLAG_0X08 = 0x08;
        #endregion


        #region Properties
    
        #endregion


        #region Constructors
    

        public Proxy()
        {
    
        }
        #endregion


        #region Static Methods
        public static bool readVarInt32(IoBuffer ioBuffer,out int value) {
            byte tag = ioBuffer.ReadByte();
		    // 1### #### (128 - (byte)0x80)
		    if((tag & FLAG_0X80) == 0) {
			    value= tag & 0x7F;
                return true;
		    }
		
		    int signal = tag & SIGNAL_MASK;
		    if(ioBuffer.ReadSize< signal) {
                Debuger.LogError("readVarInt32 可读字节长度不足" + signal);
                value = 0;
                return false;
		    }
		
		    if(signal > 4 || signal < 1) {
                Debuger.LogError("readVarInt32 字节数必须1~4");
                value = 0;
                return false;
		    }

            value = 0;
		    for(int i = 8*(signal-1); i>=0; i-=8) {
                byte b = ioBuffer.ReadByte();
                value |= (b & 0xFF) << i;
		    }
		    return true;
	    }
        public static void putVarInt32(IoBuffer ioBuffer, int value)  {
		    if(value < 0) {
			    // 不能 < 0
			    Debuger.LogError("不能小于0");
		    }
		
		    // 1### #### (128 - (byte)0x80)
		    if(value < FLAG_0X80) {
			    byte b = (byte) value;
			    ioBuffer.Write(b);
		    } 
            else if((value >> 24) > 0) {
				byte b = (byte) (FLAG_0X80 | 4);
				ioBuffer.Write(b);
				// 
				byte b1 = (byte)(value >> 24 & 0xFF);
				byte b2 = (byte)(value >> 16 & 0xFF);
				byte b3 = (byte)(value >> 8 & 0xFF);
				byte b4 = (byte)(value & 0xFF);
				ioBuffer.Write(b1);
				ioBuffer.Write(b2);
				ioBuffer.Write(b3);
				ioBuffer.Write(b4);
			} else if((value >> 16) > 0) {
				byte b = (byte) (FLAG_0X80 | 3);
				ioBuffer.Write(b);
				//
				byte b2 = (byte)(value >> 16 & 0xFF);
				byte b3 = (byte)(value >> 8 & 0xFF);
				byte b4 = (byte)(value & 0xFF);
				ioBuffer.Write(b2);
				ioBuffer.Write(b3);
				ioBuffer.Write(b4);
			} else if((value >> 8) > 0) {
				byte b = (byte) (FLAG_0X80 | 2);
				ioBuffer.Write(b);
				// 
				byte b3 = (byte)(value >> 8 & 0xFF);
				byte b4 = (byte)(value & 0xFF);
				ioBuffer.Write(b3);
				ioBuffer.Write(b4);
			} else {
				byte b = (byte) (FLAG_0X80 | 1);
				ioBuffer.Write(b);
				//
				byte b4 = (byte)(value & 0xFF);
				ioBuffer.Write(b4);
			}
		     
	    }

        public static bool readVarInt64(IoBuffer ioBuffer,out long value) {
            byte tag = ioBuffer.ReadByte();
		    // 1### #### (128 - (byte)0x80)
		    if((tag & FLAG_0X80) == 0) {
			    value= tag & 0x7F;
                return true;
		    }
        
		    int signal = tag & SIGNAL_MASK;
		    if(ioBuffer.ReadSize< signal) {
                Debuger.LogError("readVarInt64 可读字节长度不足" + signal);
                value = 0;
                return false;
		    }
		
		    if(signal > 8 || signal < 1) {
                Debuger.LogError("readVarInt64 字节数必须1~8");
                value = 0;
                return false;
		    }

            value = 0;
		    for(int i = 8*(signal-1); i>=0; i-=8) {
                byte b = ioBuffer.ReadByte();
                value |= (long)(b & 0xFF) << i;
		    }
            return true;
	    }

        public static void putVarInt64(IoBuffer ioBuffer, long value)  {
            if (value < 0)
            {
                // 不能 < 0
                Debuger.LogError("不能小于0");
            }

            if(value <= int.MaxValue)
            {
                putVarInt32(ioBuffer, (int)value);
            }
            else if((value >> 56) > 0) {
				byte b = (byte) (FLAG_0X80 | 8);
				ioBuffer.Write(b);
				// 
                byte b1 = (byte)(value >> 56 & 0xFF);
				byte b2 = (byte)(value >> 48 & 0xFF);
				byte b3 = (byte)(value >> 40 & 0xFF);
				byte b4 = (byte)(value >> 32 & 0xFF);
				byte b5 = (byte)(value >> 24 & 0xFF);
				byte b6 = (byte)(value >> 16 & 0xFF);
				byte b7 = (byte)(value >> 8 & 0xFF);
				byte b8 = (byte)(value & 0xFF);
				ioBuffer.Write(b1);
				ioBuffer.Write(b2);
				ioBuffer.Write(b3);
				ioBuffer.Write(b4);
				ioBuffer.Write(b5);
				ioBuffer.Write(b6);
				ioBuffer.Write(b7);
                ioBuffer.Write(b8);
			}
            else if((value >> 48) > 0) {
				byte b = (byte) (FLAG_0X80 | 7);
				ioBuffer.Write(b);
				// 
				byte b1 = (byte)(value >> 48 & 0xFF);
				byte b2 = (byte)(value >> 40 & 0xFF);
				byte b3 = (byte)(value >> 32 & 0xFF);
				byte b4 = (byte)(value >> 24 & 0xFF);
				byte b5 = (byte)(value >> 16 & 0xFF);
				byte b6 = (byte)(value >> 8 & 0xFF);
				byte b7 = (byte)(value & 0xFF);
				ioBuffer.Write(b1);
				ioBuffer.Write(b2);
				ioBuffer.Write(b3);
				ioBuffer.Write(b4);
				ioBuffer.Write(b5);
				ioBuffer.Write(b6);
				ioBuffer.Write(b7);
			} else if((value >> 40) > 0) {
				byte b = (byte) (FLAG_0X80 | 6);
				ioBuffer.Write(b);
				//
				byte b2 = (byte)(value >> 40 & 0xFF);
				byte b3 = (byte)(value >> 32 & 0xFF);
				byte b4 = (byte)(value >> 24 & 0xFF);
				byte b5 = (byte)(value >> 16 & 0xFF);
				byte b6 = (byte)(value >> 8 & 0xFF);
				byte b7 = (byte)(value & 0xFF);
				ioBuffer.Write(b2);
				ioBuffer.Write(b3);
				ioBuffer.Write(b4);
				ioBuffer.Write(b5);
				ioBuffer.Write(b6);
				ioBuffer.Write(b7);
			} else if((value >> 32) > 0) {
				byte b = (byte) (FLAG_0X80 | 5);
				ioBuffer.Write(b);
				// 
				byte b3 = (byte)(value >> 32 & 0xFF);
				byte b4 = (byte)(value >> 24 & 0xFF);
				byte b5 = (byte)(value >> 16 & 0xFF);
				byte b6 = (byte)(value >> 8 & 0xFF);
				byte b7 = (byte)(value & 0xFF);
				ioBuffer.Write(b3);
				ioBuffer.Write(b4);
				ioBuffer.Write(b5);
				ioBuffer.Write(b6);
				ioBuffer.Write(b7);
			}
            else
            {
                byte b = (byte)(FLAG_0X80 | 4);
                ioBuffer.Write(b);
                // 
                byte b4 = (byte)(value >> 24 & 0xFF);
                byte b5 = (byte)(value >> 16 & 0xFF);
                byte b6 = (byte)(value >> 8 & 0xFF);
                byte b7 = (byte)(value & 0xFF);
                ioBuffer.Write(b4);
                ioBuffer.Write(b5);
                ioBuffer.Write(b6);
                ioBuffer.Write(b7);
            }
		    
	    }

        /**
         * Types
         * 
         * @return #### 0000
         */
        public static byte getFlagTypes(byte flag)
        {
            byte code = (byte)(flag & TYPE_MASK);
            if (code == 0)
            {
                return flag;
            }
            return code;
        }

        /**
         * Signal
         * 
         * @return 0000 0###
         */
        public static byte getFlagSignal(byte flag)
        {
            byte signal = (byte)(flag & SIGNAL_MASK);
            return signal;
        }
        #endregion


        #region Private Methods
    
        #endregion


        public abstract bool getValue(IoBuffer ioBuffer, System.Type type, byte flag,out object value);
        public abstract bool setValue(IoBuffer ioBuffer, object value);

        
        public static void AddLog( string str)
        {
#if COM_DEBUG
            ProtocolCoder.Instance.Log.AppendFormat("{0}", str);
#endif
        }

        public static void AddLogNewLine(string str)
        {
#if COM_DEBUG
            ProtocolCoder.Instance.Log.AppendFormat("\n{0}{1}", "".PadLeft(ProtocolCoder.Instance.Indent * 4, ' '), str);
#endif
        }


        public static void AddLog(object obj)
        {
#if COM_DEBUG
            if (obj== null)
                ProtocolCoder.Instance.Log.Append("空");
            else
                ProtocolCoder.Instance.Log.Append(obj.ToString());
#endif
        }

        public static void BeginParentLog(string name, int count)
        {
#if COM_DEBUG
            ProtocolCoder.Instance.Log.AppendFormat("{0}{1} 数量:{2}{{", "".PadLeft(ProtocolCoder.Instance.Indent * 4, ' '), name, count);
            ProtocolCoder.Instance.Indent +=1;
#endif
        }
        public static void EndParentLog()
        {
#if COM_DEBUG
            ProtocolCoder.Instance.Indent -= 1;
            ProtocolCoder.Instance.Log.AppendFormat("{0}}} ", "".PadLeft(ProtocolCoder.Instance.Indent * 4));
#endif
        }

    }
}