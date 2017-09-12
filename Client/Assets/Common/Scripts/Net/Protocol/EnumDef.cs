#region Header
/**
 * 名称：EnumDef
 
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
    public class EnumDef
    {
        #region Fields
        public int code;
        public Type type;
        public String[] names;
        public Enum[] values;
        #endregion


        #region Properties
    
        #endregion


        #region Constructors


        	
        #endregion


        #region Static Methods
        public static EnumDef valueOf( Type type) {
		    EnumDef e = new EnumDef();
            if(!type.IsEnum)
            {
                Debuger.LogError("逻辑错误。{0}不是枚举",type.Name);
                return null;
            }
            e.code = type.Name.GetHashCode();
            e.type = type;
            e.names = System.Enum.GetNames(type);
            Array arr = System.Enum.GetValues(type);
            e.values = new Enum[arr.Length];
            for(int i =0;i<arr.Length;++i)
            {
                e.values[i] = (Enum)arr.GetValue(i);
               // Debuger.LogError("" + (int)(object)(e.values[i]));
            }
		    return e;
	    }
        #endregion


        #region Private Methods
    
        #endregion
        
        
    }
}
