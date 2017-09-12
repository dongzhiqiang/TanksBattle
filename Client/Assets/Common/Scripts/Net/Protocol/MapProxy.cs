#region Header
/**
 * 名称：NumberProxy
 
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
    public class MapProxy : Proxy
    {
        #region Fields
        
        #endregion


        #region Properties
    
        #endregion


        #region Constructors


        public MapProxy()
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

            // 对象解析
            value = Activator.CreateInstance(type);
            IDictionary dict = (IDictionary)value;
            if (dict == null)
            {
                Debuger.LogError("类型不是map:{0}", type.Name);

                return false;
            }

            //获取key/value的类型
            MapDef def = ProtocolCoder.instance.getMapDef(type);
            if (def == null)
            {
                Debuger.LogError("map定义{0}不存在", type.FullName);
                return false;
            }
            Type keyType = def.keyType;
            Type valueType = def.valueType;

            //取键数
            int len;
            if (!readVarInt32(ioBuffer, out len))
            {
                return false;
            }

            BeginParentLog("Dictionary", len);
            for (int i = 0; i < len; i++)
            {
                AddLogNewLine("[key]:");//加个分隔符
                object keyObj;
                if (!ProtocolCoder.instance.Decode(ioBuffer, keyType, out keyObj))
                {
                    return false;
                }
                AddLog(" [value]:");//加个分隔符
                object valueObj;
                if (!ProtocolCoder.instance.Decode(ioBuffer, valueType, out valueObj))
                {
                    return false;
                }
                dict.Add(keyObj, valueObj);
                AddLog(",");//加个分隔符
            }
            EndParentLog();

            return true;
        }

        public override bool setValue(IoBuffer ioBuffer, object value)
        {
		    byte flag = Types.MAP;

            // #### 0000
            ioBuffer.Write(flag);
            IDictionary d = (IDictionary)value;

            BeginParentLog("Dictionary", d.Count);
            //写入键数
            putVarInt32(ioBuffer, d.Count);
            foreach (DictionaryEntry e in d)
            {
                AddLogNewLine("[key]:");//加个分隔符
                ProtocolCoder.instance.Encode(ioBuffer, e.Key);
                AddLog(" [value]:");//加个分隔符    
                ProtocolCoder.instance.Encode(ioBuffer, e.Value);
                AddLog(",");//加个分隔符
            }
            EndParentLog();

            return true;
        }
    }
}
