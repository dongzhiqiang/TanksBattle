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
using System;

namespace NetCore
{
    public class DateProxy : Proxy
    {
        #region Fields
       
        #endregion


        #region Properties
    
        #endregion


        #region Constructors


        public DateProxy()
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

		    long timestamp;
            if(!readVarInt64(ioBuffer,out timestamp))
            {                
                return false;
            }
                

		    // #### 0000
            value = TimeMgr.instance.TimestampToClientDateTime(timestamp);
            AddLog(value);
            return true;
        }

        public override bool setValue(IoBuffer ioBuffer, object value)
        {
            byte flag = Types.DATE_TIME;
		    // #### 0000
            ioBuffer.Write(flag);

            long timestame = Math.Max(0, TimeMgr.instance.GetTimestampFromDate((System.DateTime)value));

            putVarInt64(ioBuffer, timestame);
            AddLog(value);
            return true;
        }

    

    }
}
