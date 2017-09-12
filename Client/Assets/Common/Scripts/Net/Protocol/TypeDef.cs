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
    public class TypeDef
    {
        #region Fields
        public int code;
        public Type type;
        public FieldInfo[] fields;
        #endregion


        #region Properties
    
        #endregion


        #region Constructors


        	
        #endregion


        #region Static Methods
        public static TypeDef valueOf(Type type, Dictionary<System.Type, TypeDef> typeDefs)
        {
            TypeDef t = new TypeDef();
            t.code = type.Name.GetHashCode();
            t.type = type;
            typeDefs.Add(type, t);
            t.fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);//注意这里不支持私有成员
            
            Type fieldType;
            for (int i = 0; i < t.fields.Length; ++i)
            {
                fieldType =t.fields[i].FieldType;
                if (ProtocolCoder.CanRegister(fieldType))
                    ProtocolCoder.instance.Register(fieldType);
            }
            
            
		    return t;
	    }
        #endregion


        #region Private Methods
    
        #endregion
        
        
    }
}
