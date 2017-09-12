using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UIScrollWrapItem), true)]
public class UIScrollWrapItemEditor : Editor
{
    UIScrollWrapItem m_comp;

    public static void OnOpenWnd(Handle.WndType wnd, object param)
    {
        if (wnd == Handle.WndType.sequenceEditor)
        {
            HandleSequenceWindow.ShowWnd(param);
        }
    }

    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        m_comp = (UIScrollWrapItem)this.target;

        if (m_comp.m_handle.CurType != Handle.Type.sequence)
        {
            m_comp.m_handle.SetType(Handle.Type.sequence);
            m_comp.m_handle.m_go = m_comp.gameObject;
        }

        m_comp.m_handle.CurHandle.OnDraw(m_comp, m_comp.m_handle, OnOpenWnd);        
    }
}
