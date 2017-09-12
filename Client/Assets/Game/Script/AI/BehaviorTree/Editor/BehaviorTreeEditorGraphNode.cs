#region Header
/**
 * 名称：行为树编辑里的ui节点绘制
 
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
    public class BehaviorTreeEditorGraphNode
    {
        public const float cell = 10f;
        const float linkParentWidth = 35;
        const float linkChildrenWidth = 50;
        const float linkTreeWidth = 44;
        const float bkHeight = 75;

        public NodeCfg cfg;
        public Node node;
        public BehaviorTreeEditorGraphTree graphTree;
        public BehaviorTreeEditorGraphNode parent;
        public List<BehaviorTreeEditorGraphNode> children = new List<BehaviorTreeEditorGraphNode>();

        bool isSel = false;
        bool isHover = false;
        bool isLink = false;

        Rect rLinkParent;//上边
        Rect rLinkChildren;//下边
        Rect rBk;
        Rect rRootCountLabel;
        Rect rIcon;
        Rect rName;
        Rect rIngore;
        Rect rNote;
        Rect rNoteArea;
        Rect rExpand;
        Rect rTree;
        Rect rRightBottom;
        Rect rLeftBottom;
        
        float yChildLine = 0;
        

        public bool IsSel { get { return isSel; } set { isSel = value; } }
        public bool IsHover { get { return isHover; } set { isHover = value; } }
        public bool IsSelLink { get { return isLink; } set { isLink = value; } }//到父节点的链接是不是被选中中
        public bool IsParentNode { get { return cfg is ParentNodeCfg; } }
        public ParentNodeCfg CfgEx{ get { return cfg as ParentNodeCfg; } }

        public Rect RectLinkParent { get { return rLinkParent; } }
        public Rect RectLinkChildren { get { return rLinkChildren; } }
        public Rect RectBk { get { return rBk; } }
        public float Width { get { return Mathf.Clamp(rBk.width, 100, 200); } }
        public float YChildLine { get { return yChildLine; } }

        public Vector2 Pos
        {
            get
            {
                Vector2 pos = cfg.localPos;
                if (parent != null)
                    pos += parent.Pos;

                pos.x = Mathf.RoundToInt(pos.x / cell) * cell;
                pos.y = Mathf.RoundToInt(pos.y / cell) * cell;
                return pos;
            }

        }

        //加上所有子节点应该有的宽度
        public float WidthNeed
        {
            get
            {
                if (!NeedExpand)
                    return rBk.width;
                else
                {
                    float wTotalNeed = 0;
                    float wChild = 0;
                    foreach (var c in children)
                    {
                        wChild += (wChild!=0?5:0)+c.RectBk.width;
                        if(c.NeedExpand)
                            wTotalNeed += ((wTotalNeed != 0) ? 5 : 0) + c.WidthNeed;
                    }
                    
                    return Mathf.Max(Mathf.Max(wChild, rBk.width), wTotalNeed);
                }
            }
        }

        public bool NeedExpand
        {
            get {
                return children.Count > 0&&CfgEx.expandChild;
            }
        }

        public BehaviorTreeEditorGraphNode(NodeCfg cfg, Node node, BehaviorTreeEditorGraphNode parent, BehaviorTreeEditorGraphTree graphTree)
        {
            this.cfg = cfg;
            this.node = node;
            this.graphTree = graphTree;
            this.parent = parent;
        }

        public static void SetPos(NodeCfg cfg, Vector2 posNew, BehaviorTreeEditorGraphNode  p)
        {
            posNew.x = Mathf.RoundToInt(posNew.x / cell) * cell;
            posNew.y = Mathf.RoundToInt(posNew.y / cell) * cell;
            cfg.localPos = posNew;
            if (p != null)
                cfg.localPos -= p.Pos;
        }


        //需要整体移动的时候才设置子节点
        public void SetPos(Vector2 posNew, bool resetChild = false)
        {
            //对齐
            SetPos(cfg, posNew,parent);

            ResetRect(resetChild);

            if (IsParentNode)
            {
                if(resetChild)
                    ResetLink();
                else
                    parent.CheckChildLine();
            }
                
            if (parent != null)
                parent.CheckChildLine();
        }
        
        public void ResetRect(bool resetChild)
        {
            float minWidth;
            float maxWidth;
            EditorUtil.MiddleLabel.CalcMinMaxWidth(new GUIContent(cfg.NodeType.name), out minWidth, out maxWidth);
            float width = Mathf.Clamp(maxWidth, 100, 200);
            

            Vector2 pos = Pos;
            rBk = new Rect(pos.x - width / 2f, pos.y - bkHeight / 2f, width, bkHeight);
            rIngore = new Rect(rBk.xMin + 5, rBk.yMin + 1, 17, 17);
            rRootCountLabel = new Rect(pos.x - linkParentWidth / 2f, pos.y - bkHeight / 2 - 20, linkParentWidth, 20f);
            rLinkParent = new Rect(pos.x - linkParentWidth / 2f, pos.y - bkHeight / 2 - 10f, linkParentWidth, 32f);
            rLinkChildren = new Rect(pos.x - linkChildrenWidth / 2f, pos.y + bkHeight / 2 - 20, linkChildrenWidth, 32f);
            rExpand = new Rect(rLinkChildren.xMax - 18f, rLinkChildren.yMax - 20f, 18f, 20f);
            rIcon = new Rect(pos.x - 22, pos.y - 30, 44f, 44f);
            rName = new Rect(pos.x - width / 2, pos.y + 13, width, 20);
            rRightBottom = new Rect(rBk.xMax -35f, rBk.yMax - 35, 35f, 35f);
            rLeftBottom = new Rect(rBk.xMin +2, rBk.yMax - 27, 25f,25f);

            //注释
            rNote = new Rect(rBk.xMax - 20, rBk.yMin + 1, 17, 17);
            rNoteArea= new Rect(rBk.xMax +3, rBk.yMin , 120, bkHeight-10);

            if (cfg.Parent == null)
            {
                float minWidthTree;
                float maxWidthTree;
                EditorUtil.MiddleLabel.CalcMinMaxWidth(new GUIContent(cfg.Tree.name), out minWidthTree, out maxWidthTree);
                rTree = new Rect(rBk.xMax, rBk.yMin, maxWidthTree+10, 30f);
            }
            
            if (resetChild && IsParentNode)
            {
                for (int i = 0, j = children.Count; i < j; ++i)
                {
                    children[i].ResetRect(true);
                }
            }
        }
        public void CheckChildLine()
        {
            yChildLine = rLinkChildren.center.y + 25;

            float yMin = float.MaxValue;//找到最小的
            foreach (var n in children)
            {
                float yNode = n.RectLinkParent.center.y - 30;
                if (yNode <= yChildLine)
                    yNode = yChildLine;

                yMin = Mathf.Min(yNode, yMin);
            }

            if (yMin != float.MaxValue)
                yChildLine = yMin;

        }

        public void ResetLink()
        {
            if (IsParentNode)
            {
                CheckChildLine();
                for (int i = 0, j = children.Count; i < j; ++i)
                {
                    children[i].ResetLink();
                }
            }
        }

        public void DrawGraphNode()
        {
            using (new AutoChangeColor(cfg.Ingore ? new Color(0.7f, 0.7f, 0.7f, 1) : Color.white))
            {
                if(this.parent == null&& graphTree.tree != null)
                {
                    Color c1 = GUIStyle.none.normal.textColor;
                    GUIStyle.none.normal.textColor=Color.white;
                    GUI.Label(rRootCountLabel, string.Format("{0}", graphTree.tree.RunCounter), GUIStyle.none);
                    GUIStyle.none.normal.textColor = c1;
                }

                enTextureColor c = node != null && node.State == enNodeState.running ? enTextureColor.green : enTextureColor.black;

                //上部，连接父节点
                //if (this.parent != null)
                GUI.Box(rLinkParent, GUIContent.none, EditorUtil.TextureColorStyle(c, false));
                //else
                //{
                //    Color c1 =GUIStyle.none.normal.textColor;
                //    GUIStyle.none.normal.textColor=Color.white;
                //    GUI.Box(rTree, new GUIContent(tree.id.ToString(), EditorGUIUtility.IconContent("tree_icon_frond").image), GUIStyle.none);
                //    GUIStyle.none.normal.textColor = c1;

                //}



                //下部，连接子节点
                if (IsParentNode)
                {
                    GUI.Box(rLinkChildren, GUIContent.none, EditorUtil.TextureColorStyle(c, false));

                    if (children.Count != 0)
                    {
                        if (GUI.Button(rExpand, EditorGUIUtility.IconContent(CfgEx.expandChild ? "Toolbar Minus" : "Toolbar Plus"), GUIStyle.none))
                            CfgEx.expandChild = !CfgEx.expandChild;
                    }
                }

                //底框
                
                GUI.Box(rBk, GUIContent.none, EditorUtil.TextureColorStyle(c, isSel));

                //已运行状态显示
                if(node != null)
                {
                    if(node.State == enNodeState.success)
                        GUI.DrawTexture(rRightBottom, EditorUtil.LoadTexture2D("success"));
                    else if (node.State == enNodeState.failure)
                        GUI.DrawTexture(rRightBottom, EditorUtil.LoadTexture2D("failure"));
                }
                
                //图标和名字
                using (new AutoChangeColor(cfg.Ingore ? new Color(0.5f, 0.5f, 0.5f, 1f) : new Color(0.7f, 0.7f, 0.7f, 1f)))
                {
                    GUI.DrawTexture(rIcon, EditorUtil.LoadTexture2D("border"));
                }
                GUI.DrawTexture(rIcon, EditorUtil.LoadTexture2D("BehaviorIcon/" + cfg.NodeType.icon), ScaleMode.StretchToFill);
                GUI.Label(rName, cfg.NodeType.name, EditorUtil.MiddleLabel);
            }

            //如果中断起效，那么显示下
            var conditonalCfg = cfg as ConditionalCfg;
            if(conditonalCfg!=null&&conditonalCfg.interrupt != enInterrupt.none)
            {
                Conditional conditional = node as Conditional;
                Color c = conditional != null && conditional.IsInterrupting ? new Color(0f, 0.698f, 0.4f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);
                using (new AutoChangeColor(c))
                {
                    GUI.DrawTexture(rLeftBottom, EditorUtil.LoadTexture2D("BehaviorIcon/interrupt"), ScaleMode.StretchToFill);
                }
            }

            if (cfg.Parent == null)
            {
                GUI.Box(rTree, GUIContent.none, EditorUtil.AreaStyle(new Color(0.2f,0.2f,0.2f)));
                GUI.Label(rTree, cfg.Tree.name, EditorUtil.MiddleLabel);
                
            }

            //左上角禁用按钮
            if (isHover|| cfg.ingore)
                cfg.ingore = GUI.Toggle(rIngore, cfg.ingore, "", EditorUtil.IngoreButton);

            //右上角注释显示按钮和注释区域
            if (!string.IsNullOrEmpty(cfg.note) || isHover || cfg.showNote)
                cfg.showNote = GUI.Toggle(rNote, cfg.showNote, "", EditorUtil.TipButton);

            if (EditorPrefs.GetBool("editorShowDebugInfo") )
                GUI.Label(rBk, string.Format("    {0}   {1}", parent == null ? 0 : parent.children.IndexOf(this),cfg.id));

        }

        //绘制到父节点的连接
        public void DrawGrapLink(bool sel)
        {
            bool running = node != null && node.State == enNodeState.running;
            bool draw = sel && (IsSelLink || running) || (!sel && !IsSelLink&&!running);//选中或者运行中画在上面
            if (parent == null || !draw)
                return;
            Vector2 parentPos = parent.RectLinkChildren.center;
            Vector2 pos = this.RectLinkParent.center;

            Color c;
            if (IsSelLink)
                c = new Color(0.188f, 0.4588f, 0.6862f);
            else if (running)
                c = new Color(0f, 0.698f, 0.4f, 0.5f);
            else
                c = new Color(1f, 1f, 1f);
            EditorUtil.DrawPolyLine(c, 2,
                parentPos,
                new Vector2(parentPos.x, parent.YChildLine),
                new Vector2(pos.x, parent.YChildLine),
                pos
                );
        }

        public void DrawGraphNote()
        {
           
            if (cfg.showNote)
            {
                using (new AutoChangeBkColor(new Color(1f, 0.8f, 0.3f, 0.8f)))
                    cfg.note = EditorGUI.TextArea(rNoteArea, cfg.note, EditorStyles.textArea);
            }
        }

        public void DrawAreaInfo(BehaviorTreeEditorGraphNode graphNode) {
            graphNode.cfg.DrawAreaInfo(graphNode.node);
        }


        public bool IsHitLink(Vector2 pos)
        {
            if (parent == null)
                return false;
            Vector2 parentPos = parent.RectLinkChildren.center;
            Vector2 childPos = this.RectLinkParent.center;
            Vector2 middlePos = new Vector2(parentPos.x, parent.YChildLine);
            Vector2 middlePos2 = new Vector2(childPos.x, parent.YChildLine);

            if (MathUtil.GetRectByTwoPoint(childPos + new Vector2(-4, 0), middlePos2 + new Vector2(4, 0)).Contains(pos))
                return true;
            if (MathUtil.GetRectByTwoPoint(middlePos + new Vector2(0, -4), middlePos2 + new Vector2(0, 4)).Contains(pos))
                return true;
            return false;
        }

        public bool IsHitLinkChildren(Vector2 pos)
        {
            Vector2 parentPos = RectLinkChildren.center;
            Vector2 middlePos = new Vector2(parentPos.x, YChildLine);
            return MathUtil.GetRectByTwoPoint(parentPos + new Vector2(-4, 0), middlePos + new Vector2(4, 0)).Contains(pos);
        }

        public Vector2 GetNewChildPos()
        {
            if (children.Count == 0)
                return this.Pos + new Vector2(0, 110);
            else
                return children[children.Count - 1].Pos + new Vector2(children[children.Count - 1].Width / 2 + 60, 0);

        }

        public int GetIndexOfPos(float x)
        {
            int i = 0;
            for (; i < children.Count; ++i)
            {
                if (x <= children[i].Pos.x)
                    return i;
            }
            return i;
        }

        //检查子节点的ui顺序和数据顺序是不是一致，不是的话调整下
        public bool CheckChildPos(BehaviorTreeEditorGraphNode n)
        {
            
            int idxOld = children.IndexOf(n);
            if (idxOld == -1)
            {
                Debuger.LogError("逻辑错误，检查子节点排序的时候");
                return true;
            }
            children.SortEx((a, b) => a.Pos.x.CompareTo(b.Pos.x));
            int idx= children.IndexOf(n);
            if (idxOld == idx)
                return false;

            var childrenCfg = CfgEx.children;
            childrenCfg.Clear();
            foreach (var c in children)
            {
                childrenCfg.Add(c.cfg);
            }
            return true;
        }

        public bool HaveParent(HashSet<BehaviorTreeEditorGraphNode> set)
        {
            BehaviorTreeEditorGraphNode p = parent;
            while (p != null)
            {
                if (set.Contains(p))
                    return true;
                p = p.parent;
            }
            return false;
        }


        public void AlignNode()
        {
            if (children.Count == 0 || !CfgEx.expandChild)
                return;
            
            List<float> ws = new List<float>();
            float total = WidthNeed;
            
            float childY = Pos.y + 110;
            float left = this.Pos.x - total / 2;
            float addSingle = 0;
            for (int i = 0;i<children.Count;++i)
            {
                var c = children[i];
                if(c.NeedExpand)
                {
                    left -= addSingle;
                    addSingle = 0;
                }


                float w = c.WidthNeed;
                c.SetPos(new Vector2(left +(i!= 0?5:0)+w/2, childY),true);
                c.AlignNode();
                left += (i!=0?5:0)+w;
                if (!c.NeedExpand)
                    addSingle += (addSingle!=0?5:0)+ w;
            }
        }
    }
}