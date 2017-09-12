using UnityEngine;
using System.Collections;

[System.Serializable]
public class HandleProgressEx : IHandle
{
    public override bool IsDurationValid { get { return true; } }
    float GetValue(Handle h)
    {
        if (!h.m_go)
        {
            Debuger.LogError("游戏对象为空");
            return 1f;
        }

        UIProgressEx c = h.m_go.GetComponent<UIProgressEx>();
        if (c)
            return c.Value;

        return 1f;
    }

    void SetValue(Handle h, float v)
    {
        if (!h.m_go)
        {
            Debuger.LogError("游戏对象为空");
        }

        UIProgressEx c = h.m_go.GetComponent<UIProgressEx>();
        if (c)
            c.Value = v;

    }

    //不在运行中时，Handle的类型或者m_go改变的时候会刷新值
    public override void OnReset(Handle h, bool resetBegin = true, bool resetEnd = false)
    {
        if (h.m_go)
        {
            h.m_b1 = h.m_go.activeSelf;
            if (resetBegin)
                h.m_fBegin = 0;
            if (resetEnd)
                h.m_fEnd = 1;
        }
    }
    //开始时调用，可以捕获开始值
    public override void OnUseNowStart(Handle h)
    {
        if (h.m_go == null)
            return;

        h.m_fBeginNow = GetValue(h);
    }
    public override void OnUpdate(Handle h, float factor)
    {
        if (h.m_go == null)
            return;

        if (h.m_isUseNowStart)
            SetValue(h, Mathf.Lerp(h.m_fBeginNow, h.m_fEnd, factor));
        else
            SetValue(h, Mathf.Lerp(h.m_fBegin, h.m_fEnd, factor));
    }

    public override void OnEnd(Handle h)
    {
        if (h.m_go == null || h.m_go.activeSelf == h.m_b1)
            return;
    }
#if UNITY_EDITOR
    public override bool OnDrawGo(Component comp, Handle h, string title = null)
    {
        return DrawGoField<UIProgressEx>(comp, h, title);
    }
    //框架，绘制属性(不包含游戏对象),syncGo的话结束值变化会同步到m_go
    public override void OnDraw(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        DrawCommonDuation(comp, h);
        OnDrawMid(comp, h, onOpenWnd, syncGo);
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
        DrawCommonDuationMid(comp, h);
    }
#endif
}
