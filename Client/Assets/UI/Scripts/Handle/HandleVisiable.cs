using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class HandleVisiable : IHandle
{
    //不在运行中时，Handle的类型或者m_go改变的时候会刷新值
    public override void OnReset(Handle h, bool resetBegin = true, bool resetEnd = false)
    {
        if (h.m_go)
        {
            h.m_b1 = h.m_go.activeSelf ;
        }
    }

    public override void OnEnd(Handle h)
    {
        if (h.m_go == null || h.m_go.activeSelf == h.m_b1)
            return;
        h.m_go.SetActive(h.m_b1);
    }
#if UNITY_EDITOR
    public override bool OnDrawGo(Component comp, Handle h, string title = null)
    {
        return DrawGoField(comp, h, title);
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
        GUI.changed = false;
        bool visiable = UnityEditor.EditorGUILayout.Toggle("", h.m_b1);
        if (GUI.changed)
        {
            EditorUtil.RegisterUndo("Handle Change", comp);
            h.m_b1 = visiable;
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
        GUI.changed = false;
        bool visiable = UnityEditor.EditorGUILayout.Toggle("可见", h.m_b1);
        if (GUI.changed)
        {
            EditorUtil.RegisterUndo("Handle Change", comp);
            h.m_b1 = visiable;
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
