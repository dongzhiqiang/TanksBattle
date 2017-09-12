#region Header
/**
 * 名称：Conditional
 
 * 日期：2016.5.13
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Simple.BehaviorTree
{
    public enum enInterrupt
    {
        none,
        successInterrtupt,
        failInterrtupt,
        customInterrupt,
    }
    
    public class ConditionalCfg : NodeCfg
    {
        public static string[] InterruptTypeNames = new string[] { "不中断","成功中断", "失败中断", "自定制中断" };
        public enInterrupt interrupt;
        public bool resetTreeWhenInterrupt=false;
    }
    public class Conditional:Node
    {
        #region Fields
        protected bool m_isInterrupting = false;//是不是中断判断中
        #endregion


        #region Properties
        public ConditionalCfg ConditionalCfg { get { return (ConditionalCfg)m_cfg; } }
        public bool IsInterrupting
        {
            get { return m_isInterrupting; }
            set
            {
                m_isInterrupting = value;
            }
        }
        #endregion



        #region Static Methods

        #endregion


        #region Private Methods

        #endregion

        public override void ResetState() {
            m_isInterrupting = false;
            base.ResetState();
        }
        

        public bool CheckInterupt()
        {
            ConditionalCfg cfg = this.ConditionalCfg;
            if (cfg.interrupt == enInterrupt.successInterrtupt)
                return Execute(enExecute.interrupt) == enNodeState.success;
            else if (cfg.interrupt == enInterrupt.failInterrtupt)
                return Execute(enExecute.interrupt) == enNodeState.failure;
            else if (cfg.interrupt == enInterrupt.customInterrupt)
                return OnCustomInterupt();
            else if (cfg.interrupt == enInterrupt.none)//可能编辑器修改了，没关系
                return false;
            else
            {
                Debuger.LogError("未知的中断类型:{0}", cfg.interrupt);
                return false;
            }
        }

        protected virtual bool OnCustomInterupt()
        {
            Debuger.LogError("这个类型的条件没有支持自定制中断:{0}", ConditionalCfg.Name); ;
            return false;
        }


    }
}