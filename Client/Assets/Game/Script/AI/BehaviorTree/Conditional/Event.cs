#region Header
/**
 * 名称：Event
 
 * 日期：2016.5.13
 * 描述：只执行一次
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Simple.BehaviorTree
{
    public class EventCfg : ConditionalCfg
    {

    }
    public class Event : Conditional
    {
        #region Fields
        int m_field = 0;
        #endregion


        #region Properties
        public int FieldRead { get { return m_field; } }

        public int Field
        {
            get { return m_field; }
            set { m_field = value; }
        }
        #endregion



        #region Static Methods
        public static void StaticFun()
        {

        }
        #endregion


        #region Private Methods
        void PrivateFun()
        {

        }
        #endregion

        public void PublicFun()
        {

        }



    }
}