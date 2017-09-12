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


namespace Simple.BehaviorTree
{
    public class ParallelCfg : ParentNodeCfg
    {
        
    }

    public class Parallel : Composite
    {
        ParallelCfg CfgEx { get { return (ParallelCfg)m_cfg; } }
        public override bool CanRuning { get { return true; } }
        //入栈。行为树遍历过程中，遍历到一个节点就会入栈它。可以用做是当前次遍历的OnInit
        protected override void OnPush() {
     
        }

        //获取下个要执行的子节点,没有返回-1
        public override int OnGetNextChildIdx(int counter) {
            return -1;
        }
        
        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) {
            if (m_childrenIdx.Count > 0)
            {
                //如果哪个子节点(子树)需要重新运行，那么这里手动入栈下
                for(int i = 0;i< m_childrenIdx.Count; ++i)
                {
                    int childIdx = m_childrenIdx[i];
                    var node = m_tree.GetNode(childIdx);
                    if (!node.Cfg.ingore &&!node.IsInStack)
                    {
                        m_tree.AddStack(childIdx);
                    }
                }
                
                return enNodeState.running;
            }
            else
                return enNodeState.success;
        }
        
    }
}