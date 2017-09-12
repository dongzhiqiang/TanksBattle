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
    public class BooleanProxy : Proxy
    {
        #region Fields
        #endregion

        #region Properties    
        #endregion

        #region Constructors
        public BooleanProxy()
        {
    
        }
        #endregion


        #region Static Methods
    
        #endregion


        #region Private Methods
    
        #endregion

        public override bool getValue(IoBuffer ioBuffer, System.Type type, byte flag, out object value)
        {
            value =null;
            byte ft = getFlagTypes(flag);
            if (ft != Types.BOOLEAN)
            {
                Debuger.LogError("不是bool类型");
                
                return false;
            }
            byte signal = getFlagSignal(flag);
            if (signal == 0x00)
            {
                value =  false;
            }
            else if (signal == 0x01)
            {
                value =  true;
            }
            else
            {
                Debuger.LogError("未知的类型:" + signal);
                
                return false;
            }
            AddLog(value);
            return true;
            
        }

        public override bool setValue(IoBuffer ioBuffer, object value)
        {
            
		    byte flag = Types.BOOLEAN;
            if((bool)value){
                flag |= 0x01;
                ioBuffer.Write(flag);
            }
            else{
                ioBuffer.Write(flag);
            }
            AddLog(value);
            return true;		    
        }
    }
}
