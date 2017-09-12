#region Header
/**
 * 名称: 属性，是Variant型，可以是任何基础数据类型和string
 
 * 日期：2015.10.10
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;



public class Property
{
    public enum enType
    {
        none,
        Int,
        Long,
        Float,
        String,
    }

    #region Fields
    private enType  _type;
    private object _value;
    
    #endregion


    #region Properties
    public enType PropType { get{return _type;}}
    public object Value { get { return _value; } }

    public int Int { get{ return Check(enType.Int)?Convert.ToInt32(_value):0;}
        set
        {
            if(_type == enType.Int)
                _value = value;
            else if(_type == enType.Long)
                _value = (long)value;
            else
                Debuger.LogError("不能设置。属性类型是:{0} 目标类型是:{1}", _type, enType.Int);
        }
    }
    public long Long { get { return Check(enType.Long) ? Convert.ToInt64(_value) : 0; }
        set
        {
           if (_type == enType.Long)
                _value = value;
            else
                Debuger.LogError("不能设置。属性类型是:{0} 目标类型是:{1}", _type, enType.Long);
        }
    }
    public float Float { get { return Check(enType.Float) ? Convert.ToSingle(_value) : 0; }
        set
        {
            if (_type == enType.Float)
                _value = value;
            else
                Debuger.LogError("不能设置。属性类型是:{0} 目标类型是:{1}", _type, enType.Float);
        }
    }
    public string String{ get { return _value==null?"":Convert.ToString(_value); }
        set
        {
            if (_type == enType.String)
                _value = value;
            else
                Debuger.LogError("不能设置。属性类型是:{0} 目标类型是:{1}", _type, enType.String);
        }
    }
    #endregion


    #region Constructors
    public Property(Property p)
    {
        this._type = p.PropType;
        this._value = p.Value;
    }
    public Property(enType t)
    {
        _type = t;
    }

    public Property(string value, enType type)
    {
        _type = type;
        switch(_type){
        case enType.Int:_value = Convert.ToInt32(value);break;
        case enType.Long: _value = Convert.ToInt64(value); break;
        case enType.Float: _value = Convert.ToSingle(value); break;
        case enType.String: _value = Convert.ToString(value); break;
        default:{
            Debuger.LogError("未知的类型");
            _value = value as object;
            };break;
        }
        
    }

    public Property(int i)
    {
        _type = enType.Int;
        _value = i;
    }

    public Property(long l)
    {
        _type = enType.Long;
        _value = l;
    }

    public Property(float f)
    {
        _type = enType.Float;
        _value = f;
    }

    public Property(string s)
    {
        _type = enType.String;
        _value = s;
    }
    #endregion

    #region Static Methods
    #endregion

    #region Private Methods
    bool Check(enType t)
    {
        if(t== _type || 
            (_type == enType.Int && t == enType.Long)||
            (_type == enType.Long&& t == enType.Int )||
            (_type == enType.Int && t == enType.Float)
            )
            return true;
        Debuger.LogError("不能转换。属性类型是:{0} 目标类型是:{1}",_type,t);
        return false;
            
    }
    #endregion

    public void SetValue(Property p)
    {
        if (this._type != p.PropType)
        {
            Debuger.LogError("类型不同不能设置，属性类型是:{0} 目标类型是:{1}", _type, p.PropType);
            return;
        }
        this._type = p.PropType;
        this._value = p.Value;
    }

    public void ToJson(LitJson.JsonWriter jsonWriter)
    {
        switch (_type)
        {
            case enType.Float:
                jsonWriter.Write(Float);
                break;
            case enType.Int:
                jsonWriter.Write(Int);
                break;
            case enType.Long:
                jsonWriter.Write(Long);
                break;
            case enType.String:
                jsonWriter.Write(String);
                break;
            default:
                jsonWriter.Write(null);
                break;
        }
    }
}
