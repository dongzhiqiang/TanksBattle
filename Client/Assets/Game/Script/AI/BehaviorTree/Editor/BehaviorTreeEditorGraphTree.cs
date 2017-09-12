#region Header
/**
 * 名称：BehaviorTreeEditorFile
 
 * 日期：2016.5.31
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditor;


namespace Simple.BehaviorTree
{
    public class BehaviorTreeEditorGraphTree
    {
        public BehaviorTreeEditorGraph graph;
        public BehaviorTreeCfg cfg;
        public BehaviorTree tree;
        public BehaviorTreeEditorGraphNode root;

        public BehaviorTreeEditorGraphTree(BehaviorTreeCfg cfg, BehaviorTreeEditorGraph graph,BehaviorTree tree)
        {
            this.cfg = cfg;
            this.tree = tree;
            this.graph = graph;
            this.root = NewChild(cfg.root, tree!= null?tree.Root: null, null);
        }

        BehaviorTreeEditorGraphNode NewChild(NodeCfg nodeCfg,Node n, BehaviorTreeEditorGraphNode parent)
        {
            BehaviorTreeEditorGraphNode graphNode = new BehaviorTreeEditorGraphNode(nodeCfg, n, parent, this);
            ParentNodeCfg parentNodeCfg = nodeCfg as ParentNodeCfg;
            if(parentNodeCfg!= null)
            {
                ParentNode pn = n as ParentNode;
                if(n != null && (pn == null || pn.ChildrenIdx.Count!= parentNodeCfg.children.Count))
                {
                    Debuger.LogError("逻辑错误，配置和当前运行中的行为树节点数不同");
                    pn = null;
                }


                for (int i = 0; i < parentNodeCfg.children.Count; ++i)
                {
                    Node childNode = pn != null ? tree.GetNode(pn.ChildrenIdx[i]):null;
                    graphNode.children.Add(NewChild(parentNodeCfg.children[i], childNode, graphNode));
                }
            }
            return graphNode;
        }

     
        public void Draw()
        {
            //绘制连线
            DrawChildLink(root, false);

            //绘制选中的连线
            DrawChildLink(root, true);

            //绘制节点
            DrawChild(root);

            //绘制提示
            DrawChildNote(root);
        }

        public void DrawChild(BehaviorTreeEditorGraphNode node)
        {
            node.DrawGraphNode();
            
            if (node.IsParentNode&& node.CfgEx.expandChild)
            {
                for (int i = 0, j = node.children.Count; i < j; ++i)
                {
                    DrawChild(node.children[i]);
                }
            }
        }

        public void DrawChildLink(BehaviorTreeEditorGraphNode node, bool sel)
        {
            node.DrawGrapLink(sel);
            
            if (node.IsParentNode && node.CfgEx.expandChild)
            {
                for (int i = 0, j = node.children.Count; i < j; ++i)
                {
                    DrawChildLink(node.children[i], sel);
                }
            }
        }

        public void DrawChildNote(BehaviorTreeEditorGraphNode node)
        {
            node.DrawGraphNote();

            if (node.IsParentNode && node.CfgEx.expandChild)
            {
                for (int i = 0, j = node.children.Count; i < j; ++i)
                {
                    DrawChildNote(node.children[i]);
                }
            }
        }
    }
}