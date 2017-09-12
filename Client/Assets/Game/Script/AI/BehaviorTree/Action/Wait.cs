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
    public class WaitCfg : NodeCfg
    {
        
        public Value<float> waitTime=new Value<float>(5);
#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            waitTime.Draw("等待时间",this,n);    
        }
#endif
        
    }

    public class Wait: Aciton
    {
        WaitCfg CfgEx { get { return (WaitCfg)m_cfg; } }

        float beginTime = -1;

        //入栈。行为树遍历过程中，遍历到一个节点就会入栈它。可以用做是当前次遍历的OnInit
        protected override void OnPush() {
            beginTime = TimeMgr.instance.logicTime ;
            //Debuger.Log("开始等待:{0}", GetValue(CfgEx.waitTime));
        }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            if(beginTime == -1)
            {
                LogError("没有先push再执行，或者是被回收后扔执行");
                return enNodeState.failure;
            }

            if(TimeMgr.instance.logicTime - beginTime > GetValue(CfgEx.waitTime))
            {
                //Debuger.Log("等待时间到:{0}",GetValue(CfgEx.waitTime));
                return enNodeState.success;
            }
            else
                return enNodeState.running;
        }

        //出栈。自己执行完了以及自己的子树执行完成后出栈
        protected override void OnPop() {
            beginTime = -1;
        }
    }
}