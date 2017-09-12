using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SequenceHandleEx), true)]
public class SequenceHandleExEditor : Editor
{
    SequenceHandleEx m_comp; 
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
        m_comp = (SequenceHandleEx)this.target;
        if (m_comp.m_handle.CurType != Handle.Type.sequence)
        {
            m_comp.m_handle.SetType(Handle.Type.sequence);
            m_comp.m_handle.m_go = m_comp.gameObject;
        }

        EditorGUI.BeginChangeCheck();
        m_comp.m_loop = EditorGUILayout.Toggle("是否循环", m_comp.m_loop);
        m_comp.m_dir = EditorGUILayout.FloatField("滚动正向", m_comp.m_dir);
        m_comp.m_dirSize = EditorGUILayout.FloatField("滚动大小", m_comp.m_dirSize);
        m_comp.m_endScroll = EditorGUILayout.Toggle("结束回滚", m_comp.m_endScroll);
        if (m_comp.m_endScroll)
        {
            m_comp.m_endScrollDuration = EditorGUILayout.FloatField("结束回滚时间", m_comp.m_endScrollDuration);
        }
        m_comp.m_clickToEnd = EditorGUILayout.Toggle("点击滚动", m_comp.m_clickToEnd);
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtil.SetDirty(m_comp);
        }

        //显示打开动画序列编辑器按钮
        m_comp.m_handle.CurHandle.OnDraw(m_comp, m_comp.m_handle, OnOpenWnd);  
        
	}
    public void OnSceneGUI()
    {
        m_comp = (SequenceHandleEx)this.target;
        Handles.color = Color.gray;
        Handles.ArrowCap(0, m_comp.transform.position, Quaternion.LookRotation(Quaternion.Euler(0, 0, m_comp.m_dir) * Vector3.right), m_comp.m_dirSize * m_comp.transform.lossyScale.x);
    }
    

}
