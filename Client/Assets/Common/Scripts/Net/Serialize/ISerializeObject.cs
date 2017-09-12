#region Header
/**
 * 名称: 序列化对象库(SO)
 
 * 日期：2015.10.10
 * 描述：
 *      1.调用Serialize和Deserialize，可以把一个对象序列化成字节
 *      2.对于基本类型、list和dict，用S开头的替代版本(比如int->SInt，List->SList)
 *      3.对于弱类型,用SVariant(只支持基本类型)
 *      4.对于自定义类型，继承自SObject(内部用反射实现)
 * 对比:
 *      1.使用上，SO有代码侵入性(必须替代基本类型或者继承)，而Protobuf-net利用Attributes来实现序列化字段
 *      2.关于动态创建，如果服务端是不支持反射的语言或者动态语言,那么SO的设计思想行不通(因为反序列化的时候需要动态创建)，
 *          只能像protobuf一样先定义好数据结构，然后生成类文件(那么这个生成出来的类里自然就会有反序列化时new对应
 *          成员类对象的代码，相当于元编程了，由此也可以推断出为何c++到现在都不支持反射，因为用这样的方式会比反射高效)
 *      3.增量序列化，SO比protobuf多干一点活，，一个角色类用protobuf实现的话，只能序列化全部，而这个类可以记录下角色变
 *          化的部分，序列化的时候只序列化这一部分。
 *      4.改进，以后舍弃这个库，在protobuf的基础上增加增量序列化的功能
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


namespace NetCore
{
    public abstract class ISerializableObject
    {
        #region Fields
        protected ISerializableObject _parent = null;
        private bool _valueChange=false;//上次发送给客户端到现在是不是改变过
        private string _fieldName = string.Empty;//如果父节点是SObject，那么这个值是字段名
        protected int _idx = -1;//在父节点中的索引，对于list和field会用到。-1表示已经删除了
        protected string _key = string.Empty;//在父节点中的key，对于dict会用到。Empty表示已经删除了
        protected List<ChangeCxt> _changes = new List<ChangeCxt>();//上次发送给客户端时到现在的变化
        private HashSet<ISerializableObject> _orderTemp = new HashSet<ISerializableObject>();//用于序列化钱整理_changes的临时变量
        #endregion

        #region Properties
        public ISerializableObject Parent{get{return _parent;}set{_parent = value;}}
        public string FieldName { get{return _fieldName;}set{_fieldName = value;}}
        public bool ValueChange{get { return _valueChange; }}

        public int Idx { get{return _idx;}set{_idx = value;}}
        public string Key { get { return _key; } set { _key = value; } }
        #endregion

        #region Private Methods
        //一个child加进来后
        protected bool AddChild(int idx, ISerializableObject item)
        {
            if (item.Parent != null)
            {
                Debuger.LogError("不能重新设置序列化对象的parent");
                return false;
            }
            if (!OnAddChild(idx, item))
                return false;
            

            ChangeCxt  cxt = new ChangeCxt();
            cxt.type = enSerializeChangeType.add;
            cxt.idx = idx;
            cxt.obj = item;
            _changes.Add(cxt);

            item._valueChange = true;
            Change();
            return true;
        }

        protected bool AddChild(string key, ISerializableObject item)
        {
            if (item.Parent != null)
            {
                Debuger.LogError("不能重新设置序列化对象的parent");
                return false;
            }
            if (!OnAddChild(key, item))
                return false;

            ChangeCxt cxt = new ChangeCxt();
            cxt.type = enSerializeChangeType.add;
            cxt.key = key;
            cxt.obj = item;
            _changes.Add(cxt);

            item._valueChange = true;
            Change();
            return true;
        }

        protected bool ChangeChild(ISerializableObject item)
        {
            if (item.Parent != this || (item.Idx == -1 && string.IsNullOrEmpty(item.Key)))//可能已经删除了
            {
                Debuger.LogError("逻辑错误");
                return false;
            }
            if (!OnChangeChild(item))
                return false;

            ChangeCxt cxt = new ChangeCxt();
            cxt.type = enSerializeChangeType.change;
            if (item._idx!=-1)
                cxt.idx = item._idx;
            else
                cxt.key = item._key;
            cxt.obj = item;
            _changes.Add(cxt);

            item._valueChange = true;
            Change();
            return true;
        }

        protected bool RemoveChild(int idx)
        {
            if (!OnRemoveChild(idx))
                return false;
            ChangeCxt cxt = new ChangeCxt();
            cxt.type = enSerializeChangeType.remove;
            cxt.idx = idx;
            _changes.Add(cxt);

            Change();
            return true;
        }

        protected bool RemoveChild(string key)
        {
            if (!OnRemoveChild(key))
                return false;
            ChangeCxt cxt = new ChangeCxt();
            cxt.type = enSerializeChangeType.remove;
            cxt.key = key;
            _changes.Add(cxt);

            Change();
            return true;
        }

        protected bool ClearChild() {
            if (!OnClearChild())
                return false;
            ChangeCxt cxt = new ChangeCxt();
            cxt.type = enSerializeChangeType.clear;
            _changes.Add(cxt);

            Change();
            return true;
        }

        //增加一个子节点
        protected virtual bool OnAddChild(int idx, ISerializableObject item) { Debuger.LogError("没有实现"); return false; }

        //增加一个子节点
        protected virtual bool OnAddChild(string key, ISerializableObject item) { Debuger.LogError("没有实现"); return false; }

        //修改一个子节点
        protected virtual bool OnChangeChild(ISerializableObject item) { Debuger.LogError("没有实现"); return false; }


        //删除一个子节点
        protected virtual bool OnRemoveChild(int idx) { Debuger.LogError("没有实现"); return false; }

        //删除一个子节点
        protected virtual bool OnRemoveChild(string key) { Debuger.LogError("没有实现"); return false; }

        //清空所有子节点
        protected virtual bool OnClearChild() { Debuger.LogError("没有实现"); return false; }
        #endregion
        
        //变化支持嵌套。变化了，并告诉父节点
        public void Change()
        {
            if(_parent == null)
            {
                _valueChange = true;//如果有_parent，则不需要设置自己的valueChange，增删查改的操作中父节点会给子节点设，以此达到递归的功能
                return;
            }
            _parent.ChangeChild(this);
        }

        //变化支持嵌套。清空变化，并告诉子节点
        public void ClearChange()
        {
            if (_valueChange ==false)
                return;

            _valueChange = false;
            if (_changes.Count > 0)
            {
                for(int i =0;i<_changes.Count;++i){
                    if (_changes[i].obj != null)
                    {
                        _changes[i].obj.ClearChange();
                    }
                }
                _changes.Clear();
            }
        }

        //在一个有子节点的序列化类，序列化前一定要调用这个函数，它会把多余的change删除
        public bool OrderChange()
        {
            if (_changes.Count == 0)
            {
                Debuger.LogError("逻辑错误");
                return false;
            }

            _orderTemp.Clear();
            
            
            ChangeCxt c;
            for(int i =_changes.Count-1;i>=0;--i){
                c =_changes[i];
                if(c.type == enSerializeChangeType.clear)//删除的情况
                {
                    _changes.RemoveRange(0, i);
                    break;
                }
                else if (c.type == enSerializeChangeType.change)
                {
                     if(_orderTemp.Contains(c.obj))
                            _changes.RemoveAt(i);
                        else
                            _orderTemp.Add(c.obj);
                        
                }
                else if(c.type == enSerializeChangeType.add)
                {
                    if(_orderTemp.Contains(c.obj))
                    {
                         for(int j = i+1;j<_changes.Count;++j){
                             if (_changes[j].obj == c.obj)
                             {
                                 _changes.RemoveAt(j);
                                 break;
                             }
                         }
                    }
                    
                }

            }

            
            _orderTemp.Clear();

            if (_changes.Count == 0)
            {
                Debuger.LogError("逻辑错误2");
                return false;
            }
            return true;
        }


        //序列化自己或者子节点变化的部分，如果没有变化，返回false
        public abstract bool SerializeChange(IoBuffer stream);

        //反序列化，从流初始化或者修改自己
        public abstract void Deserialize(IoBuffer stream);

        

    }
}