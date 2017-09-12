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
    public class InverterCfg : DecoratorCfg
    {
        

#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            
        }
#endif
    }

    public class Inverter : Decorator
    {
        InverterCfg CfgEx { get { return (InverterCfg)m_cfg; } }
        enNodeState m_lastNodeState = enNodeState.success;


        //入栈。行为树遍历过程中，遍历到一个节点就会入栈它。可以用做是当前次遍历的OnInit
        protected override void OnPush() {
            m_lastNodeState = enNodeState.success;
        }

      
        
        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            if (m_lastNodeState == enNodeState.success)
                return enNodeState.failure;
            else if (m_lastNodeState == enNodeState.failure)
                return enNodeState.success;
            else
                return m_lastNodeState;

        }

        //字节点执行完,告知父节点
        public override void OnChildPop(int childIdx, enNodeState childState)
        {
            
            m_lastNodeState = childState;
        }
    }
}