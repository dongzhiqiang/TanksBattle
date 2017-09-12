#region Header
/**
 * 名称：RamdomInt
 
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
    public class RandomIntCfg : NodeCfg
    {
        public Value<int> min =new Value<int>(0);
        public Value<int> max = new Value<int>(1);

        public Value<int> ret= new Value<int>(0,enValueRegion.tree);
#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            min.Draw("最小值",this,n);
            max.Draw("最大值", this, n);
            ret.Draw("变量",this,n);
        }
#endif
        
    }


    public class RandomInt : Aciton
    {
        RandomIntCfg CfgEx { get { return (RandomIntCfg)m_cfg; } }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            SetValue(CfgEx.ret, UnityEngine.Random.Range(GetValue(CfgEx.min), GetValue(CfgEx.max)+1));//这里要加一，unity这个随机函数右边是开区间
            return enNodeState.success;
        }
    }
}