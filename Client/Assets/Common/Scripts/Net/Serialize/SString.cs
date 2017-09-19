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
    public class SString : ISerializableObject
    {

        #region Fields
        private string _value="";
        #endregion


        #region Properties
        public string Value { get { return _value; } set { if (_value == value)return; _value = value; Change(); } }
        #endregion


        #region Constructors
        public SString() { }
        public SString(string value)
        {
            _value = value;
        }
        #endregion

        #region Static Methods

        #endregion

        #region Private Methods

        #region Implicit Conversions
        public static implicit operator string(SString s)
        {
            return s._value;
        }

        //如果用隐式重载来实现SInt能和int做运算可能会会有效率问题(因为要内部new SInt，GC会过高)
        //所以当想当成int做运算或者赋值int给一个SInt的时候请用Value属性
        //public static implicit operator SInt(int i)
        //{
        //    SInt s = new SInt();
        //    s._int = i;
        //    return s;
        //}

        public bool Equals(SString obj)
        {
            return this._value == ((SString)obj)._value;
        }


        public override bool Equals(object obj)
        {
            if (obj is string)
                return this._value == (string)obj;
            else if (obj is SString)
                return this._value == ((SString)obj)._value;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(SString a, SString b){return a._value == b._value;}
        public static bool operator !=(SString a, SString b){return !(a == b);}
        public static bool operator ==(string a, SString b) { return a == b._value; }
        public static bool operator !=(string a, SString b) { return !(a == b); }
        public static bool operator ==(SString a, string b) { return a._value == b; }
        public static bool operator !=(SString a, string b) { return !(a == b); }
       
        public override string ToString()
        {
            return this._value;
        }

        #endregion

        #endregion
        //序列化,返回false说明没有任何改变
        public override bool SerializeChange(IoBuffer stream)
        {
            if (!ValueChange)
                return false;
            stream.Write(_value);
#if SERIALIZE_DEBUG
            SerializeUtil.AddLog(this);
#endif
            ClearChange();
            return true;
        }
        //反序列化
        public override void Deserialize(IoBuffer stream)
        {
            _value = stream.ReadStr();
#if SERIALIZE_DEBUG
            SerializeUtil.AddLog(this);
#endif
        }


    }
}