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
    public class SList<T> : ISerializableObject, IList<T>
        where T : ISerializableObject
    {
        

        #region Fields
        private List<T> _list = new List<T>();
        
        #endregion


        #region Properties
        
        #endregion

        #region Constructors
        public SList() { }
        #endregion

        #region Static Methods

        #endregion

        #region  interface
        //有子节点变化
        public T this[int index]
        {
            get { return _list[index]; }
            set
            {
                if (RemoveChild(index))
                    AddChild(index, value);
            }
        }

        public int Count { get { return _list.Count; } }
        public bool IsReadOnly { get { return false; } }
        IEnumerator IEnumerable.GetEnumerator(){ return _list.GetEnumerator();}

        IEnumerator<T> IEnumerable<T>.GetEnumerator() { return _list.GetEnumerator(); }

        public int IndexOf(T item) { return _list.IndexOf(item); }

        //有子节点变化
        public void Insert(int index, T item) { AddChild(index, item); }

        //有子节点变化
        public bool Remove(T item)
        {
            int idx = IndexOf(item);
            if (idx== -1)
                return false;
            return RemoveChild(idx);
        }

        //有子节点变化
        public void RemoveAt(int index) { 
            RemoveChild(index);
        }

        //有子节点变化
        public void Add(T item)
        { 
            AddChild(_list.Count, item);
        }

        //有子节点变化
        public void Clear() {
            if (_list.Count == 0) return;
            ClearChild();
        }
        public bool Contains(T item) { return _list.Contains(item); }
        public void CopyTo(T[] array, int arrayIndex) { Debuger.LogError("不支持CopyTo"); }

        
        #endregion

        #region Private Methods
        //增加一个子节点
        protected override bool OnAddChild(int idx, ISerializableObject item)
        {
            if (idx > _list.Count)
            {
                Debuger.LogError("逻辑错误，越界");
                return false;
            }

            //维护索引
            item.Parent = this;
            item.Idx = idx;

            if (idx == _list.Count)
                _list.Add((T)item);
            else if (idx < _list.Count)
            {
                for (int i = idx ; i < _list.Count; ++i){//维护索引
                    _list[i].Idx += 1;
                }

                _list.Insert(idx, (T)item);
            }
                
            
            return true;
        }

        //修改一个子节点
        protected override bool OnChangeChild(ISerializableObject item) { return true; }

        //删除一个子节点
        protected override bool OnRemoveChild(int idx)
        {
            if (idx >=_list.Count)
                return false;
            for(int i=idx+1;i<_list.Count;++i ){//维护索引
                _list[i].Idx -= 1;
            }

            //维护索引
            _list[idx].Parent = null;
            _list[idx].Idx = -1;

            _list.RemoveAt(idx); 
            return true;
        }

        //清空所有子节点
        protected override bool OnClearChild()
        {
            if (_list.Count == 0)
                return false;

            //维护索引
            for (int i = 0; i < _list.Count; ++i)
            {
                _list[i].Parent = null;
                _list[i].Idx=-1;
            }

            _list.Clear();
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
            SerializeUtil.BeginParentLog(this, _changes.Count, true);
#endif
            ChangeCxt c;
            for(int i =0;i<_changes.Count;++i){
                c = _changes[i];
                stream.Write((byte)c.type);
                if (c.type != enSerializeChangeType.clear)
                    stream.Write(c.idx);
#if SERIALIZE_DEBUG
                SerializeUtil.BeginChangeLog(i, c.type, c.idx.ToString());
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
                    if (!c.obj.SerializeChange(stream)) Debuger.LogError("逻辑错误，一定会有要序列化的东西");
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
                int idx = -1;
                if (type != enSerializeChangeType.clear)
                    idx = stream.ReadInt32();
#if SERIALIZE_DEBUG
                SerializeUtil.BeginChangeLog(i, type, idx.ToString());
#endif
                if (type == enSerializeChangeType.add)
                {
                    T item= (T)Activator.CreateInstance(typeof(T));
                    item.Parent = this;
                    item.Idx = idx;
                    item.Deserialize(stream);
                    _list.Insert(item.Idx,item);
                }
                else if (type == enSerializeChangeType.remove)
                {
                    _list.RemoveAt(idx);
                }
                else if (type == enSerializeChangeType.change)
                {
                    _list[idx].Deserialize(stream);
                }
                else if (type == enSerializeChangeType.clear)
                {
                    _list.Clear();
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