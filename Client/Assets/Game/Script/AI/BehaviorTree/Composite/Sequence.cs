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
    public class SequenceCfg : ParentNodeCfg
    {
        
    }

    public class Sequence: Composite
    {
        int m_curIdx = -1;
        enNodeState m_lastNodeState = enNodeState.success;

        SequenceCfg CfgEx { get { return (SequenceCfg)m_cfg; } }

        //入栈。行为树遍历过程中，遍历到一个节点就会入栈它。可以用做是当前次遍历的OnInit
        protected override void OnPush() {
            m_curIdx = -1;
            m_lastNodeState = enNodeState.success;
        }

        //获取下个要执行的子节点,没有返回-1
        public override int OnGetNextChildIdx(int counter) {
            if (m_lastNodeState != enNodeState.success)
                return -1;
            
            return base.OnGetNextChildIdx(counter);
        }

        //字节点执行完,告知父节点
        public override void OnChildPop(int childIdx, enNodeState childState) {
            if(childState== enNodeState.inactive || childState == enNodeState.running)
            {
                LogError("Sequence OnChildPop只能接收成功或者失败:{0},子节点:{1}", childState, childIdx);
                return;
            }

            m_lastNodeState = childState;
        }
        

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected override enNodeState OnExecute(enExecute executeType) { return m_lastNodeState; }
        
    }
}