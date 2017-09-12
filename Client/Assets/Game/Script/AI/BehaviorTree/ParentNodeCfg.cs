#region Header
/**
 * 名称：NodeCfg
 
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
   

    public class ParentNodeCfg: NodeCfg
    {
        public List<NodeCfg> children = new List<NodeCfg>();
        public bool expandChild = true;

        //允许拥有的子节点数量
        public virtual int MaxChildren { get { return int.MaxValue; } }

        public bool CanAddChild()
        {
            return children.Count + 1 <= MaxChildren;

        }

    }
}