using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;


//事件组编辑器
public class EventGroupEditor : MultiWindow
{
    public const int COL_WIDTH = 20;
    public const int COL_SPACE = 5;

    static GUIStyle rowStyle ;
    static GUIStyle rowStyle2 ;
    static GUIStyle headStyle ;
    SkillEventGroupCfg m_group;
    int m_observer;
    Transform m_curTran;
    string m_flyerId;
    Role m_grabRole;
    List<Flyer> m_flyers=new List<Flyer>();
    bool m_expand;
    public override void OnEnable()
    {
        m_observer = EventMgr.AddAll(MSG.MSG_FRAME, MSG_FRAME.FRAME_DRAW_GL, OnDrawGL);
    }

    public override void OnDisable()
    {
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
    }
    void OnDrawGL(object obj){
    
        if (m_group == null ) return ;
        DrawGL draw = (DrawGL)obj;

        RoleModel model = m_curTran == null ? null : m_curTran.GetComponent<RoleModel>();
        if (model != null)//当前是角色的话
        {
            if(model.Parent == null) return;
            TranPart tranPart = model.TranPart;
            if (tranPart == null) return ;
            

            int totalRow = 0;
            int maxFrameRow;

            Vector3 dir = m_curTran.forward;
            Vector3 pos = tranPart.Pos;
            Role r = model == null ? null : model.Parent;
            CombatPart combatPart = r != null && r.State == Role.enState.alive ? r.CombatPart : null;
            SkillEventGroup g = combatPart != null && combatPart.CurSkill != null && combatPart.CurSkill.EventGroup.Cfg == m_group ? combatPart.CurSkill.EventGroup : null;
            List<SkillEventFrame> frames = g == null ? null : g.Frames;
            SkillEventFrame f;
            SkillEventFrameCfg c;
            for (int i = 0; i < m_group.frames.Count; ++i)
            {
                c = m_group.frames[i];
                f = frames != null && i < frames.Count && frames[i].Cfg == c ? frames[i] : null;


                maxFrameRow = Mathf.Max(1, c.conditions.Count, c.targetConditions.Count, c.targetRanges.Count, c.events.Count);
                for (int row = 0; row < maxFrameRow; ++row)
                {
                    TargetRangeCfg targetCfg = row < c.targetRanges.Count ? c.targetRanges[row] : null;
                    if (targetCfg != null && targetCfg.targetType != enSkillEventTargetType.selfAlway)
                    {
                        CollideUtil.Draw(targetCfg.range, draw, pos, dir, Color.HSVToRGB(((30f * totalRow + 70) % 360f) / 360f, 1, 1), f == null ? 0 : f.Factor);
                    }
                    ++totalRow;
                }
            }
        }
        else if (m_grabRole != null)//战斗管理器托管的事件组或者抓取事件组
        {
            Role target =m_grabRole.GetGrabTarget();
            if(target == null)return;
            TranPart tranPart = target.TranPart;
            RoleSubBeGrab grabState = target.RSM.StateBehit.CurState as RoleSubBeGrab;
            if(grabState == null)return;
            SkillEventGroup g = grabState.EventGroup;
            List<SkillEventFrame> frames = g == null ? null : g.Frames;

            int totalRow = 0;
            int maxFrameRow;
            Vector3 dir = target.transform.forward;
            Vector3 pos = tranPart.Pos;
            
            SkillEventFrame f;
            SkillEventFrameCfg c;
            for (int i = 0; i < m_group.frames.Count; ++i)
            {
                c = m_group.frames[i];
                f = frames != null && i < frames.Count && frames[i].Cfg == c ? frames[i] : null;


                maxFrameRow = Mathf.Max(1, c.conditions.Count, c.targetConditions.Count, c.targetRanges.Count, c.events.Count);
                for (int row = 0; row < maxFrameRow; ++row)
                {
                    TargetRangeCfg targetCfg = row < c.targetRanges.Count ? c.targetRanges[row] : null;
                    if (targetCfg != null && targetCfg.targetType != enSkillEventTargetType.selfAlway)
                    {
                        CollideUtil.Draw(targetCfg.range, draw, pos, dir, Color.HSVToRGB(((30f * totalRow + 70) % 360f) / 360f, 1, 1), f == null ? 0 : f.Factor);
                    }
                    ++totalRow;
                }
            }
        }
        else if(!string.IsNullOrEmpty(m_flyerId))//当前是飞出物的话
        {
            CombatMgr.instance.GetFlyers(m_flyerId,ref m_flyers);
            foreach (Flyer flyer in m_flyers)
            {
                Vector3 dir = flyer.Root.forward;
                Vector3 pos = flyer.Root.position;
                SkillEventGroup g = flyer.EventGroup;
                List<SkillEventFrame> frames = g == null ? null : g.Frames;
                SkillEventFrame f;
                SkillEventFrameCfg c;
                int totalRow = 0;
                int maxFrameRow;
                for (int i = 0; i < m_group.frames.Count; ++i)
                {
                    c = m_group.frames[i];
                    f = frames != null && i < frames.Count && frames[i].Cfg == c ? frames[i] : null;


                    maxFrameRow = Mathf.Max(1, c.conditions.Count, c.targetConditions.Count, c.targetRanges.Count, c.events.Count);
                    for (int row = 0; row < maxFrameRow; ++row)
                    {
                        TargetRangeCfg targetCfg = row < c.targetRanges.Count ? c.targetRanges[row] : null;
                        if (targetCfg != null && targetCfg.targetType != enSkillEventTargetType.selfAlway)
                        {
                            CollideUtil.Draw(targetCfg.range, draw, pos, dir, Color.HSVToRGB(((30f * totalRow + 70) % 360f) / 360f, 1, 1), f == null ? 0 : f.Factor);
                        }
                        ++totalRow;
                    }
                }
            }
        }
    }

    //设置抓取者
    public void SetGrabRole(Role grabRole)
    {
        m_grabRole= grabRole;
        m_flyerId = string.Empty;
        m_curTran = null;
    }

    public void SetFlyerId(string flyerId)
    {
        m_grabRole = null;
        m_flyerId = flyerId;
        m_curTran = null;
    }

    public void SetTran(Transform t)
    {
        m_grabRole = null;
        m_flyerId = string.Empty;
        m_curTran =t;
        
    }

    public void SetGroup(SkillEventGroupCfg g)
    {
        m_group=g;
        
    }


    //绘制窗口时调用
    public override void OnGUI()
    {
        //如果切换场景之类的，角色被回收了，那么要清空角色
        if (m_grabRole != null && (m_grabRole.State == Role.enState.init))
        {
            m_grabRole = null;
        }

        //工具栏，选择事件组
        using (new AutoBeginHorizontal(EditorStyles.toolbarButton))
        {
            DrawToolBar();
        }

        if(m_group== null)return ;
       
        
        Draw();
    }


    void DrawToolBar()
    {
        //选择事件组
        int idx = m_group == null ? -1 : Array.IndexOf(SkillEventGroupCfg.GroupIds, m_group.file);
        int newIdx = EditorGUILayout.Popup(idx, SkillEventGroupCfg.GroupIds, EditorStyles.toolbarPopup, GUILayout.Width(150));
        int len = SkillEventGroupCfg.GroupIds.Length;
        if (newIdx != -1 && idx != newIdx && newIdx != len - 2)
        {
            if (m_group != null && EditorUtility.DisplayDialog("", "是否保存" + m_group.file + "的修改?", "保存", "否"))
                m_group.Save();
            else
            {
                // SkillEventGroupCfg.RemoveCache(m_group);//FIX:如果加了这一行会导致下次切过来的时候编辑的不是当前活动的配置
                SetGroup(null);
            }

            if (newIdx == len - 1)
            {
                MessageBoxWindow.ShowAsInputBox(string.Format("event_group_{0:00000}", SkillCfgMgr.instance.eventGroups.Count+1), (string groupName, object context) =>
                {                    
                    SkillEventGroupCfg c = SkillEventGroupCfg.Add(groupName);
                    if (c ==  null)
                        return;
                    SetGroup(c);
                }, (string groupName, object context) => { });
            }
            else
                SetGroup(SkillEventGroupCfg.Get(SkillEventGroupCfg.GroupIds[newIdx]));
        }


        if (m_group==null)
            return;

        //保存
        if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Duplicate"), EditorStyles.toolbarButton, GUILayout.Width(30)))
        {
            m_group.Save();
            adapterWindow.ShowNotification(new GUIContent("保存成功"));
            
        }

        //重载
        if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Refresh"), EditorStyles.toolbarButton, GUILayout.Width(30)))
        {
            if (EditorUtility.DisplayDialog("", "是否确定要重载当前事件表，所有未保存的修改都将还原?", "是", "否"))
            {
                string id = m_group.file;
                SkillEventGroupCfg.RemoveCache(m_group);
                SetGroup(SkillEventGroupCfg.Get(id));
            }
        }
        //删除
        if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyles.toolbarButton, GUILayout.Width(30)))
        {
            if (EditorUtility.DisplayDialog("", "是否确定要删除事件组?", "是", "否"))
            {
                SkillEventGroupCfg.Remove(m_group);
                SetGroup(null);
                
            }
        }

        //画事件组id和当前拥有者
        EditorGUILayout.TextField(GUIContent.none, m_group.file, EditorStyles.toolbarTextField, GUILayout.Width(200));
        m_curTran = (Transform)EditorGUILayout.ObjectField(GUIContent.none, m_curTran, typeof(Transform), true, GUILayout.Width(100));
        
    }

    

    public static void CheckStyle()
    {
        if (rowStyle == null) rowStyle = "OL EntryBackEven";
        if (rowStyle2 == null) rowStyle2 = "OL EntryBackOdd";
        if (headStyle == null) headStyle = "OL title";
    }

    public void Draw()
    {
        if(m_group==null)
            return;
        Event current = Event.current;
        EventType eventType = current.type;
        CheckStyle();

        Rect rHeader = new Rect(0, 0, 0, 20);
        //GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar

        GUILayout.BeginScrollView(new Vector2(m_group.contentScroll.x, 0), GUIStyle.none,GUIStyle.none ,GUILayout.Height(20));
            //画下列头
            SkillEventFrameCfg selCfg = m_group.GetById(m_group.selId);
            
            //没有的话先画空的列头替代
            if (selCfg == null)
            {
                rHeader.width =  COL_WIDTH;
                if (GUI.Button(rHeader, EditorGUIUtility.IconContent("Toolbar Plus More"), headStyle))
                {
                    GenericMenu genericMenu = new GenericMenu();
                    for (int i = 0; i < SkillEventFactory.TypeName.Length; ++i)
                    {
                        int type = i;
                        genericMenu.AddItem(new GUIContent(SkillEventFactory.TypeName[i]), false, () =>
                        {
                            m_group.AddFrame((enSkillEventType)type);
                        });
                    }
                    genericMenu.ShowAsContext();
                }
                rHeader.x+=rHeader.width;

                for (int i = 0; i < 20; ++i)
                {
                    rHeader.width = COL_WIDTH*2;
                    GUI.Box(rHeader,GUIContent.none, headStyle);
                    rHeader.x += rHeader.width;
                }
                rHeader.width = rHeader.x;
                rHeader.x =0;
            }
            else
            {
                //事件帧的列头
                bool end = false;
                int idx = 0;
                do
                {
                    end = DrawFrameHeader(ref rHeader, selCfg, m_group, idx, MessageBoxWindow.ShowMessageBox);
                    ++idx;
                } while (!end);

                //事件的列头
                SkillEventCfg selEventCfg = selCfg.GetById(selCfg.selId);
                if (selEventCfg != null)
                {
                    end = false;
                    idx = 0;
                    do
                    {
                        end = selEventCfg.DrawHeader(ref rHeader, selCfg, m_group, idx, MessageBoxWindow.ShowMessageBox, HeaderButton);  
                        ++idx;
                    } while (!end);
                }

                rHeader.width = rHeader.x;
                rHeader.x = 0;
            }
            m_group.maxWidth = Mathf.Max(m_group.maxWidth,rHeader.width);
            GUILayoutUtility.GetRect(m_group.maxWidth, 20);
            
        GUILayout.EndScrollView();

        //绘制所有帧事件
        m_group.contentScroll = GUILayout.BeginScrollView(m_group.contentScroll);
            Rect r = new Rect(0, 0, rHeader.width,22);
            int totalRow = 0;
            int maxFrameRow;
            bool rowEnd = false;
            int col = 0;
            
            GUIStyle s;
            SkillEventCfg eventCfg;
            float oldX;
            bool change=false;
            float maxWidth = 0;
            foreach (SkillEventFrameCfg c in m_group.frames)
            {
                maxFrameRow = Mathf.Max(1, c.conditions.Count, c.targetConditions.Count, c.targetRanges.Count, c.events.Count);
                for (int row = 0; row < maxFrameRow; ++row)
                {
                    eventCfg = row < c.events.Count ? c.events[row] : null;
                    //r = GUILayoutUtility.GetRect(rHeader.width, 22,GUILayout.ExpandWidth(true));
                    s = totalRow % 2 == 0 ? rowStyle : rowStyle2;
                    if (eventType == EventType.Repaint)
                        s.Draw(r, GUIContent.none, false, false, eventCfg != null && m_group.selId == c.id && c.selId == eventCfg.id, false);

                    r.y += 3;
                    r.height = 16;
                    //画帧相关的列
                    col = 0;
                    rowEnd = false;
                    do
                    {
                        oldX = r.x;
                        rowEnd = DrawFrameGrid(ref r, eventCfg, c, m_group, row, col, totalRow, ref change);
                        if (change) goto Label_Break;

                        if (r.x != oldX)
                            r.x+=COL_SPACE;
                        ++col;
                    } while (!rowEnd);

                    //画事件相关的列
                    if (eventCfg != null)
                    {
                        col = 0;
                        rowEnd = false;
                        do
                        {
                            oldX = r.x;
                            enSkillEventEditorMsg msg = enSkillEventEditorMsg.none;
                            rowEnd = eventCfg.DrawGrid(ref r,  c, m_group, row, col , totalRow,ref change, m_curTran);
                            if (change) goto Label_Break;
                          

                            if (r.x != oldX)
                                r.x += COL_SPACE;
                            ++col;
                        } while (!rowEnd);
                    }


                    if (maxWidth < r.x)
                        maxWidth = r.x;
                    ++totalRow;
                    r.x = 0;
                    r.y -= 3;
                    r.height = 22;
                    r.width = maxWidth;
                    
                    //处理鼠标选中操作
                    if (eventType == EventType.MouseDown && current.button == 0 && r.Contains(current.mousePosition) && eventCfg != null)
                    {
                        c.selId = eventCfg.id;
                        m_group.selId = c.id;
                    }
                    r.y += 22;
                }

            }
            if (maxWidth != m_group.maxWidth)
                m_group.maxWidth = maxWidth;
            
Label_Break:;
        Rect rc =GUILayoutUtility.GetRect(m_group.maxWidth, r.y);
        
        if (eventType == EventType.MouseDrag && current.button == 2 )
        {
            m_group.contentScroll.x +=current.delta.x;
            current.Use();
        }

        GUILayout.EndScrollView();
            
        
        
    }


    public enum enCol
    {
        addEvent, remove,ingore, frameMove,eventMove, frameId, eventId, eventType,ingoreIfHero, priority, frameBegin, frameType, targetType, range,
        expand, frameInterval, frameEnd, beginOffsetAngle, endOffsetAngle, begingOffsetPos, endOffsetPos, 
        countLimit, countTargetLimit, countFrameLimit, frameLimit, eventLimit, eventCountFrameLimit,
        targetOrderType, conditions, targetConditions, expandCol, 
    }

    
    public static bool HeaderButton(ref Rect r, string content, int width) { return HeaderButton(ref r,new GUIContent(content),width);}

    public static bool HeaderButton(ref Rect r, GUIContent content,int width)
    {
        r.width = width + COL_SPACE;
        bool click =GUI.Button(r,content, headStyle);
        r.x += width + COL_SPACE;
        return click;
    }

    public  bool DrawFrameHeader(ref Rect r,SkillEventFrameCfg frameCfg,SkillEventGroupCfg g, int col, System.Action<string> onTip)
    {
    switch ((enCol)col)
    {
        case enCol.addEvent:
            {
                r.width = COL_WIDTH + COL_SPACE;
                if (GUI.Button(r,EditorGUIUtility.IconContent("Toolbar Plus More"), headStyle))
                {
                    GenericMenu genericMenu = new GenericMenu();
                    for (int i = 0; i < SkillEventFactory.TypeName.Length; ++i)
                    {
                        int type = i;
                        genericMenu.AddItem(new GUIContent(SkillEventFactory.TypeName[i]), false, () =>
                        {
                            g.AddFrame((enSkillEventType)type);
                        });
                    }
                    genericMenu.ShowAsContext();
                }
                r.x += COL_WIDTH + COL_SPACE;
            }; return false;
        case enCol.remove: if (HeaderButton(ref r,EditorGUIUtility.IconContent("TreeEditor.Trash"), COL_WIDTH )) onTip("删除帧"); return false;
        case enCol.ingore: if (HeaderButton(ref r, EditorGUIUtility.IconContent("ViewToolOrbit On"), COL_WIDTH))
                {
                    if (g.frames.Count == 0)
                        return false;
                    bool ingore = !g.frames[0].events[0].ingore;
                    foreach(var f in g.frames)
                    {
                        foreach(var e in f.events)
                        {
                            e.ingore = ingore;
                        }
                    }

                };return false;
        case enCol.frameId: if (HeaderButton(ref r, "帧id", COL_WIDTH * 2)) onTip("帧id，暂时没有其他用处"); return false;
        case enCol.eventId: if (HeaderButton(ref r, "事件id", COL_WIDTH * 2)) onTip("事件id，用于前置事件中填写"); return false;
        case enCol.eventType: if (HeaderButton(ref r, "事件类型", COL_WIDTH * 3)) onTip("事件类型"); return false;
        case enCol.ingoreIfHero: if (HeaderButton(ref r, "主角事", COL_WIDTH * 2)) onTip("如果不是主角就不执行，一般用避免别的角色导致的情况来做相机震动"); return false;
        case enCol.priority: if (HeaderButton(ref r, "优先级", COL_WIDTH * 2)) onTip("事件优先级。如果这个优先级大于目标所拥有的免疫事件状态的优先级，那么免疫无效。(比如霸体 = 免疫被击+免疫击飞+免疫浮空，目标身上有优先级为0的霸体、但是这里的优先级为1，那么霸体无效)"); return false;
        case enCol.frameMove: if (HeaderButton(ref r, "帧微调", COL_WIDTH * 4 )) onTip("开始帧的调整"); return false;
        case enCol.eventMove: if (HeaderButton(ref r, "事微调", COL_WIDTH * 4)) onTip("处在同一帧的第几个事件"); return false;                
        case enCol.frameBegin: if (HeaderButton(ref r, "第几帧", COL_WIDTH * 2 )) onTip("在第几帧进行事件"); return false;
        case enCol.frameType: if (HeaderButton(ref r, "帧类型",COL_WIDTH * 2 )) onTip(
     @"单帧
多帧，允许设置间隔
缓冲后帧，每间隔都判断下，如果找到了作用对象，那么结束帧的时候执行(暂时不做)

注意，间隔以帧为单位，最小间隔是一帧
"); return false;
       
        case enCol.targetType: if (HeaderButton(ref r, "对象",COL_WIDTH * 5 )) onTip(
      @"作用对象类型:
    释放者，总是执行到，除非角色不可战斗了
    释放者，受碰撞检测
    友方，同一阵营但是不是自己
    敌人阵营
    中立阵营
    友方和自己
    当前技能的目标，使用技能的时候可能会指定目标，否则就会自动选择目标
"); return false;
        case enCol.range: if (HeaderButton(ref r, "范围", COL_WIDTH * 8 )) onTip(
@"作用范围类型:
    圆。参数(半径、垂直距离)
    扇形。参数(半径、夹角限制、垂直距离)
    矩形。参数(长、宽、垂直距离)
    碰撞。参数(垂直距离),碰撞大小在释放者的根节点的Collider组件调整。注意要加RigidBody和Collider，RigidBody的IK勾上，Collider的Trigger勾上
垂直距离，-1表示不用判断了"); return false;
        case enCol.expand:
            {
                using (new AutoChangeBkColor(Color.green))
                {
                    if (!m_expand && HeaderButton(ref r, m_expand ? EditorGUIUtility.IconContent("Icon Dropdown") : new GUIContent("›"), COL_WIDTH / 2))
                        m_expand = !m_expand; 
                }
                
            };   return !m_expand;
        case enCol.frameInterval: if (HeaderButton(ref r, "帧间隔", COL_WIDTH * 2)) onTip("帧间隔"); return false;
        case enCol.frameEnd: if (HeaderButton(ref r, "结束帧", COL_WIDTH * 2)) onTip("结束帧，注意无论是不是在间隔帧都会执行到"); return false;
        case enCol.beginOffsetAngle: if (HeaderButton(ref r, "开始角", COL_WIDTH * 2)) onTip("开始偏移角度，相对于主角前方\n注意碰撞检测的时候是先进行角度偏移然后再位置偏移"); return false;
        case enCol.begingOffsetPos: if (HeaderButton(ref r, "开始偏移位置", COL_WIDTH * 7)) onTip("开始偏移位置，相对于主角前方\n注意碰撞检测的时候是先进行角度偏移然后再位置偏移"); return false;
        case enCol.endOffsetAngle: if (HeaderButton(ref r, "结束角", COL_WIDTH * 2)) onTip("结束偏移角度，相对于主角前方\n注意碰撞检测的时候是先进行角度偏移然后再位置偏移"); return false;
        case enCol.endOffsetPos: if (HeaderButton(ref r, "结束偏移位置", COL_WIDTH * 7)) onTip("结束偏移位置，相对于主角前方\n注意碰撞检测的时候是先进行角度偏移然后再位置偏移"); return false;
        case enCol.countLimit: if (HeaderButton(ref r, "次数", COL_WIDTH * 2 )) onTip("触发次数限制，总的作用对象次数不能超过这个值"); return false;
        case enCol.countTargetLimit: if (HeaderButton(ref r, "对象次", COL_WIDTH * 2 )) onTip("对象触发次数限制，对同一个作用对象而言触发次数不能超过这个值"); return false;
        case enCol.countFrameLimit: if (HeaderButton(ref r, "帧次数", COL_WIDTH * 2 )) onTip("当前帧触发的次数限制，当前帧能同时作用的对象的数量的上限"); return false;
        case enCol.frameLimit: if (HeaderButton(ref r, "有效次", COL_WIDTH * 2 )) onTip("有效帧次数，如果一帧作用过至少一个对象，那么就算有效帧。\n能有效帧超过这个值那么就不会再执行"); return false;
        case enCol.eventLimit: if (HeaderButton(ref r, "事件次", COL_WIDTH * 2)) onTip("事件触发次数超过这个值那么就不会再执行"); return false;
        case enCol.eventCountFrameLimit: if (HeaderButton(ref r, "帧事次", COL_WIDTH * 2)) onTip("帧事件次数，一帧能同时作用的对象的次数"); return false;
        case enCol.targetOrderType: if (HeaderButton(ref r, "排序", COL_WIDTH * 2)) onTip(
        @"
无，一般是按照创建顺序
洗牌
距离
仇恨
        "); return false;
        case enCol.conditions: if (HeaderButton(ref r, "前置条件", COL_WIDTH * 8 )) onTip("前置条件,如果前置事件没有执行，那么不执行" + SkillEventFactory.ConditionTip); return false;
        case enCol.targetConditions: if (HeaderButton(ref r, "前置对象条件", COL_WIDTH * 8 )) onTip("前置对象条件，如果前置事件没有对这个作用对象执行过，那么不执行" + SkillEventFactory.ConditionTip); return false;
        case enCol.expandCol: {
                using (new AutoChangeBkColor(Color.green))
                {
                    if (m_expand && HeaderButton(ref r, m_expand ? EditorGUIUtility.IconContent("Icon Dropdown") : new GUIContent("›"), COL_WIDTH / 2))
                        m_expand = !m_expand; 
                }
            };return true;
        default: Debuger.LogError("未知的列{0}", col); return true;
    }
    }
   
    float lastChangeFrameTime = 0;
    float changeFrameSplit =0.15f;
    public  bool DrawFrameGrid(ref Rect r, SkillEventCfg eventCfg, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int row, int col, int totalRow, ref bool change)
    {
        switch ((enCol)col)
        {
            case enCol.addEvent:
                {
                    r.width = COL_WIDTH + COL_SPACE;
                    if (row==0)
                    {
                        if(GUI.Button(r, EditorGUIUtility.IconContent("Toolbar Plus More"), headStyle)){
                            GenericMenu genericMenu = new GenericMenu();
                            for (int i = 0; i < SkillEventFactory.TypeName.Length; ++i)
                            {
                                int type = i;
                                genericMenu.AddItem(new GUIContent(SkillEventFactory.TypeName[i]), false, () =>
                                {
                                    g.AddEvent(frameCfg,(enSkillEventType)type);
                                });
                            }
                            genericMenu.ShowAsContext();
                        }
                    }
                    r.x += COL_WIDTH;
                }; return false;
            case enCol.remove:
                {
                    r.width = COL_WIDTH + COL_SPACE;
                    if (row == 0 ){
                        if(GUI.Button(r, EditorGUIUtility.IconContent("TreeEditor.Trash"), headStyle)){
                            if(EditorUtility.DisplayDialog("", "是否确定删除?", "是", "否"))
                            {
                                g.RemoveFrame(frameCfg);
                                change = true;
                                return true;
                            }
                            
                        }
                    }
                    else
                    {
                        if (eventCfg != null && GUI.Button(r, EditorGUIUtility.IconContent("TreeEditor.Trash"), headStyle))
                        {
                            if (EditorUtility.DisplayDialog("", "是否确定删除?", "是", "否"))
                            {
                                g.RemoveEvent(frameCfg, eventCfg);
                                change = true;
                                return true;
                            }
                            
                        }
                    }
                        
                    r.x += COL_WIDTH;
                }; return false;
            case enCol.ingore:
                {
                    r.width = COL_WIDTH + COL_SPACE;
                    if (eventCfg != null && GUI.Button(r, EditorGUIUtility.IconContent(eventCfg.ingore ? "ViewToolOrbit" : "ViewToolOrbit On"), headStyle))
                    {
                        eventCfg.ingore= !eventCfg.ingore;
                    } 
                    

                    r.x += COL_WIDTH;
                }; return false;
            case enCol.frameId:
                {
                    r.width = COL_WIDTH * 2;
                    if (row == 0)
                        EditorGUI.LabelField(r, frameCfg.id.ToString());
                    r.x += COL_WIDTH * 2;
                }; return false;
            case enCol.eventId:
                {
                    r.width = COL_WIDTH * 2;
                    if (eventCfg != null)
                        EditorGUI.LabelField(r, eventCfg.id.ToString());
                    r.x += COL_WIDTH * 2;
                }; return false;
            case enCol.eventType:
                {
                    
                    if (eventCfg != null)
                    {
                        r.width = COL_WIDTH * 2.5f;
                        EditorGUI.LabelField(r, SkillEventFactory.TypeName[(int)eventCfg.Type]);
                        r.x += r.width;

                        r.width = COL_WIDTH * 0.5f;
                        if (GUI.Button(r, EditorGUIUtility.IconContent("sv_icon_dot10_sml"),GUIStyle.none))
                        {
                            if(EditorUtility.DisplayDialog("",string.Format("是否在代码编辑器里打开{0}事件的代码", SkillEventFactory.TypeName[(int)eventCfg.Type]),"是","否"))
                                EditorUtil.OpenScriptEditorByObj(eventCfg);
                        }
                        r.x += r.width;
                    }
                    else
                        r.x += COL_WIDTH * 3f;

                }; return false;
            case enCol.ingoreIfHero:
                {
                    r.width = COL_WIDTH * 2;
                    if (eventCfg != null)
                        eventCfg.ingoreIfNotHero =EditorGUI.Toggle(r, eventCfg.ingoreIfNotHero);
                    r.x += COL_WIDTH * 2;
                }; return false;
            case enCol.priority: {
                    r.width = COL_WIDTH * 2;
                    if (eventCfg != null)
                        eventCfg.priority = EditorGUI.IntField(r, GUIContent.none, eventCfg.priority);
                    r.x += COL_WIDTH * 2;
                }; return false; 
            case enCol.frameMove:
                {
                    if (row == 0)
                    {
                        r.width = COL_WIDTH;

                        if (GUI.RepeatButton(r, "«", headStyle) && Util.time - lastChangeFrameTime > changeFrameSplit)
                        {
                            lastChangeFrameTime = Util.time;
                            frameCfg.frameBegin = Mathf.Max(0, frameCfg.frameBegin - 5);
                            g.frames.SortEx((a, b) => a.frameBegin.CompareTo(b.frameBegin));
                            change = true;
                            return true;
                        }
                        r.x += r.width;

                        if (GUI.RepeatButton(r, "‹", headStyle) && Util.time - lastChangeFrameTime > changeFrameSplit)
                        {
                            lastChangeFrameTime = Util.time;
                            frameCfg.frameBegin = Mathf.Max(0, frameCfg.frameBegin - 1);
                            g.frames.SortEx((a, b) => a.frameBegin.CompareTo(b.frameBegin));
                            change = true;
                            return true;
                        }
                        r.x += r.width;


                        if (GUI.RepeatButton(r, "›", headStyle) && Util.time - lastChangeFrameTime > changeFrameSplit)
                        {
                            lastChangeFrameTime = Util.time;
                            frameCfg.frameBegin = Mathf.Max(0, frameCfg.frameBegin + 1);
                            g.frames.SortEx((a, b) => a.frameBegin.CompareTo(b.frameBegin));
                            change = true;
                            return true;
                        }
                        r.x += r.width;

                        if (GUI.RepeatButton(r, "»", headStyle) && Util.time - lastChangeFrameTime > changeFrameSplit)
                        {
                            lastChangeFrameTime = Util.time;
                            frameCfg.frameBegin = Mathf.Max(0, frameCfg.frameBegin + 5);
                            g.frames.SortEx((a, b) => a.frameBegin.CompareTo(b.frameBegin));
                            change = true;
                            return true;
                        }
                        r.x += r.width;
                       
                    }
                    else
                        r.x += COL_WIDTH * 4;
                }; return false;
            case enCol.eventMove:
                {
                    r.width = COL_WIDTH * 4;
                    if (eventCfg != null)
                    {
                        r.width = COL_WIDTH;
                        int idx = frameCfg.events.IndexOf(eventCfg);
                        using (new AutoEditorDisabledGroup(idx ==0 || frameCfg.events.Count ==1))
                        {
                            if (GUI.Button(r, "«", headStyle))
                            {
                                frameCfg.events.RemoveAt(idx);
                                frameCfg.events.Insert(0, eventCfg);
                                change = true;
                                return true;
                            }
                        }
                            
                        r.x += r.width;

                        using (new AutoEditorDisabledGroup(idx == 0 || frameCfg.events.Count == 1))
                        {
                            if (GUI.Button(r, "‹", headStyle))
                            {
                                frameCfg.events.RemoveAt(idx);
                                frameCfg.events.Insert(idx - 1, eventCfg);
                                change = true;
                                return true;
                            }
                        }

                        r.x += r.width;

                        using (new AutoEditorDisabledGroup(idx == frameCfg.events.Count-1 || frameCfg.events.Count == 1))
                        {
                            if (GUI.Button(r, "›", headStyle))
                            {
                                frameCfg.events.RemoveAt(idx);
                                frameCfg.events.Insert(idx + 1, eventCfg);
                                change = true;
                                return true;
                            }
                        }
                       
                        r.x += r.width;
                        using (new AutoEditorDisabledGroup(idx == frameCfg.events.Count - 1 || frameCfg.events.Count == 1))
                        {
                            if (GUI.Button(r, "»", headStyle))
                            {
                                frameCfg.events.RemoveAt(idx);
                                frameCfg.events.Add(eventCfg);
                                change = true;
                                return true;
                            }
                        }

                        r.x += r.width;
                    }
                    else
                        r.x += COL_WIDTH * 4;
                }; return false;
            case enCol.frameBegin:
                {
                    r.width = COL_WIDTH * 2;
                    if (row == 0 )
                        EditorGUI.LabelField(r, frameCfg.frameBegin.ToString());
                    r.x += COL_WIDTH * 2;
                }; return false;
            case enCol.frameType:
                {
                    r.width = COL_WIDTH * 2;
                    if (row == 0)
                        frameCfg.frameType = (enSkillEventFrameType)EditorGUI.Popup(r, (int)frameCfg.frameType, SkillEventFrameCfg.FrameTypeName);

                    r.x += COL_WIDTH * 2;
                }; return false;
            case enCol.targetType:
                {
                    float beginX = r.x;
                    
                    TargetRangeCfg targetCfg =row <frameCfg.targetRanges.Count?frameCfg.targetRanges[row]:null;
                    if (targetCfg != null)
                    {
                        r.width = COL_WIDTH * 3;
                        targetCfg.targetType = (enSkillEventTargetType)EditorGUI.Popup(r, (int)targetCfg.targetType, SkillEventFrameCfg.TargetTypeName);                        r.x += r.width;

                        //第一行只能增加，而其他行只能删除，刚好放在同一个位置
                        r.width = COL_WIDTH * 1;
                        if (row == 0)
                        {
                            if (GUI.Button(r, EditorGUIUtility.IconContent("Toolbar Plus More"), headStyle))
                            {
                                frameCfg.targetRanges.Add(new TargetRangeCfg());
                                change = true;
                                return true;
                            }
                        }
                        else
                        {
                            if (GUI.Button(r, EditorGUIUtility.IconContent("TreeEditor.Trash"), headStyle))
                            {
                                frameCfg.targetRanges.RemoveAt(row);
                                change = true;
                                return true;
                            }
                        }
                        r.x += r.width;

                        if (targetCfg.targetType != enSkillEventTargetType.selfAlway)
                        {
                            r.width = COL_WIDTH * 1;
                            Color c = GUI.backgroundColor;
                            GUI.backgroundColor = Color.HSVToRGB(((30f * totalRow +70) % 360f) / 360f, 1, 1);
                            targetCfg.range.showRange = GUI.Toggle(r, targetCfg.range.showRange, GUIContent.none);
                            GUI.backgroundColor = c;
                            r.x += r.width;
                        }
                    }


                    r.x = beginX + COL_WIDTH * 5;
                }; return false;
            case enCol.range:
                {
                    float beginX= r.x;
                    TargetRangeCfg targetCfg = row < frameCfg.targetRanges.Count ? frameCfg.targetRanges[row] : null;
                    if (targetCfg != null && targetCfg.targetType != enSkillEventTargetType.selfAlway)
                    {
                        r.width = COL_WIDTH*2 ;
                        targetCfg.range.type = (enRangeType)EditorGUI.Popup(r, (int)targetCfg.range.type, RangeCfg.RangeTypeName);
                        r.x += r.width;
                        r.width = COL_WIDTH*2;
                        if (targetCfg.range.type != enRangeType.collider)
                        {
                            targetCfg.range.distance = EditorGUI.FloatField(r, targetCfg.range.distance);//半径、长
                            r.x += r.width;
                        }
                        
                        if(targetCfg.range.type == enRangeType.sector)//角度
                        {
                            targetCfg.range.angleLimit = EditorGUI.FloatField(r, targetCfg.range.angleLimit);
                            r.x += r.width;
                        }
                        else if (targetCfg.range.type == enRangeType.rect)//宽
                        {
                            targetCfg.range.rectLimit = EditorGUI.FloatField(r, targetCfg.range.rectLimit);
                            r.x += r.width;
                            
                        }

                        targetCfg.range.heightLimit = EditorGUI.FloatField(r, targetCfg.range.heightLimit);//高
                        r.x += r.width;
                    }
                     
                    r.x = beginX + COL_WIDTH * 8;
                        
                }; return false; 
            case enCol.expand:
                {
                    if (!m_expand) r.x += COL_WIDTH / 2;
                };  return !m_expand;
            case enCol.frameInterval:
                {
                    r.width = COL_WIDTH * 2;
                    if (row == 0 && frameCfg.frameType != enSkillEventFrameType.once)
                        frameCfg.frameInterval = EditorGUI.IntField(r, GUIContent.none, frameCfg.frameInterval);
                    r.x += COL_WIDTH * 2;
                }; return false;
            case enCol.frameEnd:
                {
                    r.width = COL_WIDTH * 2;
                    if (row == 0 && frameCfg.frameType != enSkillEventFrameType.once)
                        frameCfg.frameEnd = EditorGUI.IntField(r, GUIContent.none, frameCfg.frameEnd);
                    r.x += COL_WIDTH * 2;
                }; return false;
            case enCol.beginOffsetAngle:
                {
                    r.width = COL_WIDTH * 2;
                    TargetRangeCfg targetCfg = row < frameCfg.targetRanges.Count ? frameCfg.targetRanges[row] : null;
                    if (targetCfg != null && targetCfg.targetType != enSkillEventTargetType.selfAlway)
                        targetCfg.range.beginOffsetAngle = EditorGUI.FloatField(r, targetCfg.range.beginOffsetAngle);
                        
                    r.x += COL_WIDTH * 2;
                }; return false;
            case enCol.begingOffsetPos:
                {
                    r.width = COL_WIDTH * 7;
                    TargetRangeCfg targetCfg = row < frameCfg.targetRanges.Count ? frameCfg.targetRanges[row] : null;
                    if (targetCfg != null && targetCfg.targetType != enSkillEventTargetType.selfAlway)
                        targetCfg.range.begingOffsetPos = EditorGUI.Vector3Field(r, GUIContent.none, targetCfg.range.begingOffsetPos);
                    r.x += COL_WIDTH * 7;
                }; return false;
            case enCol.endOffsetAngle:
                {
                    r.width = COL_WIDTH * 2;
                    TargetRangeCfg targetCfg = row < frameCfg.targetRanges.Count ? frameCfg.targetRanges[row] : null;
                    if (targetCfg != null && targetCfg.targetType != enSkillEventTargetType.selfAlway && frameCfg.frameType != enSkillEventFrameType.once)
                        targetCfg.range.endOffsetAngle = EditorGUI.FloatField(r, targetCfg.range.endOffsetAngle);

                    r.x += COL_WIDTH * 2;
                }; return false;
            case enCol.endOffsetPos:
                {
                    r.width = COL_WIDTH * 7;
                    TargetRangeCfg targetCfg = row < frameCfg.targetRanges.Count ? frameCfg.targetRanges[row] : null;
                    if (targetCfg != null && targetCfg.targetType != enSkillEventTargetType.selfAlway && frameCfg.frameType != enSkillEventFrameType.once)
                        targetCfg.range.endOffsetPos = EditorGUI.Vector3Field(r, GUIContent.none, targetCfg.range.endOffsetPos);
                    r.x += COL_WIDTH * 7;
                }; return false;
            case enCol.countLimit:
                {
                    r.width = COL_WIDTH * 2;
                    if (row == 0)
                        frameCfg.countLimit = EditorGUI.IntField(r, frameCfg.countLimit);
                    r.x += COL_WIDTH * 2;
                }; return false;
            case enCol.countTargetLimit:
                {
                    r.width = COL_WIDTH *  2;
                    if (row == 0)
                        frameCfg.countTargetLimit = EditorGUI.IntField(r, frameCfg.countTargetLimit);
                    r.x += COL_WIDTH *  2;
                }; return false;
            case enCol.countFrameLimit:
                {
                    r.width = COL_WIDTH *  2;
                    if (row == 0)
                        frameCfg.countFrameLimit = EditorGUI.IntField(r, frameCfg.countFrameLimit);
                    r.x += COL_WIDTH *  2;
                }; return false;
            case enCol.frameLimit:
                {
                    r.width = COL_WIDTH *  2;
                    if (row == 0)
                        frameCfg.frameLimit = EditorGUI.IntField(r, frameCfg.frameLimit);
                    r.x += COL_WIDTH *  2;
                }; return false;
            case enCol.eventLimit:
                {
                    r.width = COL_WIDTH * 2;
                    if (eventCfg != null)
                        eventCfg.eventCountLimit = EditorGUI.IntField(r, eventCfg.eventCountLimit);
                    r.x += COL_WIDTH * 2;
                }; return false;
            case enCol.eventCountFrameLimit:
                {
                    r.width = COL_WIDTH * 2;
                    if (eventCfg != null)
                        eventCfg.eventCountFrameLimit = EditorGUI.IntField(r, eventCfg.eventCountFrameLimit);
                    r.x += COL_WIDTH * 2;
                }; return false;
            case enCol.targetOrderType:
                {
                    r.width = COL_WIDTH * 2;
                    if (row == 0)
                        frameCfg.targetOrderType = (enTargetOrderType)EditorGUI.Popup(r, (int)frameCfg.targetOrderType, SkillEventFrameCfg.TargetOrderTypeName);
                    r.x += COL_WIDTH *  2;
                }; return false;
            case enCol.conditions:
            case enCol.targetConditions:
                {
                    float beginX = r.x;
                    
                    //增加，只在第一行有就可以了
                    if (row == 0)
                    {
                        r.width = COL_WIDTH * 1;
                        if (GUI.Button(r, EditorGUIUtility.IconContent("Toolbar Plus More"), headStyle))
                        {
                            GenericMenu genericMenu = new GenericMenu();
                            for (int i = 0; i < SkillEventFactory.ConditionTypeName.Length; ++i)
                            {
                                int type = i;
                                genericMenu.AddItem(new GUIContent(SkillEventFactory.ConditionTypeName[type]), false, () =>
                                {
                                    if ((enCol)col == enCol.conditions)
                                        frameCfg.conditions.Add(SkillEventFactory.CreateCondition((enSkillEventConditionType)type));
                                    else
                                        frameCfg.targetConditions.Add(SkillEventFactory.CreateCondition((enSkillEventConditionType)type));

                                    
                                });
                            }
                            genericMenu.ShowAsContext();
                        }
                        r.x += r.width;
                    }

                    SkillEventConditionCfg c;
                    if ((enCol)col == enCol.conditions)
                        c = row < frameCfg.conditions.Count ? frameCfg.conditions[row] : null;
                    else
                        c = row < frameCfg.targetConditions.Count ? frameCfg.targetConditions[row] : null;

                    if (c != null )
                    {
                        //删除
                        r.width = COL_WIDTH * 1;
                        if (GUI.Button(r, EditorGUIUtility.IconContent("TreeEditor.Trash"), headStyle))
                        {
                            if ((enCol)col == enCol.conditions)
                                frameCfg.conditions.RemoveAt(row);
                            else
                                frameCfg.targetConditions.RemoveAt(row);

                            change = true;
                            return true;
                        }
                        r.x += r.width;

                        //条件名
                        r.width = COL_WIDTH * 2;
                        EditorGUI.LabelField(r, SkillEventFactory.ConditionTypeName[(int)c.Type]);                        
                        r.x += r.width;

                        //参数
                        r.width = COL_WIDTH * 4;
                        c.OnDraw(r);
                        r.x += r.width;
                    }

                    r.x = beginX +COL_WIDTH * 8;
                }; return false;
            case enCol.expandCol:
                {
                    r.x += COL_WIDTH /2;  
                }; return true;
            default: Debuger.LogError("未知的列{0}", col); return true;
        }
    }

}
