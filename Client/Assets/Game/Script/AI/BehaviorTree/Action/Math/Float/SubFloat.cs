#region Header
/**
 * 名称：SubFloat
 
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
    public class SubFloatCfg : NodeCfg
    {
        public Value<float> v1 =new Value<float>(0);
        public Value<float> v2 = new Value<float>(1);
        public Value<float> ret = new Value<float>(0,enValueRegion.tree);
#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            v1.Draw("值1",this,n);
            v2.Draw("值2",this,n);
            ret.Draw("结果",this,n);
        }
#endif

    }


    public class SubFloat : Aciton
    {
        SubFloatCfg CfgEx { get { return (SubFloatCfg)m_cfg; } }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            float v1 = GetValue(CfgEx.v1);
            float v2 = GetValue(CfgEx.v2);
            SetValue(CfgEx.ret, v1-v2);
            return enNodeState.success;
        }
    }
}