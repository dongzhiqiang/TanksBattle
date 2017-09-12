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
    public class SVariant : ISerializableObject
    {
        public enum enType
        {
            none,
            Bool,
            Byte,
            Short,
            Int,
            Long,
            Float,
            Double,
            String,
        }

        #region Fields
        private enType  _type;
        private object _value;
        private bool cache=false;
        #endregion


        #region Properties
        public object Value { get { return _value; } 
            set {
                Cache(value);
                if(_type == enType.Bool)
                    _value = (bool)value;              
                else if(_type == enType.Byte)
                    _value = (byte)value;              
                else if(_type == enType.Short)
                    _value = (short)value;              
                else if(_type == enType.Int)
                    _value = (int)value;              
                else if(_type == enType.Long)
                    _value = (long)value;              
                else if(_type == enType.Float)
                    _value = (float)value;              
                else if(_type == enType.Double)
                    _value = (double)value;              
                else if(_type == enType.String)
                    _value = (string)value;                          
                else
                    Debuger.LogError("未知的类型");
                Change(); 
        } }
        #endregion


        #region Constructors
        public SVariant() { 
        }
        public SVariant(object value)
        {
            Cache(value);
            _value = value;
        }
        #endregion
        #region Implicit Conversions
        public static implicit operator bool(SVariant s)
        {
            if (s._type == enType.Bool)
                return (bool)s._value;
            Debuger.LogError("类型不正确");
            return false;
        }

        public static implicit operator byte(SVariant s)
        {
            if (s._type == enType.Byte)
                return (byte)s._value;
            Debuger.LogError("类型不正确");
            return (byte)0;
        }

        public static implicit operator short(SVariant s)
        {
            if (s._type == enType.Short)
                return (short)s._value;
            Debuger.LogError("类型不正确");
            return (short)0;
        }

        public static implicit operator int(SVariant s)
        {
            if (s._type == enType.Int)
                return (int)s._value;
            else if (s._type == enType.Short)
                return (int)(short)s._value;
            else if (s._type == enType.Byte)
                return (int)(byte)s._value;
            Debuger.LogError("类型不正确");
            return 0;
        }

        public static implicit operator long(SVariant s)
        {
            if (s._type == enType.Long)
                return (long)s._value;
            Debuger.LogError("类型不正确");
            return 0;
        }

        public static implicit operator float(SVariant s)
        {
            if (s._type == enType.Float)
                return (float)s._value;
            else if (s._type == enType.Double)
                return (float)(double)s._value;
            Debuger.LogError("类型不正确");
            return 0f;
        }

        public static implicit operator double(SVariant s)
        {
            if (s._type == enType.Float)
                return (double)(float)s._value;
            else if (s._type == enType.Double)
                return (float)s._value;
            Debuger.LogError("类型不正确");
            return 0;
        }

        public static implicit operator string(SVariant s)
        {
            if (s._type == enType.String)
                return (string)s._value;
            Debuger.LogError("类型不正确");
            return "";
        }
        
        public override string ToString()
        {
            return string.Format("{0}|{1}", _type, this._value.ToString());
        }

        #endregion
        

        #region Static Methods

        #endregion

        #region Private Methods
        void Cache(object value)
        {
            if (cache)return;
            cache = true;
            
            if (value is bool)
                SetType(enType.Bool);
            else if (value is byte)
                SetType(enType.Byte);
            else if (value is short)
                SetType(enType.Short);
            else if (value is int)
                SetType(enType.Int);
            else if (value is long)
                SetType(enType.Long);
            else if (value is float)
                SetType(enType.Float);
            else if (value is double)
                SetType(enType.Double);
            else if (value is string)
                SetType(enType.String);
            else
                Debuger.LogError("未知的类型");
        }

        void SetType(enType type)
        {
            if(_type == type)return;

            _type = type;

            if (_type == enType.Bool)
                _value = false;
            else if (_type == enType.Byte)
                _value = (byte)0;
            else if (_type == enType.Short)
                _value = (short)0;
            else if (_type == enType.Int)
                _value = (int)0;
            else if (_type == enType.Long)
                _value = (long)0;
            else if (_type == enType.Float)
                _value = (float)0;
            else if (_type == enType.Double)
                _value = (double)0;
            else if (_type == enType.String)
                _value = "";
            else
                Debuger.LogError("未知的类型");
        }

        #endregion
        
        //序列化,返回false说明没有任何改变
        public override bool SerializeChange(IoBuffer stream)
        {
            if (!ValueChange)
                return false;
#if SERIALIZE_DEBUG
            SerializeUtil.AddLog(this);
#endif
            stream.Write((byte)_type);
            if (_type == enType.Bool)
                stream.Write((bool)_value);
            else if (_type == enType.Byte)
                stream.Write((byte)_value);
            else if (_type == enType.Short)
                stream.Write((short)_value);
            else if (_type == enType.Int)
                stream.Write((int)_value);
            else if (_type == enType.Long)
                stream.Write((long)_value);
            else if (_type == enType.Float)
                stream.Write((float)_value);
            else if (_type == enType.Double)
                stream.Write((double)_value);
            else if (_type == enType.String)
                stream.Write((string)_value);
            else
                Debuger.LogError("未知的类型或者没有初始化");

            ClearChange();
            return true;
        }
        //反序列化
        public override void Deserialize(IoBuffer stream)
        {
            enType t= (enType)stream.ReadByte();
            SetType(t);
            if (_type == enType.Bool)
                _value = stream.ReadBool();
            else if (_type == enType.Byte)
                _value = stream.ReadByte();
            else if (_type == enType.Short)
                _value = stream.ReadInt16();
            else if (_type == enType.Int)
                _value = stream.ReadInt32();
            else if (_type == enType.Long)
                _value = stream.ReadInt64();
            else if (_type == enType.Float)
                _value = stream.ReadFloat();
            else if (_type == enType.Double)
                _value = stream.ReadDouble();
            else if (_type == enType.String)
                _value = stream.ReadStr();
            else
                Debuger.LogError("未知的类型");
#if SERIALIZE_DEBUG
            SerializeUtil.AddLog(this);
#endif
        }
    }
}