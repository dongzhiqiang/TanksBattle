#region Header
/**
 * 名称：NegationBool
 
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
    public class NegationBoolCfg : NodeCfg
    {   
        public Value<bool> ret = new Value<bool>(false, enValueRegion.tree);
#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            
            ret.Draw("结果",this,n);
        }
#endif

    }


    public class NegationBool : Aciton
    {
        NegationBoolCfg CfgEx { get { return (NegationBoolCfg)m_cfg; } }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType)
        {
            
            SetValue(CfgEx.ret, GetValue(CfgEx.ret)?false:true);
            return enNodeState.success;
        }
    }
}