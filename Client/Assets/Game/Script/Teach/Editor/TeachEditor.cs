using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LitJson;

public class TeachEditor : EditorWindow
{
    private const string Drag_Data_Name = "TeachEditorDrag";
    private static Color Warning_Color = new Color(255, 255, 0);

    private TeachMgr m_teachMgr;
    private List<TeachConfig> m_configs = null;
    private string m_nameInEdit = "";
    private int m_curTeachIndex = -1;
    private int m_curSelStep = -1;
    private Vector2 m_nameScrollPos = Vector2.zero;
    private Vector2 m_stepScrollPos = Vector2.zero;    
    private float m_lastRightWidth = 1000.0f;
    private float m_lastHeaderWidth = 1000.0f;
    private bool m_cancelActionArea = false;
    private bool m_triggerCondsArea = false;
    private bool m_checkCondsArea = false;
    private bool m_checkBackArea = false;
    private string m_copyStepJsonStr = "";

    [MenuItem("Tool/引导编辑器", false, 108)]
    public static void OpenWindow()
    {
        var win = EditorWindow.GetWindow<TeachEditor>(false, "引导编辑器", true);
        win.minSize = new Vector2(200, 200);
        win.titleContent.image = EditorGUIUtility.IconContent("tree_icon_leaf").image;
    }

    public class AutoWarningColor : IDisposable
    {
        private bool showWarning;
        private Color backClr;
        public AutoWarningColor(bool warning)
        {
            showWarning = warning;
            if (showWarning)
            {
                backClr = GUI.backgroundColor;
                GUI.backgroundColor = Warning_Color;
            }
        }

        public void Dispose()
        {
            if (showWarning)
            {
                showWarning = false;
                GUI.backgroundColor = backClr;
            }
        }
    }

    public class AutoTextColor : IDisposable
    {
        private Color backClr;
        public AutoTextColor(Color clr)
        {
            backClr = GUI.contentColor;
            GUI.contentColor = clr;
        }

        public void Dispose()
        {
            GUI.contentColor = backClr;
        }
    }

    public TeachEditor()
    {
    }

    void Update()
    {
        Repaint();
    }

    private void CallbackForGameUI(int type, object cxt)
    {
        switch (type)
        {
            case 0:
                ShowNotice((string)cxt);
                break;
            case 1:
                SetCurConfig((int)cxt, false);
                break;
        }
    }

    private void ShowNotice(string msg)
    {
        ShowNotification(new GUIContent(msg));
        Repaint();
    }

    private TeachConfig GetCurConfig()
    {
        if (m_curTeachIndex >= 0 && m_curTeachIndex < m_configs.Count)
            return m_configs[m_curTeachIndex];
        else
            return null;
    }

    private void SetCurConfig(int index, bool notifyGameUI = true)
    {
        m_curTeachIndex = index;
        var config = m_curTeachIndex >= 0 && m_curTeachIndex < m_configs.Count ? m_configs[m_curTeachIndex] : null;
        m_nameInEdit = config == null ? "" : config.teachName;
        m_curSelStep = -1;
        RemoveTextFocus(); //为了让有焦点的文本框能刷新
        //这里不用调用Repaint了
        if (m_teachMgr != null && notifyGameUI)
            m_teachMgr.SetCurTeachConfig(config);
        //收起无Item的FadeArea
        if (config != null)
        {
            m_cancelActionArea = config.cancelActions != null && config.cancelActions.Count > 0;
            m_triggerCondsArea = config.triggerConds != null && config.triggerConds.Count > 0;
            m_checkCondsArea = config.checkConds != null && config.checkConds.Count > 0;
            m_checkBackArea = config.backChecks != null && config.backChecks.Count > 0;
        }
    }

    private void SelectCurrentConfig()
    {
        if (m_teachMgr != null && m_teachMgr.CurTeachConfig != null)
        {
            var curCfg = m_teachMgr.CurTeachConfig;
            var index = m_configs.IndexOf(curCfg);
            SetCurConfig(index);
        }
        else
        {
            SetCurConfig(-1);
        }
    }

    private void RemoveTextFocus()
    {
        EditorGUIUtility.editingTextField = false;
        //EditorGUI.FocusTextInControl(""); //其实，这个也可以的
    }

    private void DrawAddButton()
    {
        if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), EditorStyles.toolbarButton, GUILayout.Width(36)))
        {
            var errMsg = TeachConfig.NewConfig(m_nameInEdit);
            if (!string.IsNullOrEmpty(errMsg))
                ShowNotice(errMsg);
            else
                SetCurConfig(m_configs.Count - 1);
        }
    }

    private void DrawRenameButton()
    {
        if (GUILayout.Button(EditorGUIUtility.IconContent("editicon.sml"), EditorStyles.toolbarButton, GUILayout.Width(36)))
        {
            do
            {
                var cfg = GetCurConfig();
                if (cfg == null)
                {
                    ShowNotice("请选中某一项");
                    break;
                }
                var oldName = cfg.teachName;
                var errMsg = cfg.Rename(m_nameInEdit);
                if (!string.IsNullOrEmpty(errMsg))
                    ShowNotice(errMsg);
                else
                {
                    TeachMgr.instance.RemoveTriggerRegister(oldName);
                    TeachMgr.instance.AddTriggerRegister(cfg);
                    TeachMgr.instance.OnRenameTeach(oldName, cfg.teachName);
                }
            } while (false);
        }
    }

    private void DrawTeachNames()
    {
        var curEvent = Event.current;
        var mousePos = curEvent.mousePosition;
        var isCxtMenu = curEvent.type == EventType.MouseUp && curEvent.button == 1;

        for (var i = 0; i < m_configs.Count; ++i)
        {
            var cfg = m_configs[i];
            var name = cfg.IsDirty ? "*" + cfg.teachName : cfg.teachName;
            var labelStyle = cfg.IsDirty ? EditorStyleEx.LabelExLeftItalic : EditorStyleEx.LabelExLeft;
            var labelStyle2 = EditorStyleEx.LabelExLeft;
            var hasDesc = !string.IsNullOrEmpty(cfg.teachDesc);
            Rect rect = GUILayoutUtility.GetRect(new GUIContent(name), labelStyle, GUILayout.ExpandWidth(true), GUILayout.Height(25));
            Rect rect2 = hasDesc ? GUILayoutUtility.GetRect(new GUIContent(cfg.teachDesc), labelStyle2, GUILayout.MaxWidth(179), GUILayout.Height(20)) : new Rect(0, 0, 0, 0);
            TryDragData(rect, cfg.teachName);
            if (hasDesc)
                TryDragData(rect2, cfg.teachName);
            bool isCur = i == m_curTeachIndex;
            if (isCur)
            {
                GUI.Box(rect, "", "ServerUpdateChangesetOn");
                if (hasDesc)
                    GUI.Box(rect2, "", "ServerUpdateChangesetOn");
            }
                

            if ((rect.Contains(mousePos) || (hasDesc && rect2.Contains(mousePos))) && isCxtMenu)
            {
                var cxtMenu = new GenericMenu();
                cxtMenu.AddItem(new GUIContent("复制名字"), false, (str) =>
                {
                    Util.PutTextToClipboard((string)str);
                }, cfg.teachName);
                cxtMenu.ShowAsContext();
            }
            else
            {
                var newVal1 = GUI.Toggle(rect, isCur, name, labelStyle);
                var newVal2 = false;
                using (new AutoTextColor(new Color(1,1,0)))
                {
                    newVal2 = hasDesc ? GUI.Toggle(rect2, isCur, cfg.teachDesc, labelStyle2) : false;
                }                    
                if ((newVal1 || newVal2) && !isCur)
                    SetCurConfig(i);
            }
        }
    }

    private void DrawRightToolBar()
    {
        using (new AutoEditorDisabledGroup(m_teachMgr.PlayNow))
        {
            var recordNow = GUILayout.Toggle(m_teachMgr.RecordNow, EditorGUIUtility.IconContent("Animation.Record", "录制"), EditorStyles.toolbarButton, GUILayout.Width(36));
            if (recordNow != m_teachMgr.RecordNow)
            {
                m_curSelStep = -1;
                m_teachMgr.StartRecord(recordNow);
                RemoveTextFocus();
            }
        }

        using (new AutoEditorDisabledGroup(m_teachMgr.RecordNow))
        {
            var playNow = GUILayout.Toggle(m_teachMgr.PlayNow, EditorGUIUtility.IconContent("Animation.Play", "播放"), EditorStyles.toolbarButton, GUILayout.Width(36));
            if (playNow != m_teachMgr.PlayNow)
            {
                //不用重置原来的选中项
                //m_curSelStep = -1;
                m_teachMgr.ClearPlayQueue();                //先清队列
                m_teachMgr.ClearCoolDownData();             //跨场景要清引导队列
                m_teachMgr.StartPlay(playNow, 0, false);    //直接播放
                RemoveTextFocus();
            }
        }

        using (new AutoEditorDisabledGroup(m_teachMgr.PlayNow || m_teachMgr.RecordNow))
        {
            var saveNow = GUILayout.Button(EditorGUIUtility.IconContent("TestPassed", "保存"), EditorStyles.toolbarButton, GUILayout.Width(36));
            if (saveNow)
            {
                var cfg = GetCurConfig();
                if (cfg != null)
                {
                    cfg.Save();
                    ShowNotice("保存完毕");
                }
            }
            var delNow = GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash", "删除"), EditorStyles.toolbarButton, GUILayout.Width(36));
            if (delNow)
            {
                var cfg = GetCurConfig();
                if (cfg != null)
                {
                    MessageBoxWindow.ShowAsMsgBox("是否真的要删除本引导？注意，删除后不可撤销。", "提示", cfg, (obj, cxt) => {
                        var cfgTemp = (TeachConfig)cxt;
                        TeachMgr.instance.RemoveTriggerRegister(cfgTemp.teachName);
                        TeachMgr.instance.RemoveFromPlayQueue(cfgTemp.teachName);
                        cfgTemp.Delete();
                        SetCurConfig(m_curTeachIndex >= m_configs.Count ? m_configs.Count - 1 : m_curTeachIndex);
                        ShowNotice("成功删除");
                        //ShowNotice有Repaint，这里不用调用了
                    }, (obj, cxt) => { });
                }
            }
            var revertChange = GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Refresh", "撤销修改"), EditorStyles.toolbarButton, GUILayout.Width(36));
            if (revertChange)
            {
                var cfg = GetCurConfig();
                if (cfg != null)
                {
                    if (cfg.IsDirty)
                    {
                        MessageBoxWindow.ShowAsMsgBox("是否真的要还原到初始状态？", "提示", cfg, (obj, cxt) =>
                        {
                            var cfgTemp = (TeachConfig)cxt;
                            cfgTemp.Revert();
                            m_curSelStep = -1;
                            RemoveTextFocus(); //让有焦点的文本框也能刷新内容
                            ShowNotice("成功撤销修改");
                            //ShowNotice有Repaint，这里不用调用了
                        }, (obj, cxt) => { });
                    }
                    else
                    {
                        m_curSelStep = -1;
                        RemoveTextFocus(); //让有焦点的文本框也能刷新内容
                        ShowNotice("内容没有修改");
                    }
                }
            }
        }
    }

    private void DrawLeftPanel()
    {
        using (new AutoEditorDisabledGroup(m_teachMgr.RecordNow || m_teachMgr.PlayNow))
        {
            using (new AutoBeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true)))
            {
                m_nameInEdit = EditorGUILayout.TextField(m_nameInEdit, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
                DrawAddButton();
                DrawRenameButton();
            }
        }
        
        using (AutoBeginScrollView scrollView = new AutoBeginScrollView(m_nameScrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
        {
            m_nameScrollPos = scrollView.Scroll;

            using (new AutoEditorDisabledGroup(m_teachMgr.RecordNow || m_teachMgr.PlayNow))
            {
                DrawTeachNames();
            }
        }        
    }

    private void DrawTeachDesc()
    {
        var cfg = GetCurConfig();
        EditorGUILayout.LabelField("引导描述：", GUILayout.Width(60));
        var newDesc = EditorGUILayout.TextField(cfg.teachDesc, GUILayout.ExpandWidth(true));
        if (newDesc != cfg.teachDesc)
        {
            cfg.teachDesc = newDesc;
            cfg.IsDirty = true;
        }
    }

    private void DrawTriggerCond(TeachConfig curConfig, TeachTriggerCondData condData)
    {
        var dirty = false;
        try
        {
            EditorGUILayout.LabelField("类型：", GUILayout.Width(30));
            var enumDescs = Util.GetEnumAllDesc<TeachTriggerCond>();
            var triggerType = (TeachTriggerCond)EditorGUILayout.Popup((int)condData.triggerType, enumDescs, GUILayout.Width(120));
            if (triggerType != condData.triggerType)
            {
                condData.triggerType = triggerType;
                dirty = true;
            }

            EditorGUILayout.LabelField("参数：", GUILayout.Width(30));
            switch (condData.triggerType)
            {
                case TeachTriggerCond.none:
                case TeachTriggerCond.mainCityFromAny:
                case TeachTriggerCond.mainCityUITop:
                    break;
                case TeachTriggerCond.mainCity:
                    {
                        using (new AutoWarningColor(!string.IsNullOrEmpty(condData.triggerParam) && RoomCfg.GetRoomCfgByID(condData.triggerParam) == null))
                        {
                            var preLevelName = EditorGUILayout.TextField(condData.triggerParam, GUILayout.Width(124));
                            if (preLevelName != condData.triggerParam)
                            {
                                condData.triggerParam = preLevelName;
                                dirty = true;
                            }
                        }
                    }
                    break;
                case TeachTriggerCond.heroLevel:
                    {
                        var num1 = StringUtil.ToInt(condData.triggerParam);
                        var newNum1 = EditorGUILayout.IntField(num1, GUILayout.Width(60));
                        if (newNum1 != num1)
                        {
                            condData.triggerParam = newNum1.ToString();
                            dirty = true;
                        }

                        EditorGUILayout.LabelField("-", GUILayout.Width(4));

                        var num2 = StringUtil.ToInt(condData.triggerParam2);
                        using (new AutoWarningColor(newNum1 > num2))
                        {
                            var newNum2 = EditorGUILayout.IntField(num2, GUILayout.Width(60));
                            if (newNum2 != num2)
                            {
                                condData.triggerParam2 = newNum2.ToString();
                                dirty = true;
                            }
                        }
                    }
                    break;
                case TeachTriggerCond.directEvent:
                    {
                        var newEventType = EditorGUILayout.TextField(condData.triggerParam, GUILayout.Width(60));
                        if (newEventType != condData.triggerParam)
                        {
                            condData.triggerParam = newEventType;
                            dirty = true;
                        }

                        EditorGUILayout.LabelField(" ", GUILayout.Width(4));

                        var newEventParam = EditorGUILayout.TextField(condData.triggerParam2, GUILayout.Width(60));
                        if (newEventParam != condData.triggerParam2)
                        {
                            condData.triggerParam2 = newEventParam;
                            dirty = true;
                        }
                    }
                    break;
                case TeachTriggerCond.openPanel:
                    {
                        ////////////////////////////
                        var oldObj = UITeach.FindUIPanel(condData.triggerParam);
                        using (new AutoWarningColor(oldObj == null))
                        {
                            var newObj = (UIPanel)EditorGUILayout.ObjectField(oldObj, typeof(UIPanel), true, GUILayout.Width(100));
                            if (newObj != oldObj)
                            {
                                condData.triggerParam = newObj == null ? "" : Util.GetGameObjectPath(newObj.gameObject);
                                dirty = true;
                            }
                        }
                        ////////////////////////////

                        ////////////////////////////
                        if (GUILayout.Button(EditorGUIUtility.IconContent(string.IsNullOrEmpty(condData.triggerParam) ? "GUISystem/align_vertically_center" : "GUISystem/align_vertically_center_active", "查看路径"), EditorStyleEx.MiniButtonEx, GUILayout.Width(24)))
                        {
                            MessageBoxWindow.ShowAsInputBox(condData.triggerParam, "修改UI路径", (str, cxt) =>
                            {
                                if (str != condData.triggerParam)
                                {
                                    condData.triggerParam = str;
                                    dirty = true;
                                    Repaint();
                                }
                            }, (str, cxt) => { });
                        }
                        ////////////////////////////
                    }
                    break;
                case TeachTriggerCond.enterScene:
                    {
                        using (new AutoWarningColor(!string.IsNullOrEmpty(condData.triggerParam) && RoomCfg.GetRoomCfgByID(condData.triggerParam) == null))
                        {
                            var levelName = EditorGUILayout.TextField(condData.triggerParam, GUILayout.Width(124));
                            if (levelName != condData.triggerParam)
                            {
                                condData.triggerParam = levelName;
                                dirty = true;
                            }
                        }
                    }
                    break;
                case TeachTriggerCond.postTeach:
                    {
                        using (new AutoWarningColor(!string.IsNullOrEmpty(condData.triggerParam) && TeachConfig.FindConfigByName(condData.triggerParam) == null))
                        {
                            Rect rectParam = EditorGUILayout.GetControlRect(GUILayout.Width(124));
                            var triggerParam = EditorGUI.TextField(rectParam, condData.triggerParam);
                            TryDropData(rectParam, ref triggerParam);
                            if (triggerParam != condData.triggerParam)
                            {
                                condData.triggerParam = triggerParam;
                                dirty = true;
                            }
                        }
                    }
                    break;
                case TeachTriggerCond.normalEvent:
                    {
                        var triggerParam = EditorGUILayout.TextField(condData.triggerParam, GUILayout.Width(60));
                        if (triggerParam != condData.triggerParam)
                        {
                            condData.triggerParam = triggerParam;
                            dirty = true;
                        }

                        EditorGUILayout.LabelField(" ", GUILayout.Width(4));

                        var triggerParam2 = EditorGUILayout.TextField(condData.triggerParam2, GUILayout.Width(60));
                        if (triggerParam2 != condData.triggerParam2)
                        {
                            condData.triggerParam2 = triggerParam2;
                            dirty = true;
                        }
                    }
                    break;
                case TeachTriggerCond.teachAgent:
                    {
                        var enumAgentTypes = Util.GetEnumAllDesc<TeachAgentType>();
                        var oldAgentType = StringUtil.ToInt(condData.triggerParam);
                        var newAgentType = EditorGUILayout.Popup(oldAgentType, enumAgentTypes, GUILayout.Width(60));
                        if (newAgentType != oldAgentType)
                        {
                            condData.triggerParam = newAgentType.ToString();
                            dirty = true;
                        }

                        EditorGUILayout.LabelField(" ", GUILayout.Width(4));

                        var triggerParam2 = EditorGUILayout.TextField(condData.triggerParam2, GUILayout.Width(120));
                        if (triggerParam2 != condData.triggerParam2)
                        {
                            condData.triggerParam2 = triggerParam2;
                            dirty = true;
                        }
                    }
                    break;
                default:
                    {
                        Rect rectParam = EditorGUILayout.GetControlRect(GUILayout.Width(124));
                        var triggerParam = EditorGUI.TextField(rectParam, condData.triggerParam);
                        TryDropData(rectParam, ref triggerParam);
                        if (triggerParam != condData.triggerParam)
                        {
                            condData.triggerParam = triggerParam;
                            dirty = true;
                        }
                    }
                    break;
            }
        }
        finally
        {
            if (dirty)
            {
                curConfig.IsDirty = true;
                TeachMgr.instance.RefreshTriggerRegister(curConfig);
            }
        }
    }

    private void DrawTriggerConds()
    {
        var curConfig = GetCurConfig();
        var triggerConds = curConfig.triggerConds;
        var deleteIndex = -1;

        for (var i = 0; i < triggerConds.Count; ++i)
        {
            var condData = triggerConds[i];

            using (new AutoBeginHorizontal())
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyleEx.MiniButtonEx, GUILayout.Width(24)))
                {
                    deleteIndex = i;
                }

                DrawTriggerCond(curConfig, condData);
            }
        }

        using (new AutoBeginHorizontal())
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), EditorStyleEx.MiniButtonEx, GUILayout.Width(24)))
            {
                triggerConds.Add(new TeachTriggerCondData());
                curConfig.IsDirty = true;
                TeachMgr.instance.RefreshTriggerRegister(curConfig);
            }
        }

        if (deleteIndex >= 0 && deleteIndex < triggerConds.Count)
        {
            triggerConds.RemoveAt(deleteIndex);
            curConfig.IsDirty = true;
            TeachMgr.instance.RefreshTriggerRegister(curConfig);
            deleteIndex = -1;
            Repaint();
        }
    }

    private void DrawCheckCond(TeachConfig curConfig, TeachCheckCondData condData)
    {
        EditorGUILayout.LabelField("类型：", GUILayout.Width(30));
        var enumDescs = Util.GetEnumAllDesc<TeachCheckCond>();
        var checkType = (TeachCheckCond)EditorGUILayout.Popup((int)condData.checkType, enumDescs, GUILayout.Width(120));
        if (checkType != condData.checkType)
        {
            condData.checkType = checkType;
            curConfig.IsDirty = true;
        }

        EditorGUILayout.LabelField("参数：", GUILayout.Width(30));
        switch (condData.checkType)
        {
            case TeachCheckCond.none:
            case TeachCheckCond.inMainCity:
            case TeachCheckCond.inLevelScene:
                break;
            case TeachCheckCond.inScene:
                {
                    using (new AutoWarningColor(!string.IsNullOrEmpty(condData.checkParam) && RoomCfg.GetRoomCfgByID(condData.checkParam) == null))
                    {
                        var levelName = EditorGUILayout.TextField(condData.checkParam, GUILayout.Width(124));
                        if (levelName != condData.checkParam)
                        {
                            condData.checkParam = levelName;
                            curConfig.IsDirty = true;
                        }
                    }
                }
                break;
            case TeachCheckCond.heroLevel:
                {
                    var num1 = StringUtil.ToInt(condData.checkParam);
                    var newNum1 = EditorGUILayout.IntField(num1, GUILayout.Width(60));
                    if (newNum1 != num1)
                    {
                        condData.checkParam = newNum1.ToString();
                        curConfig.IsDirty = true;
                    }

                    EditorGUILayout.LabelField("-", GUILayout.Width(4));

                    var num2 = StringUtil.ToInt(condData.checkParam2);
                    using (new AutoWarningColor(newNum1 > num2))
                    {
                        var newNum2 = EditorGUILayout.IntField(num2, GUILayout.Width(60));
                        if (newNum2 != num2)
                        {
                            condData.checkParam2 = newNum2.ToString();
                            curConfig.IsDirty = true;
                        }
                    }
                }
                break;
            case TeachCheckCond.panelShow:
                {
                    ////////////////////////////
                    var oldObj = UITeach.FindUIPanel(condData.checkParam);
                    using (new AutoWarningColor(oldObj == null))
                    {
                        var newObj = (UIPanel)EditorGUILayout.ObjectField(oldObj, typeof(UIPanel), true, GUILayout.Width(100));
                        if (newObj != oldObj)
                        {
                            condData.checkParam = newObj == null ? "" : Util.GetGameObjectPath(newObj.gameObject);
                            curConfig.IsDirty = true;
                        }
                    }
                    ////////////////////////////

                    ////////////////////////////
                    if (GUILayout.Button(EditorGUIUtility.IconContent(string.IsNullOrEmpty(condData.checkParam) ? "GUISystem/align_vertically_center" : "GUISystem/align_vertically_center_active", "查看路径"), EditorStyleEx.MiniButtonEx, GUILayout.Width(24)))
                    {
                        MessageBoxWindow.ShowAsInputBox(condData.checkParam, "修改UI路径", (str, cxt) =>
                        {
                            if (str != condData.checkParam)
                            {
                                condData.checkParam = str;
                                curConfig.IsDirty = true;
                                Repaint();
                            }
                        }, (str, cxt) => { });
                    }
                    ////////////////////////////
                }
                break;
            case TeachCheckCond.levelId:
                {
                    var num1 = StringUtil.ToInt(condData.checkParam);
                    var newNum1 = EditorGUILayout.IntField(num1, GUILayout.Width(60));
                    if (newNum1 != num1)
                    {
                        condData.checkParam = newNum1.ToString();
                        curConfig.IsDirty = true;
                    }

                    EditorGUILayout.LabelField("-", GUILayout.Width(4));

                    var num2 = StringUtil.ToInt(condData.checkParam2);
                    using (new AutoWarningColor(newNum1 > num2))
                    {
                        var newNum2 = EditorGUILayout.IntField(num2, GUILayout.Width(60));
                        if (newNum2 != num2)
                        {
                            condData.checkParam2 = newNum2.ToString();
                            curConfig.IsDirty = true;
                        }
                    }
                }
                break;
            case TeachCheckCond.teachData:
                {
                    var newKey = EditorGUILayout.TextField(condData.checkParam, GUILayout.Width(60));
                    if (newKey != condData.checkParam)
                    {
                        condData.checkParam = newKey;
                        curConfig.IsDirty = true;
                    }

                    EditorGUILayout.LabelField(" ", GUILayout.Width(4));

                    var arr = string.IsNullOrEmpty(condData.checkParam2) ? new string[] { "", "" } : condData.checkParam2.Split(new char[] { ',' });
                    var opType = arr.Length >= 1 ? StringUtil.ToInt(arr[0]) : 0;
                    var value = arr.Length >= 2 ? StringUtil.ToInt(arr[1]) : 0;

                    var newOpType = EditorGUILayout.Popup(opType, new string[] { "等于", "不等于" }, GUILayout.Width(50));
                    if (newOpType != opType)
                    {
                        opType = newOpType;
                        condData.checkParam2 = opType + "," + value;
                        curConfig.IsDirty = true;
                    }

                    var newVal = EditorGUILayout.IntField(value, GUILayout.Width(50));
                    if (newVal != value)
                    {
                        value = newVal;
                        condData.checkParam2 = opType + "," + value;
                        curConfig.IsDirty = true;
                    }
                }
                break;
            case TeachCheckCond.taskState:
                {
                    var newTaskId = EditorGUILayout.TextField(condData.checkParam, GUILayout.Width(60));
                    if (newTaskId != condData.checkParam)
                    {
                        condData.checkParam = newTaskId;
                        curConfig.IsDirty = true;
                    }

                    EditorGUILayout.LabelField(" ", GUILayout.Width(4));

                    var newTaskState = EditorGUILayout.TextField(condData.checkParam2, GUILayout.Width(60));
                    if (newTaskState != condData.checkParam2)
                    {
                        condData.checkParam2 = newTaskState;
                        curConfig.IsDirty = true;
                    }
                }
                break;
            case TeachCheckCond.uiPanelFunc:
                {
                    ////////////////////////////
                    var oldObj = UITeach.FindUIPanel(condData.checkParam);
                    using (new AutoWarningColor(oldObj == null || !TeachMgr.HasTeachCheckFunc(oldObj.GetComponent<UIPanel>())))
                    {
                        var newObj = (UIPanel)EditorGUILayout.ObjectField(oldObj, typeof(UIPanel), true, GUILayout.Width(100));
                        if (newObj != oldObj)
                        {
                            condData.checkParam = newObj == null ? "" : Util.GetGameObjectPath(newObj.gameObject);
                            curConfig.IsDirty = true;

                            if (newObj != null)
                            {
                                if (!TeachMgr.HasTeachCheckFunc(newObj.GetComponent<UIPanel>()))
                                {
                                    ShowNotice("该UIPanel没有带有一个string参数并返回bool的实例方法" + TeachMgr.TEACH_CHECK_FUNC);
                                }
                            }
                        }
                    }
                    ////////////////////////////

                    ////////////////////////////
                    if (GUILayout.Button(EditorGUIUtility.IconContent(string.IsNullOrEmpty(condData.checkParam) ? "GUISystem/align_vertically_center" : "GUISystem/align_vertically_center_active", "查看路径"), EditorStyleEx.MiniButtonEx, GUILayout.Width(24)))
                    {
                        MessageBoxWindow.ShowAsInputBox(condData.checkParam, "修改UI路径", (str, cxt) =>
                        {
                            if (str != condData.checkParam)
                            {
                                condData.checkParam = str;
                                curConfig.IsDirty = true;
                                Repaint();
                            }
                        }, (str, cxt) => { });
                    }
                    ////////////////////////////

                    ////////////////////////////
                    var checkParam2 = EditorGUILayout.TextField(condData.checkParam2, GUILayout.Width(100));
                    if (checkParam2 != condData.checkParam2)
                    {
                        condData.checkParam2 = checkParam2;
                        curConfig.IsDirty = true;
                    }
                    ////////////////////////////
                }
                break;
            case TeachCheckCond.systemIcon:
                {
                    var names = Enum.GetNames(typeof(enSystem));
                    var oldEnum = (enSystem)StringUtil.ToInt(condData.checkParam);
                    var oldName = Enum.GetName(typeof(enSystem), oldEnum);
                    var oldIdx = Array.IndexOf(names, oldName); 
                    var newIdx = EditorGUILayout.Popup(oldIdx, names, GUILayout.Width(62));
                    if (newIdx != oldIdx)
                    {
                        var newName = names[newIdx];
                        var newEnum = ((enSystem)Enum.Parse(typeof(enSystem), newName));
                        condData.checkParam = ((int)newEnum).ToString();
                        curConfig.IsDirty = true;
                    }

                    var oldEnable = Mathf.Clamp(StringUtil.ToInt(condData.checkParam2), 0, 1);
                    var newEnable = EditorGUILayout.Popup(oldEnable, new string[] { "不可用", "可用" }, GUILayout.Width(62));
                    if (oldEnable != newEnable)
                    {
                        condData.checkParam2 = newEnable.ToString();
                        curConfig.IsDirty = true;
                    }
                }
                break;
            default:
                {
                    Rect rectParam = EditorGUILayout.GetControlRect(GUILayout.Width(124));
                    var checkParam = EditorGUI.TextField(rectParam, condData.checkParam);
                    TryDropData(rectParam, ref checkParam);
                    if (checkParam != condData.checkParam)
                    {
                        condData.checkParam = checkParam;
                        curConfig.IsDirty = true;
                    }
                }
                break;
        }
    }

    private void DrawCheckConds()
    {
        var curConfig = GetCurConfig();
        var checkConds = curConfig.checkConds;
        var deleteIndex = -1;

        for (var i = 0; i < checkConds.Count; ++i)
        {
            var condData = checkConds[i];

            using (new AutoBeginHorizontal())
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyleEx.MiniButtonEx, GUILayout.Width(24)))
                {
                    deleteIndex = i;
                }

                DrawCheckCond(curConfig, condData);
            }
        }

        using (new AutoBeginHorizontal())
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), EditorStyleEx.MiniButtonEx, GUILayout.Width(24)))
            {
                checkConds.Add(new TeachCheckCondData());
                curConfig.IsDirty = true;
            }
        }

        if (deleteIndex >= 0 && deleteIndex < checkConds.Count)
        {
            checkConds.RemoveAt(deleteIndex);
            curConfig.IsDirty = true;
            deleteIndex = -1;
            Repaint();
        }
    }

    private void DrawCancelAction(TeachConfig curConfig, TeachCancelActionData actionData)
    {
        EditorGUILayout.LabelField("类型：", GUILayout.Width(30));
        var enumDescs = Util.GetEnumAllDesc<TeachCancelActionType>();
        var actionType = (TeachCancelActionType)EditorGUILayout.Popup((int)actionData.actionType, enumDescs, GUILayout.Width(120));
        if (actionType != actionData.actionType)
        {
            actionData.actionType = actionType;
            curConfig.IsDirty = true;
        }

        EditorGUILayout.LabelField("参数：", GUILayout.Width(30));
        switch (actionData.actionType)
        {
            case TeachCancelActionType.openPanel:
            case TeachCancelActionType.closePanel:
                {
                    ////////////////////////////
                    var oldObj = UITeach.FindUIPanel(actionData.actionParam);
                    using (new AutoWarningColor(oldObj == null))
                    {
                        var newObj = (UIPanel)EditorGUILayout.ObjectField(oldObj, typeof(UIPanel), true, GUILayout.Width(100));
                        if (newObj != oldObj)
                        {
                            actionData.actionParam = newObj == null ? "" : Util.GetGameObjectPath(newObj.gameObject);
                            curConfig.IsDirty = true;
                        }
                    }
                    ////////////////////////////

                    ////////////////////////////
                    if (GUILayout.Button(EditorGUIUtility.IconContent(string.IsNullOrEmpty(actionData.actionParam) ? "GUISystem/align_vertically_center" : "GUISystem/align_vertically_center_active", "查看路径"), EditorStyleEx.MiniButtonEx, GUILayout.Width(24)))
                    {
                        MessageBoxWindow.ShowAsInputBox(actionData.actionParam, "修改UI路径", (str, cxt) =>
                        {
                            if (str != actionData.actionParam)
                            {
                                actionData.actionParam = str;
                                curConfig.IsDirty = true;
                                Repaint();
                            }
                        }, (str, cxt) => { });
                    }
                    ////////////////////////////
                }
                break;
            case TeachCancelActionType.showUINode:
            case TeachCancelActionType.hideUINode:
                {
                    ////////////////////////////
                    var oldObj = UITeach.FindRectTransform(actionData.actionParam);
                    using (new AutoWarningColor(oldObj == null))
                    {
                        var newObj = (RectTransform)EditorGUILayout.ObjectField(oldObj, typeof(RectTransform), true, GUILayout.Width(100));
                        if (newObj != oldObj)
                        {
                            actionData.actionParam = newObj == null ? "" : Util.GetGameObjectPath(newObj.gameObject);
                            curConfig.IsDirty = true;
                        }
                    }
                    ////////////////////////////

                    ////////////////////////////
                    if (GUILayout.Button(EditorGUIUtility.IconContent(string.IsNullOrEmpty(actionData.actionParam) ? "GUISystem/align_vertically_center" : "GUISystem/align_vertically_center_active", "查看路径"), EditorStyleEx.MiniButtonEx, GUILayout.Width(24)))
                    {
                        MessageBoxWindow.ShowAsInputBox(actionData.actionParam, "修改UI路径", (str, cxt) =>
                        {
                            if (str != actionData.actionParam)
                            {
                                actionData.actionParam = str;
                                curConfig.IsDirty = true;
                                Repaint();
                            }
                        }, (str, cxt) => { });
                    }
                    ////////////////////////////
                }
                break;
            case TeachCancelActionType.stateHandleState:
                {
                    var oldObj = UITeach.FindStateHandle(actionData.actionParam);
                    using (new AutoWarningColor(oldObj == null))
                    {
                        var newObj = (StateHandle)EditorGUILayout.ObjectField(oldObj, typeof(StateHandle), true, GUILayout.Width(100));
                        if (newObj != oldObj)
                        {
                            actionData.actionParam = newObj == null ? "" : Util.GetGameObjectPath(newObj.gameObject);
                            curConfig.IsDirty = true;
                        }
                    }

                    var oldNum = StringUtil.ToInt(actionData.actionParam2);
                    var newNum = EditorGUILayout.IntField(oldNum, GUILayout.Width(50));
                    if (newNum != oldNum)
                    {
                        actionData.actionParam2 = newNum.ToString();
                        curConfig.IsDirty = true;
                    }
                }
                break;
            case TeachCancelActionType.uiPanelFunc:
                {
                    ////////////////////////////
                    var oldObj = UITeach.FindUIPanel(actionData.actionParam);
                    using (new AutoWarningColor(oldObj == null || !TeachMgr.HasTeachActionFunc(oldObj.GetComponent<UIPanel>())))
                    {
                        var newObj = (UIPanel)EditorGUILayout.ObjectField(oldObj, typeof(UIPanel), true, GUILayout.Width(100));
                        if (newObj != oldObj)
                        {
                            actionData.actionParam = newObj == null ? "" : Util.GetGameObjectPath(newObj.gameObject);
                            curConfig.IsDirty = true;

                            if (newObj != null)
                            {
                                if (!TeachMgr.HasTeachActionFunc(newObj.GetComponent<UIPanel>()))
                                {
                                    ShowNotice("该UIPanel没有带有一个string参数的实例方法" + TeachMgr.TEACH_ACTION_FUNC);
                                }
                            }
                        }
                    }
                    ////////////////////////////

                    ////////////////////////////
                    if (GUILayout.Button(EditorGUIUtility.IconContent(string.IsNullOrEmpty(actionData.actionParam) ? "GUISystem/align_vertically_center" : "GUISystem/align_vertically_center_active", "查看路径"), EditorStyleEx.MiniButtonEx, GUILayout.Width(24)))
                    {
                        MessageBoxWindow.ShowAsInputBox(actionData.actionParam, "修改UI路径", (str, cxt) =>
                        {
                            if (str != actionData.actionParam)
                            {
                                actionData.actionParam = str;
                                curConfig.IsDirty = true;
                                Repaint();
                            }
                        }, (str, cxt) => { });
                    }
                    ////////////////////////////

                    ////////////////////////////
                    var actionParam2 = EditorGUILayout.TextField(actionData.actionParam2, GUILayout.Width(50));
                    if (actionParam2 != actionData.actionParam2)
                    {
                        actionData.actionParam2 = actionParam2;
                        curConfig.IsDirty = true;
                    }
                    ////////////////////////////
                }
                break;
        }
    }

    private void DrawCancelActions()
    {
        var curConfig = GetCurConfig();
        var cancelActions = curConfig.cancelActions;
        var deleteIndex = -1;

        for (var i = 0; i < cancelActions.Count; ++i)
        {
            var actionData = cancelActions[i];

            using (new AutoBeginHorizontal())
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyleEx.MiniButtonEx, GUILayout.Width(24)))
                {
                    deleteIndex = i;
                }

                DrawCancelAction(curConfig, actionData);
            }
        }

        using (new AutoBeginHorizontal())
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), EditorStyleEx.MiniButtonEx, GUILayout.Width(24)))
            {
                cancelActions.Add(new TeachCancelActionData());
                curConfig.IsDirty = true;
            }
        }

        if (deleteIndex >= 0 && deleteIndex < cancelActions.Count)
        {
            cancelActions.RemoveAt(deleteIndex);
            curConfig.IsDirty = true;
            deleteIndex = -1;
            Repaint();
        }
    }

    private void DrawBackCheck(TeachConfig curConfig, TeachBackCheckData checkData)
    {
        EditorGUILayout.LabelField("第", GUILayout.Width(15));
        var fromStep = EditorGUILayout.IntField(checkData.fromStep, GUILayout.Width(30));
        if (fromStep != checkData.fromStep)
        {
            checkData.fromStep = fromStep;
            curConfig.IsDirty = true;
        }
        EditorGUILayout.LabelField("步到第", GUILayout.Width(40));
        var toStep = EditorGUILayout.IntField(checkData.toStep, GUILayout.Width(30));
        if (toStep != checkData.toStep)
        {
            checkData.toStep = toStep;
            curConfig.IsDirty = true;
        }
        EditorGUILayout.LabelField("步，", GUILayout.Width(30));

        EditorGUILayout.LabelField("检测类型：", GUILayout.Width(60));
        var checkTypeStrs = Util.GetEnumAllDesc<TeachBackCheckType>();
        var checkType = (TeachBackCheckType)EditorGUILayout.Popup((int)checkData.checkType, checkTypeStrs, GUILayout.Width(60));
        if (checkType != checkData.checkType)
        {
            checkData.checkType = checkType;
            curConfig.IsDirty = true;
        }

        switch (checkData.checkType)
        {
            case TeachBackCheckType.none:
                break;
            case TeachBackCheckType.closePanel:
            case TeachBackCheckType.beCovered:
                {
                    ////////////////////////////
                    var oldObj = UITeach.FindUIPanel(checkData.checkParam);
                    using (new AutoWarningColor(oldObj == null))
                    {
                        var newObj = (UIPanel)EditorGUILayout.ObjectField(oldObj, typeof(UIPanel), true, GUILayout.Width(76));
                        if (newObj != oldObj)
                        {
                            checkData.checkParam = newObj == null ? "" : Util.GetGameObjectPath(newObj.gameObject);
                            curConfig.IsDirty = true;
                        }
                    }
                    ////////////////////////////

                    ////////////////////////////
                    if (GUILayout.Button(EditorGUIUtility.IconContent(string.IsNullOrEmpty(checkData.checkParam) ? "GUISystem/align_vertically_center" : "GUISystem/align_vertically_center_active", "查看路径"), EditorStyleEx.MiniButtonEx, GUILayout.Width(24)))
                    {
                        MessageBoxWindow.ShowAsInputBox(checkData.checkParam, "修改UI路径", (str, cxt) =>
                        {
                            if (str != checkData.checkParam)
                            {
                                checkData.checkParam = str;
                                curConfig.IsDirty = true;
                                Repaint();
                            }
                        }, (str, cxt) => { });
                    }
                    ////////////////////////////
                }
                break;
            case TeachBackCheckType.stateHandle:
                {
                    ////////////////////////////
                    var oldObj = UITeach.FindStateHandle(checkData.checkParam);
                    using (new AutoWarningColor(oldObj == null))
                    {
                        var newObj = (StateHandle)EditorGUILayout.ObjectField(oldObj, typeof(StateHandle), true, GUILayout.Width(76));
                        if (newObj != oldObj)
                        {
                            checkData.checkParam = newObj == null ? "" : Util.GetGameObjectPath(newObj.gameObject);
                            curConfig.IsDirty = true;
                        }
                    }
                    ////////////////////////////

                    ////////////////////////////
                    if (GUILayout.Button(EditorGUIUtility.IconContent(string.IsNullOrEmpty(checkData.checkParam) ? "GUISystem/align_vertically_center" : "GUISystem/align_vertically_center_active", "查看路径"), EditorStyleEx.MiniButtonEx, GUILayout.Width(24)))
                    {
                        MessageBoxWindow.ShowAsInputBox(checkData.checkParam, "修改UI路径", (str, cxt) =>
                        {
                            if (str != checkData.checkParam)
                            {
                                checkData.checkParam = str;
                                curConfig.IsDirty = true;
                                Repaint();
                            }
                        }, (str, cxt) => { });
                    }
                    ////////////////////////////

                    ////////////////////////////
                    var oldNum = StringUtil.ToInt(checkData.checkParam2);
                    var newNum = EditorGUILayout.IntField(oldNum, GUILayout.Width(40));
                    if (newNum != oldNum)
                    {
                        checkData.checkParam2 = newNum.ToString();
                        curConfig.IsDirty = true;
                    }
                    ////////////////////////////
                }
                break;
            case TeachBackCheckType.stateGroup:
                {
                    ////////////////////////////
                    var oldObj = UITeach.FindStateGroup(checkData.checkParam);
                    using (new AutoWarningColor(oldObj == null))
                    {
                        var newObj = (StateGroup)EditorGUILayout.ObjectField(oldObj, typeof(StateGroup), true, GUILayout.Width(76));
                        if (newObj != oldObj)
                        {
                            checkData.checkParam = newObj == null ? "" : Util.GetGameObjectPath(newObj.gameObject);
                            curConfig.IsDirty = true;
                        }
                    }
                    ////////////////////////////

                    ////////////////////////////
                    if (GUILayout.Button(EditorGUIUtility.IconContent(string.IsNullOrEmpty(checkData.checkParam) ? "GUISystem/align_vertically_center" : "GUISystem/align_vertically_center_active", "查看路径"), EditorStyleEx.MiniButtonEx, GUILayout.Width(24)))
                    {
                        MessageBoxWindow.ShowAsInputBox(checkData.checkParam, "修改UI路径", (str, cxt) =>
                        {
                            if (str != checkData.checkParam)
                            {
                                checkData.checkParam = str;
                                curConfig.IsDirty = true;
                                Repaint();
                            }
                        }, (str, cxt) => { });
                    }
                    ////////////////////////////

                    ////////////////////////////
                    var oldNum = StringUtil.ToInt(checkData.checkParam2);
                    var newNum = EditorGUILayout.IntField(oldNum, GUILayout.Width(40));
                    if (newNum != oldNum)
                    {
                        checkData.checkParam2 = newNum.ToString();
                        curConfig.IsDirty = true;
                    }
                    ////////////////////////////
                }
                break;
            case TeachBackCheckType.directEvent:
            case TeachBackCheckType.normalEvent:
                {
                    ////////////////////////////
                    var checkParam = EditorGUILayout.TextField(checkData.checkParam, GUILayout.Width(40));
                    if (checkParam != checkData.checkParam)
                    {
                        checkData.checkParam = checkParam.ToString();
                        curConfig.IsDirty = true;
                    }
                    ////////////////////////////

                    ////////////////////////////
                    var checkParam2 = EditorGUILayout.TextField(checkData.checkParam2, GUILayout.Width(40));
                    if (checkParam2 != checkData.checkParam2)
                    {
                        checkData.checkParam2 = checkParam2.ToString();
                        curConfig.IsDirty = true;
                    }
                    ////////////////////////////
                }
                break;
        }

        EditorGUILayout.LabelField("行为类型：", GUILayout.Width(60));
        var actionTypeStrs = Util.GetEnumAllDesc<TeachBackActionType>();
        var actionType = (TeachBackActionType)EditorGUILayout.Popup((int)checkData.actionType, actionTypeStrs, GUILayout.Width(60));
        if (actionType != checkData.actionType)
        {
            checkData.actionType = actionType;
            curConfig.IsDirty = true;
        }

        switch (checkData.actionType)
        {
            case TeachBackActionType.none:
            case TeachBackActionType.cancelPlay:
                break;            
            case TeachBackActionType.backStepTo:
                {
                    var oldNum = StringUtil.ToInt(checkData.actionParam);
                    var newNum = EditorGUILayout.IntField(oldNum, GUILayout.Width(40));
                    if (newNum != oldNum)
                    {
                        checkData.actionParam = newNum.ToString();
                        curConfig.IsDirty = true;
                    }
                }
                break;

        }
    }

    private void DrawBackChecks()
    {
        var curConfig = GetCurConfig();
        var backChecks = curConfig.backChecks;
        var deleteIndex = -1;

        for (var i = 0; i < backChecks.Count; ++i)
        {
            var checkData = backChecks[i];

            using (new AutoBeginHorizontal())
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyleEx.MiniButtonEx, GUILayout.Width(24)))
                {
                    deleteIndex = i;
                }

                DrawBackCheck(curConfig, checkData);
            }
        }

        using (new AutoBeginHorizontal())
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), EditorStyleEx.MiniButtonEx, GUILayout.Width(24)))
            {
                backChecks.Add(new TeachBackCheckData());
                curConfig.IsDirty = true;
            }
        }

        if (deleteIndex >= 0 && deleteIndex < backChecks.Count)
        {
            backChecks.RemoveAt(deleteIndex);
            curConfig.IsDirty = true;
            deleteIndex = -1;
            Repaint();
        }
    }

    private void DrawPriority()
    {
        var cfg = GetCurConfig();

        EditorGUILayout.LabelField("优先级：", GUILayout.Width(50));
        var priority = EditorGUILayout.IntField(cfg.priority, GUILayout.Width(60));
        if (priority != cfg.priority)
        {
            cfg.priority = priority;
            cfg.IsDirty = true;
        }

        EditorGUILayout.LabelField("可被打断：", GUILayout.Width(60));
        var canInterrupt = EditorGUILayout.Toggle(cfg.canInterrupt, GUILayout.Width(32));
        if (canInterrupt != cfg.canInterrupt)
        {
            cfg.canInterrupt = canInterrupt;
            cfg.IsDirty = true;
        }
    }

    private void DrawDBDataKey()
    {
        var cfg = GetCurConfig();
        EditorGUILayout.LabelField("数据库键名：", GUILayout.Width(70));
        GUI.SetNextControlName("dataKeyParam");
        Rect rectParam = EditorGUILayout.GetControlRect(GUILayout.Width(100));
        var dataKey = EditorGUI.TextField(rectParam, cfg.dataKey);
        TryDropData(rectParam, ref dataKey);
        if (dataKey != cfg.dataKey)
        {
            cfg.dataKey = dataKey;
            cfg.IsDirty = true;
        }
        TeachConfig otherCfg = null;
        if (!string.IsNullOrEmpty(dataKey) && !TeachConfig.IsTeachKeyOK(dataKey))
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent("console.erroricon"), EditorStyles.label, GUILayout.Width(22), GUILayout.Height(22)))
            {
                MessageBoxWindow.ShowAsMsgBox(TeachConfig.GetTeachKeyFormatDesc(), "警告", (obj, cxt) =>
                {
                    Focus();
                    EditorGUI.FocusTextInControl("dataKeyParam");
                });
            }
        }
        else if ((otherCfg = TeachConfig.FindConfigByDataKey(dataKey, cfg)) != null)
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent("console.warnicon"), EditorStyles.label, GUILayout.Width(22), GUILayout.Height(22)))
            {
                MessageBoxWindow.ShowAsMsgBox("有另一个引导" + otherCfg.teachName + "也是用这个键名，是否填错了？", "提示", (obj, cxt) =>
                {
                    Focus();
                    EditorGUI.FocusTextInControl("dataKeyParam");
                });
            }
        }
    }

    private void DrawReplayUnfinished()
    {
        var cfg = GetCurConfig();
        EditorGUILayout.LabelField("重播未完成引导：", GUILayout.Width(90));
        using (new AutoEditorDisabledGroup(!TeachConfig.IsTeachKeyOK(cfg.dataKey)))
        {
            var replayUnfinished = EditorGUILayout.Toggle(cfg.replayUnfinished, GUILayout.Width(32));
            if (replayUnfinished != cfg.replayUnfinished)
            {
                cfg.replayUnfinished = replayUnfinished;
                cfg.IsDirty = true;
            }
        }
    }

    private void DrawPostTeach()
    {
        var cfg = GetCurConfig();
        EditorGUILayout.LabelField("后续引导：", GUILayout.Width(60));
        GUI.SetNextControlName("postTeach");
        Rect rectPostTeach = EditorGUILayout.GetControlRect(GUILayout.Width(120));
        var postTeach = EditorGUI.TextField(rectPostTeach, cfg.postTeach).Trim();
        TryDropData(rectPostTeach, ref postTeach);
        if (postTeach != cfg.postTeach)
        {
            cfg.postTeach = postTeach;
            cfg.IsDirty = true;
        }
        bool isPostTeachNameOK = true;
        if (!string.IsNullOrEmpty(postTeach))
        {
            if (postTeach == cfg.teachName)
            {
                isPostTeachNameOK = false;
                if (GUILayout.Button(EditorGUIUtility.IconContent("console.warnicon"), EditorStyles.label, GUILayout.Width(22), GUILayout.Height(22)))
                {
                    MessageBoxWindow.ShowAsMsgBox("不能填自己！", "警告", (obj, cxt) =>
                    {
                        Focus();
                        EditorGUI.FocusTextInControl("postTeach");
                    });
                }
            }
            else if (TeachConfig.FindConfigByName(postTeach) == null)
            {
                isPostTeachNameOK = false;
                if (GUILayout.Button(EditorGUIUtility.IconContent("console.warnicon"), EditorStyles.label, GUILayout.Width(22), GUILayout.Height(22)))
                {
                    MessageBoxWindow.ShowAsMsgBox("找不到后续引导的名字！", "警告", (obj, cxt) =>
                    {
                        Focus();
                        EditorGUI.FocusTextInControl("postTeach");
                    });
                }
            }
        }
        if (isPostTeachNameOK)
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent("console.infoicon"), EditorStyles.label, GUILayout.Width(22), GUILayout.Height(22)))
            {
                MessageBoxWindow.ShowAsMsgBox("可从左边引导名列表拖动名字到这里。", "提示", (obj, cxt) =>
                {
                    Focus();
                });
            }
        }
    }

    private void DrawStopSoundOpt()
    {
        var cfg = GetCurConfig();
        EditorGUILayout.LabelField("结束时停止声音：", GUILayout.Width(90));
        var stopSoundWhenFinish = EditorGUILayout.Toggle(cfg.stopSoundWhenFinish, GUILayout.Width(32));
        if (stopSoundWhenFinish != cfg.stopSoundWhenFinish)
        {
            cfg.stopSoundWhenFinish = stopSoundWhenFinish;
            cfg.IsDirty = true;
        }
    }

    private void DrawCoolTimeCfg()
    {
        var cfg = GetCurConfig();
        EditorGUILayout.LabelField("冷却时间：", GUILayout.Width(60));
        var coolDown = EditorGUILayout.IntField(cfg.coolDown, GUILayout.Width(50));
        if (coolDown != cfg.coolDown)
        {
            cfg.coolDown = coolDown;
            cfg.IsDirty = true;
        }
        EditorGUILayout.LabelField("秒", GUILayout.Width(30));
    }

    private void DrawTeachParam()
    {
        using (new AutoBeginHorizontal())
        {
            DrawTeachDesc();
        }

        using (new AutoBeginHorizontal())
        {
            DrawPriority();

            DrawDBDataKey();

            DrawReplayUnfinished();

            DrawPostTeach();

            DrawStopSoundOpt();

            DrawCoolTimeCfg();
        }

        EditorGUILayoutEx.FadeArea triggerCondsArea = EditorGUILayoutEx.instance.BeginFadeArea(m_triggerCondsArea, "触发条件", "triggerCondsArea", EditorStyleEx.PixelBoxStyle);
        m_triggerCondsArea = triggerCondsArea.open;
        if (triggerCondsArea.Show())
        {
            using (new AutoBeginVertical())
            {
                DrawTriggerConds();
            }
        }
        EditorGUILayoutEx.instance.EndFadeArea();

        EditorGUILayoutEx.FadeArea checkCondsArea = EditorGUILayoutEx.instance.BeginFadeArea(m_checkCondsArea, "检测条件", "checkCondsArea", EditorStyleEx.PixelBoxStyle);
        m_checkCondsArea = checkCondsArea.open;
        if (checkCondsArea.Show())
        {
            using (new AutoBeginVertical())
            {
                DrawCheckConds();
            }
        }
        EditorGUILayoutEx.instance.EndFadeArea();

        EditorGUILayoutEx.FadeArea checkBackArea = EditorGUILayoutEx.instance.BeginFadeArea(m_checkBackArea, "回退检测", "checkBackArea", EditorStyleEx.PixelBoxStyle);
        m_checkBackArea = checkBackArea.open;
        if (checkBackArea.Show())
        {
            using (new AutoBeginVertical())
            {
                DrawBackChecks();
            }
        }
        EditorGUILayoutEx.instance.EndFadeArea();

        EditorGUILayoutEx.FadeArea cancelActionArea = EditorGUILayoutEx.instance.BeginFadeArea(m_cancelActionArea, "取消播放后行为", "cancelActionArea", EditorStyleEx.PixelBoxStyle);
        m_cancelActionArea = cancelActionArea.open;
        if (cancelActionArea.Show())
        {
            using (new AutoBeginVertical())
            {
                DrawCancelActions();
            }
        }
        EditorGUILayoutEx.instance.EndFadeArea();
    }

    private void TryDragData(Rect dragArea, string dragData)
    {
        Event currentEvent = Event.current;
        EventType currentEventType = currentEvent.type;

        if (currentEventType == EventType.DragExited)
            DragAndDrop.PrepareStartDrag();

        if (!dragArea.Contains(currentEvent.mousePosition))
            return;

        switch (currentEventType)
        {
            case EventType.MouseDown:
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData(Drag_Data_Name, dragData);
                DragAndDrop.objectReferences = new UnityEngine.Object[0];//注意，这个值必须赋值，不然会导致拖动时OnGUI不会调用
                break;
            case EventType.MouseDrag:
                string dragData2 = DragAndDrop.GetGenericData(Drag_Data_Name) as string;
                if (dragData2 != null)
                {
                    DragAndDrop.StartDrag(Drag_Data_Name);
                    currentEvent.Use();
                }
                break;
        }
    }

    private void TryDropData(Rect dropArea, ref string dropData)
    {
        Event currentEvent = Event.current;
        EventType currentEventType = currentEvent.type;

        if (currentEventType == EventType.DragExited)
            DragAndDrop.PrepareStartDrag();

        if (!dropArea.Contains(currentEvent.mousePosition))
            return;

        switch (currentEventType)
        {
            case EventType.DragUpdated:
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                currentEvent.Use();
                break;
            case EventType.Repaint:
                if (DragAndDrop.visualMode == DragAndDropVisualMode.None || DragAndDrop.visualMode == DragAndDropVisualMode.Rejected)
                    break;
                EditorGUI.DrawRect(dropArea, Color.grey);
                break;
            case EventType.DragPerform:
                DragAndDrop.AcceptDrag();
                dropData = DragAndDrop.GetGenericData(Drag_Data_Name) as string;
                dropData = dropData == null ? "" : dropData;
                RemoveTextFocus(); //让有焦点的文本框也能刷新内容
                currentEvent.Use();
                break;
            case EventType.MouseUp:
                DragAndDrop.PrepareStartDrag();
                break;
        }
    }

    private void OnStepContextMenuInsert(object cxt)
    {
        var curConfig = GetCurConfig();
        var stepList = curConfig.stepList;

        var type = (int)cxt;
        var index = 0;
        if (m_curSelStep >= 0 && m_curSelStep <= stepList.Count)
        {
            if (type == 0)
                index = m_curSelStep;
            else if (type == 1)
                index = m_curSelStep + 1;
            else
                index = stepList.Count;
        }
        else
        {
            index = stepList.Count;
        }

        stepList.Insert(index, new TeachStepConfig());
        m_curSelStep = index;
        curConfig.IsDirty = true;
        RemoveTextFocus(); //让有焦点的文本框也能刷新内容
    }

    private void OnStepContextMenuDelete()
    {
        var curConfig = GetCurConfig();
        var stepList = curConfig.stepList;

        if (m_curSelStep >= 0 && m_curSelStep < stepList.Count)
        {
            MessageBoxWindow.ShowAsMsgBox("是否真的要删除选中的步骤？", "提示", (obj, cxt) =>
            {
                stepList.RemoveAt(m_curSelStep);
                m_curSelStep = m_curSelStep >= stepList.Count ? stepList.Count - 1 : m_curSelStep;
                curConfig.IsDirty = true;
                RemoveTextFocus(); //让有焦点的文本框也能刷新内容
                Repaint();
            }, (obj, cxt) => { });
        }
        else
        {
            MessageBoxWindow.ShowAsMsgBox("请先选择一个步骤。", "提示");
        }
    }

    private void OnStepContextMenuMoveUp()
    {
        var curConfig = GetCurConfig();
        var stepList = curConfig.stepList;

        if (m_curSelStep >= 1 && m_curSelStep < stepList.Count)
        {
            var temp = stepList[m_curSelStep - 1];
            stepList[m_curSelStep - 1] = stepList[m_curSelStep];
            stepList[m_curSelStep] = temp;
            --m_curSelStep;
            curConfig.IsDirty = true;
            RemoveTextFocus(); //让有焦点的文本框也能刷新内容
        }
        else if (m_curSelStep == 0)
        {
            MessageBoxWindow.ShowAsMsgBox("不能再上移了。", "提示");
        }
        else
        {
            MessageBoxWindow.ShowAsMsgBox("请先选择一个步骤。", "提示");
        }
    }

    private void OnStepContextMenuMoveDown()
    {
        var curConfig = GetCurConfig();
        var stepList = curConfig.stepList;

        if (m_curSelStep >= 0 && m_curSelStep < stepList.Count - 1)
        {
            var temp = stepList[m_curSelStep + 1];
            stepList[m_curSelStep + 1] = stepList[m_curSelStep];
            stepList[m_curSelStep] = temp;
            ++m_curSelStep;
            curConfig.IsDirty = true;
            RemoveTextFocus(); //让有焦点的文本框也能刷新内容
        }
        else if (m_curSelStep == stepList.Count - 1)
        {
            MessageBoxWindow.ShowAsMsgBox("不能再下移了。", "提示");
        }
        else
        {
            MessageBoxWindow.ShowAsMsgBox("请先选择一个步骤。", "提示");
        }
    }

    private void OnStepContextMenuPlay(object cxt)
    {
        //不用重置原来的选中项
        //m_curSelStep = -1;
        var index = (int)cxt;
        m_teachMgr.StartPlay(true, index);
        RemoveTextFocus();
    }

    private void OnStepContextMenuPlayWithCheck()
    {
        var cfg = GetCurConfig();
        if (cfg == null)
            return;
        //清队列
        m_teachMgr.ClearPlayQueue();
        //引导的冷却时间也清一下吧
        m_teachMgr.ClearCoolDownData();
        //带检测地播放
        m_teachMgr.PlayTeach(cfg.teachName);
        RemoveTextFocus();
    }

    private void OnStepContextMenuRecord(object cxt)
    {
        //不用重置原来的选中项
        //m_curSelStep = -1;
        var index = (int)cxt;
        m_teachMgr.StartRecord(true, index);
        RemoveTextFocus();
    }

    private void OnStepContextMenuCopy()
    {
        var curConfig = GetCurConfig();
        var stepList = curConfig.stepList;

        if (m_curSelStep >= 0 && m_curSelStep < stepList.Count)
        {
            var stepCfg = stepList[m_curSelStep];
            m_copyStepJsonStr = JsonMapper.ToJson(stepCfg);
        }
        else
        {
            MessageBoxWindow.ShowAsMsgBox("请先选择一个步骤。", "提示");
        }
    }

    private void OnStepContextMenuPasteOverwrite()
    {
        var curConfig = GetCurConfig();
        var stepList = curConfig.stepList;

        if (m_curSelStep >= 0 && m_curSelStep < stepList.Count)
        {
            var stepObj = JsonMapper.ToObject<TeachStepConfig>(m_copyStepJsonStr);
            curConfig.stepList[m_curSelStep] = stepObj;
            curConfig.IsDirty = true;
            RemoveTextFocus(); //让有焦点的文本框也能刷新内容
        }
        else
        {
            MessageBoxWindow.ShowAsMsgBox("请先选择一个步骤。", "提示");
        }
    }

    private void OnStepContextMenuPasteInsert(object cxt)
    {
        var curConfig = GetCurConfig();
        var stepList = curConfig.stepList;

        var type = (int)cxt;
        var index = 0;
        if (m_curSelStep >= 0 && m_curSelStep <= stepList.Count)
        {
            if (type == 0)
                index = m_curSelStep;
            else if (type == 1)
                index = m_curSelStep + 1;
            else
                index = stepList.Count;
        }
        else
        {
            index = stepList.Count;
        }

        var stepObj = JsonMapper.ToObject<TeachStepConfig>(m_copyStepJsonStr);
        curConfig.stepList.Insert(index, stepObj);
        m_curSelStep = index;
        curConfig.IsDirty = true;
        RemoveTextFocus(); //让有焦点的文本框也能刷新内容
    }

    private void DrawTeachStepToolBar()
    {
        var curConfig = GetCurConfig();
        var stepList = curConfig.stepList;

        ////////////////////////////        
        if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus", "添加菜单"), EditorStyleEx.ToolbarPopupEx, GUILayout.Width(32)))
        {
            var hasSel = m_curSelStep >= 0 && m_curSelStep <= stepList.Count;
            var hasCopy = !string.IsNullOrEmpty(m_copyStepJsonStr);

            var cxtMenu = new GenericMenu();
            cxtMenu.AddItem(new GUIContent("追加新步骤到最后"), false, OnStepContextMenuInsert, 2);
            cxtMenu.AddSeparator(string.Empty);
            if (hasSel)
            {
                cxtMenu.AddItem(new GUIContent("插入新步骤到行前"), false, OnStepContextMenuInsert, 0);
                cxtMenu.AddItem(new GUIContent("插入新步骤到行后"), false, OnStepContextMenuInsert, 1);
            }
            else
            {
                cxtMenu.AddDisabledItem(new GUIContent("插入新步骤到行前"));
                cxtMenu.AddDisabledItem(new GUIContent("插入新步骤到行后"));
            }
            cxtMenu.AddSeparator(string.Empty);
            if (hasCopy)
                cxtMenu.AddItem(new GUIContent("粘贴步骤到最后"), false, OnStepContextMenuPasteInsert, 2);
            else
                cxtMenu.AddDisabledItem(new GUIContent("粘贴步骤到最后"));
            cxtMenu.AddSeparator(string.Empty);
            if (hasCopy && hasSel)
            {
                cxtMenu.AddItem(new GUIContent("复制本步骤数据"), false, OnStepContextMenuCopy);
                cxtMenu.AddItem(new GUIContent("粘贴覆盖本步骤"), false, OnStepContextMenuPasteOverwrite);
                cxtMenu.AddItem(new GUIContent("粘贴到本步骤前"), false, OnStepContextMenuPasteInsert, 0);
                cxtMenu.AddItem(new GUIContent("粘贴到本步骤后"), false, OnStepContextMenuPasteInsert, 1);
                
            }
            else
            {
                cxtMenu.AddDisabledItem(new GUIContent("复制本步骤数据"));
                cxtMenu.AddDisabledItem(new GUIContent("粘贴覆盖本步骤"));
                cxtMenu.AddDisabledItem(new GUIContent("粘贴到本步骤前"));
                cxtMenu.AddDisabledItem(new GUIContent("粘贴到本步骤后"));
            }
            cxtMenu.ShowAsContext();
        }
        ////////////////////////////

        ////////////////////////////
        if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus", "删除"), EditorStyleEx.ToolBarButtonEx, GUILayout.Width(24)))
        {
            OnStepContextMenuDelete();
        }
        ////////////////////////////

        ////////////////////////////
        if (GUILayout.Button("⇧", EditorStyleEx.ToolBarButtonEx, GUILayout.Width(24)))
        {
            OnStepContextMenuMoveUp();
        }
        ////////////////////////////

        ////////////////////////////
        if (GUILayout.Button("⇩", EditorStyleEx.ToolBarButtonEx, GUILayout.Width(24)))
        {
            OnStepContextMenuMoveDown();
        }
        ////////////////////////////

        ////////////////////////////
        if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.Record", "录制"), EditorStyleEx.ToolbarPopupEx, GUILayout.Width(32)))
        {
            var cxtMenu = new GenericMenu();
            if (m_curSelStep >= 0 && m_curSelStep <= stepList.Count)
            {                
                cxtMenu.AddItem(new GUIContent("当前插入录制"), false, OnStepContextMenuRecord, m_curSelStep);
                cxtMenu.AddItem(new GUIContent("当前追加录制"), false, OnStepContextMenuRecord, m_curSelStep + 1);
            }
            else
            {
                cxtMenu.AddDisabledItem(new GUIContent("当前插入录制"));
                cxtMenu.AddDisabledItem(new GUIContent("当前追加录制"));
            }
            cxtMenu.AddItem(new GUIContent("末尾追加录制"), false, OnStepContextMenuRecord, int.MaxValue);
            cxtMenu.AddSeparator(string.Empty);
            cxtMenu.AddItem(new GUIContent("清空再录制"), false, OnStepContextMenuRecord, int.MinValue);
            cxtMenu.ShowAsContext();
        }
        ////////////////////////////

        ////////////////////////////        
        if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.Play", "播放"), EditorStyleEx.ToolbarPopupEx, GUILayout.Width(32)))
        {
            var cxtMenu = new GenericMenu();
            if (m_curSelStep >= 0 && m_curSelStep <= stepList.Count)
            {                
                cxtMenu.AddItem(new GUIContent("从选中步骤开始播放"), false, OnStepContextMenuPlay, m_curSelStep);
            }
            else
            {
                cxtMenu.AddDisabledItem(new GUIContent("从选中步骤开始播放"));
            }
            cxtMenu.AddSeparator(string.Empty);
            cxtMenu.AddItem(new GUIContent("从头开始播放"), false, OnStepContextMenuPlay, 0);
            cxtMenu.AddItem(new GUIContent("从头开始播放（带条件检测）"), false, OnStepContextMenuPlayWithCheck);
            cxtMenu.ShowAsContext();
        }
        ////////////////////////////

        ////////////////////////////
        if (GUILayout.Button(EditorGUIUtility.IconContent("RectTransformBlueprint", "清空步骤"), EditorStyleEx.ToolBarButtonEx, GUILayout.Width(24)))
        {
            MessageBoxWindow.ShowAsMsgBox("是否真要清空全部步骤？", "提示", (obj, cxt) =>
            {
                stepList.Clear();
                m_curSelStep = -1;
                curConfig.IsDirty = true;
                RemoveTextFocus(); //让有焦点的文本框也能刷新内容
                Repaint();
            }, (obj, cxt) => { });
        }
        ////////////////////////////

        ////////////////////////////
        var recordStateHandleState = GUILayout.Toggle(TeachMgr.instance.RecordStateHandleState, "录制状态变化", EditorStyles.toolbarButton, GUILayout.Width(70));
        if (recordStateHandleState != TeachMgr.instance.RecordStateHandleState)
        {
            TeachMgr.instance.RecordStateHandleState = recordStateHandleState;
        }
        ////////////////////////////
    }

    private void DrawSelStepDesc()
    {
        var descStr = "";
        var curConfig = GetCurConfig();
        var stepList = curConfig.stepList;
        var stepIndex = m_teachMgr.PlayNow ? m_teachMgr.CurPlayIndex : (m_teachMgr.RecordNow ? m_teachMgr.CurRecordIndex : m_curSelStep);
        if (stepIndex >= 0 && stepIndex < stepList.Count)
        {
            var stepCfg = stepList[stepIndex];
            descStr = stepCfg.sceneOpType != TeachSceneOpType.none ? stepCfg.sceneOpParam : stepCfg.uiOpParam;
        }
        else if (m_teachMgr.RecordNow)
        {
            descStr = "新步骤将追加在最后";
        }

        Rect rect = GUILayoutUtility.GetRect(new GUIContent(descStr), EditorStyles.label, GUILayout.ExpandWidth(true));
        GUI.Box(rect, descStr, EditorStyles.label);
    }

    private void DrawStepHeader()
    {
        var padSize = new Vector2(5, 5);
        var cellRect = new Rect(0, 0, 120, 16);
        var rowRect = new Rect(0, 0, m_lastRightWidth * 2, cellRect.height); //为了能跟着下面滚动，这里撑大

        EditorGUI.LabelField(rowRect, GUIContent.none, EditorStyles.toolbar);

        cellRect.x = padSize.x;
        cellRect.width = cellRect.height * 2;
        GUI.Button(cellRect, "序号", EditorStyleEx.ToolBarButtonEx);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = cellRect.height * 6;
        GUI.Button(cellRect, "名字", EditorStyleEx.ToolBarButtonEx);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = cellRect.height * 2;
        GUI.Button(cellRect, "强制", EditorStyleEx.ToolBarButtonEx);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = cellRect.height * 4;
        GUI.Button(cellRect, "背景Alpha", EditorStyleEx.ToolBarButtonEx);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = cellRect.height * 2;
        GUI.Button(cellRect, "暂停", EditorStyleEx.ToolBarButtonEx);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = cellRect.height * 2;
        GUI.Button(cellRect, "关键", EditorStyleEx.ToolBarButtonEx);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = cellRect.height * 3;
        GUI.Button(cellRect, "UI提示", EditorStyles.toolbarButton);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = cellRect.height * 3;
        GUI.Button(cellRect, "大字提示", EditorStyles.toolbarButton);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = cellRect.height * 2;
        GUI.Button(cellRect, "停声", EditorStyleEx.ToolBarButtonEx);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = cellRect.height * 2;
        GUI.Button(cellRect, "禁乐", EditorStyleEx.ToolBarButtonEx);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = 30 + cellRect.height * 1.5f + padSize.x;
        GUI.Button(cellRect, "声音", EditorStyleEx.ToolBarButtonEx);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = cellRect.height * 5;
        GUI.Button(cellRect, "超时处理", EditorStyleEx.ToolBarButtonEx);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = cellRect.height * 4;
        GUI.Button(cellRect, "步骤目标", EditorStyleEx.ToolBarButtonEx);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = 100 + cellRect.height * 1.5f;
        GUI.Button(cellRect, "目标参数", EditorStyleEx.ToolBarButtonEx);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = cellRect.height * 4;
        GUI.Button(cellRect, "UI操作", EditorStyleEx.ToolBarButtonEx);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = cellRect.height * 3;
        GUI.Button(cellRect, "圈型", EditorStyleEx.ToolBarButtonEx);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = cellRect.height * 3;
        GUI.Button(cellRect, "手型", EditorStyleEx.ToolBarButtonEx);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = 200 + cellRect.height * 1.5f * 2 + padSize.x;
        GUI.Button(cellRect, "UI操作参数", EditorStyleEx.ToolBarButtonEx);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = cellRect.height * 4;
        GUI.Button(cellRect, "场景操作", EditorStyleEx.ToolBarButtonEx);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = cellRect.height * 3;
        GUI.Button(cellRect, "指引", EditorStyleEx.ToolBarButtonEx);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = 200 + cellRect.height * 1.5f * 2 + padSize.x;
        GUI.Button(cellRect, "场景操作参数", EditorStyleEx.ToolBarButtonEx);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = cellRect.height * 6.5f;
        GUI.Button(cellRect, "步骤跳过", EditorStyleEx.ToolBarButtonEx);

        cellRect.x = cellRect.xMax + padSize.x;
        cellRect.width = 150 + cellRect.height * 1.5f + padSize.x;
        GUI.Button(cellRect, "步骤跳过参数", EditorStyleEx.ToolBarButtonEx);

        m_lastHeaderWidth = Mathf.Max(cellRect.xMax, m_lastRightWidth);
        GUILayoutUtility.GetRect(Mathf.Max(cellRect.xMax, rowRect.xMax), rowRect.yMax);
    }

    private bool NeedDisableStepObj(TeachStepConfig stepCfg)
    {
        switch (stepCfg.uiOpType)
        {
            case TeachUIOpType.uiPanelFunc:
            case TeachUIOpType.uiPanelFuncSync:
            case TeachUIOpType.showUINode:
            case TeachUIOpType.hideUINode:
                return true;
        }
        switch (stepCfg.sceneOpType)
        {
            case TeachSceneOpType.fireSceneAction:
            case TeachSceneOpType.enterScene:
            case TeachSceneOpType.leaveScene:
            case TeachSceneOpType.teachData:
            case TeachSceneOpType.enableHeroAI:
                return true;
        }
        return false;
    }

    private bool NeedDisableUIOpType(TeachStepConfig stepCfg)
    {
        switch (stepCfg.sceneOpType)
        {
            case TeachSceneOpType.fireSceneAction:
            case TeachSceneOpType.enterScene:
            case TeachSceneOpType.leaveScene:
            case TeachSceneOpType.teachData:
            case TeachSceneOpType.enableHeroAI:
                return true;
        }
        return false;
    }

    private bool NeedDisableSceneOpType(TeachStepConfig stepCfg)
    {
        switch (stepCfg.uiOpType)
        {
            case TeachUIOpType.uiPanelFunc:
            case TeachUIOpType.uiPanelFuncSync:
            case TeachUIOpType.showUINode:
            case TeachUIOpType.hideUINode:
                return true;
        }
        return false;
    }

    private bool NeedDisableSkipType(TeachStepConfig stepCfg)
    {
        switch (stepCfg.uiOpType)
        {
            case TeachUIOpType.uiPanelFunc:
            case TeachUIOpType.uiPanelFuncSync:
            case TeachUIOpType.showUINode:
            case TeachUIOpType.hideUINode:
                return true;
        }
        switch (stepCfg.sceneOpType)
        {
            case TeachSceneOpType.fireSceneAction:
            case TeachSceneOpType.enterScene:
            case TeachSceneOpType.leaveScene:
            case TeachSceneOpType.teachData:
            case TeachSceneOpType.enableHeroAI:
                return true;
        }
        return false;
    }

    private void DrawTeachSteps()
    {
        var curConfig = GetCurConfig();
        var curEvent = Event.current;
        var stepList = curConfig.stepList;
        var curPlayIndex = m_teachMgr.CurPlayIndex;
        var curRecordIndex = m_teachMgr.CurRecordIndex;
        var padSize = new Vector2(5, 5);
        var cellRect = new Rect(0, 0, 120, 16);
        var rowRect = new Rect(0, 0, m_lastHeaderWidth, cellRect.height + padSize.y * 2);
        var colorEven = new Color(0.172f, 0.172f, 0.172f);
        var colorOdd = new Color(0.188f, 0.188f, 0.188f);
        var colorSel = new Color(0.243f, 0.372f, 0.588f);
        for (var i = 0; i < stepList.Count; ++i)
        {
            var stepCfg = stepList[i];

            ////////////////////////////
            var needRepaint = false;
            var mustForceMode = false;
            switch (stepCfg.uiOpType)
            {
                case TeachUIOpType.fullScreenImg:
                case TeachUIOpType.windowImg:
                case TeachUIOpType.fullScreenClick:
                    {
                        mustForceMode = true;
                        if (stepCfg.force != true)
                        {
                            stepCfg.force = true;
                            curConfig.IsDirty = true;
                            needRepaint = true;
                        }
                    }
                    break;
                case TeachUIOpType.uiPanelFunc:
                case TeachUIOpType.uiPanelFuncSync:
                case TeachUIOpType.showUINode:
                case TeachUIOpType.hideUINode:
                    {
                        if (stepCfg.stepObj != TeachStepObj.uiOp)
                        {
                            stepCfg.stepObj = TeachStepObj.uiOp;
                            curConfig.IsDirty = true;
                            needRepaint = true;
                        }
                        if (stepCfg.sceneOpType != TeachSceneOpType.none)
                        {
                            stepCfg.sceneOpType = TeachSceneOpType.none;
                            curConfig.IsDirty = true;
                            needRepaint = true;
                        }
                        if (stepCfg.timeoutInMS != 0)
                        {
                            stepCfg.timeoutInMS = 0;
                            curConfig.IsDirty = true;
                            needRepaint = true;
                        }
                        if (stepCfg.stepSkipType != TeachStepSkipType.none)
                        {
                            stepCfg.stepSkipType = TeachStepSkipType.none;
                            curConfig.IsDirty = true;
                            needRepaint = true;
                        }
                    }
                    break;
            }
            switch (stepCfg.sceneOpType)
            {
                case TeachSceneOpType.fireSceneAction:
                case TeachSceneOpType.enterScene:
                case TeachSceneOpType.leaveScene:
                case TeachSceneOpType.teachData:
                case TeachSceneOpType.enableHeroAI:
                    {
                        if (stepCfg.stepObj != TeachStepObj.sceneOp)
                        {
                            stepCfg.stepObj = TeachStepObj.sceneOp;
                            curConfig.IsDirty = true;
                            needRepaint = true;
                        }
                        if (stepCfg.uiOpType != TeachUIOpType.none)
                        {
                            stepCfg.uiOpType = TeachUIOpType.none;
                            curConfig.IsDirty = true;
                            needRepaint = true;
                        }
                        if (stepCfg.timeoutInMS != 0)
                        {
                            stepCfg.timeoutInMS = 0;
                            curConfig.IsDirty = true;
                            needRepaint = true;
                        }
                        if (stepCfg.stepSkipType != TeachStepSkipType.none)
                        {
                            stepCfg.stepSkipType = TeachStepSkipType.none;
                            curConfig.IsDirty = true;
                            needRepaint = true;
                        }
                    }
                    break;
            }
            if (needRepaint)
                Repaint();
            ////////////////////////////

            ////////////////////////////
            if (i > 0)
                rowRect.y += rowRect.height;
            cellRect.y = rowRect.y + padSize.y;
            ////////////////////////////

            ////////////////////////////
            bool isCur = m_teachMgr.PlayNow ? i == curPlayIndex : (m_teachMgr.RecordNow ? i == curRecordIndex : i == m_curSelStep);
            EditorGUI.DrawRect(rowRect, isCur ? colorSel : (i % 2 == 0 ? colorEven : colorOdd));
            ////////////////////////////

            ////////////////////////////
            cellRect.x = padSize.x;
            cellRect.width = cellRect.height * 2;
            EditorGUI.LabelField(cellRect, (i + 1).ToString());
            ////////////////////////////

            ////////////////////////////
            cellRect.x = cellRect.xMax + padSize.x;
            cellRect.width = cellRect.height * 6;
            var newName = EditorGUI.TextField(cellRect, stepCfg.name);
            if (newName != stepCfg.name)
            {
                stepCfg.name = newName;
                curConfig.IsDirty = true;
            }
            ////////////////////////////

            ////////////////////////////
            cellRect.x = cellRect.xMax + padSize.x;
            cellRect.width = cellRect.height * 2;
            using (new AutoEditorDisabledGroup(mustForceMode))
            {
                var force = EditorGUI.Toggle(cellRect, stepCfg.force);
                if (force != stepCfg.force)
                {
                    stepCfg.force = force;
                    curConfig.IsDirty = true;
                }
            }                
            ////////////////////////////

            ////////////////////////////
            cellRect.x = cellRect.xMax + padSize.x;
            cellRect.width = cellRect.height * 4;
            using (new AutoEditorDisabledGroup(!stepCfg.force))
            {
                if (stepCfg.force)
                {
                    var maskAlpha = EditorGUI.IntField(cellRect, stepCfg.maskAlpha);
                    if (maskAlpha != stepCfg.maskAlpha)
                    {
                        stepCfg.maskAlpha = Mathf.Clamp(maskAlpha, 0, 255);
                        curConfig.IsDirty = true;
                    }
                }
                else
                {
                    EditorGUI.TextField(cellRect, "N/A");
                }
            }
            ////////////////////////////

            ////////////////////////////
            cellRect.x = cellRect.xMax + padSize.x;
            cellRect.width = cellRect.height * 2;
            var pauseModeStrs = Util.GetEnumAllDesc<TeachPauseMode>();
            var pauseMode = (TeachPauseMode)EditorGUI.Popup(cellRect, (int)stepCfg.pauseMode, pauseModeStrs);
            if (pauseMode != stepCfg.pauseMode)
            {
                stepCfg.pauseMode = pauseMode;
                curConfig.IsDirty = true;
            }
            ////////////////////////////

            ////////////////////////////
            cellRect.x = cellRect.xMax + padSize.x;
            cellRect.width = cellRect.height * 2;
            var keyStep = EditorGUI.Toggle(cellRect, stepCfg.keyStep, EditorStyles.radioButton);
            if (keyStep != stepCfg.keyStep)
            {
                stepCfg.keyStep = keyStep;
                if (keyStep)
                {
                    for (var j = 0; j < stepList.Count; ++j)
                    {
                        var stepCfg2 = stepList[j];
                        if (i != j)
                            stepCfg2.keyStep = false;
                    }
                }
                curConfig.IsDirty = true;
            }
            ////////////////////////////

            ////////////////////////////
            cellRect.x = cellRect.xMax + padSize.x;
            cellRect.width = cellRect.height * 1.5f;
            if (GUI.Button(cellRect, EditorGUIUtility.IconContent(string.IsNullOrEmpty(stepCfg.tipMsg) ? "GUISystem/align_vertically_center" : "GUISystem/align_vertically_center_active", "提示文字"), EditorStyleEx.OLTitleAlignCenter))
            {
                MessageBoxWindow.ShowAsInputBox(stepCfg.tipMsg, "修改UI提示文字", i, (str, cxt) =>
                {
                    var index = (int)cxt;
                    if (str != stepList[index].tipMsg)
                    {
                        stepList[index].tipMsg = str;
                        curConfig.IsDirty = true;
                        Repaint();
                    }
                }, (str, cxt) => { });
            }
            cellRect.width = cellRect.height * 3.0f;
            ////////////////////////////

            ////////////////////////////
            cellRect.x = cellRect.xMax + padSize.x;
            cellRect.width = cellRect.height * 1.5f;
            if (GUI.Button(cellRect, EditorGUIUtility.IconContent(string.IsNullOrEmpty(stepCfg.centerTip) ? "GUISystem/align_vertically_center" : "GUISystem/align_vertically_center_active", "提示文字"), EditorStyleEx.OLTitleAlignCenter))
            {
                MessageBoxWindow.ShowAsInputBox(stepCfg.centerTip, "修改大字提示文字", i, (str, cxt) =>
                {
                    var index = (int)cxt;
                    if (str != stepList[index].centerTip)
                    {
                        stepList[index].centerTip = str;
                        curConfig.IsDirty = true;
                        Repaint();
                    }
                }, (str, cxt) => { });
            }
            cellRect.width = cellRect.height * 3.0f;
            ////////////////////////////

            ////////////////////////////
            cellRect.x = cellRect.xMax + padSize.x;
            cellRect.width = cellRect.height * 2;
            var stopPreSnd = EditorGUI.Toggle(cellRect, stepCfg.stopPreSnd);
            if (stopPreSnd != stepCfg.stopPreSnd)
            {
                stepCfg.stopPreSnd = stopPreSnd;
                curConfig.IsDirty = true;
            }
            ////////////////////////////

            ////////////////////////////
            cellRect.x = cellRect.xMax + padSize.x;
            cellRect.width = cellRect.height * 2;
            var muteMusic = EditorGUI.Toggle(cellRect, stepCfg.muteMusic);
            if (muteMusic != stepCfg.muteMusic)
            {
                stepCfg.muteMusic = muteMusic;
                curConfig.IsDirty = true;
            }
            ////////////////////////////

            ////////////////////////////
            cellRect.x = cellRect.xMax + padSize.x;
            cellRect.width = 30;
            var oldSoundId = stepCfg.soundId;
            var sndCfg = oldSoundId == 0 ? null : SoundCfg.Get(oldSoundId);
            var newSoundId = 0;
            using (new AutoWarningColor(oldSoundId != 0 && sndCfg == null))
            {
                newSoundId = EditorGUI.IntField(cellRect, oldSoundId);
            }
            if (newSoundId != oldSoundId)
            {
                stepCfg.soundId = newSoundId;
                curConfig.IsDirty = true;
            }
            ////////////////////////////

            ////////////////////////////
            cellRect.x = cellRect.xMax + padSize.x;
            cellRect.width = cellRect.height * 1.5f;
            using (new AutoEditorDisabledGroup(sndCfg == null))
            {
                if (GUI.Button(cellRect, EditorGUIUtility.IconContent("Animation.Play", "试听声音"), EditorStyleEx.OLTitleAlignCenter))
                {
                    if (SoundMgr.instance == null)
                        ShowNotice("声音模块没初始化，不能播放");
                    else
                        SoundMgr.instance.Play2DSound(Sound2DType.other, newSoundId);
                }
            }
            ////////////////////////////

            ////////////////////////////
            {
                var canAutoNext = TeachMgr.CanAutoNext(stepCfg);

                {
                    cellRect.x = cellRect.xMax + padSize.x;
                    cellRect.width = cellRect.height * 2.5f;

                    var timeoutInMS = EditorGUI.IntField(cellRect, stepCfg.timeoutInMS);
                    if (timeoutInMS != stepCfg.timeoutInMS)
                    {
                        stepCfg.timeoutInMS = Mathf.Max(0, timeoutInMS);
                        curConfig.IsDirty = true;
                    }
                }                

                using (new AutoEditorDisabledGroup(!canAutoNext))
                {
                    cellRect.x = cellRect.xMax;
                    cellRect.width = cellRect.height * 2.5f;

                    if (!canAutoNext && !stepCfg.timeoutStop)
                    {
                        stepCfg.timeoutStop = true;
                        stepCfg.timeoutInMS = 0;
                        curConfig.IsDirty = true;
                    }
                    var timeoutStopNum = EditorGUI.Popup(cellRect, stepCfg.timeoutStop ? 1 : 0, new string[] { "后续", "停止" });
                    var timeoutStop = timeoutStopNum != 0 ? true : false;                    
                    if (timeoutStop != stepCfg.timeoutStop)
                    {
                        stepCfg.timeoutStop = timeoutStop;
                        curConfig.IsDirty = true;
                    }
                }
            }
            ////////////////////////////

            ////////////////////////////
            cellRect.x = cellRect.xMax + padSize.x;
            cellRect.width = cellRect.height * 4;
            var stepObjStrs = Util.GetEnumAllDesc<TeachStepObj>();
            var disableStepObjType = NeedDisableStepObj(stepCfg);
            using (new AutoEditorDisabledGroup(disableStepObjType))
            {
                var stepObj = (TeachStepObj)EditorGUI.Popup(cellRect, (int)stepCfg.stepObj, stepObjStrs);
                if (stepObj != stepCfg.stepObj)
                {
                    stepCfg.stepObj = stepObj;
                    curConfig.IsDirty = true;
                }
            }
            ////////////////////////////

            ////////////////////////////
            switch (stepCfg.stepObj)
            {
                case TeachStepObj.showUINode:
                case TeachStepObj.hideUINode:
                    {
                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 100;
                        var oldRectTrans = UITeach.FindRectTransform(stepCfg.stepObjParam);
                        var newRectTrans = (RectTransform)EditorGUI.ObjectField(cellRect, oldRectTrans, typeof(RectTransform), true);
                        if (newRectTrans != oldRectTrans)
                        {
                            stepCfg.stepObjParam = newRectTrans == null ? "" : Util.GetGameObjectPath(newRectTrans.gameObject);
                            stepCfg.stepObjParam2 = "";
                            curConfig.IsDirty = true;
                        }
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax;
                        cellRect.width = cellRect.height * 1.5f;
                        if (GUI.Button(cellRect, EditorGUIUtility.IconContent(string.IsNullOrEmpty(stepCfg.stepObjParam) ? "GUISystem/align_vertically_center" : "GUISystem/align_vertically_center_active", "查看路径"), EditorStyleEx.OLTitleAlignCenter))
                        {
                            MessageBoxWindow.ShowAsInputBox(stepCfg.stepObjParam, "修改UI路径", i, (str, cxt) =>
                            {
                                var index = (int)cxt;
                                if (str != stepList[index].stepObjParam)
                                {
                                    stepList[index].stepObjParam = str;
                                    stepList[index].stepObjParam2 = "";
                                    curConfig.IsDirty = true;
                                    Repaint();
                                }
                            }, (str, cxt) => { });
                        }
                        ////////////////////////////
                    }
                    break;
                case TeachStepObj.uiOp:
                    {
                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 65;
                        EditorGUI.LabelField(cellRect, "等待UI显示");
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax;
                        cellRect.width = 35;
                        var oldWaitUITime = StringUtil.ToInt(stepCfg.stepObjParam);
                        //原来不是数字？用默认值填上
                        if (stepCfg.stepObjParam != oldWaitUITime.ToString())
                            oldWaitUITime = UITeach.DEF_WAIT_UI_TIME;
                        var newWaitUITime = EditorGUI.IntField(cellRect, oldWaitUITime);
                        if (newWaitUITime != oldWaitUITime)
                        {
                            stepCfg.stepObjParam = newWaitUITime.ToString();
                            stepCfg.stepObjParam2 = "";
                            curConfig.IsDirty = true;
                        }
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax;
                        cellRect.width = cellRect.height * 1.5f;
                        EditorGUI.LabelField(cellRect, "毫秒");
                        ////////////////////////////
                    }
                    break;
                case TeachStepObj.none:                
                case TeachStepObj.sceneOp:
                    {
                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 100 + cellRect.height * 1.5f;
                        ////////////////////////////
                    }
                    break;
                case TeachStepObj.directEvent:
                case TeachStepObj.directEventEx:
                case TeachStepObj.normalEvent:
                case TeachStepObj.normalEventEx:
                    {
                        ////////////////////////////
                        var ctrlwidth = 100 + cellRect.height * 1.5f;
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = ctrlwidth / 2;
                        var stepObjParam = EditorGUI.TextField(cellRect, stepCfg.stepObjParam);
                        if (stepObjParam != stepCfg.stepObjParam)
                        {
                            stepCfg.stepObjParam = stepObjParam;
                            curConfig.IsDirty = true;
                        }
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax;
                        cellRect.width = ctrlwidth / 2;
                        var stepObjParam2 = EditorGUI.TextField(cellRect, stepCfg.stepObjParam2);
                        if (stepObjParam2 != stepCfg.stepObjParam2)
                        {
                            stepCfg.stepObjParam2 = stepObjParam2;
                            curConfig.IsDirty = true;
                        }
                        ////////////////////////////
                    }
                    break;
                default:
                    {
                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 100 + cellRect.height * 1.5f;
                        var stepObjParam = EditorGUI.TextField(cellRect, stepCfg.stepObjParam);
                        if (stepObjParam != stepCfg.stepObjParam)
                        {
                            stepCfg.stepObjParam = stepObjParam;
                            curConfig.IsDirty = true;
                        }
                        ////////////////////////////
                    }
                    break;
            }
            ////////////////////////////

            ////////////////////////////
            cellRect.x = cellRect.xMax + padSize.x;
            cellRect.width = cellRect.height * 4;
            var uiOpStrs = Util.GetEnumAllDesc<TeachUIOpType>();
            var disableUIOpType = NeedDisableUIOpType(stepCfg);
            using (new AutoEditorDisabledGroup(disableUIOpType))
            {
                var uiOpType = (TeachUIOpType)EditorGUI.Popup(cellRect, (int)stepCfg.uiOpType, uiOpStrs);
                if (uiOpType != stepCfg.uiOpType)
                {
                    stepCfg.uiOpType = uiOpType;
                    curConfig.IsDirty = true;
                }
            }
            ////////////////////////////

            var noShowIndicator = TeachMgr.NoShowIndicatorUIOp(stepCfg);

            ////////////////////////////
            cellRect.x = cellRect.xMax + padSize.x;
            cellRect.width = cellRect.height * 3;
            var circleTypeStrs = Util.GetEnumAllDesc<TeachCircleType>();
            using (new AutoEditorDisabledGroup(noShowIndicator))
            {
                if (!noShowIndicator)
                {                    
                    var circleType = (TeachCircleType)EditorGUI.Popup(cellRect, (int)stepCfg.circleType, circleTypeStrs);
                    if (circleType != stepCfg.circleType)
                    {
                        stepCfg.circleType = circleType;
                        curConfig.IsDirty = true;
                    }
                }
                else
                {
                    EditorGUI.Popup(cellRect, 0, circleTypeStrs);
                }
            }
            ////////////////////////////

            ////////////////////////////
            cellRect.x = cellRect.xMax + padSize.x;
            cellRect.width = cellRect.height * 3;
            var arrowTypeStrs = Util.GetEnumAllDesc<TeachArrowType>();
            using (new AutoEditorDisabledGroup(noShowIndicator))
            {
                if (!noShowIndicator)
                {
                    var arrowType = (TeachArrowType)EditorGUI.Popup(cellRect, (int)stepCfg.arrowType, arrowTypeStrs);
                    if (arrowType != stepCfg.arrowType)
                    {
                        stepCfg.arrowType = arrowType;
                        curConfig.IsDirty = true;
                    }
                }
                else
                {
                    EditorGUI.Popup(cellRect, 0, arrowTypeStrs);
                }
            }
            ////////////////////////////

            ////////////////////////////
            switch (stepCfg.uiOpType)
            {
                case TeachUIOpType.stateHandle:
                case TeachUIOpType.stateGroup:
                    {
                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 100;
                        switch (stepCfg.uiOpType)
                        {
                            case TeachUIOpType.stateHandle:
                                {
                                    var oldObj = UITeach.FindStateHandle(stepCfg.uiOpParam);
                                    using (new AutoWarningColor(oldObj == null))
                                    {
                                        var newObj = (StateHandle)EditorGUI.ObjectField(cellRect, oldObj, typeof(StateHandle), true);
                                        if (newObj != oldObj)
                                        {
                                            stepCfg.uiOpParam = newObj == null ? "" : Util.GetGameObjectPath(newObj.gameObject);
                                            curConfig.IsDirty = true;
                                        }
                                    }
                                }
                                break;
                            case TeachUIOpType.stateGroup:
                                {
                                    var oldObj = UITeach.FindStateGroup(stepCfg.uiOpParam);
                                    using (new AutoWarningColor(oldObj == null))
                                    {
                                        var newObj = (StateGroup)EditorGUI.ObjectField(cellRect, oldObj, typeof(StateGroup), true);
                                        if (newObj != oldObj)
                                        {
                                            stepCfg.uiOpParam = newObj == null ? "" : Util.GetGameObjectPath(newObj.gameObject);
                                            curConfig.IsDirty = true;
                                        }
                                    }
                                }
                                break;
                        }
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax;
                        cellRect.width = cellRect.height * 1.5f;
                        if (GUI.Button(cellRect, EditorGUIUtility.IconContent(string.IsNullOrEmpty(stepCfg.uiOpParam) ? "GUISystem/align_vertically_center" : "GUISystem/align_vertically_center_active", "查看路径"), EditorStyleEx.OLTitleAlignCenter))
                        {
                            MessageBoxWindow.ShowAsInputBox(stepCfg.uiOpParam, "修改UI路径", i, (str, cxt) =>
                            {
                                var index = (int)cxt;
                                if (str != stepList[index].uiOpParam)
                                {
                                    stepList[index].uiOpParam = str;
                                    curConfig.IsDirty = true;
                                    Repaint();
                                }
                            }, (str, cxt) => { });
                        }
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 100 + cellRect.height * 1.5f;
                        var oldNum = StringUtil.ToInt(stepCfg.uiOpParam2);
                        var newNum = EditorGUI.IntField(cellRect, oldNum);
                        if (newNum != oldNum)
                        {
                            stepCfg.uiOpParam2 = newNum.ToString();
                            curConfig.IsDirty = true;
                        }
                        ////////////////////////////
                    }
                    break;
                case TeachUIOpType.fullScreenImg:
                    {
                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 100 + cellRect.height * 1.5f * 2;
                        var oldImg = UIMgr.GetSprite(stepCfg.uiOpParam);
                        var newImg = (Sprite)EditorGUI.ObjectField(cellRect, oldImg, typeof(Sprite), true);
                        if (newImg != oldImg)
                        {
                            stepCfg.uiOpParam = newImg == null ? "" : newImg.name;
                            curConfig.IsDirty = true;
                        }
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 100;
                        var filterModes = new string[] { "去边沿", "显示全" };
                        var oldFilterMode = stepCfg.uiOpParam2 == "inMode" ? 1 : 0;
                        var newFilterMode = EditorGUI.Popup(cellRect, oldFilterMode, filterModes);
                        if (newFilterMode != oldFilterMode)
                        {
                            stepCfg.uiOpParam2 = newFilterMode == 1 ? "inMode" : "";
                            curConfig.IsDirty = true;
                        }
                        ////////////////////////////
                    }
                    break;
                case TeachUIOpType.windowImg:
                    {
                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 100 + cellRect.height * 1.5f * 2;
                        var oldImg = UIMgr.GetSprite(stepCfg.uiOpParam);
                        var newImg = (Sprite)EditorGUI.ObjectField(cellRect, oldImg, typeof(Sprite), true);
                        if (newImg != oldImg)
                        {
                            stepCfg.uiOpParam = newImg == null ? "" : newImg.name;
                            curConfig.IsDirty = true;
                        }
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 100;
                        var windowtitle = EditorGUI.TextField(cellRect, stepCfg.uiOpParam2);
                        if (windowtitle != stepCfg.uiOpParam2)
                        {
                            stepCfg.uiOpParam2 = windowtitle;
                            curConfig.IsDirty = true;
                        }
                        ////////////////////////////
                    }
                    break;
                case TeachUIOpType.playStory:
                    {
                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 200 + cellRect.height * 1.5f * 2 + padSize.x;
                        var oldStoryId = stepCfg.uiOpParam;
                        var storyCfg = string.IsNullOrEmpty(oldStoryId) ? null : StorySaveCfg.GetCfg(oldStoryId);
                        var newStoryId = "";
                        using (new AutoWarningColor(!string.IsNullOrEmpty(oldStoryId) && storyCfg == null))
                        {
                            newStoryId = EditorGUI.TextField(cellRect, oldStoryId);
                        }
                        if (newStoryId != oldStoryId)
                        {
                            stepCfg.uiOpParam = newStoryId;
                            curConfig.IsDirty = true;
                        }
                        ////////////////////////////
                    }
                    break;
                case TeachUIOpType.none:
                case TeachUIOpType.showMainCityUI:
                    {
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 200 + cellRect.height * 1.5f * 2 + padSize.x;
                    }
                    break;
                case TeachUIOpType.uiPanelOpenTop:
                case TeachUIOpType.uiPanelClose:
                    {
                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 100;
                        var oldObj = UITeach.FindUIPanel(stepCfg.uiOpParam);
                        using (new AutoWarningColor(oldObj == null))
                        {
                            var newObj = (UIPanel)EditorGUI.ObjectField(cellRect, oldObj, typeof(UIPanel), true);
                            if (newObj != oldObj)
                            {
                                stepCfg.uiOpParam = newObj == null ? "" : Util.GetGameObjectPath(newObj.gameObject);
                                curConfig.IsDirty = true;
                            }
                        }
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax;
                        cellRect.width = cellRect.height * 1.5f;
                        if (GUI.Button(cellRect, EditorGUIUtility.IconContent(string.IsNullOrEmpty(stepCfg.uiOpParam) ? "GUISystem/align_vertically_center" : "GUISystem/align_vertically_center_active", "查看路径"), EditorStyleEx.OLTitleAlignCenter))
                        {
                            MessageBoxWindow.ShowAsInputBox(stepCfg.uiOpParam, "修改UI路径", i, (str, cxt) =>
                            {
                                var index = (int)cxt;
                                if (str != stepList[index].uiOpParam)
                                {
                                    stepList[index].uiOpParam = str;
                                    curConfig.IsDirty = true;
                                    Repaint();
                                }
                            }, (str, cxt) => { });
                        }
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 100 + cellRect.height * 1.5f;
                        ////////////////////////////
                    }
                    break;
                case TeachUIOpType.uiPanelFunc:
                case TeachUIOpType.uiPanelFuncSync:
                    {
                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 100;
                        var oldObj = UITeach.FindUIPanel(stepCfg.uiOpParam);
                        using (new AutoWarningColor(oldObj == null || !TeachMgr.HasTeachActionFunc(oldObj.GetComponent<UIPanel>())))
                        {
                            var newObj = (UIPanel)EditorGUI.ObjectField(cellRect, oldObj, typeof(UIPanel), true);
                            if (newObj != oldObj)
                            {
                                stepCfg.uiOpParam = newObj == null ? "" : Util.GetGameObjectPath(newObj.gameObject);
                                curConfig.IsDirty = true;

                                if (newObj != null)
                                {
                                    if (!TeachMgr.HasTeachActionFunc(newObj.GetComponent<UIPanel>()))
                                    {
                                        ShowNotice("该UIPanel没有带有一个string参数的实例方法" + TeachMgr.TEACH_ACTION_FUNC);
                                    }
                                }
                            }
                        }
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax;
                        cellRect.width = cellRect.height * 1.5f;
                        if (GUI.Button(cellRect, EditorGUIUtility.IconContent(string.IsNullOrEmpty(stepCfg.uiOpParam) ? "GUISystem/align_vertically_center" : "GUISystem/align_vertically_center_active", "查看路径"), EditorStyleEx.OLTitleAlignCenter))
                        {
                            MessageBoxWindow.ShowAsInputBox(stepCfg.uiOpParam, "修改UI路径", i, (str, cxt) =>
                            {
                                var index = (int)cxt;
                                if (str != stepList[index].uiOpParam)
                                {
                                    stepList[index].uiOpParam = str;
                                    curConfig.IsDirty = true;
                                    Repaint();
                                }
                            }, (str, cxt) => { });
                        }
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 100 + cellRect.height * 1.5f;
                        var uiOpParam2 = EditorGUI.TextField(cellRect, stepCfg.uiOpParam2);
                        if (uiOpParam2 != stepCfg.uiOpParam2)
                        {
                            stepCfg.uiOpParam2 = uiOpParam2;
                            curConfig.IsDirty = true;
                        }
                        ////////////////////////////
                    }
                    break;
                default:
                    {
                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 100;
                        var oldRectTrans = string.IsNullOrEmpty(stepCfg.uiOpParam) ? null : UITeach.FindRectTransform(stepCfg.uiOpParam);
                        var newRectTrans = (RectTransform)EditorGUI.ObjectField(cellRect, oldRectTrans, typeof(RectTransform), true);
                        if (newRectTrans != oldRectTrans)
                        {
                            stepCfg.uiOpParam = newRectTrans == null ? "" : Util.GetGameObjectPath(newRectTrans.gameObject);
                            curConfig.IsDirty = true;
                        }
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax;
                        cellRect.width = cellRect.height * 1.5f;
                        if (GUI.Button(cellRect, EditorGUIUtility.IconContent(string.IsNullOrEmpty(stepCfg.uiOpParam) ? "GUISystem/align_vertically_center" : "GUISystem/align_vertically_center_active", "查看路径"), EditorStyleEx.OLTitleAlignCenter))
                        {
                            MessageBoxWindow.ShowAsInputBox(stepCfg.uiOpParam, "修改UI路径", i, (str, cxt) =>
                            {
                                var index = (int)cxt;
                                if (str != stepList[index].uiOpParam)
                                {
                                    stepList[index].uiOpParam = str;
                                    curConfig.IsDirty = true;
                                    Repaint();
                                }
                            }, (str, cxt) => { });
                        }
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 100;
                        oldRectTrans = string.IsNullOrEmpty(stepCfg.uiOpParam2) ? null : UITeach.FindRectTransform(stepCfg.uiOpParam2);
                        newRectTrans = (RectTransform)EditorGUI.ObjectField(cellRect, oldRectTrans, typeof(RectTransform), true);
                        if (newRectTrans != oldRectTrans)
                        {
                            stepCfg.uiOpParam2 = newRectTrans == null ? "" : Util.GetGameObjectPath(newRectTrans.gameObject);
                            curConfig.IsDirty = true;
                        }
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax;
                        cellRect.width = cellRect.height * 1.5f;
                        if (GUI.Button(cellRect, EditorGUIUtility.IconContent(string.IsNullOrEmpty(stepCfg.uiOpParam2) ? "GUISystem/align_vertically_center" : "GUISystem/align_vertically_center_active", "查看路径"), EditorStyleEx.OLTitleAlignCenter))
                        {
                            MessageBoxWindow.ShowAsInputBox(stepCfg.uiOpParam2, "修改UI路径", i, (str, cxt) =>
                            {
                                var index = (int)cxt;
                                if (str != stepList[index].uiOpParam2)
                                {
                                    stepList[index].uiOpParam2 = str;
                                    curConfig.IsDirty = true;
                                    Repaint();
                                }
                            }, (str, cxt) => { });
                        }
                        ////////////////////////////
                    }
                    break;
            }
            ////////////////////////////

            ////////////////////////////
            cellRect.x = cellRect.xMax + padSize.x;
            cellRect.width = cellRect.height * 4;
            var sceneOpStrs = Util.GetEnumAllDesc<TeachSceneOpType>();
            var disableSceneOpType = NeedDisableSceneOpType(stepCfg);
            using (new AutoEditorDisabledGroup(disableSceneOpType))
            {
                var sceneOpType = (TeachSceneOpType)EditorGUI.Popup(cellRect, (int)stepCfg.sceneOpType, sceneOpStrs);
                if (sceneOpType != stepCfg.sceneOpType)
                {
                    stepCfg.sceneOpType = sceneOpType;
                    curConfig.IsDirty = true;

                    if (sceneOpType != TeachSceneOpType.none)
                    {
                        if (stepCfg.sceneGuideType == TeachSceneGuideType.none)
                            stepCfg.sceneGuideType = TeachSceneGuideType.fullPathGuide;
                        if (stepCfg.stepObj == TeachStepObj.none)
                            stepCfg.stepObj = TeachStepObj.sceneOp;
                    }
                }
            }
            ////////////////////////////

            ////////////////////////////
            cellRect.x = cellRect.xMax + padSize.x;
            cellRect.width = cellRect.height * 3;
            var sceneGuideStrs = Util.GetEnumAllDesc<TeachSceneGuideType>();
            var sceneGuideType = (TeachSceneGuideType)EditorGUI.Popup(cellRect, (int)stepCfg.sceneGuideType, sceneGuideStrs);
            if (sceneGuideType != stepCfg.sceneGuideType)
            {
                stepCfg.sceneGuideType = sceneGuideType;
                curConfig.IsDirty = true;
            }
            ////////////////////////////

            ////////////////////////////
            switch (stepCfg.sceneOpType)
            {
                case TeachSceneOpType.movePos:
                    {
                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 160;
                        var oldPos = Vector3.zero;
                        StringUtil.TryParse(stepCfg.sceneOpParam, out oldPos);
                        var newPos = (Vector3)EditorGUI.Vector3Field(cellRect, GUIContent.none, oldPos);
                        if (oldPos != newPos)
                        {
                            stepCfg.sceneOpParam = string.Format("{0},{1},{2}", newPos.x, newPos.y, newPos.z);
                            curConfig.IsDirty = true;
                        }
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 12;
                        EditorGUI.LabelField(cellRect, "R");
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 42;
                        var oldRadius = StringUtil.ToFloat(stepCfg.sceneOpParam2);
                        var newRadius = EditorGUI.FloatField(cellRect, GUIContent.none, oldRadius);
                        if (newRadius != oldRadius)
                        {
                            stepCfg.sceneOpParam2 = newRadius.ToString();
                            curConfig.IsDirty = true;
                        }
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = cellRect.height * 1.5f;
                        if (GUI.Button(cellRect, EditorGUIUtility.IconContent("AvatarInspector/DotFill", "获取主角坐标"), EditorStyleEx.OLTitleAlignCenter))
                        {
                            var hero = RoleMgr.instance.Hero;
                            if (hero == null || hero.State != Role.enState.alive)
                            {
                                ShowNotice("主角暂时不存在或不在场景里");
                            }
                            else
                            {
                                newPos = hero.TranPart.Pos;
                                if (oldPos != newPos)
                                {
                                    stepCfg.sceneOpParam = string.Format("{0},{1},{2}", newPos.x, newPos.y, newPos.z);
                                    curConfig.IsDirty = true;
                                }
                            }
                        }
                        ////////////////////////////
                    }
                    break;
                case TeachSceneOpType.fireSceneAction:
                    {
                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 50;
                        EditorGUI.LabelField(cellRect, "事件名：");
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 150 + cellRect.height * 1.5f * 2;
                        var newEventName = EditorGUI.TextField(cellRect, stepCfg.sceneOpParam);
                        if (newEventName != stepCfg.sceneOpParam)
                        {
                            stepCfg.sceneOpParam = newEventName;
                            curConfig.IsDirty = true;
                        }
                        ////////////////////////////
                    }
                    break;
                case TeachSceneOpType.enterScene:
                    {
                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 50;
                        EditorGUI.LabelField(cellRect, "副本名：");
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 150 + cellRect.height * 1.5f * 2;
                        using (new AutoWarningColor(!string.IsNullOrEmpty(stepCfg.sceneOpParam) && RoomCfg.GetRoomCfgByID(stepCfg.sceneOpParam) == null))
                        {
                            var newRoomId = EditorGUI.TextField(cellRect, stepCfg.sceneOpParam);
                            if (newRoomId != stepCfg.sceneOpParam)
                            {
                                stepCfg.sceneOpParam = newRoomId;
                                curConfig.IsDirty = true;
                            }
                        }                            
                        ////////////////////////////
                    }
                    break;
                case TeachSceneOpType.teachData:
                    {
                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 100 + cellRect.height * 1.5f;
                        using (new AutoWarningColor(!string.IsNullOrEmpty(stepCfg.sceneOpParam) && !TeachConfig.IsTeachKeyOK(stepCfg.sceneOpParam)))
                        {                     
                            var sceneOpParam = EditorGUI.TextField(cellRect, stepCfg.sceneOpParam);
                            if (sceneOpParam != stepCfg.sceneOpParam)
                            {
                                stepCfg.sceneOpParam = sceneOpParam;
                                curConfig.IsDirty = true;
                            }
                        }
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 100 + cellRect.height * 1.5f;
                        var oldNum = StringUtil.ToInt(stepCfg.sceneOpParam2);
                        var newNum = EditorGUI.IntField(cellRect, oldNum);
                        if (oldNum != newNum)
                        {
                            stepCfg.sceneOpParam2 = newNum.ToString();
                            curConfig.IsDirty = true;
                        }
                        ////////////////////////////
                    }
                    break;
                case TeachSceneOpType.enableHeroAI:
                    {
                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 100;
                        var oldVal = StringUtil.ToInt(stepCfg.sceneOpParam);
                        var newVal = EditorGUI.Popup(cellRect, oldVal, new string[] { "不开启", "开启" });
                        if (newVal != oldVal)
                        {
                            stepCfg.sceneOpParam = newVal.ToString();
                            curConfig.IsDirty = true;
                        }
                        ////////////////////////////

                        ////////////////////////////
                        cellRect.width = 200 + cellRect.height * 1.5f * 2 + padSize.x;
                        ////////////////////////////
                    }
                    break;
                default:
                    {
                        ////////////////////////////
                        cellRect.x = cellRect.xMax + padSize.x;
                        cellRect.width = 200 + cellRect.height * 1.5f * 2 + padSize.x;
                        ////////////////////////////
                    }
                    break;
            }
            ////////////////////////////

            ////////////////////////////
            var skipTypeStrs = Util.GetEnumAllDesc<TeachStepSkipType>();
            var disableSkipType = NeedDisableSkipType(stepCfg);
            using (new AutoEditorDisabledGroup(disableSkipType))
            {
                cellRect.x = cellRect.xMax + padSize.x;
                cellRect.width = cellRect.height * 4;

                var stepSkipType = (TeachStepSkipType)EditorGUI.Popup(cellRect, (int)stepCfg.stepSkipType, skipTypeStrs);
                if (stepSkipType != stepCfg.stepSkipType)
                {
                    stepCfg.stepSkipType = stepSkipType;
                    curConfig.IsDirty = true;
                }

                cellRect.x = cellRect.xMax;
                cellRect.width = cellRect.height * 2.5f;

                var stopIfNeedSkipNum = EditorGUI.Popup(cellRect, stepCfg.stopIfNeedSkip ? 1 : 0, new string[] { "后续", "停止" });
                var stopIfNeedSkip = stopIfNeedSkipNum != 0 ? true : false;
                if (stopIfNeedSkip != stepCfg.stopIfNeedSkip)
                {
                    stepCfg.stopIfNeedSkip = stopIfNeedSkip;
                    curConfig.IsDirty = true;
                }
            }
            ////////////////////////////

            ////////////////////////////
            if (stepCfg.stepSkipType == TeachStepSkipType.none)
            {
                cellRect.x = cellRect.xMax + padSize.x;
                cellRect.width = 150 + cellRect.height * 1.5f + padSize.x;
            }
            else if (stepCfg.stepSkipType == TeachStepSkipType.teachData)
            {
                cellRect.x = cellRect.xMax + padSize.x;
                cellRect.width = 50 + cellRect.height * 1.5f + padSize.x;

                var newKey = EditorGUI.TextField(cellRect, stepCfg.skipParam);
                if (newKey != stepCfg.skipParam)
                {
                    stepCfg.skipParam = newKey;
                    curConfig.IsDirty = true;
                }

                var arr = string.IsNullOrEmpty(stepCfg.skipParam2) ? new string[] { "", "" } : stepCfg.skipParam2.Split(new char[] { ',' });
                var opType = arr.Length >= 1 ? StringUtil.ToInt(arr[0]) : 0;
                var value = arr.Length >= 2 ? StringUtil.ToInt(arr[1]) : 0;

                cellRect.x = cellRect.xMax;
                cellRect.width = 50;

                var newOpType = EditorGUI.Popup(cellRect, opType, new string[] { "等于", "不等于" });
                if (newOpType != opType)
                {
                    opType = newOpType;
                    stepCfg.skipParam2 = opType + "," + value;
                    curConfig.IsDirty = true;
                }

                cellRect.x = cellRect.xMax;
                cellRect.width = 50;

                var newVal = EditorGUI.IntField(cellRect, value);
                if (newVal != value)
                {
                    value = newVal;
                    stepCfg.skipParam2 = opType + "," + value;
                    curConfig.IsDirty = true;
                }
            }
            else if (stepCfg.stepSkipType == TeachStepSkipType.uiPanelFunc)
            {
                ////////////////////////////
                cellRect.x = cellRect.xMax + padSize.x;
                cellRect.width = 80;
                var oldObj = UITeach.FindUIPanel(stepCfg.skipParam);
                using (new AutoWarningColor(oldObj == null || !TeachMgr.HasTeachCheckFunc(oldObj.GetComponent<UIPanel>())))
                {
                    var newObj = (UIPanel)EditorGUI.ObjectField(cellRect, oldObj, typeof(UIPanel), true);
                    if (newObj != oldObj)
                    {
                        stepCfg.skipParam = newObj == null ? "" : Util.GetGameObjectPath(newObj.gameObject);
                        curConfig.IsDirty = true;

                        if (newObj != null)
                        {
                            if (!TeachMgr.HasTeachCheckFunc(newObj.GetComponent<UIPanel>()))
                            {
                                ShowNotice("该UIPanel没有带有一个string参数并返回bool的实例方法" + TeachMgr.TEACH_CHECK_FUNC);
                            }
                        }
                    }
                }
                ////////////////////////////

                ////////////////////////////
                cellRect.x = cellRect.xMax;
                cellRect.width = cellRect.height * 1.5f;
                if (GUI.Button(cellRect, EditorGUIUtility.IconContent(string.IsNullOrEmpty(stepCfg.skipParam) ? "GUISystem/align_vertically_center" : "GUISystem/align_vertically_center_active", "查看路径"), EditorStyleEx.MiniButtonEx))
                {
                    MessageBoxWindow.ShowAsInputBox(stepCfg.skipParam, "修改UI路径", (str, cxt) =>
                    {
                        if (str != stepCfg.skipParam)
                        {
                            stepCfg.skipParam = str;
                            curConfig.IsDirty = true;
                            Repaint();
                        }
                    }, (str, cxt) => { });
                }
                ////////////////////////////

                ////////////////////////////
                cellRect.x = cellRect.xMax + padSize.x;
                cellRect.width = 70;
                var skipParam2 = EditorGUI.TextField(cellRect, stepCfg.skipParam2);
                if (skipParam2 != stepCfg.skipParam2)
                {
                    stepCfg.skipParam2 = skipParam2;
                    curConfig.IsDirty = true;
                }
                ////////////////////////////
            }
            else
            {
                ////////////////////////////
                cellRect.x = cellRect.xMax + padSize.x;
                cellRect.width = 100;
                switch (stepCfg.stepSkipType)
                {
                    case TeachStepSkipType.stateHandleState:
                        {
                            var oldObj = UITeach.FindStateHandle(stepCfg.skipParam);
                            using (new AutoWarningColor(oldObj == null))
                            {
                                var newObj = (StateHandle)EditorGUI.ObjectField(cellRect, oldObj, typeof(StateHandle), true);
                                if (newObj != oldObj)
                                {
                                    stepCfg.skipParam = newObj == null ? "" : Util.GetGameObjectPath(newObj.gameObject);
                                    curConfig.IsDirty = true;
                                }
                            }
                        }
                        break;
                    case TeachStepSkipType.stateGroupCurSel:
                        {
                            var oldObj = UITeach.FindStateGroup(stepCfg.skipParam);
                            using (new AutoWarningColor(oldObj == null))
                            {
                                var newObj = (StateGroup)EditorGUI.ObjectField(cellRect, oldObj, typeof(StateGroup), true);
                                if (newObj != oldObj)
                                {
                                    stepCfg.skipParam = newObj == null ? "" : Util.GetGameObjectPath(newObj.gameObject);
                                    curConfig.IsDirty = true;
                                }
                            }
                        }
                        break;
                    case TeachStepSkipType.uiPanelOpenTop:
                    case TeachStepSkipType.uiPanelOpen:
                        {
                            var oldObj = UITeach.FindUIPanel(stepCfg.skipParam);
                            using (new AutoWarningColor(oldObj == null))
                            {
                                var newObj = (UIPanel)EditorGUI.ObjectField(cellRect, oldObj, typeof(UIPanel), true);
                                if (newObj != oldObj)
                                {
                                    stepCfg.skipParam = newObj == null ? "" : Util.GetGameObjectPath(newObj.gameObject);
                                    curConfig.IsDirty = true;
                                }
                            }
                        }
                        break;
                    default:
                        {
                            var oldObj = UITeach.FindRectTransform(stepCfg.skipParam);
                            using (new AutoWarningColor(oldObj == null))
                            {
                                var newObj = (RectTransform)EditorGUI.ObjectField(cellRect, oldObj, typeof(RectTransform), true);
                                if (newObj != oldObj)
                                {
                                    stepCfg.skipParam = newObj == null ? "" : Util.GetGameObjectPath(newObj.gameObject);
                                    curConfig.IsDirty = true;
                                }
                            }
                        }
                        break;
                }
                ////////////////////////////

                ////////////////////////////
                cellRect.x = cellRect.xMax;
                cellRect.width = cellRect.height * 1.5f;
                if (GUI.Button(cellRect, EditorGUIUtility.IconContent(string.IsNullOrEmpty(stepCfg.skipParam) ? "GUISystem/align_vertically_center" : "GUISystem/align_vertically_center_active", "查看路径"), EditorStyleEx.OLTitleAlignCenter))
                {
                    MessageBoxWindow.ShowAsInputBox(stepCfg.skipParam, "修改UI路径", i, (str, cxt) =>
                    {
                        var index = (int)cxt;
                        if (str != stepList[index].skipParam)
                        {
                            stepList[index].skipParam = str;
                            curConfig.IsDirty = true;
                            Repaint();
                        }
                    }, (str, cxt) => { });
                }
                ////////////////////////////

                ////////////////////////////
                cellRect.x = cellRect.xMax + padSize.x;
                cellRect.width = 50;
                switch (stepCfg.stepSkipType)
                {
                    case TeachStepSkipType.stateHandleState:
                    case TeachStepSkipType.stateGroupCurSel:
                        {
                            var oldNum = StringUtil.ToInt(stepCfg.skipParam2);
                            var newNum = EditorGUI.IntField(cellRect, oldNum);
                            if (newNum != oldNum)
                            {
                                stepCfg.skipParam2 = newNum.ToString();
                                curConfig.IsDirty = true;
                            }
                        }
                        break;
                }
                ////////////////////////////
            }
            ////////////////////////////

            ////////////////////////////
            //这个放在最后面是因为防止点击里面的按钮什么的也导致行被选中，感觉不好
            if (rowRect.Contains(curEvent.mousePosition))
            {
                if (curEvent.type == EventType.MouseDown)
                {
                    m_curSelStep = i;
                    Repaint();
                }
                else if (curEvent.type == EventType.ContextClick)
                {
                    DrawStepContextMenu();
                }
            }
            ////////////////////////////
        }

        GUILayoutUtility.GetRect(Mathf.Max(rowRect.xMax, cellRect.xMax), rowRect.yMax);
    }

    private void DrawStepContextMenu()
    {
        Event.current.Use();
        var cxtMenu = new GenericMenu();
        cxtMenu.AddItem(new GUIContent("插入新步骤到行前"), false, OnStepContextMenuInsert, 0);
        cxtMenu.AddItem(new GUIContent("插入新步骤到行后"), false, OnStepContextMenuInsert, 1);
        cxtMenu.AddSeparator(string.Empty);
        cxtMenu.AddItem(new GUIContent("删除本步骤"), false, OnStepContextMenuDelete);
        cxtMenu.AddSeparator(string.Empty);
        cxtMenu.AddItem(new GUIContent("上移本步骤"), false, OnStepContextMenuMoveUp);
        cxtMenu.AddItem(new GUIContent("下移本步骤"), false, OnStepContextMenuMoveDown);
        cxtMenu.AddSeparator(string.Empty);
        cxtMenu.AddItem(new GUIContent("从本步骤插入录制"), false, OnStepContextMenuRecord, m_curSelStep);
        cxtMenu.AddItem(new GUIContent("从本步骤追加录制"), false, OnStepContextMenuRecord, m_curSelStep + 1);
        cxtMenu.AddSeparator(string.Empty);
        cxtMenu.AddItem(new GUIContent("从本步骤开始播放"), false, OnStepContextMenuPlay, m_curSelStep);
        cxtMenu.AddSeparator(string.Empty);
        cxtMenu.AddItem(new GUIContent("复制本步骤数据"), false, OnStepContextMenuCopy);
        if (!string.IsNullOrEmpty(m_copyStepJsonStr))
        {
            cxtMenu.AddItem(new GUIContent("粘贴覆盖本步骤"), false, OnStepContextMenuPasteOverwrite);
            cxtMenu.AddItem(new GUIContent("粘贴到本步骤前"), false, OnStepContextMenuPasteInsert, 0);
            cxtMenu.AddItem(new GUIContent("粘贴到本步骤后"), false, OnStepContextMenuPasteInsert, 1);
        }            
        else
        {
            cxtMenu.AddDisabledItem(new GUIContent("粘贴覆盖本步骤"));
            cxtMenu.AddDisabledItem(new GUIContent("粘贴到本步骤前"));
            cxtMenu.AddDisabledItem(new GUIContent("粘贴到本步骤后"));
        }
        cxtMenu.ShowAsContext();
    }

    private void DrawRightPanel()
    {
        using (new AutoBeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true)))
        {
            DrawRightToolBar();
        }

        if (GetCurConfig() == null)
        {
            EditorGUILayout.HelpBox("选择一个引导才可显示其参数", MessageType.Warning);
        }
        else
        {

            using (new AutoEditorDisabledGroup(m_teachMgr.PlayNow || m_teachMgr.RecordNow))
            {
                using (new AutoBeginVertical(EditorStyles.helpBox))
                {                
                    DrawTeachParam();
                }
            }

            using (new AutoBeginVertical(EditorStyles.helpBox))
            {
                using (new AutoEditorDisabledGroup(m_teachMgr.PlayNow || m_teachMgr.RecordNow))
                {
                    using (new AutoBeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true)))
                    {
                        DrawTeachStepToolBar();
                    }
                
                    var lastRect = GUILayoutUtility.GetLastRect();
                    m_lastRightWidth = lastRect.width > 1 ? lastRect.width : m_lastRightWidth;
                
                    DrawSelStepDesc();
                }

                using (AutoBeginScrollView scrollView = new AutoBeginScrollView(new Vector2(m_stepScrollPos.x, 0), GUIStyle.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(18)))
                {
                    using (new AutoEditorDisabledGroup(m_teachMgr.PlayNow || m_teachMgr.RecordNow))
                    {
                        DrawStepHeader();
                    }
                }

                using (AutoBeginScrollView scrollView = new AutoBeginScrollView(m_stepScrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                {
                    m_stepScrollPos = scrollView.Scroll;

                    using (new AutoEditorDisabledGroup(m_teachMgr.PlayNow || m_teachMgr.RecordNow))
                    {
                        DrawTeachSteps();
                    }
                }
            }
        }
    }

    void OnGUI()
    {
        if (m_teachMgr == null)
        {
            if ((m_teachMgr = TeachMgr.instance) == null)
            {
                EditorGUILayout.HelpBox("必须游戏运行并初始化完后才能正常使用", MessageType.Warning);
                return;
            }
            else
            {
                m_teachMgr.EditorCallback += CallbackForGameUI;
            }
        }

        if (m_configs == null)
        {
            if (TeachConfig.Configs == null)
            {
                EditorGUILayout.HelpBox("必须游戏运行并初始化完后才能正常使用", MessageType.Warning);
                return;
            }
            else
            {
                m_configs = TeachConfig.Configs;
                SelectCurrentConfig();
            }
        }

        using (new AutoBeginHorizontal())
        {
            using (new AutoBeginVertical("PreferencesSectionBox", GUILayout.Width(180)))
            {
                DrawLeftPanel();
            }

            using (new AutoBeginVertical("PreferencesSectionBox"))
            {
                using (new AutoEditorDisabledGroup(GetCurConfig() == null))
                {
                    DrawRightPanel();
                }
            }
        }
    }

    void Awake()
    {
        if (TeachMgr.instance == null)
            return;

        m_teachMgr = TeachMgr.instance;
        m_teachMgr.EditorCallback += CallbackForGameUI;
        m_configs = TeachConfig.Configs;
        SelectCurrentConfig();
    }

    void OnDestroy()
    {
        if (m_teachMgr == null)
            return;

        m_teachMgr.StartRecord(false);
        //m_teachMgr.StartPlay(false);
        m_teachMgr.EditorCallback -= CallbackForGameUI;
        m_teachMgr = null;
        m_configs = null;
    }
}