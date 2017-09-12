#region Header
/**
 * 名称：SetFloat
 
 * 日期：2016.5.18
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
    public class SetFloatCfg: NodeCfg
    {
        public Value<float> val =new Value<float>(0);
        public Value<float> ret= new Value<float>(0,enValueRegion.tree);
#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            val.Draw("值",this,n);
            ret.Draw("变量",this,n);
            
        }
#endif
        
    }


    public class SetFloat: Aciton
    {
        SetFloatCfg CfgEx { get { return (SetFloatCfg)m_cfg; } }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            SetValue(CfgEx.ret, GetValue(CfgEx.val));
            return enNodeState.success;
        }

        //重置临时数据.销毁的时候或者onEnable的时候执行，可以当成数据初始化的地方
        protected override void OnResetState() {

        }
    }
}