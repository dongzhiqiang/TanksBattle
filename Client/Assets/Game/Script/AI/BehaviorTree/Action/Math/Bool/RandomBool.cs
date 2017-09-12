#region Header
/**
 * 名称：RamdomBool
 
 * 日期：2016.6.3
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
    public class RandomBoolCfg : NodeCfg
    {
        public Value<bool> ret= new Value<bool>(false,enValueRegion.tree);
#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            ret.Draw("变量",this,n);
        }
#endif
        
    }


    public class RandomBool : Aciton
    {
        RandomBoolCfg CfgEx { get { return (RandomBoolCfg)m_cfg; } }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            SetValue(CfgEx.ret, UnityEngine.Random.Range(0,2) == 0);//这里要加一，unity这个随机函数右边是开区间
            return enNodeState.success;
        }
    }
}