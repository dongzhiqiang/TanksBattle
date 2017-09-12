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
    public class SStringDictionary<T> : ISerializableObject, IDictionary<string, T>
        where T : ISerializableObject
    {
        

        #region Fields
        private Dictionary<string, T> _dict = new Dictionary<string, T>();
        #endregion


        #region Properties
        
        #endregion

        #region Constructors
        public SStringDictionary() { }
        #endregion

        #region Static Methods

        #endregion

        #region  interface
        public ICollection<string> Keys { get { return _dict.Keys; } }
        public  ICollection<T> Values { get { return _dict.Values; } }

        public int Count { get { return _dict.Count; } }
        public bool IsReadOnly { get { return false; } }

        //有子节点变化
        public T this[string index]
        {
            get { return _dict[index]; }
            set
            {
                T item;
                if (_dict.TryGetValue(index, out item))
                {
                    if (RemoveChild(index))
                        AddChild(index, value);
                }
                else
                    AddChild(index, value);
            }
        }
        IEnumerator IEnumerable.GetEnumerator() { return _dict.GetEnumerator(); }

        IEnumerator<KeyValuePair<string, T>> IEnumerable<KeyValuePair<string, T>>.GetEnumerator() { return _dict.GetEnumerator(); }
        public T Get(string index) { return _dict.Get(index); } 
        //有子节点变化
        public void Add(string key, T value)
        {
            if (_dict.ContainsKey(key))
            {
                Debuger.LogError("不能添加已经有的key");
                return;
            }
            AddChild(key, value);
        }

        public bool ContainsKey(string key) { return _dict.ContainsKey(key); }

        //有子节点变化
        public bool Remove(string key) { return RemoveChild(key); }

        public bool TryGetValue(string key, out T value) { return _dict.TryGetValue(key, out value); }

        public void Add(KeyValuePair<string, T> pair) { Add(pair.Key, pair.Value); }
        public void Clear()
        {
            if (_dict.Count == 0) return;
            ClearChild();
        }
        public bool Contains(KeyValuePair<string, T> pair)
        {
            T item;
            if (!_dict.TryGetValue(pair.Key, out item)) return false;
            return item == pair.Value;
        }
        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            Debuger.LogError("不支持CopyTo");
        }
        public bool Remove(KeyValuePair<string, T> pair)
        {
            if(!Contains( pair))return false;
            return Remove(pair.Key);
        }
        #endregion

        #region Private Methods
        //增加一个子节点
        protected override bool OnAddChild(string key, ISerializableObject item)
        {
            //维护索引
            item.Parent = this;
            item.Key = key;

            _dict[key] = (T)item;
            return true;
        }

        //修改一个子节点
        protected override bool OnChangeChild(ISerializableObject item) { return true; }

        //删除一个子节点
        protected override bool OnRemoveChild(string key)
        {
            T item;
            if (!_dict.TryGetValue(key, out item)){
                Debuger.LogError("找不到key");
                return false;
            }

            //维护索引
            item.Parent = null;
            item.Key = string.Empty;

            return _dict.Remove(key);
        }

        //清空所有子节点
        protected override bool OnClearChild()
        {
            if (_dict.Count == 0)
                return false;

            //维护索引
            foreach(T item in _dict.Values){
                item.Parent = null;
                item.Key = string.Empty;
            }
            _dict.Clear();
            return true;
        }

        #endregion

        //序列化,返回false说明没有任何改变
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
            SerializeUtil.BeginParentLog(this, _changes.Count,true);
#endif
            ChangeCxt c;
            for(int i =0;i<_changes.Count;++i){
                c = _changes[i];
                stream.Write((byte)c.type);
                if (c.type != enSerializeChangeType.clear)
                    stream.Write(c.key);
#if SERIALIZE_DEBUG
                SerializeUtil.BeginChangeLog(i, c.type, c.key);
#endif
                if (c.type == enSerializeChangeType.add)
                {
                    if (!c.obj.SerializeChange(stream)) Debuger.LogError("逻辑错误，一定会有要序列化的东西");
                }
                else if (c.type == enSerializeChangeType.remove)
                {
                    
                }
                else if (c.type == enSerializeChangeType.change)
                {   
                    if (!c.obj.SerializeChange(stream))Debuger.LogError("逻辑错误，一定会有要序列化的东西");   
                }
                else if (c.type == enSerializeChangeType.clear)
                {

                }
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
                enSerializeChangeType type = (enSerializeChangeType)stream.ReadByte();
                string key = "";
                if (type != enSerializeChangeType.clear)
                    key = stream.ReadStr();
#if SERIALIZE_DEBUG
                SerializeUtil.BeginChangeLog(i, type,key);
#endif
                if (type == enSerializeChangeType.add)
                {
                    T item = (T)Activator.CreateInstance(typeof(T));
                    item.Parent = this;
                    item.Key = key;
                    item.Deserialize(stream);
                    _dict[item.Key] = item;
                }
                else if (type == enSerializeChangeType.remove)
                {
                    if (!_dict.Remove(key)) Debuger.LogError("同步失败");
                }
                else if (type == enSerializeChangeType.change)
                {
                    T item = _dict.Get(key);
                    if (item == null) Debuger.LogError("同步失败2");
                    item.Deserialize(stream);
                }
                else if (type == enSerializeChangeType.clear)
                {
                    _dict.Clear();
                }
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