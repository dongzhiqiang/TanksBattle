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
    public class LogCfg : NodeCfg
    {
        
        public Value<string> log=new Value<string>("空");
#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            log.Draw("日志内容",this,n);
        }
#endif
    }

    public class Log: Aciton
    {
        LogCfg CfgEx { get { return (LogCfg)m_cfg; } }

        //重置临时数据.销毁的时候或者onEnable的时候执行，可以当成数据初始化的地方
        protected override void OnResetState()
        {
            
        }

        //入栈。行为树遍历过程中，遍历到一个节点就会入栈它。可以用做是当前次遍历的OnInit
        protected override void OnPush() {
            
        }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            Debuger.Log(GetValue(CfgEx.log));
            return enNodeState.success;
        }

        //出栈。自己执行完了以及自己的子树执行完成后出栈
        protected override void OnPop() {
            
        }
    }
}