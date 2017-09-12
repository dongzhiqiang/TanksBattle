/*
 * *********************************************************
 * 名称：处理序列类
 
 * 日期：2015.7.23
 * 描述：
 * 1.按照延迟时间顺序地播放多个子处理
 * *********************************************************
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class HandleSequence : IHandle
{
    
    public override bool IsDurationValid { get { return true; } }

    public void SetTime(Handle h, float time)
    {
        if (h.m_go == null) return;
        SequenceHandle seq = h.m_go.GetComponent<SequenceHandle>();
        if (seq == null) return;
        foreach (var sub in seq.m_handles)
        {
            sub.SetTime(time, false, false, true);
            
        }
    }

    public void Start(Handle h)
    {
        if (h.m_go == null) return;
        SequenceHandle seq = h.m_go.GetComponent<SequenceHandle>();
        if (seq == null) return;
        foreach (var sub in seq.m_handles)
        {
            sub.Start();
        }
    }

    public override void OnUpdate(Handle h, float factor) {
        SetTime(h, h.m_delay + factor * h.Duration);        
    }

    public override void OnEnd(Handle h)
    {
        SetTime(h, h.m_delay + h.Duration);
    }

    //不在运行中时，Handle的类型或者m_go改变的时候会刷新值
    public override void OnReset(Handle h, bool resetBegin = true, bool resetEnd = false)
    {
        return;
    }

#if UNITY_EDITOR
    //返回值表示有没有改变
    public override bool OnDrawGo(Component comp, Handle h, string title = null)
    {
        return DrawGoField<SequenceHandle>(comp, h, title);
    }

    //框架，绘制属性(不包含游戏对象),syncGo的话结束值变化会同步到m_go
    public override void OnDraw(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        if (GUILayout.Button("打开动画序列编辑器"))
        {
            onOpenWnd(Handle.WndType.sequenceEditor, new object[] { comp, h });
        }
    }

    //框架，绘制属性(不包含游戏对象)，syncGo的话结束值变化会同步到m_go。这里是最小化绘制，只绘制最需要的属性
    public override void OnDrawMin(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        if (GUILayout.Button("打开动画序列编辑器"))
        {
            onOpenWnd(Handle.WndType.sequenceEditor, new object[] { comp, h });
        }
    }

    //框架，绘制属性(不包含游戏对象)，syncGo的话结束值变化会同步到m_go。这里是最小化绘制，只绘制最需要的属性
    public override void OnDrawMid(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        if (GUILayout.Button("打开动画序列编辑器"))
        {
            onOpenWnd(Handle.WndType.sequenceEditor, new object[] { comp, h });
        }
    }

#endif

    

}
