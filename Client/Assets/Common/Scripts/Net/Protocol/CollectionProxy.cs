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

namespace NetCore
{
    public class CollectionProxy : Proxy
    {
        #region Fields
        
        #endregion


        #region Properties
    
        #endregion


        #region Constructors


        public CollectionProxy()
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
            IList list = (IList)value;
            if (list == null)
            {
                Debuger.LogError("类型不是list:{0}", type.Name);
                return false;
            }

            //获取元素类型
            CollectionDef def = ProtocolCoder.instance.getCollectionDef(type);
            if (def == null)
            {
                Debuger.LogError("list定义{0}不存在", type.FullName);

                return false;
            }
            Type elementType = def.elementType;

            // 取出元素个数
            int len;
            if (!readVarInt32(ioBuffer, out len))
                return false;

            BeginParentLog("List", len);
            bool needNewLine = elementType.IsClass && typeof(string) != elementType;

            for (int i = 0; i < len; i++)
            {
                if (needNewLine) AddLogNewLine("");
                object e;
                if (!ProtocolCoder.instance.Decode(ioBuffer, elementType, out e))
                {
                    return false;
                }

                list.Add(e);
                AddLog(",");//加个分隔符
            }
            EndParentLog();

            return true;
        }

        public override bool setValue(IoBuffer ioBuffer, object value)
        {
		    byte flag = Types.COLLECTION;
            IList l = (IList)value;

            //获取元素类型
            CollectionDef def = ProtocolCoder.instance.getCollectionDef(value.GetType());
            if (def == null)
            {
                if (ProtocolCoder.CanRegister(value.GetType()))
                    ProtocolCoder.instance.Register(value.GetType());
                def = ProtocolCoder.instance.getCollectionDef(value.GetType());
                if (def == null)
                {
                    Debuger.LogError("list定义{0}不存在", value.GetType().FullName);
                    return false;
                }

            }

            // #### 0000
            ioBuffer.Write(flag);
            putVarInt32(ioBuffer, l.Count);
            BeginParentLog("List", l.Count);
            bool needNewLine = def.elementType.IsClass && typeof(string) != def.elementType;
            for (int i = 0; i < l.Count; i++)
            {
                if (needNewLine) AddLogNewLine("");
                if (!ProtocolCoder.instance.Encode(ioBuffer, l[i]))
                    return false;
                AddLog(",");//加个分隔符
            }
            EndParentLog();
		    
            return true;
        }

    }
}
