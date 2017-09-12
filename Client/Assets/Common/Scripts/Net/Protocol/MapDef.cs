#region Header
/**
 * 名称：TypeDef
 
 * 日期：2015.11.16
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
 
namespace NetCore
{
    public class MapDef
    {
        #region Fields
        public int code;
        public Type type;
        public Type keyType;
        public Type valueType;
        
        #endregion


        #region Properties
    
        #endregion


        #region Constructors


        	
        #endregion


        #region Static Methods

        public static MapDef valueOf(Type type)
        {
            
            MapDef t = new MapDef();
            t.code = type.GetHashCode();
            t.type = type;
            MethodInfo m = type.GetMethod("get_Item");//list的索引器的返回值就是对应类型
            if (m == null)
            {
                Debuger.LogError("找不到索引器 不能确定类型:" + type.Name);
                return null;
            }
            
            t.keyType = m.GetParameters()[0].ParameterType;
            t.valueType = m.ReturnType;

            if (ProtocolCoder.CanRegister(t.keyType))
                ProtocolCoder.instance.Register(t.keyType);
            if (ProtocolCoder.CanRegister(t.valueType))
                ProtocolCoder.instance.Register(t.valueType);               
            
		    return t;
	    }
        #endregion


        #region Private Methods
    
        #endregion
        
        
    }
}
