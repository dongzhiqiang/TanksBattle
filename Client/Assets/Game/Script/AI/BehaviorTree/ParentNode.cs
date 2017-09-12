#region Header
/**
 * 名称：ParentNode
 
 * 日期：2016.5.16
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

    

    public class ParentNode: Node
    {
        #region 用于被继承的属性
        protected List<int> m_childrenIdx = new List<int>();
        protected int m_childCounter = 0;
        public List<int> ChildrenIdx { get { return m_childrenIdx; }  }
        public override bool CanRuning { get { return false; } }
        #endregion

        #region 框架内部调用
        public override void Push()
        {
            base.Push();
            m_childCounter = -1;
        }

        //获取下个要执行的子节点,没有返回-1
        public int GetNextChildIdx() {
            ++m_childCounter;
            return OnGetNextChildIdx(m_childCounter);
        }
        #endregion

        #region 用于被继承的接口


        public virtual int OnGetNextChildIdx(int counter)
        {
            return m_childCounter >= ChildrenIdx.Count ?-1: ChildrenIdx[m_childCounter];
        }
        //字节点执行完,告知父节点
        public virtual void OnChildPop(int childIdx,enNodeState childState) { }
        #endregion
        public override void OnClear()
        {
            base.OnClear();
            m_childrenIdx.Clear();
        }

    }
}