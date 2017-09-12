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
    public class SDate : ISerializableObject
    {

        #region Fields
        private DateTime _value;
        #endregion


        #region Properties
        public DateTime Value { get { return _value; } set { 
            if (_value.Ticks == value.Ticks)return;
            _value = value; 
            Change();
         } }
        #endregion


        #region Constructors
        public SDate() { _value = new DateTime(0); }//注意这里不能用now，不然可能client和server而不会同步
        public SDate(long ticks){_value = new DateTime(ticks);}
        public SDate(int year, int month, int day) { _value = new DateTime(year,month,day); }
        public SDate(int year, int month, int day, int hour, int minute, int second) { _value = new DateTime(year, month, day, hour, minute, second); }
        #endregion

        #region Static Methods

        #endregion

        #region Private Methods

        #endregion
        #region Implicit Conversions
        public override string ToString()
        {
            return this._value.ToString();
        }
        #endregion

        
        //序列化,返回false说明没有任何改变
        public override bool SerializeChange(IoBuffer stream)
        {
            if (!ValueChange)
                return false;
            stream.Write(_value.Ticks);
#if SERIALIZE_DEBUG
            SerializeUtil.AddLog(this);
#endif
            ClearChange();
            return true;
        }
        //反序列化
        public override void Deserialize(IoBuffer stream)
        {
            _value = new DateTime(stream.ReadInt64());
#if SERIALIZE_DEBUG
            SerializeUtil.AddLog(this);
#endif
        }


    }
}