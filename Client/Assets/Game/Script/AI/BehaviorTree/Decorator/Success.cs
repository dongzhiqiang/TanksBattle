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
    public class SuccessCfg : DecoratorCfg
    {
        

#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            
        }
#endif
    }

    public class Success : Decorator
    {
        SuccessCfg CfgEx { get { return (SuccessCfg)m_cfg; } }
        
        
        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            return enNodeState.success;
        }

        
        
    }
}