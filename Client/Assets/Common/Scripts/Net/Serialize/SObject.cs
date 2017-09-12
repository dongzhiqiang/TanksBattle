#region Header
/**
 * 名称: 序列化类
 
 * 日期：2015.10.10
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
    public class SObject: ISerializableObject
    {
        #region Fields
        List<ISerializableObject>   _fields = new List<ISerializableObject>();
        List<string> _fieldsName = new List<string>();
        #endregion


        #region Properties
        
        #endregion

        #region Constructors
        public SObject()
        {
            //找到序列化字段
            _fields = new List<ISerializableObject>();
            ISerializableObject obj;
            foreach (FieldInfo info in this.GetType().GetFields())//只找public的字段
            {
                obj = info.GetValue(this) as ISerializableObject;
                if (obj != null)
                {
                    obj.Parent = this;
                    obj.Idx = _fields.Count;
                    obj.FieldName = info.Name;
                    _fields.Add(obj);
                    _fieldsName.Add(info.Name);
                }
            }
        }
       
        #endregion

        #region Static Methods

        #endregion


        #region Private Methods
        //修改一个子节点
        protected override bool OnChangeChild(ISerializableObject item) { return true; }

        
        #endregion

        
        //序列化变化的部分,返回false说明没有任何改变
        public override bool SerializeChange(IoBuffer stream)
        {
            if (!ValueChange)
                return false;
            if (!OrderChange())
            {
                ClearChange();
                return false;
            }
            stream.Write(_changes.Count);//变化数量
#if SERIALIZE_DEBUG
            SerializeUtil.BeginParentLog(this, _changes.Count, true);
#endif
            ChangeCxt c;
            for (int i = 0; i < _changes.Count; ++i)
            {
                c = _changes[i];
                if (c.type != enSerializeChangeType.change)
                {
                    Debuger.LogError("逻辑错误，SObject的子节点只能被修改，不能增删。{0}", c.type);
                    continue;
                }

                stream.Write(c.idx);
#if SERIALIZE_DEBUG
                SerializeUtil.BeginChangeLog(i, enSerializeChangeType.change,_fieldsName[c.idx]);
#endif
                if (!c.obj.SerializeChange(stream)) Debuger.LogError("逻辑错误，一定会有要序列化的东西");
#if SERIALIZE_DEBUG
                SerializeUtil.EndChangeLog();
#endif
            }
            
            ClearChange();
#if SERIALIZE_DEBUG
            SerializeUtil.EndParentLog(this);
#endif
            return true;
        }
        //反序列化
        public override void Deserialize(IoBuffer stream)
        {
            int changeCount = stream.ReadInt32();
#if SERIALIZE_DEBUG
            SerializeUtil.BeginParentLog(this, changeCount, false);
#endif
            for (int i = 0; i < changeCount; ++i)
            {
                int idx = stream.ReadInt32();
#if SERIALIZE_DEBUG
                SerializeUtil.BeginChangeLog(i, enSerializeChangeType.change, _fieldsName[idx]);
#endif
                _fields[idx].Deserialize(stream);
#if SERIALIZE_DEBUG
                SerializeUtil.EndChangeLog();
#endif
            }
#if SERIALIZE_DEBUG
            SerializeUtil.EndParentLog(this);
#endif
        }


    }
}