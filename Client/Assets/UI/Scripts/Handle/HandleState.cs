using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class HandleState : IHandle
{
    //不在运行中时，Handle的类型或者m_go改变的时候会刷新值
    public override void OnReset(Handle h, bool resetBegin = true, bool resetEnd = false)
    {
        if (h.m_go)
        {
            StateHandle s = h.m_go.GetComponent<StateHandle>();
            if(s){
                h.m_iBegin = s.CurStateIdx;
            }
        }
    }

    public override void OnEnd(Handle h)
    {
        if (h.m_go == null)
            return;
        StateHandle s = h.m_go.GetComponent<StateHandle>();
        if(s==null)
            return;
        s.SetState(h.m_iBegin,true);
        
    }
#if UNITY_EDITOR
    public override bool OnDrawGo(Component comp, Handle h, string title = null)
    {
        return DrawGoField<StateHandle>(comp, h, title);
    }
    //框架，绘制属性(不包含游戏对象),syncGo的话结束值变化会同步到m_go
    public override void OnDraw(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        OnDrawMid(comp, h, onOpenWnd, syncGo );
        DrawCommonImmediate(comp, h);
    }

    
    //框架，绘制属性(不包含游戏对象)，syncGo的话结束值变化会同步到m_go。这里是最小化绘制，只绘制最需要的属性
    public override void OnDrawMin(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        if (h.m_go==null)
            return;
        GUI.changed = false;
        StateHandle s = h.m_go.GetComponent<StateHandle>();
        int idx= UnityEditor.EditorGUILayout.Popup("", h.m_iBegin, s.StateName);
        if (GUI.changed)
        {
            EditorUtil.RegisterUndo("Handle Change", comp);
            h.m_iBegin = idx;
            EditorUtil.SetDirty(comp);
            if (syncGo && h.m_go)
            {
                End(h);
                EditorUtil.SetDirty(h.m_go);
            }
        }
    }

    //框架，绘制属性(不包含游戏对象)，syncGo的话结束值变化会同步到m_go。这里是最小化绘制，只绘制最需要的属性
    public override void OnDrawMid(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        if (h.m_go == null)
            return;
        GUI.changed = false;
        StateHandle s = h.m_go.GetComponent<StateHandle>();
        int idx = UnityEditor.EditorGUILayout.Popup("状态", h.m_iBegin, s.StateName);
        if (GUI.changed)
        {
            EditorUtil.RegisterUndo("Handle Change", comp);
            h.m_iBegin = idx;
            EditorUtil.SetDirty(comp);
            if (syncGo && h.m_go)
            {
                End(h);
                EditorUtil.SetDirty(h.m_go);
            }
        }
    }
#endif
}
