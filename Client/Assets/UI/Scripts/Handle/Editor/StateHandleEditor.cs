using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StateHandle), true)]
public class StateHandleEditor : Editor
{
    StateHandle m_cur;
    
	public override void OnInspectorGUI ()
	{
        using (new AutoLabelWidth(100))
        {
            //初始状态
            GUI.changed = false;
            m_cur.m_curState = EditorGUILayout.Popup("初始状态", m_cur.m_curState, m_cur.StateName);
            float duration = UnityEditor.EditorGUILayout.FloatField("持续时间", m_cur.Duration);
            bool isRealtime = UnityEditor.EditorGUILayout.Toggle("真实时间", m_cur.IsRealTime);
            if (GUI.changed) 
            {
                m_cur.SetState(m_cur.m_curState, false);
                if (duration != m_cur.Duration)
                    m_cur.Duration = duration;
                if (isRealtime != m_cur.IsRealTime)
                    m_cur.IsRealTime = isRealtime;
                EditorUtil.SetDirty(m_cur);
            }
            
            if(m_cur.EnableCtrlState){
                GUI.changed = false;
                m_cur.m_ctrlType = (StateHandle.CtrlType)EditorGUILayout.EnumPopup("控制类型", m_cur.m_ctrlType);
                if (GUI.changed) EditorUtil.SetDirty(m_cur);
            }
            
            
            //共有状态的操作对象
            DrawPublicGo();
            
            //状态们
            for(int i=0;i<m_cur.m_states.Count;++i){
                DrawState(m_cur.m_states[i],i);
            }

            //增加状态按钮
            if (GUILayout.Button("增加状态",GUILayout.Height(30)))
            {
                m_cur.AddState();
            }
        }
        
	}

    void OnEnable()
    {
        m_cur = target as StateHandle;
        if (Application.isEditor && !EditorApplication.isPlaying)
        {
            EditorApplication.update = m_cur.LateUpdate;
        }
    }

    void OnDisable()
    {
        if (Application.isEditor && !EditorApplication.isPlaying)
        {
            EditorApplication.update = null;
        }
    }

    //共有操作的操作对象
    void DrawPublicGo()
    {
        if (m_cur.m_states.Count == 0)
            return;
        if (!EditorUtil.DrawHeader("共有操作的操作对象"))
            return;
        using (new AutoContent())
        {
            StateHandle.State s = m_cur.m_states[0];
            for (int i = 0; i < s.publicHandles.Count; ++i)
            {
                Handle h = s.publicHandles[i];
                if (h.CurHandle == null)
                {
                    //Debuger.LogError("没有处理");
                    continue;
                }
                using (new AutoBeginHorizontal())
                {
                    //游戏对象改变
                    if (h.CurHandle.OnDrawGo(m_cur, h, h.CurTypeName))
                    {
                        for (int j = 0; j < m_cur.m_states.Count; ++j)
                        {
                            StateHandle.State s2 = m_cur.m_states[j];
                            Handle h2 = s2.publicHandles[i];
                            h2.m_go = h.m_go;
                            h2.CurHandle.OnReset(h2,false,true);
                        }
                        EditorUtil.SetDirty(m_cur);
                    }
                    //删除
                    if (GUILayout.Button("删除", GUILayout.Width(40)))
                    {
                        m_cur.RemovePublicHandle(i);
                        return;
                    }
                }
            }
                
            //增加共有操作
            int type = UnityEditor.EditorGUILayout.Popup("增加共有操作", -1, Handle.TypeName);
            if (type != -1 && type != 0)
            {
                m_cur.AddPublicHandle((Handle.Type)type);
            }    
        }
    }
    
    //绘制状态
    void DrawState(StateHandle.State s,int idx)
    {
        if (!EditorUtil.DrawHeader("状态-"+s.name,"StateHandle_state_"+idx)) {     
            return;
        }

        bool syncGo = idx == m_cur.m_curState;//如果改变值的话要不要改变游戏变量
        using (new AutoContent())
        {
            //状态的信息
            using (new AutoBeginHorizontal())
            {
                GUI.changed = false;
                s.name = EditorGUILayout.TextField("名字", s.name);//名字
                using (new AutoLabelWidth(30))
                {
                    s.isDuration = EditorGUILayout.Toggle("渐变", s.isDuration, GUILayout.Width(45));//渐变
                }
                //上移
                if (m_cur.CanMoveState(idx,true)&& GUILayout.Button("上移", GUILayout.Width(40)))
                {
                    m_cur.MoveState(idx, true);
                    return;
                }
                //下移
                if (m_cur.CanMoveState(idx, false) && GUILayout.Button("下移", GUILayout.Width(40)))
                {
                    m_cur.MoveState(idx, false);
                    return;
                }
                if (GUILayout.Button("删除", GUILayout.Width(40)))
                {
                    m_cur.RemoveState(idx);
                    return;
                }
                
                if (GUI.changed) EditorUtil.SetDirty(m_cur);
            }
            using (new AutoBeginHorizontal()) //音效，位置不够另起一行
            {
                EditorGUILayout.LabelField("音效", GUILayout.Width(85));
                using (new AutoLabelWidth(30))
                {
                    s.isEnterSound = EditorGUILayout.Toggle("进入", s.isEnterSound, GUILayout.Width(45));
                    if (s.isEnterSound)
                    {
                        int.TryParse(EditorGUILayout.TextField("ID", s.enterSoundId.ToString(), GUILayout.Width(60)), out s.enterSoundId);
                    }
                }

                using (new AutoLabelWidth(30))
                {
                    s.isExitSound = EditorGUILayout.Toggle("离开", s.isExitSound, GUILayout.Width(45));
                    if (s.isExitSound)
                    {
                        int.TryParse(EditorGUILayout.TextField("ID", s.exitSoundId.ToString(), GUILayout.Width(60)), out s.exitSoundId);
                    }
                }

            }

            //共有操作，不用画对象，也不能在这删除它
            for (int i = 0; i < s.publicHandles.Count; ++i)
            {
                Handle h = s.publicHandles[i];
                if (h.CurHandle == null)
                {
                    //Debuger.LogError("没有处理");
                    continue;
                }

                using (new AutoBeginHorizontal())
                {
                    EditorGUILayout.LabelField(h.CurTypeName, GUILayout.Width(85));
                    using (new AutoBeginVertical())
                    {
                        using (new AutoLabelWidth(50))
                        {
                            h.CurHandle.OnDrawMin(m_cur, h, SimpleHandleEditor.OnOpenWnd,  syncGo);
                        }
                    }
                }
                
                
            }

            //私有操作
            for (int i = 0; i < s.privateHandles.Count; ++i)
            {
                Handle h = s.privateHandles[i];
                if (h.CurHandle == null)
                {
                    //Debuger.LogError("没有处理");
                    continue;
                }
                
                //对象和删除操作
                using (new AutoBeginHorizontal())
                {
                    //游戏对象改变
                    if (h.CurHandle.OnDrawGo(m_cur, h, h.CurTypeName))
                    {
                        h.CurHandle.OnReset(h,false,true);
                    }
                    
                    //删除
                    if (GUILayout.Button("删除", GUILayout.Width(40)))
                    {
                        m_cur.RemovePrivateHandle(idx,i);
                        return;
                    }
                }
                //内容
                using (new AutoBeginHorizontal())
                {
                    EditorGUILayout.LabelField("", GUILayout.Width(85));
                    using (new AutoBeginVertical())
                    {
                        using (new AutoLabelWidth(50))
                        {
                            h.CurHandle.OnDrawMin(m_cur, h, SimpleHandleEditor.OnOpenWnd,  syncGo);
                        }
                    }
                }
                
            }

            //增加私有操作
            int type = UnityEditor.EditorGUILayout.Popup("增加私有操作", -1, Handle.TypeName);
            if (type != -1 && type != 0)
            {
                m_cur.AddPrivateHandle(idx,(Handle.Type)type);
            } 
        }
        
        
    }
}
