#region Header
/**
 * 名称：MulIntCfg
 
 * 日期：2016.6.2
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
    public class MulIntCfg : NodeCfg
    {
        public Value<int> v1 =new Value<int>(0);
        public Value<int> v2 = new Value<int>(0);
        public Value<int> ret = new Value<int>(0,enValueRegion.tree);
#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            v1.Draw("值1",this,n);
            v2.Draw("值2",this,n);
            ret.Draw("结果",this,n);
        }
#endif

    }


    public class MulInt : Aciton
    {
        MulIntCfg CfgEx { get { return (MulIntCfg)m_cfg; } }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            var v1 = GetValue(CfgEx.v1);
            var v2 = GetValue(CfgEx.v2);
            SetValue(CfgEx.ret, v1*v2);
            return enNodeState.success;
        }
    }
}