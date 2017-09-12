using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Simple.BehaviorTree
{
    //行为树编辑器
    public class BehaviorTreeEditor : MultiWindow
    {
        static BehaviorTreeEditorWindow1 s_wnd1;
        static BehaviorTreeEditorWindow2 s_wnd2;

        [MenuItem("Tool/行为树编辑器 %F7", false, 109)]
        public static void Show()
        {
            Show("");
        }

        public static void Show(string id)
        {
            if (s_wnd1 == null || s_wnd1.MultiWindow == null)
            {
                BehaviorTreeEditor editor = Create<BehaviorTreeEditor, BehaviorTreeEditorWindow1>("行为树编辑器1", 600, 300.0f,true);
                s_wnd1 = (BehaviorTreeEditorWindow1)editor.adapterWindow;
                editor.multiId = 1;
                editor.Init();
                return;
            }
            //else if (((BehaviorTreeEditor)s_wnd1.MultiWindow).m_cxt == grabCxt){
            //    s_wnd1.Focus();
            //    return;
            //}
            else if (s_wnd2 == null || s_wnd2.MultiWindow == null)
            {
                BehaviorTreeEditor editor = Create<BehaviorTreeEditor, BehaviorTreeEditorWindow2>("行为树编辑器2", 600, 300.0f, true);
                s_wnd2 = (BehaviorTreeEditorWindow2)editor.adapterWindow;
                editor.multiId = 2;
                editor.Init();
                return;
            }
            //else if (((GrabEditor)s_wnd2.MultiWindow).m_cxt == grabCxt)
            //{
            //    s_wnd2.Focus();
            //    return;
            //}
            
            EditorUtility.DisplayDialog("", "不能同时打开超过两个行为树编辑器", "确定");
        }


        public static Color BkClr = new Color(0.1647f, 0.1647f, 0.1647f);//暗色
        public static Color SelectClr = new Color(0.188f, 0.4588f, 0.6862f, 0.5f);//蓝色
        public static Color SelectClr2 = new Color(0.38f, 0.9f, 1f, 1f);//蓝色
        public static Color LineClr = new Color(0.21f, 0.21f, 0.21f, 1);//暗灰色
        public static Color RunningClr = new Color(0f, 0.698f, 0.4f, 0.5f);//绿色

        static HashSet<int> s_copyIdx = new HashSet<int>();
        static string s_copyFile = "";
        

        bool m_debugArea = false;//调试面板
        bool m_globalArea = true;//全局面板
        bool m_treeArea = true;//树面板
        bool m_nodeArea = true;//节点面板
        string m_searchFile = "";

        //配置、实现、界面
        BehaviorTreeFileCfg m_cfg;
        BehaviorTree m_curTree;
        BehaviorTreeEditorGraph m_graph;

        Vector2 m_leftScroll = new Vector2(0, 0);

        //右边图相关
        Rect m_rGraph ;
        Rect m_rScrollView ;//比rGraph多了两条滚动条
        Rect m_rGraphTotal = new Rect(0, 0, 4000, 4000);
        Vector2 m_rightScroll ;
        Vector2 m_mousePosition ;
        bool m_isFirstRectGraph=true;
        string m_debugBehavior = "";

        //GL绘制
        int m_observer;


        public void Init()
        {
            SetCfg(null,null);     
        }

        public override void OnEnable()
        {
            m_observer = EventMgr.AddAll(MSG.MSG_FRAME, MSG_FRAME.FRAME_DRAW_GL, OnDrawGL);
        }

        public override void OnDisable()
        {
            if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
        }

        void OnDrawGL(object obj)
        {
            if (m_cfg == null || m_graph == null|| m_curTree==null) return;
            DrawGL draw = (DrawGL)obj;

            HashSet<BehaviorTreeEditorGraphNode> nodeIdx = new HashSet<BehaviorTreeEditorGraphNode>(m_graph.selNodes);

            m_graph.FindAll<BehaviorTreeEditorGraphNode>((gn, param, l) =>
            {
                gn.cfg.DrawGL(draw,gn.node, nodeIdx.Contains(gn));

            }, null, null);

          
        }

        public override void OnDestroy() { SetCfg(null, null); }

        
        //绘制窗口时调用
        public override void OnGUI()
        {
            //做一些判断
            if (m_curTree!=null&&(!m_curTree.enabled|| !m_curTree.gameObject.activeInHierarchy))
                SetCfg(m_cfg,null);

            m_mousePosition =UnityEngine.Event.current.mousePosition;
            //工具栏
            using (new AutoBeginHorizontal(EditorStyles.toolbarButton))
            {
                DrawToolBar();
            }

            using (new AutoBeginHorizontal())
            {
                //左边参数填写区域
                using (AutoBeginScrollView sv = new AutoBeginScrollView(m_leftScroll, "PreferencesSectionBox", GUILayout.Width(400)))
                {
                    m_leftScroll= sv.Scroll;
                    using (new AutoLabelWidth(80))
                    {
                        EditorGUILayoutEx.FadeArea area = EditorGUILayoutEx.instance.BeginFadeArea(m_debugArea, "调试面板", GetPrefs("m_debugArea"), EditorStyleEx.BoxStyle);
                        m_debugArea = area.open;
                        if (area.Show())
                            DrawDebugArea();
                        EditorGUILayoutEx.instance.EndFadeArea();

                        area = EditorGUILayoutEx.instance.BeginFadeArea(m_globalArea, "全局面板", GetPrefs("m_globalArea"), EditorStyleEx.BoxStyle);
                        m_globalArea = area.open;
                        if (area.Show())
                            DrawGrobalArea();
                        EditorGUILayoutEx.instance.EndFadeArea();

                        if(m_cfg != null)
                        {
                            area = EditorGUILayoutEx.instance.BeginFadeArea(m_treeArea, "树面板", GetPrefs("m_treeArea"), EditorStyleEx.BoxStyle);
                            m_treeArea = area.open;
                            if (area.Show())
                                DrawTreeArea();
                            EditorGUILayoutEx.instance.EndFadeArea();

                            area = EditorGUILayoutEx.instance.BeginFadeArea(m_nodeArea, "节点面板", GetPrefs("m_nodeArea"), EditorStyleEx.BoxStyle);
                            m_nodeArea = area.open;
                            if (area.Show())
                                DrawNodeArea();
                            EditorGUILayoutEx.instance.EndFadeArea();
                        }
                        
                    }

                }

                //右边，行为树的图,这里先转成Rect画会方便点
                GUILayout.Box(GUIContent.none, EditorUtil.BoxStyle(BkClr), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                if (UnityEngine.Event.current.type == EventType.Repaint)
                {
                    m_rScrollView = GUILayoutUtility.GetLastRect();//比rGraph多了两条滚动条
                    m_rGraph = new Rect(m_rScrollView.x, m_rScrollView.y, m_rScrollView.width - 15, m_rScrollView.height - 15);

                    if(m_isFirstRectGraph)
                    {
                        m_isFirstRectGraph = false;
                        m_rightScroll = new Vector2((m_rGraphTotal.width- m_rScrollView.width)/2, (m_rGraphTotal.height - m_rScrollView.height) / 2);
                        
                    }
                }
                DrawGraph();
                
                //GUI.Label(m_rGraph, string.Format("{0} {1} {2} {3} {4}", m_rGraph, m_rGraphTotal, m_rightScroll, MouseToGraphPos(m_mousePosition), m_mousePosition));
            }

           
            //事件处理
            if (m_cfg != null)
            {
                HandleGraph();
            }
            
        }

        //工具栏
        void DrawToolBar()
        {
            //选择ai文件
            if (GUILayout.Button(m_cfg == null? "选择行为树" :  m_cfg.File, EditorStyles.toolbarPopup, GUILayout.Width(160)))
            {
                GenericMenu genericMenu = new GenericMenu();
                foreach (var f in BehaviorTreeMgrCfg.instance.files)
                {
                    string ff = f;
                    if (!string.IsNullOrEmpty(m_searchFile) && !f.Contains(m_searchFile))
                        continue;
                    genericMenu.AddItem(new GUIContent(ff), m_cfg == null ? false: m_cfg.File == ff, () =>
                    {
                        BehaviorTreeFileCfg cfg = BehaviorTreeMgrCfg.instance.GetFile(ff);
                        if (cfg == null)
                            return;
                        CloseFile();//之前有的话先删掉
                        SetCfg(cfg, m_curTree);
                    });
                }
                genericMenu.AddSeparator(string.Empty);//华丽的分割线
                genericMenu.AddItem(new GUIContent("新建"), false, () =>
                {
                    MessageBoxWindow.ShowAsInputBox(string.Format("ai_{0}", DateTime.Now.Ticks), (string file, object context) =>
                    {
                        BehaviorTreeFileCfg cfg = BehaviorTreeMgrCfg.instance.AddFile(file);
                        if (cfg == null)
                            return;
                        CloseFile();//之前有的话先删掉
                        SetCfg(cfg, m_curTree);
                    });
                });

                 
                genericMenu.ShowAsContext();
            }

            //设置筛选
            using (new AutoBeginHorizontal(GUILayout.Width(200)))
            {
                m_searchFile = EditorGUILayout.TextField(GUIContent.none, m_searchFile, "ToolbarSeachTextField", GUILayout.ExpandWidth(true));
                if (GUILayout.Button("", string.IsNullOrEmpty(m_searchFile) ? "ToolbarSeachCancelButtonEmpty" : "ToolbarSeachCancelButton"))
                    m_searchFile = "";
            }

            if (m_cfg != null)
            {
                //保存
                if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Duplicate"), EditorStyles.toolbarButton, GUILayout.Width(30)))
                {
                    BehaviorTreeMgrCfg.instance.SaveFile(m_cfg);
                    adapterWindow.ShowNotification(new GUIContent("保存成功"));
                }

                //重载
                if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Refresh"), EditorStyles.toolbarButton, GUILayout.Width(30)))
                {
                    if (EditorUtility.DisplayDialog("", "是否确定要重载ai文件，所有未保存的修改都将还原?(注意要重新进场景或者重新创建怪物，怪物上的ai才会重载)", "是", "否"))
                    {
                        string f = m_cfg.File;
                        BehaviorTreeMgrCfg.instance.RemoveFileCache(m_cfg);
                        SetCfg(BehaviorTreeMgrCfg.instance.GetFile(f), m_curTree);
                    }
                }

                //删除
                if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyles.toolbarButton, GUILayout.Width(30)))
                {
                    if (EditorUtility.DisplayDialog("", "是否确定要删除ai文件?", "是", "否"))
                    {
                        BehaviorTreeMgrCfg.instance.RemoveFile(m_cfg);
                        SetCfg(null, m_curTree);
                    }
                }
            }

            GUILayout.Box("", EditorStyles.toolbar,GUILayout.ExpandWidth(true));
            if (m_cfg != null)
            {
                if (GUILayout.Button("显示所有注释", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    m_cfg.DoAll(n =>
                    {
                        if (!string.IsNullOrEmpty(n.note))
                            n.showNote = true;
                    });
                }

                if (GUILayout.Button("隐藏所有注释", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    m_cfg.DoAll(n => n.showNote = false);
                }
            }

            bool editorShowDebugInfo = EditorPrefs.GetBool("editorShowDebugInfo");
            bool newEditorShowDebugInfo = GUILayout.Toggle(editorShowDebugInfo, "调试信息", EditorStyles.toolbarButton, GUILayout.Width(80));
            if(newEditorShowDebugInfo!= editorShowDebugInfo)
            {
                EditorPrefs.SetBool("editorShowDebugInfo", newEditorShowDebugInfo);
            }
        }

        void SetCfg(BehaviorTreeFileCfg cfg, BehaviorTree curTree)
        {
            if (m_cfg == cfg && m_curTree == curTree)
                return;

            if (m_curTree != null)
                m_curTree.onChangeTree -= OnTreeChange;
            
            m_cfg = cfg;
            m_curTree = curTree;
            if(Application.isPlaying&& BehaviorTreeMgr.instance != null)
            {
                if (multiId == 1)
                    BehaviorTreeMgr.instance.m_debug1 = m_curTree;
                else if(multiId == 2)
                    BehaviorTreeMgr.instance.m_debug2 = m_curTree;

            }

            if (m_cfg != null)
                m_graph = new BehaviorTreeEditorGraph(this, m_cfg, m_curTree);
            else
                m_graph = null;

            if (m_curTree != null)
                m_curTree.onChangeTree += OnTreeChange;
        }

        //当前行为树完全停止了或者配置上的树有结构性改动
        public void OnTreeCfgChange()
        {
            if(m_cfg!= null)
            {
                //暂停所有正在运行的行为树
                if(Application.isPlaying)
                    BehaviorTreeMgr.instance.ReCreate(m_cfg);

                
                m_graph = new BehaviorTreeEditorGraph(this, m_cfg, m_curTree);
            }
        }

        public void OnTreeChange()
        {
            if(m_cfg != null )
            {
                if((m_curTree.Cfg != null && m_curTree.Cfg.File == m_cfg)|| m_graph.haveAcitveTree)
                    m_graph = new BehaviorTreeEditorGraph(this, m_cfg, m_curTree);
            }
        }

        void CloseFile()
        {
            if (m_cfg == null)
                return;
            if (EditorUtility.DisplayDialog("", "是否保存" + m_cfg.File + "的修改?", "保存", "否"))
                BehaviorTreeMgrCfg.instance.SaveFile(m_cfg);
            //else
            //{
            //    BehaviorTreeMgrCfg.instance.RemoveFileCache(m_cfg);//FIX:如果加了这一行会导致下次切过来的时候编辑的不是当前活动的配置
            //}
            SetCfg(m_cfg, m_curTree);
        }
 
        //调试面板
        void DrawDebugArea()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.LabelField("运行的时候才可以调试");
                return;
            }

            if (m_curTree != null)
            {
                if (m_curTree.Cfg != null)
                    EditorGUILayout.LabelField(string.Format("当前行为:{0} {1}", m_curTree.BehaviorName, m_curTree.IsPlaying ? "播放中" : "暂停"));
                else
                    EditorGUILayout.LabelField("无行为");
            }

            //寻找调试对象
            using (new AutoBeginHorizontal())
            {
                BehaviorTree t = (BehaviorTree)EditorGUILayout.ObjectField("调试对象", m_curTree, typeof(BehaviorTree), true);
                if (t != m_curTree)
                    SetCfg(m_cfg, t);

                if(m_cfg != null && GUILayout.Button("下一个", EditorStyles.toolbarButton))
                {
                    t =BehaviorTreeMgr.instance.FindNextTree(m_cfg, m_debugBehavior, m_curTree);
                    if(t!= null&& m_curTree!=t)
                        SetCfg(m_cfg, t);
                }
            }
            

            if (m_cfg != null)
            {
                int oldIdx = Array.IndexOf(m_cfg.names, m_debugBehavior);
                int idx = EditorGUILayout.Popup("调试行为", oldIdx, m_cfg.names);
                if (oldIdx != idx)
                {
                    if (idx == -1)
                        m_debugBehavior = "";
                    else
                        m_debugBehavior = m_cfg.names[idx];
                }
            }

            if (m_curTree!= null)
            {   
                using (new AutoBeginHorizontal()) { 

                    if (m_curTree.Cfg != null)
                    {
                        if (GUILayout.Button(m_curTree.IsPlaying?"暂停":"播放", EditorStyles.toolbarButton))
                        {
                            if (m_curTree.IsPlaying)
                                m_curTree.Pause();
                            else
                                m_curTree.RePlay(false);
                        }
                    }

                    if(m_cfg!= null&&!string.IsNullOrEmpty(m_debugBehavior))
                    {
                        if (m_curTree.Cfg == null || m_curTree.Cfg.File != m_cfg || m_curTree.Cfg.name != m_debugBehavior)
                        {
                            if (GUILayout.Button("播放调试行为", EditorStyles.toolbarButton))
                            {
                               m_curTree.Play(m_cfg.File, m_debugBehavior);
                            }
                        }
                        else if (GUILayout.Button("从头播放调试行为", EditorStyles.toolbarButton))
                        {
                            m_curTree.Stop();
                            m_curTree.Play(m_cfg.File, m_debugBehavior);
                        }
                    }

                    if(m_curTree.IsPlaying)
                    {
                        if(GUILayout.Button(m_curTree.m_runtimeUpdateCancel? "自动更新":"不更新", EditorStyles.toolbarButton))
                        {
                            m_curTree.m_runtimeUpdateCancel = !m_curTree.m_runtimeUpdateCancel;
                        }
                        if (GUILayout.Button("下一帧", EditorStyles.toolbarButton))
                        {
                            if (!m_curTree.m_runtimeUpdateCancel)
                                m_curTree.m_runtimeUpdateCancel = true;
                            m_curTree.CallUpdate();
                        }
                    }   
                }
            }

            using (new AutoBeginHorizontal())
            {
                //播放所有
                if (GUILayout.Button("播放所有", EditorStyles.toolbarButton))
                {
                    BehaviorTreeMgr.instance.PlayAll();
                }

                //暂停所有
                if (GUILayout.Button("暂停所有", EditorStyles.toolbarButton))
                {
                    BehaviorTreeMgr.instance.PauseAll();
                }

            }
        }

        //全局面板
        void DrawGrobalArea()
        {
            //全局变量
            ValueMgr mgr = !Application.isPlaying ? null : BehaviorTreeMgr.instance.m_valueMgr;
            BehaviorTreeMgrCfg.instance.valueMgrCfg.Draw(mgr);
        }
        
        //树面板
        void DrawTreeArea()
        {
            EditorGUILayout.TextField("文件名",m_cfg.File);
            foreach (var tree in m_graph.graphTrees)
            {

                bool isSel = m_graph.IsSelectTree(tree);
                string title = string.Format("树:{0}",tree.cfg.name);
                Color c= GUI.contentColor;
                GUI.contentColor = isSel ? SelectClr2 : Color.white;
                EditorGUILayoutEx.FadeArea area = EditorGUILayoutEx.instance.BeginFadeArea(tree.cfg.expand, title, GetPrefs("tree_" + tree.cfg.name), EditorStyleEx.BoxStyle);
                GUI.contentColor = c;
                tree.cfg.expand = area.open;
                if (area.Show())
                {
                    DrawTree(tree);
                }
                EditorGUILayoutEx.instance.EndFadeArea();
            }
        }

        void DrawTree(BehaviorTreeEditorGraphTree tree)
        {
            using (new AutoBeginHorizontal())
            {
                tree.cfg.SetName(EditorGUILayout.TextField("名字", tree.cfg.name));
                
                if (GUILayout.Button(EditorGUIUtility.IconContent("tree_icon_frond"), EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    
                    GraphScrollToPos(tree.root.Pos);
                    m_graph.SetSelect(tree.root);
                }   
            }

            //树变量
            tree.cfg.valueMgrCfg.Draw(tree.tree ==null?null:tree.tree.ValueMgr);

        }

        //节点面板
        void DrawNodeArea()
        {
            BehaviorTreeEditorGraphNode graphNode = m_graph.selNodes.Count == 0 ? null : m_graph.selNodes[0];
            
            if (graphNode == null  )
            {
                EditorGUILayout.LabelField("请先选中一个节点");
            }
            else
            {
                var n = graphNode.cfg;
                using (new AutoFontSize(13, EditorStyles.helpBox))
                {
                    string content = string.Format("id:{0} 节点名:{1} 树:{2}\n{3}", n.id, n.NodeType.name, n.Tree.name, n.NodeType.desc);
                    float height = EditorStyles.helpBox.CalcHeight(new GUIContent(content), 370);
                    EditorGUILayout.LabelField(content, EditorStyles.helpBox,GUILayout.Height(height));
                }

                ConditionalCfg conditionalCfg = n as ConditionalCfg;
                if(conditionalCfg!=null)
                {
                    using (new AutoEditorTipButton(@"当希望一个节点在运行时状态下可以被中断，可以把它的子条件节点设置为中断.
那么这个条件节点就会在每次更新时判断下，如果达到中断条件则让父节点重新执行(或者树重新执行)
注意只有运行中的节点的子条件节点可以中断它，嵌套多层的条件节点无效
中断类型:
不中断,不判断中断
成功中断，调用正常逻辑判断条件是否成功
失败中断，调用正常逻辑判断条件是否失败
自定制中断，具体的实现提供不同的功能，比如仇恨目标发生变化等
"))
                    {
                        conditionalCfg.interrupt = (enInterrupt)EditorGUILayout.Popup("中断类型", (int)conditionalCfg.interrupt, ConditionalCfg.InterruptTypeNames);
                    }
                    if(conditionalCfg.interrupt!= enInterrupt.none)
                    {
                        using (new AutoEditorTipButton("勾选则中断的时候树重新执行，否则父节点重新执行"))
                            conditionalCfg.resetTreeWhenInterrupt = EditorGUILayout.Toggle("中断时重置树",conditionalCfg.resetTreeWhenInterrupt);
                    }
                    
                }

                graphNode.DrawAreaInfo(graphNode);
            }
        }

        //右边，行为树的图
        void DrawGraph()
        {
            if (m_graph == null)
                return;
            EditorUtil.DrawGrid(LineClr, 10f, new Vector2(m_rightScroll.x % 10f, m_rightScroll.y % 10f), m_rGraph);
            EditorUtil.DrawGrid(LineClr * 1.2f, 50f, new Vector2(m_rightScroll.x % 50f, m_rightScroll.y % 50f), m_rGraph);
            if (UnityEngine.Event.current.type != EventType.ScrollWheel)//不接受滚动响应
            {
                m_rightScroll = GUI.BeginScrollView(m_rScrollView, m_rightScroll, m_rGraphTotal, true, true);
                {
                    //连接中的线
                    if (m_graph.isLinking)
                    {
                        Vector2 graphPos = MouseToGraphPos(m_mousePosition);
                        Vector2 linkPos = graphPos;
                        if (m_graph.linkParent != null)
                            linkPos = m_graph.linkParent.RectLinkParent.center;
                        else if (m_graph.linkChild != null)
                            linkPos = m_graph.linkChild.RectLinkParent.center;

                        float half = (graphPos.y + linkPos.y) / 2;
                        EditorUtil.DrawPolyLine(new Color(1f, 1f, 1f), 2,
                            graphPos,
                            new Vector2(graphPos.x, half),
                            new Vector2(linkPos.x, half),
                            linkPos
                        );

                    }

                    m_graph.Draw();

                    //绘制拖动选中区域
                    if (m_graph.isSelectingArea && MathUtil.CanRect(m_graph.selAreaBegin, m_graph.selAreaEnd))
                    {
                        GUI.Box(MathUtil.GetRectByTwoPoint(m_graph.selAreaBegin, m_graph.selAreaEnd), GUIContent.none, EditorUtil.BoxStyle(SelectClr));
                    }
                }
                
                GUI.EndScrollView();
            } 
        }
        
        void HandleGraph()
        {
            var e = UnityEngine.Event.current;
            
            if (e.type == EventType.Repaint || e.type == EventType.Layout || !m_rGraph.Contains(e.mousePosition))
                return;
            Vector2 graphPos = MouseToGraphPos(e.mousePosition);
            switch (e.type)
            {
                case EventType.MouseDown:
                    {
                        if(HandleMouseDown(graphPos,e))
                            e.Use();
                    }
                    break;
                case EventType.MouseUp:
                    {
                        //连线
                        if(m_graph.isLinking)
                        {
                            if(m_graph.linkParent != null)
                            {
                                var linkChild = m_graph.FindLinkChild(graphPos);
                                if (linkChild == null)
                                    linkChild = m_graph.FindNode(graphPos) ;

                                if(linkChild != null&& linkChild.IsParentNode)
                                    m_graph.LinkNode(linkChild, m_graph.linkParent);
                            }
                            else if (m_graph.linkChild != null)
                            {
                                var linkParent = m_graph.FindLinkParent(graphPos);
                                if (linkParent == null)
                                    linkParent = m_graph.FindNode(graphPos) ;
                                if (linkParent != null)
                                    m_graph.LinkNode(m_graph.linkChild, linkParent);
                            }
                            e.Use();
                        }
                        //弹出菜单
                        else if(e.button ==1)
                        {
                            GenericMenu menu = new GenericMenu();
                            var n = m_graph.FindNode(graphPos);
                            if (n == null)
                            {
                                //添加节点
                                AddAllNodeToMenu(menu, "添加节点", OnAddNew, null);
                                //黏贴
                                if (s_copyIdx.Count >0)
                                    menu.AddItem(new GUIContent("黏贴"), false, OnPasteNode);
                                else
                                    menu.AddDisabledItem(new GUIContent("黏贴"));
                            }
                            else if (n.IsParentNode)
                            {

                                //添加子节点
                                AddAllNodeToMenu(menu, "添加", OnAddChild, null);
                                //替换
                                AddAllNodeToMenu(menu, "替换", OnReplace, n.cfg.NodeType);
                                //复制
                                menu.AddItem(new GUIContent("复制"), false, OnCopyNode);
                                //黏贴
                                if (s_copyIdx.Count > 0)
                                    menu.AddItem(new GUIContent("黏贴"), false, OnPasteNode);
                                else
                                    menu.AddDisabledItem(new GUIContent("黏贴"));
                                //排列
                                menu.AddItem(new GUIContent("排列"), false, OnAlignNode);
                                //删除
                                menu.AddItem(new GUIContent("删除"), false, m_graph.RemoveSelectNode);
                                //代码
                                menu.AddItem(new GUIContent("代码"), false, () => OpenScript(n.cfg));
                            }
                            else
                            {
                                //替换
                                AddAllNodeToMenu(menu, "替换", OnReplace, n.cfg.NodeType);
                                //复制
                                menu.AddItem(new GUIContent("复制"), false, OnCopyNode);
                                //黏贴
                                if (s_copyIdx.Count > 0)
                                    menu.AddItem(new GUIContent("黏贴"), false, OnPasteNode);
                                else
                                    menu.AddDisabledItem(new GUIContent("黏贴"));
                                //删除
                                menu.AddItem(new GUIContent("删除"), false, m_graph.RemoveSelectNode);
                                //代码
                                menu.AddItem(new GUIContent("代码"), false, () => OpenScript(n.cfg));
                            }

                            menu.ShowAsContext();
                            e.Use();
                            return;
                        }
                        m_graph.ClearAllSelect();
                    };break;
                case EventType.MouseMove:
                    {
                        //hover逻辑
                        var n = m_graph.FindNode(graphPos);
                        m_graph.SetHover(n);
                    }
                    break;
                case EventType.MouseDrag:
                    {
                        //拖动区域以选中节点
                        if(m_graph.isSelectingArea)
                        {
                            m_graph.selAreaEnd = graphPos;
                            m_graph.SetSelect(m_graph.FindNodes(MathUtil.GetRectByTwoPoint(m_graph.selAreaBegin, m_graph.selAreaEnd)));
                            
                        }//拖动节点
                        else if(m_graph.dragPos)
                        {
                            m_graph.dragDelta += e.delta;
                            float x = ((int)(m_graph.dragDelta.x/NodeCfg.cell))*NodeCfg.cell;
                            float y = ((int)(m_graph.dragDelta.y/NodeCfg.cell))*NodeCfg.cell;
                            m_graph.dragDelta.x-=x;
                            m_graph.dragDelta.y-=y;
                            m_graph.MoveNode(m_graph.selNodes,new Vector2(x,y));
                        }
                        //alt+左键 或者鼠标中键 滚动区域
                        else if(e.button == 2 || (e.alt &&e.button==0))
                        {
                            m_rightScroll -= e.delta;
                            
                        }
                        e.Use();
                    }
                    break;
                case EventType.ScrollWheel:
                    {
                        //放大缩小区域
                        //不支持，要在ScrollView中用GUI.matrix做矩阵变换，不太熟。要做的时候参考AutoBeginZoomGroup
                    }
                    break;
                case EventType.KeyUp:
                    {
                        //删除
                        if (e.keyCode == KeyCode.Delete || e.commandName.Equals("Delete"))
                        {
                            if(m_graph.selNodes.Count>0)
                                m_graph.RemoveSelectNode();
                            if (m_graph.selLinks.Count > 0)
                                m_graph.RemoveSelectLink();
                            e.Use();
                        }
                    };break;
                default:break;
            }
        }
        
        //HandleGraph逻辑太多了，分出来写
        bool HandleMouseDown(Vector2 graphPos, UnityEngine.Event e)
        {
            m_graph.ClearAllSelect();

            //可能接下来要滚动界面
            if (e.alt)
                return true;

            if (e.button == 0)
            {
                var n = m_graph.FindNode(graphPos);
                if(n!=null)
                {
                    if (m_graph.selNodes.Contains(n))//选中已经选中的界面可能是想要拖动
                        m_graph.dragPos = true;
                    else if (e.control)//选中节点(ctrl多选)
                        m_graph.AddSelect(n);
                    else //选中节点
                    {
                        m_graph.SetSelect(n);
                        m_graph.dragPos = true;
                    }
                    return true;
                }

                //连线
                m_graph.linkParent = m_graph.FindLinkParent(graphPos);
                if(m_graph.linkParent !=null)
                {
                    m_graph.isLinking = true;
                    return true;
                }
                m_graph.linkChild = m_graph.FindLinkChild(graphPos);
                if (m_graph.linkChild != null)
                {
                    m_graph.isLinking = true;
                    return true;
                }

                //选中线
                HashSet<BehaviorTreeEditorGraphNode> l = new HashSet<BehaviorTreeEditorGraphNode>();
                m_graph.FindLinks(graphPos, l);
                if(l.Count!=0)
                {
                    if (e.control)
                        m_graph.AddLink(l);
                    else
                        m_graph.SetLink(l);
                    return true;
                }

                //拖出一个区域来选中区域内的节点
                m_graph.ClearSelect();
                m_graph.ClearSelectLinks();
                m_graph.isSelectingArea = true;
                m_graph.selAreaBegin = graphPos;
                m_graph.selAreaEnd = graphPos;
                return true;
            }
            else if (e.button == 1)
            {
                //没选中选中，已经选中的提前到第一个
                var n = m_graph.FindNode(graphPos);
                if (n != null && m_graph.selNodes.Contains(n))
                {
                    m_graph.selNodes.Remove(n);
                    m_graph.selNodes.Insert(0, n);
                }
                else
                    m_graph.SetSelect(n);

             
                return true;
            }
            return false;
        }

        #region 菜单相关
        //在代码编辑器里打开
        public static void OpenScript(NodeCfg obj)
        {
            string scriptName = obj.GetType().Name;
            scriptName = scriptName.Substring(0, scriptName.Length-3);
            //List<MonoScript> array = EditorUtil.LoadAssetsAtPath<MonoScript>("Game/Script/AI");
            MonoScript[] array = (MonoScript[])Resources.FindObjectsOfTypeAll(typeof(MonoScript));
            for (int i = 0; i < array.Length; i++)
            {
                var a = array[i];
                if (a == null)
                    continue;

                var c = a.GetClass();
                if (c == null)
                    continue;

                if ( c.Name == scriptName)//c.IsSubclassOf(typeof(Node))&&
                {
                    AssetDatabase.OpenAsset(a);
                    return;
                }
            }
        }
       

        void OnAddNew(object userData)
        {
            NodeType nodeType = (NodeType)userData;
            NodeCfg nodeCfg = m_graph.AddNode(null, nodeType.cfgType);
            if (nodeCfg == null)
                return;
            BehaviorTreeEditorGraphNode.SetPos(nodeCfg, MouseToGraphPos(m_mousePosition), null);
            //选中逻辑
            m_graph.SetSelect(nodeCfg);
            m_graph.FindNode(nodeCfg).ResetRect(true);
            m_graph.FindNode(nodeCfg).ResetLink();
        }
        void OnAddChild(object userData)
        {
            NodeType nodeType = (NodeType)userData;
            if (m_graph.selNodes.Count == 0)
                return;
            
            if (!m_graph.selNodes[0].IsParentNode)
            {
                Debuger.LogError("当前节点不是父节点类型，不能添加子节点");
                return;
            }
                
            NodeCfg nodeCfg = m_graph.AddNode(m_graph.selNodes[0], nodeType.cfgType);
            m_graph.SetSelect(nodeCfg);
        }
        void OnReplace(object userData)
        {
            NodeType nodeType = (NodeType)userData;
            if (m_graph.selNodes.Count == 0)
                return;

            NodeCfg nodeCfg = m_graph.ReplaceNode(m_graph.selNodes[0], nodeType.cfgType);
            m_graph.SetSelect(nodeCfg);
        }
        void AddAllNodeToMenu(GenericMenu menu, string root, GenericMenu.MenuFunction2 cb,NodeType t)
        {
            foreach (var n in BehaviorTreeFactory.s_types)
                menu.AddItem(new GUIContent(root + "/" + n.menuPath), t != null ? t == n : false, cb, n);
        }

        
        

        void OnCopyNode()
        {
            if (m_graph.selNodes.Count == 0)
                return;
            m_graph.CopyNode(m_graph.selNodes,ref s_copyIdx,ref s_copyFile);
        }

        void OnPasteNode()
        {
            if (s_copyIdx.Count ==0)
                return;
         
            Vector2 graphPos = MouseToGraphPos(m_mousePosition);
            List<NodeCfg> l =m_graph.PasteNode(s_copyIdx, s_copyFile, graphPos);
            m_graph.SetSelect(l);
        }

        
        void OnAlignNode()
        {
            if (m_graph.selNodes.Count == 0)
                return;
            var graphNode =m_graph.selNodes[0];
            graphNode.AlignNode();
        }
        #endregion
        #region 图坐标的相关计算
        Vector2 MouseToGraphPos(Vector2 pos)
        {
            pos -= new Vector2(m_rGraph.xMin, m_rGraph.yMin);
            pos += this.m_rightScroll;
            return pos;
        }

        Vector2 GraphToMousePos(Vector2 pos)
        {
            pos -= m_rightScroll;
            pos += m_rGraph.min;
            return pos;
        }

        void GraphScrollToPos(Vector2 pos)
        {
            m_rightScroll = pos;
            m_rightScroll -= m_rGraph.size / 2;
            m_rightScroll.y += m_rScrollView.size.y / 2-100;
        }
        #endregion

        
    }
}