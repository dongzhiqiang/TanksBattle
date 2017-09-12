using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;


//技能编辑器
public class SkillEditor : MultiWindow
{
    public int m_id;
    EventGroupEditor m_eventGroupEditor;
    RoleCfg m_roleCfg;
    RoleSkillCfg m_roleSkillCfg;
    SkillCfg m_skillCfg;
    string[] m_aniNames = null;
    int m_frameTotal= -1;
    Role m_curRole;
    int m_curId;
    int m_observer;
    string m_searchSkill = "";
    AniFxGroup m_aniFxGroup;//注意切换角色、切换技能、切换动作的时候都要重新计算
    
    bool m_debugDetailArea=false;
    bool m_infoArea = true;
    bool m_debugArea = true;
    bool m_aniArea = true;
    bool m_rangeArea = true;
    bool m_switchSkillArea = true;
    bool m_posAndRotArea =true;

    public static string[] AniTypeName = new string[] { "单次", "循环", "来回" };

    public void SetRole(Role role)
    {
        if (role == null || role.State != Role.enState.alive || role.Cfg == null || role.CombatPart.RoleSkillCfg == null)
        {
            Debuger.LogError("技能编辑器设置角色失败");
            return;
        }

        //先找出之前的技能id
        string skillId = string.Empty;
        if (m_roleSkillCfg != null && m_roleSkillCfg == role.CombatPart.RoleSkillCfg && m_skillCfg!=null)
            skillId =m_skillCfg.skillId;

        m_curRole = role;
        m_curId = role.Id;
        m_roleCfg = role.Cfg;
        m_roleSkillCfg = role.CombatPart.RoleSkillCfg;
        CombatMgr.instance.m_debugRoleId =m_curRole.Id;
        //选中技能
        SkillCfg skillCfg =string.IsNullOrEmpty(skillId)?null: m_roleSkillCfg.GetBySkillId(skillId,false);
        if (skillCfg!= null)
            SetSkill(skillCfg);
        else if(m_roleSkillCfg.skills.Count>0)
            SetSkill(m_roleSkillCfg.skills[0]);
        else
            SetSkill(null);           
    }

    public void SetSkill(SkillCfg skillCfg)
    {
        

        m_frameTotal = -1;
        m_skillCfg = skillCfg;
        if(m_skillCfg != null){
            if (m_curRole != null && !m_curRole.IsDestroy(m_curId))
            {
                CombatMgr.instance.m_debugSkillId =skillCfg.skillId;
            }
               

            m_eventGroupEditor.SetTran(m_curRole != null ? m_curRole.transform : null);
            m_eventGroupEditor.SetGroup(skillCfg.eventGroup);
        }
        else
        {
            
            m_eventGroupEditor.SetTran(null);
            m_eventGroupEditor.SetGroup(null);
        }

        ResetFxGroup();
            
    }

    public override void OnEnable()
    { 
        
        m_eventGroupEditor = new EventGroupEditor();
        m_eventGroupEditor.OnEnable();
        m_eventGroupEditor.OnFocus();
        m_observer = DrawGL.Add(OnDrawGL);
    }

    public override void OnDisable()
    {
        if (m_eventGroupEditor != null)
            m_eventGroupEditor.OnDisable();
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
    }

    void OnDrawGL(object obj)
    {
        if (m_skillCfg == null || m_curRole == null || m_curRole.transform==null) return  ;

        DrawGL draw = (DrawGL)obj;
        Vector3 pos =m_curRole.transform.position;
        Vector3 dir = m_curRole.transform.forward;
        CollideUtil.Draw(m_skillCfg.firstRange, draw, pos, dir,Color.HSVToRGB(0/ 360f,1,1));
        CollideUtil.Draw(m_skillCfg.secondRange, draw, pos, dir,Color.HSVToRGB(15f / 360f,1,1));
        CollideUtil.Draw(m_skillCfg.thirdRange, draw, pos, dir,Color.HSVToRGB(30f / 360f,1,1));
        CollideUtil.Draw(m_skillCfg.attackRange, draw, pos, dir,Color.HSVToRGB(45f / 360f,1,1));

    }
 
    Vector2 vv=Vector2.zero;
    //绘制窗口时调用
    public override void OnGUI()
    {
        //如果切换场景之类的，角色被回收了，那么要清空角色
        if (m_curRole != null && (m_curRole.State == Role.enState.init))
        {
            m_curRole = null;
            m_eventGroupEditor.SetTran(null);
        }

        //工具栏
        using (new AutoBeginHorizontal(EditorStyles.toolbarButton))
        {
            DrawToolBar();
        }

        
        using (new AutoBeginHorizontal())
        {
            //左边，技能所有的信息
            using (new AutoBeginVertical("PreferencesSectionBox", GUILayout.Width(250)))
            {
                using (new AutoLabelWidth(80))
                {
                    EditorGUILayoutEx.FadeArea area = EditorGUILayoutEx.instance.BeginFadeArea(m_debugArea, "调试面板", "skillDebugArea" + m_id, EditorStyleEx.BoxStyle);
                    m_debugArea = area.open;
                    if (area.Show())
                        DrawDebug();
                    EditorGUILayoutEx.instance.EndFadeArea();

                    if (m_skillCfg == null)
                        return;

                    area = EditorGUILayoutEx.instance.BeginFadeArea(m_infoArea, "基本信息", "skillInfoArea" + m_id, EditorStyleEx.BoxStyle);
                    m_infoArea = area.open;
                    if (area.Show())
                        DrawInfo();
                    EditorGUILayoutEx.instance.EndFadeArea();

                    area = EditorGUILayoutEx.instance.BeginFadeArea(m_aniArea, "动作", "aniArea" + m_id, EditorStyleEx.BoxStyle);
                    m_aniArea = area.open;
                    if (area.Show())
                        DrawAni();
                    EditorGUILayoutEx.instance.EndFadeArea();

                    area = EditorGUILayoutEx.instance.BeginFadeArea(m_posAndRotArea, "位置和方向", "posAndRotArea" + m_id, EditorStyleEx.BoxStyle);
                    m_posAndRotArea = area.open;
                    if (area.Show())
                        DrawPosAndRot();
                    EditorGUILayoutEx.instance.EndFadeArea();

                    area = EditorGUILayoutEx.instance.BeginFadeArea(m_rangeArea, "攻击范围", "rangeArea" + m_id, EditorStyleEx.BoxStyle);
                    m_rangeArea = area.open;
                    if (area.Show())
                        DrawRanges();
                    EditorGUILayoutEx.instance.EndFadeArea();

                    area = EditorGUILayoutEx.instance.BeginFadeArea(m_switchSkillArea, "取消和连击技能", "switchSkillArea" + m_id, EditorStyleEx.BoxStyle);
                    m_switchSkillArea = area.open;
                    if (area.Show())
                        DrawSwitchSkill();
                    EditorGUILayoutEx.instance.EndFadeArea();
                }

            }

            //右边，技能的事件表
            using (new AutoBeginVertical(GUILayout.ExpandWidth(true), GUILayout.Height(adapterWindow.position.height-20)))
            {
                if (m_eventGroupEditor != null)
                {
                    m_skillCfg.eventGroup.AniFxGroup = m_aniFxGroup;
                    m_eventGroupEditor.Draw();
                    m_skillCfg.eventGroup.AniFxGroup = null;
                }
                    
            }
            
        }
    }

    void DrawToolBar()
    {
        //选择角色
        int idx = m_roleCfg == null ? -1 : Array.IndexOf(RoleCfg.RoleIds, m_roleCfg.id);
        if (GUILayout.Button(idx == -1 ? "选择角色" : RoleCfg.RoleIds[idx], EditorStyles.toolbarPopup, GUILayout.Width(130)))
        {
            GenericMenu genericMenu = new GenericMenu();
            for (int i = 0; i < RoleCfg.RoleIds.Length; ++i)
            {
                string roleId = RoleCfg.RoleIds[i];
                genericMenu.AddItem(new GUIContent(roleId), false, () =>
                {
                    if (m_roleSkillCfg != null && EditorUtility.DisplayDialog("", "是否保存" + m_roleCfg.name + "的技能?", "保存", "否"))
                        m_roleSkillCfg.Save();
                    else
                    {
                      //  RoleSkillCfg.RemoveCache(m_roleSkillCfg);//FIX:如果加了这一行会导致下次切过来的时候编辑的不是当前活动的配置
                        m_roleSkillCfg = null;
                    }

                    m_aniNames = null;
                    m_frameTotal = -1;
                    m_roleCfg = RoleCfg.Get(roleId);
                    m_roleSkillCfg = RoleSkillCfg.Get(m_roleCfg.skillFile);

                    //如果游戏运行中，那么找到角色
                    Role r = RoleMgr.instance == null ? null : RoleMgr.instance.GetRoleByRoleId(m_roleCfg.id);
                    if (r != null)
                        SetRole(r);
                    else
                        SetSkill(m_roleSkillCfg.skills.Count == 0 ? null : m_roleSkillCfg.skills[0]);
                });
            }
            genericMenu.ShowAsContext();
        }

        //从角色浏览器选择角色
        if (GUILayout.Button("角色浏览器", EditorStyles.toolbarPopup, GUILayout.Width(70)))//EditorGUIUtility.IconContent("AvatarInspector/HeadZoomSilhouette")
        {
            if (m_roleCfg != null && EditorUtility.DisplayDialog("", "是否保存" + m_roleCfg.name + "的技能?", "保存", "否"))
                m_roleSkillCfg.Save();
            else
            {
                RoleSkillCfg.RemoveCache(m_roleSkillCfg);
                m_roleSkillCfg = null;
            }

            RoleBrowserWindow.ShowWindow((string roleId)=>{
                m_roleCfg = RoleCfg.Get(roleId);
                m_roleSkillCfg = RoleSkillCfg.Get(m_roleCfg.skillFile);
                m_aniNames = null;
                
                //如果游戏运行中，那么找到角色
                Role r = RoleMgr.instance == null ? null : RoleMgr.instance.GetRoleByRoleId(m_roleCfg.id);
                if (r != null)
                    SetRole(r);
                else
                    SetSkill(m_roleSkillCfg.skills.Count == 0 ? null : m_roleSkillCfg.skills[0]);
                
            });
        }


        if(m_roleSkillCfg == null)
            return;


        //选择技能
        idx = m_skillCfg == null ? -1 : Array.IndexOf(m_roleSkillCfg.SkillIds, m_skillCfg.skillId);
        if (GUILayout.Button(m_skillCfg == null ? "选择技能" : m_skillCfg.skillId, EditorStyles.toolbarPopup, GUILayout.Width(130)))
        {
            int len = m_roleSkillCfg.SkillIds.Length;
            GenericMenu genericMenu = new GenericMenu();
            for (int i = 0; i < m_roleSkillCfg.SkillIds.Length; ++i)
            {
                int ii = i;
                string skillId = m_roleSkillCfg.SkillIds[ii];
                bool isLine = ii == len - 2;//分割线
                bool isCreate = ii == len - 1;//新建
                if (!isLine&& !isCreate &&!string.IsNullOrEmpty(m_searchSkill) && !skillId.Contains(m_searchSkill))
                    continue;

                if (isLine)
                    genericMenu.AddSeparator(string.Empty);//华丽的分割线
                else if (isCreate)
                    genericMenu.AddItem(new GUIContent(skillId), false, () => SetSkill(m_roleSkillCfg.Add()));
                else
                    genericMenu.AddItem(new GUIContent(skillId), false, () => SetSkill(m_roleSkillCfg.GetBySkillId(skillId)));
            }
            genericMenu.ShowAsContext();
        }

        //技能筛选下，免得显示太多了
        using (new AutoBeginHorizontal(GUILayout.Width(200)))
        {
            m_searchSkill = EditorGUILayout.TextField(GUIContent.none, m_searchSkill, "ToolbarSeachTextField", GUILayout.ExpandWidth(true));
            if (GUILayout.Button("", string.IsNullOrEmpty(m_searchSkill) ?  "ToolbarSeachCancelButtonEmpty": "ToolbarSeachCancelButton"))
                m_searchSkill = "";
        }

        //保存角色所有技能
        if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Duplicate"), EditorStyles.toolbarButton, GUILayout.Width(30)))
        {
            m_roleSkillCfg.Save();
            adapterWindow.ShowNotification(new GUIContent("保存成功"));
        }

        //重载技能配置
        if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Refresh"), EditorStyles.toolbarButton, GUILayout.Width(30)))
        {
            if (EditorUtility.DisplayDialog("", "是否确定要重载当前角色的所有技能配置，所有未保存的修改都将还原?", "是", "否"))
            {
                RoleSkillCfg.RemoveCache(m_roleSkillCfg);
                m_roleSkillCfg = RoleSkillCfg.Get(m_roleSkillCfg.file);

                if (m_roleSkillCfg.skills.Count == 0)
                    m_skillCfg = null;
                else if (m_skillCfg == null)
                    m_skillCfg = m_roleSkillCfg.skills[0];
                else
                {
                    m_skillCfg = m_roleSkillCfg.GetBySkillId(m_skillCfg.skillId);
                    if (m_skillCfg == null)
                        m_skillCfg = m_roleSkillCfg.skills[0];
                }
                SetSkill(m_skillCfg);
            }
                
        }
        //删除技能
        if (m_skillCfg != null && GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyles.toolbarButton, GUILayout.Width(30)))
        {
            if (EditorUtility.DisplayDialog("", "是否确定要删除技能?", "是", "否"))
            {
                m_roleSkillCfg.Remove(m_skillCfg);
                m_skillCfg = m_roleSkillCfg.skills.Count == 0 ? null : m_roleSkillCfg.skills[0];
                SetSkill(m_skillCfg);
            }
        }

        //删除技能
        if (GUILayout.Button("复制技能", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            GenericMenu genericMenu = new GenericMenu();
            for (int i = 0; i < RoleCfg.RoleIds.Length; ++i)
            {
                string roleId = RoleCfg.RoleIds[i];
                genericMenu.AddItem(new GUIContent(roleId), false, () =>
                {
                    if (m_roleSkillCfg != null && EditorUtility.DisplayDialog("", "是否确定要复制"+roleId+"的所有技能?", "是", "否"))
                    {
                       m_roleSkillCfg.CopyFrom(roleId);
                       m_aniNames = null;
                       m_frameTotal = -1;
                       SetSkill(m_roleSkillCfg.skills.Count == 0 ? null : m_roleSkillCfg.skills[0]);
                    }
                });
            }
            genericMenu.ShowAsContext();
        }

        
    }

    void DrawDebug()
    {
        RoleModel model = m_curRole==null?null:m_curRole.RoleModel;
        RoleModel newModel = (RoleModel)EditorGUILayout.ObjectField("角色",model , typeof(RoleModel), true);
        if (newModel != model && newModel!=null)
        {
            if(newModel.Parent!= null && newModel.Parent.State == Role.enState.alive )
            {
                SetRole(newModel.Parent);
            }
        }
        if (RoleMgr.instance == null)
            return;
        using (new AutoBeginHorizontal())
        {
            Role hero = RoleMgr.instance.Hero;
            if (hero != null && hero != m_curRole&& hero.State == Role.enState.alive && GUILayout.Button("主角"))
            {
                SetRole(hero);
            }
            if (m_roleCfg!=null &&GUILayout.Button("下一个角色"))
            {
                Role r = RoleMgr.instance.FindNextRole(m_roleCfg.id, m_curRole);
                if (r != null)
                    SetRole(r);
            }
        }


        if (m_curRole == null || m_curRole.State != Role.enState.alive)
            return;
        
        CombatPart combatPart =m_curRole.CombatPart;
        Skill curSkill = m_curRole.CombatPart.CurSkill;
        EditorGUILayout.TextField("当前技能", curSkill!=null?curSkill.Cfg.skillId:"");
        EditorGUILayout.IntField("当前帧", curSkill != null ? curSkill.CurFrame: 0);
        if (m_skillCfg != null)
        {
            if (GUILayout.Button("使用技能"))
            {
                combatPart.Play(m_skillCfg.skillId,null,false, true);
            }
            
            Event e = Event.current;
            if (e.type == EventType.keyDown && e.keyCode == KeyCode.PageDown)
            {
                e.Use();
                combatPart.Play(m_skillCfg.skillId, null, false, true);
            }
        }

        
        AnimationState st =m_curRole.AniPart.Ani.CurSt;
        if (st != null)
        {
            EditorGUILayoutEx.FadeArea area = EditorGUILayoutEx.instance.BeginFadeArea(m_debugDetailArea, "详细", "aniDetail", EditorStyleEx.BoxStyle);
            m_debugDetailArea = area.open;
            if (area.Show())
            {
                EditorGUILayout.LabelField(string.Format("动作名:{0}", st.name));
                EditorGUILayout.LabelField(string.Format("wrapMode:{0}", st.wrapMode));
                EditorGUILayout.LabelField(string.Format("enable:{0}", st.enabled));
                EditorGUILayout.LabelField(string.Format("speed:{0}", st.speed));
                EditorGUILayout.LabelField(string.Format("time:{0}", st.time));
                EditorGUILayout.LabelField(string.Format("length:{0}", st.length));
                EditorGUILayout.LabelField(string.Format("normalizedSpeed:{0}", st.normalizedSpeed));
                EditorGUILayout.LabelField(string.Format("normalizedTime:{0}", st.normalizedTime));
            }
            EditorGUILayoutEx.instance.EndFadeArea();
            
           
        }
        
    }

    void DrawInfo()
    {
       // EditorGUILayout.IntField("唯一id", m_skillCfg.id);
        string id=EditorGUILayout.TextField("技能id", m_skillCfg.skillId);
        if(id!=m_skillCfg.skillId){
            m_skillCfg.skillId=id;
            m_roleSkillCfg.Reset();
        }
        
        m_skillCfg.name = EditorGUILayout.TextField("技能名", m_skillCfg.name);
        //EditorGUILayout.PrefixLabel("描述");
        //m_skillCfg.desc = EditorGUILayout.TextArea( m_skillCfg.desc);
        //m_skillCfg.icon = EditorGUILayout.TextField("图标", m_skillCfg.icon);
        // ImageExEditor.ShowSelectImage((string icon) => m_skillCfg.icon = icon);
        using (new AutoEditorTipButton("使用技能会扣的耐力"))
            m_skillCfg.mp = EditorGUILayout.IntField("耐力消耗", m_skillCfg.mp);
        using (new AutoEditorTipButton("如果不为-1,那么使用技能的时候会判断下耐力够不够这个只，如果是-1，那么取耐力消耗"))
            m_skillCfg.mpNeed = EditorGUILayout.IntField("耐力需要", m_skillCfg.mpNeed);
        m_skillCfg.cd = EditorGUILayout.FloatField("冷却时间", m_skillCfg.cd);
        m_skillCfg.isAirSkill = EditorGUILayout.Toggle("是否空中技能", m_skillCfg.isAirSkill);
        m_skillCfg.showWeapon = EditorGUILayout.Toggle("显示武器", m_skillCfg.showWeapon);
        using (new AutoEditorTipButton(@"是不是内部技能
一个技能要做出来可能要通过释放第二个技能来实现，那么这第二个技能就属于内部技能
暂时用来计算连击，内部技能不影响连击的判断
子技能完全结束后父技能才会开始计算cd"))
            m_skillCfg.isInternal = EditorGUILayout.Toggle("内部技能", m_skillCfg.isInternal);

        if(m_skillCfg.isInternal)
            m_skillCfg.parentSkillId = EditorGUILayout.TextField("所属技能", m_skillCfg.parentSkillId);
        
        //using(new AutoEditorTipButton(""))
        {
            string s = EditorGUILayout.TextField("技能期间状态", m_skillCfg.SkillStatesStr);
            if (s != m_skillCfg.SkillStatesStr)
                m_skillCfg.SkillStatesStr = s;
        }

        using (new AutoEditorTipButton("技能结束时给释放者加状态"))
        {
            string s = EditorGUILayout.TextField("技能结束状态", m_skillCfg.EndStatesStr);
            if (s != m_skillCfg.EndStatesStr)
                m_skillCfg.EndStatesStr = s;
        }

        using (new AutoEditorTipButton("技能结束时释放者释放的事件组。注意，执行这个事件组的第0帧的时候当前技能的技能期间状态还在"))
        {
            string s = EditorGUILayout.TextField("结束时的事件组", m_skillCfg.endEventGroupId);
            if (s != m_skillCfg.endEventGroupId)
                m_skillCfg.endEventGroupId = s;
        }

        using (new AutoEditorTipButton("如果当前技能是被自己的其他技能连击或者取消的，也执行结束事件组和结束状态"))
        {
            m_skillCfg.endIfCancel = EditorGUILayout.Toggle("被取消仍结束处理", m_skillCfg.endIfCancel);
        }

        using (new AutoEditorTipButton("如果当前技能因为被击等外力中断，也执行结束事件组和结束状态"))
        {
            m_skillCfg.endIfBehit = EditorGUILayout.Toggle("被中断仍结束处理", m_skillCfg.endIfBehit);
        }
        
    }

    void DrawAni()
    {
        //先找到这个角色的所有动作
        if (m_aniNames == null)
            FindAniNames();

        int idx = Array.IndexOf(m_aniNames, m_skillCfg.aniName);
        int idxNew = EditorGUILayout.Popup("动作名", idx, m_aniNames);
        if (idx != idxNew)
        {
            m_skillCfg.aniName = m_aniNames[idxNew];
            ResetFxGroup();
            m_frameTotal = -1;
        }
            
        idxNew = EditorGUILayout.Popup("类型", (int)m_skillCfg.aniType, AniTypeName);
        if ((int)m_skillCfg.aniType != idxNew) {
            m_frameTotal= -1;
            m_skillCfg.aniType = (SkillCfg.enSkillAniType)idxNew;
            //自动设置下持续时间
            if (m_skillCfg.aniType != SkillCfg.enSkillAniType.ClampForever){
                Animation ani = GetAni();
                 if (ani != null){
                    AnimationState st = ani[m_skillCfg.aniName];
                    if (st != null)
                    {
                        m_skillCfg.duration = st.length;
                        m_frameTotal = -1;
                    }
                        
                 }
                
            }
                
        }

        using (new AutoEditorTipButton("开始渐变时间，如果填0则不渐变"))
            m_skillCfg.fade = EditorGUILayout.FloatField("开始渐变", m_skillCfg.fade);

        using(new AutoEditorTipButton(
@"-1，则技能在动作播放完结束，动作帧率调整会导致帧率间隔不一。
否则，技能在持续时间到时结束，动作帧率间隔始终是1秒24帧。
只有动作循环类型是单次的时候填-1才有效,其他类型下一定不能填-1。"))
        {
            float d = EditorGUILayout.FloatField("时间", m_skillCfg.duration);
            if (d != m_skillCfg.duration)
            {
                m_skillCfg.duration =d;
                m_frameTotal = -1;
            }
        }

        using (new AutoEditorTipButton("持续时间到了，但是技能键按紧的情况下，是不是继续技能"))
        {
            bool b  = EditorGUILayout.Toggle("按紧持续", m_skillCfg.continueIfPress);
            if (b != m_skillCfg.continueIfPress)
            {
                m_skillCfg.continueIfPress = b;
                if (b == true)
                {
                    //自动设置下持续时间
                    Animation ani = GetAni();
                    if (ani != null)
                    {
                        AnimationState st = ani[m_skillCfg.aniName];
                        if (st != null)
                        {
                            m_skillCfg.duration = st.length;
                            m_frameTotal = -1;
                        }
                            
                    }
                }
            }
             
        }

        //画帧率调节
        DrawAniRate();

    }

    void DrawPosAndRot()
    {

        m_skillCfg.moveType = (SkillCfg.enMoveType)EditorGUILayout.Popup("移动类型", (int)m_skillCfg.moveType, SkillCfg.MoveTypeName);
        m_skillCfg.dirType = (SkillCfg.enDirType)EditorGUILayout.Popup("方向类型", (int)m_skillCfg.dirType, SkillCfg.DirTypeName);

        using (new AutoEditorTipButton("从技能的第几帧开始允许位移或转向"))
            m_skillCfg.beginTranFrame = EditorGUILayout.IntField("开始帧", m_skillCfg.beginTranFrame);
        using (new AutoEditorTipButton("从技能的第几帧结束移动或者转向，-1则为到技能结束"))
            m_skillCfg.endTranFrame = EditorGUILayout.IntField("结束帧", m_skillCfg.endTranFrame);

        if (m_skillCfg.moveType == SkillCfg.enMoveType.joystick || m_skillCfg.moveType == SkillCfg.enMoveType.keepMove || m_skillCfg.moveType == SkillCfg.enMoveType.findTarget)
            m_skillCfg.moveSpeed = EditorGUILayout.FloatField("移动速度", m_skillCfg.moveSpeed);

        if (m_skillCfg.dirType == SkillCfg.enDirType.keepRotate)
            m_skillCfg.rotateSpeed = EditorGUILayout.FloatField("自动转向速度", m_skillCfg.rotateSpeed);
    }

    void ResetFxGroup()
    {
        if (m_roleCfg == null || m_skillCfg == null){ m_aniFxGroup = null; return; }
        
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/FBX/Resources/" + m_roleCfg.mod + ".prefab");
        if (prefab == null) { m_aniFxGroup = null; return; }

        Transform t = prefab.transform.Find("model");
        if (t == null) { m_aniFxGroup = null; return; }

        AniFxMgr aniFxMgr = t.GetComponent<AniFxMgr>();
        if (aniFxMgr== null){m_aniFxGroup = null;return;}
        m_aniFxGroup = aniFxMgr.GetGroup(m_skillCfg.aniName);
    }

    Animation GetAni() 
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/FBX/Resources/" + m_roleCfg.mod + ".prefab");
        if (prefab == null)
            return null;

        Transform t = prefab.transform.Find("model");
        if (t == null)
            return null;

        Animation ani = t.GetComponent<Animation>();
        return ani;
    }
    void FindAniNames()
    {
        Animation ani = GetAni();
        if (ani == null)
        {
            m_aniNames = new string[0];
            return;
        }
        
        m_aniNames = ani.GetNames();
    }

    void FindAniFrame()
    {
        Animation ani = GetAni();
        if (ani == null)
        {
            m_frameTotal = 0;
            return;
        }

        if (m_skillCfg.aniType != SkillCfg.enSkillAniType.ClampForever){
            m_frameTotal = (int)(m_skillCfg.duration / Util.One_Frame);
            return ;
        }

        AnimationState st = ani[m_skillCfg.aniName];
        if (st == null)
        {
            m_frameTotal = 0;
            return;
        }

        m_frameTotal =(int)(st.length/Util.One_Frame);
    }
    
    float cursorRectWidth =6;
    int cursorDragSel =-1;
    void DrawAniRate()
    {
        if (m_frameTotal ==-1)
            FindAniFrame();

        if (m_frameTotal == 0)
        {
            EditorGUILayout.LabelField("帧率调节(无)");
            return;
        }

        
        EditorGUILayout.LabelField("帧率调节(" + m_frameTotal+"帧)");

        Rect rect = GUILayoutUtility.GetRect(0, 40f, "LODSliderBG", GUILayout.ExpandWidth(true));
        float frameWidth = (rect.width - m_skillCfg.aniRate.clips.Count * cursorRectWidth) / m_frameTotal;

        Event current =Event.current;
        EventType eventType = current.type;
        if (eventType == EventType.MouseUp && current.button == 0)
            cursorDragSel = -1;
        
        float lastX= 0;
        float dragFrameMinX=0;
        int dragFrameMin=1;
        int dragFrameMax=m_frameTotal-1;
        for(int i = 0;i<m_skillCfg.aniRate.clips.Count;++i)
        {

            AniClipCfg clip = m_skillCfg.aniRate.clips[i];

            //绘制区间
            Rect r = new Rect(rect.x + lastX, rect.y+15, frameWidth * clip.frame - lastX, 25);
            lastX=frameWidth*clip.frame;
            GUI.Box(r,"","LODSliderRange");
            clip.speed = EditorGUI.FloatField(r, clip.speed);//速率

            //右键菜单，增加、删除
            if(eventType ==EventType.MouseDown && current.button ==1 &&r.Contains(current.mousePosition))
			{
                AniClipCfg clip2 = clip;
                int frameMax = clip.frame-1;
                int frameMin = i == 0?1:m_skillCfg.aniRate.clips[i-1].frame+1;
                GenericMenu genericMenu = new GenericMenu();
                if (frameMax >= frameMin)
                {
                    int frame = Mathf.Min(frameMin+(int)((current.mousePosition.x-r.x)/frameWidth),frameMax);
                    int ii =i;
                    genericMenu.AddItem(new GUIContent("增加"), false, () =>
                    {
                        AniClipCfg clipNew = new AniClipCfg();
                        clipNew.frame = frame;
                        m_skillCfg.aniRate.clips.Insert(ii, clipNew);
                    });
                }
                else
                    genericMenu.AddDisabledItem(new GUIContent("增加"));
                
                genericMenu.AddItem(new GUIContent("删除"), false, () =>
                {
                    m_skillCfg.aniRate.clips.Remove(clip2);
                });
                genericMenu.ShowAsContext();

                current.Use();		
            }
            //计算拖动时的最小范围和最大范围
            if(cursorDragSel-1 == i)
                dragFrameMin = clip.frame + 1;
            if (cursorDragSel == i)
                dragFrameMinX = r.x;
            if(cursorDragSel+1 ==i)
                dragFrameMax=clip.frame-1;

            //绘制拖动区域的鼠标
            r.x =r.x+r.width;
            r.width = cursorRectWidth;            
            EditorGUIUtility.AddCursorRect(r, MouseCursor.ResizeHorizontal);
            lastX += cursorRectWidth;

            //计算拖动
            if (eventType == EventType.MouseDown && current.button == 0 && r.Contains(current.mousePosition))
            {
                cursorDragSel = i;
                current.Use();
            }
           
            //绘制帧
            r.x-=3;
            r.y-=15;
            r.width=30;
            GUI.Label(r, clip.frame.ToString());
        }

        //计算拖动
        if(eventType == EventType.MouseDrag && cursorDragSel > -1){
            current.Use();
            int frame = Mathf.Clamp(dragFrameMin + (int)((current.mousePosition.x - dragFrameMinX) / frameWidth), dragFrameMin, dragFrameMax);
            m_skillCfg.aniRate.clips[cursorDragSel].frame = frame;
        }

        //默认区域的绘制
        Rect defaultRect = new Rect(rect.x + lastX, rect.y+15, rect.width - rect.x - lastX, 25);
        GUI.Box(defaultRect, "", "LODSliderRange");
        m_skillCfg.aniRate.speed = EditorGUI.FloatField(defaultRect, m_skillCfg.aniRate.speed);
        if (eventType == EventType.MouseDown && current.button == 1 && defaultRect.Contains(current.mousePosition))
        {
            int frameMax = m_frameTotal - 1;
            int frameMin = m_skillCfg.aniRate.clips.Count == 0 ? 1 : m_skillCfg.aniRate.clips[m_skillCfg.aniRate.clips.Count - 1].frame + 1;
            GenericMenu genericMenu = new GenericMenu();
            if (frameMax >= frameMin)
            {
                int frame = Mathf.Min(frameMin +(int)((current.mousePosition.x - defaultRect.x) / frameWidth),  frameMax);
                genericMenu.AddItem(new GUIContent("增加"), false, () =>
                {
                    AniClipCfg clipNew = new AniClipCfg();
                    clipNew.frame = frame;
                    m_skillCfg.aniRate.clips.Add( clipNew);
                });
            }
            else
                genericMenu.AddDisabledItem(new GUIContent("增加"));
            genericMenu.AddDisabledItem(new GUIContent("删除"));
            genericMenu.ShowAsContext();
        }
        
        
    }

    void DrawRanges()
    {
        using (new AutoEditorTipButton("自动朝向，注意无论有没有自动朝向都会根据下面的朝向去找技能目标，这里勾选了就会朝向这个目标"))
        {
            m_skillCfg.autoFace = EditorGUILayout.Toggle("自动朝向", m_skillCfg.autoFace);
        }
        DrawRange(m_skillCfg.firstRange, "第一朝向", Color.HSVToRGB(0, 1, 1),
@"释放技能时，
摇杆前方有没有敌人，有则自动朝向最近的敌人
主角前方有没有上次打的敌人，有则自动朝向这个敌人
主角前方敌人，有则自动朝向最近的敌人
        ");
        DrawRange(m_skillCfg.secondRange, "第二朝向", Color.HSVToRGB(15f / 360f, 1, 1), "释放技能时，主角前方有没有敌人，有则自动朝向");
        DrawRange(m_skillCfg.thirdRange, "第三朝向", Color.HSVToRGB(30f / 360f, 1, 1), "释放技能时，范围内有没有敌人，有则自动朝向");
        DrawRange(m_skillCfg.attackRange, "可攻击范围", Color.HSVToRGB(45f / 360f, 1, 1), "用于判断能不能攻击到的范围，一般用于ai和预警");
    }

    void DrawRange(RangeCfg range,string title,Color clr,string tip)
    {
        Color tmp1 = GUI.color;
        EditorGUILayoutEx.FadeArea area = EditorGUILayoutEx.instance.BeginFadeArea(range.showArea, "", title, EditorStyleEx.BoxStyle);//
        range.showArea = area.open;
        Color tmp2 = GUI.color;
        GUI.color = tmp1;

        using (new AutoBeginHorizontal())
        {
            if (GUILayout.Button("", EditorStyleEx.GraphInfoButtonStyle))
                MessageBoxWindow.ShowAsMsgBox(tip);

            Color c = GUI.backgroundColor;
            GUI.backgroundColor = clr;
            range.showRange = GUILayout.Toggle(range.showRange, title, GUILayout.Width(70));
            GUI.backgroundColor = c;

            if (GUILayout.Button("", EditorGUILayoutEx.defaultLabelStyle))
                range.showArea = !range.showArea;
        }
        
        if (area.Show())
        {
            GUI.color = tmp2;

            range.type = (enRangeType)EditorGUILayout.Popup("类型", (int)range.type, RangeCfg.RangeTypeName);
            if (range.type == enRangeType.circle)
                range.distance = EditorGUILayout.FloatField("半径", range.distance);
            else if (range.type == enRangeType.sector)
                range.distance = EditorGUILayout.FloatField("半径", range.distance);
            else if (range.type == enRangeType.rect)
                range.distance = EditorGUILayout.FloatField("长", range.distance);

            if (range.type == enRangeType.sector)
                range.angleLimit = EditorGUILayout.FloatField("夹角", range.angleLimit);
            else if (range.type == enRangeType.rect)
                range.rectLimit = EditorGUILayout.FloatField("宽", range.rectLimit);

            //range.heightLimit = EditorGUILayout.FloatField("垂直距离", range.heightLimit);
            //range.beginOffsetAngle = EditorGUILayout.FloatField("偏移角度", range.beginOffsetAngle);
            /*using (new AutoBeginHorizontal())
            {
                EditorGUILayout.PrefixLabel("偏移位置");
                range.begingOffsetPos = EditorGUILayout.Vector3Field("", range.begingOffsetPos);
            }*/

            
        }


           
        EditorGUILayoutEx.instance.EndFadeArea();
    }

    void DrawSwitchSkill()
    {
        if (GUILayout.Button("", EditorStyleEx.GraphInfoButtonStyle))
        {
            MessageBoxWindow.ShowAsMsgBox(
@"优先级高的技能可以取消优先级低的技能。
空中技能和非空中技能不能互相取消。

技能在以下时机取消:
    取消前帧之前，使用取消技能，立即取消。
    取消前帧之后、取消后帧之前，使用取消技能，缓存到后帧之后取消。
    取消后帧之后，使用取消技能，立即取消。

技能在以下时机连击:
    连击前帧之前，使用连击技能，无效。
    连击前帧之后、连击后帧之前，使用连击技能，缓存到后帧之后连击。
    连击后帧之后，使用连击技能，立即连击。
    技能结束之后，连击等待帧数内，使用连击技能，立即连击。

");
        }

        m_skillCfg.cancelPriority = EditorGUILayout.IntField("取消优先级", m_skillCfg.cancelPriority);
        m_skillCfg.canCanel = EditorGUILayout.Toggle("可以取消别人", m_skillCfg.canCanel);
        m_skillCfg.canBeCanel = EditorGUILayout.Toggle("可以被取消", m_skillCfg.canBeCanel);
        m_skillCfg.cancelPreFrame = EditorGUILayout.IntField("取消前帧", m_skillCfg.cancelPreFrame);
        m_skillCfg.cancelPostFrame = EditorGUILayout.IntField("取消后帧", m_skillCfg.cancelPostFrame);

        GUILayout.Space(10);

        string newId = EditorGUILayout.TextField("连击技能", m_skillCfg.comboSkillId);
        if (newId != m_skillCfg.comboSkillId)
        {
            m_skillCfg.comboSkillId = newId;
            m_roleSkillCfg.NeedCalcCombo = true;
        }
        using (new AutoEditorTipButton("战斗界面下方是不是展示3个连击图标"))
            m_skillCfg.showComboIcon = EditorGUILayout.Toggle("是否显示ui", m_skillCfg.showComboIcon);
        m_skillCfg.comboPreFrame = EditorGUILayout.IntField("连击前帧", m_skillCfg.comboPreFrame);
        m_skillCfg.comboPostFrame = EditorGUILayout.IntField("连击后帧", m_skillCfg.comboPostFrame);
        m_skillCfg.comboWaitFrame = EditorGUILayout.IntField("连击等待帧", m_skillCfg.comboWaitFrame);
    }


    
}
