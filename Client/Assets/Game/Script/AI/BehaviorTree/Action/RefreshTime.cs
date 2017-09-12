#region Header
/**
 * 名称：刷新时间
 
 * 日期：2016.7.27
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
    public class RefreshTimeCfg : NodeCfg
    {
        public Value<float> ret=new Value<float>(0, enValueRegion.tree);
#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            ret.Draw("结果", this, n);
        }
#endif
    }

    public class RefreshTime : Aciton
    {
        RefreshTimeCfg CfgEx { get { return (RefreshTimeCfg)m_cfg; } }

        //入栈。行为树遍历过程中，遍历到一个节点就会入栈它。可以用做是当前次遍历的OnInit
        protected override void OnPush() {
            
        }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            SetValue(CfgEx.ret, TimeMgr.instance.logicTime);
            return enNodeState.success;
        }

        //出栈。自己执行完了以及自己的子树执行完成后出栈
        protected override void OnPop() {
            
        }
    }
}