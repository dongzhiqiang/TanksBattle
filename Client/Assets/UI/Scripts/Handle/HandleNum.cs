using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class HandleNum : IHandle
{
    public override bool IsDurationValid { get { return true; } }

    int GetNum(Handle h)
    {
        if (!h.m_go)
        {
            Debuger.LogError("游戏对象为空");
            return 0;
        }
        Text t = h.m_go.GetComponent<Text>();
        
        if(string.IsNullOrEmpty(t.text))
            return 0;
        int i ;
        if(int.TryParse(t.text,out i))
            return i;
        Debuger.LogError("无法解析为数字");
        return 0;
    }
    void SetNum(Handle h,int num)
    {
        if (!h.m_go)
        {
            Debuger.LogError("游戏对象为空");
            return ;
        }
        Text t = h.m_go.GetComponent<Text>();
        if (string.IsNullOrEmpty(h.m_s1))
            t.text = num.ToString();
        else
        {
            if (h.m_s1.IndexOf("{0}") != -1)
                t.text = string.Format(h.m_s1, num);
            else
                t.text = h.m_s1;
        }
    }

    public override void OnUseNowStart(Handle h)
    {
        if (h.m_go == null)
            return;

        h.m_iBeginNow = GetNum(h);
    }

    public override void OnUpdate(Handle h, float factor) {
        if (h.m_go == null)
            return;
        if (h.m_isUseNowStart)
            SetNum(h, h.m_iBeginNow + (int)((float)(h.m_iEnd - h.m_iBeginNow) * factor));        
        else
            SetNum(h, h.m_iBegin + (int)((float)(h.m_iEnd - h.m_iBegin) * factor));        
    }

    public override void OnEnd(Handle h)
    {
        if (h.m_go == null)
            return;
        SetNum(h, h.m_iEnd);        
    }

    //不在运行中时，Handle的类型或者m_go改变的时候会刷新值
    public override void OnReset(Handle h, bool resetBegin = true, bool resetEnd = false)
    {
        if (h.m_go && h.m_go.GetComponent<Text>())
        {
            int i = GetNum(h); 
            if(resetBegin)
                h.m_iBegin = i;
            if(resetEnd)
                h.m_iEnd= i;
        }
    }

#if UNITY_EDITOR
    public override bool OnDrawGo(Component comp, Handle h, string title = null)
    {
        return DrawGoField<Text>(comp, h, title);
    }

    
    //框架，绘制属性(不包含游戏对象),syncGo的话结束值变化会同步到m_go
    public override void OnDraw(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        DrawStartEndInt(comp, h, true, true, true, true, false, syncGo, GetNum);
        GUI.changed = false;
        string format = UnityEditor.EditorGUILayout.TextField("格式", h.m_s1);
        if (GUI.changed)
        {
            
            EditorUtil.RegisterUndo("Handle Change", comp);
            h.m_s1 = format;
            EditorUtil.SetDirty(comp);
        }
        DrawCommonDuation(comp, h);
    }

    bool m_showFormat = false;
    void DrawShowFormat()
    {
        //这里加一个可以设置格式的按钮
        if (GUILayout.Button(m_showFormat ? "\u25BC" : "\u25BA", GUILayout.Width(20)))
            m_showFormat = !m_showFormat;
    }
    
    //框架，绘制属性(不包含游戏对象)，syncGo的话结束值变化会同步到m_go。这里是最小化绘制，只绘制最需要的属性
    public override void OnDrawMin(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        DrawStartEndInt(comp, h, false, false, false, false, true, syncGo, GetNum, DrawShowFormat);
        GUI.changed = false;
        string format = h.m_s1;
        if (m_showFormat == true)
            format = UnityEditor.EditorGUILayout.TextField("格式", h.m_s1);
        if (GUI.changed)
        {
            EditorUtil.RegisterUndo("Handle Change", comp);
            h.m_s1 = format;
            EditorUtil.SetDirty(comp);
        }
        
    }

    //框架，绘制属性(不包含游戏对象)，syncGo的话结束值变化会同步到m_go。这里是最小化绘制，只绘制最需要的属性
    public override void OnDrawMid(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        DrawStartEndInt(comp, h, true, true, false, true, false, syncGo, GetNum);
        GUI.changed = false;
        string format = UnityEditor.EditorGUILayout.TextField("格式", h.m_s1);
        if (GUI.changed)
        {

            EditorUtil.RegisterUndo("Handle Change", comp);
            h.m_s1 = format;
            EditorUtil.SetDirty(comp);
        }
        DrawCommonDuationMid(comp, h);
    }
#endif
}
