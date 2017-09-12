#region Header
/**
 * 名称：BehaviorTreeCfg
 
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
    
    public class BehaviorTreeCfg
    {
        
        public string name="";
        public bool expand = false;
        public NodeCfg root;
        public ValueMgrCfg valueMgrCfg = new ValueMgrCfg();

        BehaviorTreeFileCfg fileCfg;

        public BehaviorTreeFileCfg File { get { return fileCfg; }set { fileCfg = value; } }
        public string BehaviorName { get { return string.Format("{0}:{1}", File.File, name); } }

        public void Reset()
        {
            valueMgrCfg.Reset();
            ResetChild(root,null );
        }

        void ResetChild(NodeCfg node, ParentNodeCfg parent)
        {
            node.Tree = this;
            node.Parent=parent ;
            node.OnReset();
            ParentNodeCfg p = node as ParentNodeCfg;
            if(p!=null)
            {
                for (int i = p.children.Count - 1; i >= 0; --i)
                {
                    ResetChild(p.children[i], p);
                }
            }
        }
        
        public bool IsSame(string file, string name)
        {
            if (fileCfg.File != file)
                return false;
            if (name != this.name)
                return false;
            return true;
        }

        public void DoAll(Action<NodeCfg> a)
        {
            DoAllChild(root, a);
        }

        void DoAllChild(NodeCfg n, Action<NodeCfg> a)
        {
            a(n);

            ParentNodeCfg p = n as ParentNodeCfg;
            if (p != null)
            {
                for (int i = 0, j = p.children.Count; i < j; ++i)
                {
                    DoAllChild(p.children[i], a);
                }
            }
        }

        public void PreLoad()
        {
            DoAll(OnPreLoad);
        }

        void OnPreLoad(NodeCfg nodeCfg)
        {
            nodeCfg.OnPreLoad();
        }

#if UNITY_EDITOR
        public void Clear()
        {
            if (root == null)
                return;
            ClearChild(root);
            root = null;
        }


        void ClearChild(NodeCfg node)
        {
            ParentNodeCfg p = node as ParentNodeCfg;
            if (p != null)
            {
                for (int i = p.children.Count - 1; i >= 0; --i)
                {
                    ClearChild(p.children[i]);
                }
                p.children.Clear();
            }
            node.Tree = null;
            node.Parent = null;
        }

        public void SetName(string newName)
        {
            if (this.name == newName)
                return;
            this.name = newName;
            fileCfg.ResetNameIdx();
        }



#endif

    }

}