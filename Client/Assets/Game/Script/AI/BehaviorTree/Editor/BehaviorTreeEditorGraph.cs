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
    public class BehaviorTreeEditorGraph
    {
        public BehaviorTreeFileCfg cfg;
        public BehaviorTree tree;
        public BehaviorTreeEditor editor;
        public List<BehaviorTreeEditorGraphTree> graphTrees = new List<BehaviorTreeEditorGraphTree>();

        //选中逻辑
        public List<BehaviorTreeEditorGraphNode> selNodes = new List<BehaviorTreeEditorGraphNode>();
        public List<BehaviorTreeEditorGraphNode> selLinks = new List<BehaviorTreeEditorGraphNode>();
        //hover逻辑
        public BehaviorTreeEditorGraphNode hover;
        //复选区域
        public bool isSelectingArea = false;
        public Vector2 selAreaBegin;
        public Vector2 selAreaEnd;
        //连线中
        public bool isLinking = false;
        public BehaviorTreeEditorGraphNode linkParent;
        public BehaviorTreeEditorGraphNode linkChild;
        //拖动节点
        public bool dragPos = false;
        public Vector2 dragDelta = Vector2.zero;

        public bool haveAcitveTree;//有运行中的树


        public BehaviorTreeEditorGraph(BehaviorTreeEditor editor,BehaviorTreeFileCfg cfg, BehaviorTree tree)
        {
            this.cfg = cfg;
            this.tree = tree;
            this.editor = editor;
            Reset();
        }
        

        void FileChange(bool needResetGraph = true)
        {
            //重置下配置
            cfg.Reset();

            //通知编辑器重新创建graph
            if(needResetGraph || tree != null)
                editor.OnTreeCfgChange();
        }

        //重新创建所有节点
        void Reset()
        {
            haveAcitveTree = false;
            graphTrees.Clear();
            foreach (var t in cfg.trees)
            {
                bool isMatch = (tree != null && tree.Cfg == t);
                graphTrees.Add(new BehaviorTreeEditorGraphTree(t,this, isMatch ? tree:null));
                if (isMatch)
                    haveAcitveTree = true;
            }
            foreach (var t in graphTrees)//计算大小
                t.root.ResetRect(true);
            foreach (var t in graphTrees)//计算连线，注意这里必须在子节点计算大小之后
                t.root.ResetLink();
        }

        #region 节点操作
        public NodeCfg AddNode(BehaviorTreeEditorGraphNode parentGraphNode, Type nodeType)
        {
            if(parentGraphNode != null && !parentGraphNode.IsParentNode)
            {
                Debuger.LogError("不能添加子节点 不是父节点");
                return null;
            }
            ParentNodeCfg parentNode = parentGraphNode != null ? parentGraphNode.CfgEx : null;
            if (parentNode != null && !parentNode.CanAddChild())
            {
                Debuger.LogError("不能添加子节点 父节点只能添加:{0}个节点", parentNode.MaxChildren);
                return null;
            }
            if (!nodeType.IsSubclassOf(typeof(NodeCfg)))
            {
                Debuger.LogError("不是NodeCfg 不能作为子节点添加:{0}", nodeType);
                return null;
            }

            NodeCfg node = (NodeCfg)Activator.CreateInstance(nodeType);
            node.id = ++cfg.nodeCounter;
            if (node == null)
            {
                Debuger.LogError("创建节点失败:{0}", nodeType);
                return null;
            }

            //如果没有父节点，新建一个树
            if (parentNode == null)
            {
                BehaviorTreeCfg tree = cfg.AddTree();
                tree.root = node;
            }
            else
            {
                parentNode.expandChild = true;
                Vector2 pos = parentGraphNode.GetNewChildPos();
                parentNode.children.Add(node);
                node.Parent=parentNode;
                BehaviorTreeEditorGraphNode.SetPos(node,pos, parentGraphNode);
            }
            FileChange();
            return node;
        }

        public void RemoveNode(List<BehaviorTreeEditorGraphNode> graphNodes)
        {
            //先整理下，以方便操作,整理成一颗一颗树来，从下往上。不然每删一个节点就要reset下
            SortedDictionary<string, SortedList<int, BehaviorTreeEditorGraphNode>> idx = new SortedDictionary<string, SortedList<int, BehaviorTreeEditorGraphNode>>();
            foreach (var n in graphNodes)
            {
                SortedList<int, BehaviorTreeEditorGraphNode> l;
                if (!idx.TryGetValue(n.graphTree.cfg.name, out l))
                {
                    l = new SortedList<int, BehaviorTreeEditorGraphNode>(new RepeatableComparer<int>());
                    idx[n.graphTree.cfg.name] = l;
                }
                l.Add(-n.cfg.Depth, n);//这里取反，以倒序排列
            }


            foreach (var l in idx.Values)
            {
                foreach (var graphNode in l.Values)
                {
                    var n = graphNode.cfg;
                    ParentNodeCfg p = graphNode.CfgEx;

                    //和子节点断开，子节点重新创建一棵树
                    if (p != null)
                    {
                        for (int i =0;i<p.children.Count;++i)
                        {
                            var child = p.children[i];
                            Vector3 pos = graphNode.children[i].Pos;
                            BehaviorTreeCfg t = cfg.AddTree();
                            t.root = child;
                            child.Parent = null;
                            BehaviorTreeEditorGraphNode.SetPos(child,pos,null);
                        }
                        p.children.Clear();
                    }

                    //和父节点断开
                    if (n.Parent != null)
                    {
                        n.Parent.children.Remove(n);
                        n.Clear();
                    }
                    else//自己是根节点，那么删掉这棵树
                    {
                        var tree = n.Tree;
                        n.Clear();
                        cfg.RemoveTree(tree);
                    }
                }
            }
            FileChange();
        }

        public NodeCfg ReplaceNode(BehaviorTreeEditorGraphNode graphNode, Type nodeType)
        {
            var node = graphNode.cfg;
            if (node.Tree == null)
            {
                Debuger.LogError("节点已经删除");
                return null;
            }
            //先记录下父节点和子节点
            ParentNodeCfg parentNode = node.Parent;
            ParentNodeCfg p = node as ParentNodeCfg;
            List<BehaviorTreeEditorGraphNode> children = p != null ? new List<BehaviorTreeEditorGraphNode>(graphNode.children) : new List<BehaviorTreeEditorGraphNode>();
            Vector2 pos = graphNode.Pos;
            BehaviorTreeEditorGraphNode graphParentNode = graphNode.parent;

            //创建替换的节点
            NodeCfg nodeNew = (NodeCfg)Activator.CreateInstance(nodeType);
            nodeNew.id = ++cfg.nodeCounter;
            ParentNodeCfg pNew = nodeNew as ParentNodeCfg;

            //子节点添加
            foreach (var child in children)
            {
                //不能添加，那么创建新树
                if ( !pNew.CanAddChild())
                {
                    Vector2 posChild = child.Pos;
                    BehaviorTreeCfg t = cfg.AddTree();
                    t.root = child.cfg;
                    child.cfg.Parent = null;
                    BehaviorTreeEditorGraphNode.SetPos(child.cfg, posChild, null);
                }
                else//可以添加
                {
                    child.cfg.Parent = pNew;
                    pNew.children.Add(child.cfg);
                    //这里不用设置位置了，因为替换是不会修改位置的，而且pNew的位置还没有初始化
                }

            }

            //清空，从父节点删除,注意要放在子节点修改后，否则子节点位置会出错
            int insertPos = -1; 
            if (parentNode != null)
            {
                insertPos = parentNode.children.IndexOf(node);
                parentNode.children.RemoveAt(insertPos);

                //加到父节点,注意要先删后加
                parentNode.children.Insert(insertPos, nodeNew);
                nodeNew.Parent = parentNode;
            }
            else if(node.Tree.root == node)
            {
                node.Tree.root = nodeNew;
            }
            else
            {
                Debuger.LogError("逻辑错误");
                return null;
            }
                
            BehaviorTreeEditorGraphNode.SetPos(nodeNew, pos, graphParentNode);
            node.Clear();
            
            FileChange();
            return nodeNew;
        }

        public void LinkNode(BehaviorTreeEditorGraphNode parentGraphNode , BehaviorTreeEditorGraphNode childGraph)
        {
            ParentNodeCfg parent = parentGraphNode.CfgEx;
            var child = childGraph.cfg;
            var pos = childGraph.Pos;
            if (parent.Tree == null || child.Tree == null)
            {
                Debuger.LogError("节点已经删除");
                return;
            }

            //如果已经连上了
            if (child.Parent == parent)
            {
                Debuger.LogError("已经连上了");
                return;
            }
            else if (parent == child)
            {
                Debuger.LogError("节点不能自己连接自己");
                return;
            }

            //不能加
            if (!parent.CanAddChild())
            {
                Debuger.LogError("子节点数量限制，加不上去");
                return;
            }

            
            //和父节点断开
            if (child.Parent != null)
                child.Parent.children.Remove(child);
            else//自己是根节点，那么删掉这棵树
            {
                var tree = child.Tree;
                tree.root = null;
                cfg.RemoveTree(tree);
            }

            //加到新的父节点
            parent.expandChild = true;
            int insertIdx = parentGraphNode.GetIndexOfPos(pos.x);
            parent.children.Insert(insertIdx,child);
            child.Parent = parent;
            
            BehaviorTreeEditorGraphNode.SetPos(child, pos, parentGraphNode);
            FileChange();
        }

        public void RemoveLink(List<BehaviorTreeEditorGraphNode> graphNodes)
        {
            foreach (var graphNode in graphNodes)
            {
                var n = graphNode.cfg;
                if (n.Parent == null || n.Tree == null)
                    continue;
                if (!n.Parent.children.Remove(n))
                {
                    Debuger.LogError("逻辑错误，断开的时候从父节点删除失败");
                    continue;
                }
                Vector3 pos = graphNode.Pos;
                n.Parent = null;
                BehaviorTreeCfg t = cfg.AddTree();
                t.root = n;
                BehaviorTreeEditorGraphNode.SetPos(n, pos, graphNode.parent);
            }
            FileChange();

        }

        public void MoveNode(List<BehaviorTreeEditorGraphNode> graphNodes, Vector2 delta)
        {
            //先把子节点删掉，因为父节点移动的时候子节点就会跟着移动
            HashSet<BehaviorTreeEditorGraphNode> set = new HashSet<BehaviorTreeEditorGraphNode>(graphNodes);
            foreach (var graphNode in graphNodes)
            {
                if (graphNode.HaveParent(set))
                    set.Remove(graphNode);
            }

            bool haveChange = false;
            foreach (var graphNode in set)
            {
                graphNode.SetPos(graphNode.Pos + delta, true);
                if (graphNode.parent != null)
                {
                    if (graphNode.parent.CheckChildPos(graphNode))
                        haveChange = true;
                }
            }

            //有子节点位置变化的情况下，需要通知下，但是不用重置界面
            if (haveChange)
                FileChange(false);
        }
        
       

        static void RemoveHashNode(NodeCfg n,ref HashSet<int> removes,ref List<NodeCfg> roots)
        {
            //先递归到子节点
            ParentNodeCfg pn = n as ParentNodeCfg;
            if(pn != null)
            {
                for (int i = pn.children.Count-1;i>=0;--i)
                {
                    RemoveHashNode(pn.children[i], ref removes, ref roots);
                }
            }

            //如果要删
            if (!removes.Contains(n.id))
            {
                Vector2 pos = n.Pos;
                //从父节点删除
                if (n.Parent != null)
                    n.Parent.children.Remove(n);
                //子节点成为独立根节点
                if (pn != null)
                {
                    for (int i = pn.children.Count - 1; i >= 0; --i)
                    {
                        var c = pn.children[i];
                        c.localPos += pos;
                        c.Parent = null;
                        roots.Add(c);
                    }
                }

            }
            //如果不删
            else if (n.Parent == null)
                roots.Add(n);
        }

        void SetNewNodeId(NodeCfg n, ref List<NodeCfg> l)
        {
            l.Add(n);
            n.id = ++cfg.nodeCounter;
           
            
            ParentNodeCfg pn = n as ParentNodeCfg;
            if (pn != null)
            {
                for (int i = pn.children.Count - 1; i >= 0; --i)
                {
                    SetNewNodeId(pn.children[i],ref l);
                }
            }
        }

        //复制节点，并且复制其树变量
        public void CopyNode(List<BehaviorTreeEditorGraphNode> graphNodes,ref HashSet<int> copyIdx,ref string copyFile)
        {
            if (graphNodes.Count == 0)
                return;

            //建立要删除的索引
            copyIdx.Clear();
            foreach (var n in graphNodes)
                copyIdx.Add(n.cfg.id);

            copyFile = LitJson.JsonMapper.ToJson(cfg, false);
        }

       


        public List<NodeCfg> PasteNode( HashSet<int> copyIdx, string copyFile, Vector2 graphPos)
        {
            List<NodeCfg> l = new List<NodeCfg>();
            if (copyIdx == null || copyIdx.Count == 0)
                return l;

            //计算出新的根节点
            List<NodeCfg> copys = new List<NodeCfg>();
            BehaviorTreeFileCfg newFile = LitJson.JsonMapper.ToObject<BehaviorTreeFileCfg>(copyFile);
            newFile.Reset();
            foreach (var tree in newFile.trees)
            {
                RemoveHashNode(tree.root, ref copyIdx, ref copys);
            }
            Vector2 offset = graphPos - copys[0].localPos;


            Dictionary<ValueMgrCfg, string> dict = new Dictionary<ValueMgrCfg, string>();
            foreach (var n in copys)
            {
                BehaviorTreeCfg tree = cfg.AddTree();
                tree.root = n;

                //拷贝树变量，用序列化拷贝
                if (!dict.ContainsKey(n.Tree.valueMgrCfg))
                    dict[n.Tree.valueMgrCfg] = LitJson.JsonMapper.ToJson(n.Tree.valueMgrCfg, false);
                tree.valueMgrCfg = LitJson.JsonMapper.ToObject<ValueMgrCfg>(dict[n.Tree.valueMgrCfg]);

                //遍历下子节点重新设置id
                tree.root.localPos += offset;
                SetNewNodeId(tree.root, ref l);
            }

            FileChange();
            return l;
        }

        #endregion

        #region 查找相关
        public BehaviorTreeEditorGraphTree FindTree(BehaviorTreeCfg tree)
        {
            foreach (var t in graphTrees)//计算大小
            {
                if (t.cfg == tree)
                    return t;
            }
            return null;
        }

        public BehaviorTreeEditorGraphNode FindNode(NodeCfg nodeCfg)
        {
            return FindOne<BehaviorTreeEditorGraphNode>(InternalFindNode2, nodeCfg,false);
        }

        BehaviorTreeEditorGraphNode InternalFindNode2(BehaviorTreeEditorGraphNode graphNode, object param)
        {
            return (graphNode.cfg == (NodeCfg)param)? graphNode : null;
        }

        public T FindOne<T>(Func<BehaviorTreeEditorGraphNode, object, T> a,object param,bool childFirst =false)
        {
            
            foreach (var graphTree in graphTrees)
            {
                var ret = FindOneChild(graphTree.root,a,param, childFirst);
                if (ret != null)
                    return ret;
            }
            return default(T);
        }

        T FindOneChild<T>(BehaviorTreeEditorGraphNode graphNode, Func<BehaviorTreeEditorGraphNode, object, T> a, object param, bool childFirst)
        {
            if(!childFirst)
            {
                var ret = a(graphNode, param);
                if (ret != null)
                    return ret;
            }

            if(graphNode.IsParentNode && graphNode.CfgEx.expandChild)
            {
                for (int i = 0, j = graphNode.children.Count; i < j; ++i)
                {
                    var ret = FindOneChild(graphNode.children[i], a, param, childFirst);
                    if (ret != null)
                        return ret;
                }
            }

            if (childFirst)
            {
                var ret = a(graphNode, param);
                if (ret != null)
                    return ret;
            }
            return default(T);
        }

        public void FindAll<T>(Action<BehaviorTreeEditorGraphNode, object, ICollection<T>> a, object param, ICollection<T> l)
        {
            foreach (var graphTree in graphTrees)
            {
                FindAllChild<T>(graphTree.root, a, param,l);    
            }
        }

        void FindAllChild<T>(BehaviorTreeEditorGraphNode graphNode, Action<BehaviorTreeEditorGraphNode, object, ICollection<T>> a, object param, ICollection<T> l)
        {
            a(graphNode, param, l);

            if (graphNode.IsParentNode && graphNode.CfgEx.expandChild)
            {
                for (int i = 0, j = graphNode.children.Count; i < j; ++i)
                {
                    FindAllChild<T>(graphNode.children[i], a, param, l);
                }
            }
        }

        public void FindAllBreak<T>(Func<BehaviorTreeEditorGraphNode, object, ICollection<T>,bool> a, object param, ICollection<T> l)
        {
            foreach (var graphTree in graphTrees)
            {
                if (FindAllBreak<T>(graphTree.root, a, param, l))
                    return;
            }
        }

        bool FindAllBreak<T>(BehaviorTreeEditorGraphNode graphNode, Func<BehaviorTreeEditorGraphNode, object, ICollection<T>, bool> a, object param, ICollection<T> l)
        {
            if (a(graphNode, param, l))
                return true;

            if (graphNode.IsParentNode && graphNode.CfgEx.expandChild)
            {
                for (int i = 0, j = graphNode.children.Count; i < j; ++i)
                {
                    if (FindAllBreak<T>(graphNode.children[i], a, param, l))
                        return true;
                }
            }
            return false;
        }

        public BehaviorTreeEditorGraphNode FindNode(Vector2 pos)
        {
            return FindOne<BehaviorTreeEditorGraphNode>(InternalFindNode,pos,true);
        }

        BehaviorTreeEditorGraphNode InternalFindNode(BehaviorTreeEditorGraphNode graphNode, object param)
        {
            return graphNode.RectBk.Contains((Vector2)param) ? graphNode : null;
        }
        
        HashSet<BehaviorTreeEditorGraphNode> temList = new HashSet<BehaviorTreeEditorGraphNode>();
        public HashSet<BehaviorTreeEditorGraphNode> FindNodes(Rect r)
        {
            temList.Clear();
            FindAll<BehaviorTreeEditorGraphNode>(InternalFindNodes, r,temList);
            return temList;
        }

        void InternalFindNodes(BehaviorTreeEditorGraphNode graphNode, object param, ICollection<BehaviorTreeEditorGraphNode> l)
        {
            if (graphNode.RectBk.Overlaps((Rect)param))
                l.Add(graphNode);
        }

        public BehaviorTreeEditorGraphNode FindLinkChild(Vector2 pos)
        {
            return FindOne<BehaviorTreeEditorGraphNode>(InternalFindLinkChild, pos, false);
        }

        BehaviorTreeEditorGraphNode InternalFindLinkChild(BehaviorTreeEditorGraphNode graphNode, object param)
        {
            if (graphNode.IsParentNode && graphNode.RectLinkChildren.Contains((Vector2)param))
                return graphNode;
            else
                return null;
        }

        public BehaviorTreeEditorGraphNode FindLinkParent(Vector2 pos)
        {
            return FindOne<BehaviorTreeEditorGraphNode>(InternalFindLinkParent, pos, false);
        }

        BehaviorTreeEditorGraphNode InternalFindLinkParent(BehaviorTreeEditorGraphNode graphNode, object param)
        {
            if (graphNode.RectLinkParent.Contains((Vector2)param))
                return graphNode;
            else
                return null;
        }

        public void FindLinks(Vector2 pos, HashSet<BehaviorTreeEditorGraphNode> l)
        {
            FindAllBreak<BehaviorTreeEditorGraphNode>(InternalFindLinks, pos, l);
        }
        bool InternalFindLinks(BehaviorTreeEditorGraphNode graphNode, object param, ICollection<BehaviorTreeEditorGraphNode> l)
        {
            Vector2 pos = (Vector2)param;
            if (graphNode.IsHitLink(pos))
            {
                l.Add(graphNode);
                return true;
            }

            if(graphNode.IsParentNode && graphNode.CfgEx.expandChild)
            {
                if (graphNode.IsHitLinkChildren(pos))
                {
                    for (int i = 0, j = graphNode.children.Count; i < j; ++i)
                        l.Add(graphNode.children[i]);
                    return true;
                }
            }
            return true;
        }
        #endregion

        #region 选中逻辑 hover逻辑
        public bool IsSelectTree(BehaviorTreeEditorGraphTree tree)
        {
            foreach (var n in selNodes)
            {
                if (n.graphTree== tree)
                    return true;
            }
            return false;

        }

        public void RemoveSelectNode()
        {
            RemoveNode(selNodes);
            ClearAllSelect(true);

        }

        public void RemoveSelectLink()
        {
            RemoveLink(selLinks);
            ClearAllSelect(true);

        }

        //清空除了选中节点逻辑和hover逻辑外的其他逻辑
        public void ClearAllSelect(bool clearMulti = false)
        {
            //ctrl可以复选的东西，有些时候不需要清空
            if (clearMulti)
            {
                ClearSelect();
                ClearSelectLinks();
            }

            ClearHover();
            isSelectingArea = false;
            dragPos = false;
            dragDelta = Vector2.zero;
            isLinking = false;
            linkParent = null;
            linkChild = null;

        }

        public void ClearSelect()
        {
            foreach (var n in selNodes)
                n.IsSel = false;
            selNodes.Clear();
        }

        public void SetSelect(NodeCfg nodeCfg)
        {
            var graphNode = FindNode(nodeCfg);
            if (graphNode == null)
                return;
            SetSelect(graphNode);
        }

        public void SetSelect(BehaviorTreeEditorGraphNode n)
        {
            if (selNodes.Count == 1 && selNodes.Contains(n))
                return;
            ClearSelect();
            if (n == null)
                return;

            n.IsSel = true;
            selNodes.Add(n);
            isSelectingArea = false;
        }

        public void SetSelect(HashSet<BehaviorTreeEditorGraphNode> l)
        {
            //删掉重复的和取消选中
            for (int i = selNodes.Count - 1; i >= 0; --i)
            {
                var n = selNodes[i];
                if (l.Contains(n))
                    l.Remove(n);
                else
                {
                    n.IsSel = false;
                    selNodes.RemoveAt(i);
                }
            }

            foreach (var n in l)
            {
                if (n == null)
                    continue;
                n.IsSel = true;
                selNodes.Add(n);
            }
        }
        public void SetSelect(List<NodeCfg> l)
        {
            ClearSelect();
            foreach (var n in l)
            {
                var graphNode = FindNode(n);
                graphNode.IsSel = true;
                selNodes.Add(graphNode);
                
            }
            isSelectingArea = false;
        }

        public void AddSelect(BehaviorTreeEditorGraphNode n)
        {
            if (n == null)
                return;
            if (selNodes.Contains(n))
                return;

            n.IsSel = true;
            selNodes.Add(n);
            isSelectingArea = false;
        }

        public void ClearHover()
        {
            if (hover != null)
                hover.IsHover = false;
            hover = null;
        }

        public void SetHover(BehaviorTreeEditorGraphNode cfg)
        {
            if (hover == cfg)
                return;
            ClearHover();
            if (cfg == null)
                return;

            cfg.IsHover = true;
            hover = cfg;
        }


        public void ClearSelectLinks()
        {
            foreach (var n in selLinks)
                n.IsSelLink = false;
            selLinks.Clear();
        }

        public void SetLink(HashSet<BehaviorTreeEditorGraphNode> l)
        {
            //删掉重复的和取消选中
            for (int i = selLinks.Count - 1; i >= 0; --i)
            {
                var n = selLinks[i];
                if (l.Contains(n))
                    l.Remove(n);
                else
                {
                    n.IsSelLink = false;
                    selLinks.RemoveAt(i);
                }
            }

            foreach (var n in l)
            {
                if (n == null)
                    continue;
                n.IsSelLink = true;
                selLinks.Add(n);
            }
        }

        public void AddLink(HashSet<BehaviorTreeEditorGraphNode> l)
        {
            //删掉重复的
            for (int i = selLinks.Count - 1; i >= 0; --i)
            {
                var n = selLinks[i];
                if (l.Contains(n))
                    l.Remove(n);
            }

            foreach (var n in l)
            {
                if (n == null)
                    continue;
                n.IsSelLink = true;
                selLinks.Add(n);
            }
        }
        #endregion

       


        public void Draw()
        {
            foreach (var graphTree in graphTrees)
            {
                graphTree.Draw();
            }
        }
        
    }
}