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
    public class EnumProxy : Proxy
    {
        #region Fields
        
        #endregion


        #region Properties
    
        #endregion


        #region Constructors


        public EnumProxy()
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

            //类型的获取            
		    EnumDef def = ProtocolCoder.instance.getEnumDef(code);
		    if (def == null)
            {
                Debuger.LogError("枚举定义{0}不存在",type.Name);                
                return false;
		    }

            if(def.type != type)
            {
                Debuger.LogError("枚举类型不匹配。EnumDef:{0} Type:",def.type.Name,type.Name);                
                return false;
		    }

		    // 枚举值
            int valNum;
            if (!readVarInt32(ioBuffer, out valNum))
            {                
                return false;
            }

            value = System.Enum.ToObject(type, valNum);
            AddLog(value);
            return true;
        }

        public override bool setValue(IoBuffer ioBuffer, object value)
        {
            byte flag = Types.ENUM;
            
            EnumDef def = ProtocolCoder.instance.getEnumDef(value.GetType());
            if (def == null)
            {
                ProtocolCoder.instance.Register(value.GetType());
                def = ProtocolCoder.instance.getEnumDef(value.GetType());
                if (def == null)
                {
                    Debuger.LogError("找不到枚举的预定义：{0}", value.GetType().Name);
                    return false;
                }                
            }

            ioBuffer.Write(flag);
            //名字哈希值
            ioBuffer.Write(def.code);
            //枚举对应的数值
            putVarInt32(ioBuffer, (int)value);
            //Debuger.Log("{0} code:{1} value:{2}",value,def.code,(int)value);
            AddLog(value);
            return true;
        }

    

    }
}
