#region Header
/**
 * 名称：Composite
 
 * 日期：2016.5.13
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
    public class RepeaterCfg : DecoratorCfg
    {
        public Value<int> loop = new Value<int>(-1);

#if UNITY_EDITOR
        public override void DrawAreaInfo(Node n)
        {
            loop.Draw("循环次数",this,n);
        }
#endif
    }

    public class Repeater : Decorator
    {
        RepeaterCfg CfgEx { get { return (RepeaterCfg)m_cfg; } }
        int m_count = 0;
        

        //入栈。行为树遍历过程中，遍历到一个节点就会入栈它。可以用做是当前次遍历的OnInit
        protected override void OnPush() {
            m_count++;
        }

        //获取下个要执行的子节点,没有返回-1
        public override int OnGetNextChildIdx(int counter)
        {
            if (!CanLoop())
                return -1;

            return base.OnGetNextChildIdx(counter);
        }
        
        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            return CanLoop() ? enNodeState.success: enNodeState.failure;
        }

        //重置临时数据.销毁的时候或者onEnable的时候执行，可以当成数据初始化的地方
        protected override void OnResetState() { m_count = 0; }

        bool CanLoop()
        {
            int loop = GetValue(CfgEx.loop);
            return loop == -1 || loop >= m_count;
        }
    }
}