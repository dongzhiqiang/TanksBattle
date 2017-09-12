using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SimpleHandle), true)]
public class SimpleHandleEditor : Editor
{
    SimpleHandle m_cur; 
    bool m_ctrlPlay=false;//正在播放
    bool m_havePlay = false;//播放过

    public static void OnOpenWnd(Handle.WndType wnd,object param)
    {
        if (wnd == Handle.WndType.sequenceEditor)
        {
            HandleSequenceWindow.ShowWnd(param);
        }
    }

	public override void OnInspectorGUI ()
	{
		//base.OnInspectorGUI();
        using (new AutoLabelWidth(100))
        {
            EditorGUILayout.LabelField("状态:", m_cur.m_handle.IsPlaying ? "播放中" : "未播放");

            //自己的设置
            GUI.changed = false;
            bool playOnEnable = EditorGUILayout.Toggle("enable时运行", m_cur.m_playOnEnable);
            bool resetOnEnable = EditorGUILayout.Toggle("enable时重置", m_cur.m_resetOnEnable);
            if (GUI.changed)
            {
                EditorUtil.RegisterUndo("SimpleHandle change", m_cur);
                m_cur.m_playOnEnable = playOnEnable;
                m_cur.m_resetOnEnable = resetOnEnable;
                EditorUtil.SetDirty(m_cur);
            }

            //操作的设置
            m_cur.m_handle.OnGUI(m_cur, OnOpenWnd, true);//serializedObject.FindProperty("m_handle")


            //结束事件
            serializedObject.Update();
            var serializedProperty = serializedObject.FindProperty("m_onEnd");
            UnityEditor.EditorGUILayout.PropertyField(serializedProperty, new GUIContent("结束事件"));
            serializedObject.ApplyModifiedProperties();
            
        }
        
	}

    void OnEnable()
    {
        m_cur = target as SimpleHandle;
        m_ctrlPlay=false;
        m_havePlay = false;

        if (Application.isEditor && !EditorApplication.isPlaying)
        {
            EditorApplication.update = m_cur.LateUpdate;
        }
    }

    void OnDisable()
    {
        if (Application.isEditor && !EditorApplication.isPlaying)
        {
            if (m_cur != null && m_havePlay)
                m_cur.ResetStop();
            EditorApplication.update = null;
        }
    }

    public void OnSceneGUI()
    {
        if(m_cur==null)
            return;
        Rect r = new Rect(Screen.width - 170, Screen.height - 120, 160, 80);
        Vector2 mouse = Event.current.mousePosition;

        using (new AutoBeginHandles())
        {
            using (new AutoBeginArea(r, m_cur.gameObject.name, "Window"))
            {
                using (new AutoBeginHorizontal(GUILayout.Height(20)))
                {
                    if (m_cur.IsPlaying)
                    {
                        if (GUILayout.Button("stop"))
                        {
                            m_cur.ResetStop();
                            m_ctrlPlay = false;
                            m_havePlay = true;
                            EditorUtil.SetDirty(m_cur);
                        }

                        if (GUILayout.Button("reset"))
                        {
                            m_cur.ResetPlay();
                            m_ctrlPlay=true;
                            m_havePlay = true;
                            EditorUtil.SetDirty(m_cur);
                        }

                        //运行时，而且控件不能自己LateUpdate的情况下，要LateUpdate下
                        if (Application.isEditor && EditorApplication.isPlaying && !m_cur.enabled && m_ctrlPlay)
                        {
                            m_cur.LateUpdate();
                        }
                        
                    }
                    else
                    {
                        m_ctrlPlay = false;
                        if (GUILayout.Button("play"))
                        {
                            m_cur.ResetPlay();
                            m_havePlay = true;
                            EditorUtil.SetDirty(m_cur);
                        }
                    }
                }
            }
        }
        

    }
}
