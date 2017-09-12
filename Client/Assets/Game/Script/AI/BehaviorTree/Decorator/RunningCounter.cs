#region Header
/**
 * 名称：Composite
 
 * 日期：2016.5.13
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Simple.BehaviorTree
{
    public class RunningCounterCfg : DecoratorCfg
    {
        public Value<int> counter = new Value<int>(0, enValueRegion.tree);

#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            counter.Draw("计数器变量", this, n);
        }
#endif
    }

    public class RunningCounter : Decorator
    {
        RunningCounterCfg CfgEx { get { return (RunningCounterCfg)m_cfg; } }
        enNodeState m_lastNodeState = enNodeState.success;

        protected override void OnPush()
        {
            m_lastNodeState = enNodeState.success;
            SetValue(CfgEx.counter, GetValue(CfgEx.counter)+1);
        }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            return m_lastNodeState;
        }

        //字节点执行完,告知父节点
        public override void OnChildPop(int childIdx, enNodeState childState)
        {
            m_lastNodeState = childState;
        }

        

        //出栈。自己执行完了以及自己的子树执行完成后出栈
        protected override void OnPop() {
            SetValue(CfgEx.counter, GetValue(CfgEx.counter) -1);
        }


    }
}