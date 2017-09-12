#region Header
/**
 * 名称：SetBool
 
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
    public class SetBoolCfg: NodeCfg
    {
        public Value<bool> val =new Value<bool>(true);
        public Value<bool> ret= new Value<bool>(true, enValueRegion.tree);
#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            val.Draw("值",this,n);
            ret.Draw("变量",this,n);
            
        }
#endif
        
    }


    public class SetBool: Aciton
    {
        SetBoolCfg CfgEx { get { return (SetBoolCfg)m_cfg; } }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            SetValue(CfgEx.ret, GetValue(CfgEx.val));
            return enNodeState.success;
        }

    }
}