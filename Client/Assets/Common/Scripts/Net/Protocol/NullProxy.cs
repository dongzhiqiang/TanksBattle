#region Header
/**
 * 名称：NullProxy
 
 * 日期：2015.11.16
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NetCore
{
    public class NullProxy : Proxy
    {
        #region Fields
    
        #endregion


        #region Properties
    
        #endregion


        #region Constructors


        public NullProxy()
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
            AddLog(null);
            return true;
        }

        public override bool setValue(IoBuffer ioBuffer, object value)
        {
            byte flag = Types.NULL;
            ioBuffer.Write(flag);
            AddLog(null);
            return true;
        }

    

    }
}