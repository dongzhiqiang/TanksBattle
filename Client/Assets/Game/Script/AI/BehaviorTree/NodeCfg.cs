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
    [JsonCanInherit]
    public class NodeCfg
    {
        public const float cell = 10f;

        public int id;
        public bool ingore = false;
        public bool showNote =false;
        public bool resetTempValueIfEnable = true;//重置临时值，在行为树启用的时候
        public string note = "";

        //编辑器用参数
        public Vector2 localPos = Vector2.zero;
        
        BehaviorTreeCfg tree;
        ParentNodeCfg parent;
        NodeType nodeType;
        
        public BehaviorTreeCfg Tree { get { return tree; } set { tree = value; } }
        public ParentNodeCfg Parent { get { return parent; }set { parent = value; } }
        public NodeType NodeType { get {
                if (nodeType == null)
                    nodeType = BehaviorTreeFactory.s_cfgIdx[this.GetType()];
                return nodeType;
            } }
        public string Name { get { return NodeType.name; } }
        public int Depth
        {
            get
            {
                int depth = 0;
                ParentNodeCfg p = parent;
                while (p != null)
                {
                    ++depth;
                    p = p.Parent;
                }
                return depth;
            }
        }

        public bool Ingore
        {
            get
            {
                if (ingore)
                    return true;
                else if (parent != null)
                    return parent.Ingore;
                else
                    return false;
            }
        }

        public Vector2 Pos
        {
            get
            {
                Vector2 pos = localPos;
                if (parent != null)
                    pos += parent.Pos;

                
                return pos;
            }
            set
            {
                localPos = value;
                if (parent != null)
                    localPos -= parent.Pos;
            }

        }
        
        public void DoAll(Action<NodeCfg> a)
        {
            a(this);
            ParentNodeCfg p = this as ParentNodeCfg;
            if (p != null)
            {
                for (int i = 0, j = p.children.Count; i < j; ++i)
                {
                    p.children[i].DoAll(a);
                }
            }

        }

        //清空，一般用来防止被删除后仍然被访问
        public void Clear()
        {
            parent = null;
            tree = null;
            ParentNodeCfg p = this as ParentNodeCfg;
            if (p != null)
            {
                p.children.Clear();
            }
        }

        public void Log(string format, params object[] ps)
        {
            string log = string.Format("{0} {1}节点 id:{2}:", tree.BehaviorName, Name, id);
            Debuger.Log(log + format, ps);
        }

        public void LogError(string format, params object[] ps)
        {
            string log = string.Format("{0} {1}节点 id:{2} 出错:", tree.BehaviorName, Name, id);
            Debuger.LogError(log + format, ps);
        }

        public virtual void OnPreLoad(){}

        public virtual void OnReset(){}
#if UNITY_EDITOR
        public virtual void DrawAreaInfo(Node n)
        {

        }


        public virtual void DrawGL(DrawGL draw,Node n,bool isSel)
        {

        }
#endif

    }
}