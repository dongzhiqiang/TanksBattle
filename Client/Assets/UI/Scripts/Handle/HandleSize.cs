using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class HandleSize : IHandle
{
    public override bool IsDurationValid { get { return true; } }

    Vector3 GetSize(Handle h)
    {
        if (!h.m_go)
        {
            Debuger.LogError("游戏对象为空");
            return Vector3.zero;
        }

        var le = h.m_go.GetComponent<LayoutElement>();
        if (le)
        {
            return new Vector3(le.minWidth, le.minHeight, 0);
        }
        else
        {
            Transform t = h.m_go.transform;
            RectTransform rt = t as RectTransform;
            return rt.sizeDelta;
        }        
    }

    void SetSize(Handle h,Vector3 size)
    {
        if (!h.m_go)
        {
            Debuger.LogError("游戏对象为空");
            return ;
        }

        var le = h.m_go.GetComponent<LayoutElement>();
        if (le)
        {
            le.minWidth = size.x;
            le.minHeight = size.y;
        }
        else
        {
            Transform t = h.m_go.transform;
            RectTransform rt = t as RectTransform;
            rt.sizeDelta = size;
        }
    }
    public override void OnUseNowStart(Handle h)
    {
        if (h.m_go == null)
            return;

        h.m_vBeginNow = GetSize(h);
    }
    
    public override void OnUpdate(Handle h, float factor) {
        if (h.m_go == null)
            return;
        if (h.m_isUseNowStart)
            SetSize(h, h.m_vBeginNow * (1f - factor) + h.m_vEnd * factor);        
        else
            SetSize(h, h.m_vBegin * (1f - factor) + h.m_vEnd * factor);        
    }

    public override void OnEnd(Handle h)
    {
        if (h.m_go == null)
            return;
        SetSize(h, h.m_vEnd);        
    }

    //不在运行中时，Handle的类型或者m_go改变的时候会刷新值
    public override void OnReset(Handle h, bool resetBegin = true, bool resetEnd = false)
    {
        if (h.m_go)
        {
            Vector3 v = GetSize(h);
            if(resetBegin)
                h.m_vBegin = v;
            if(resetEnd)
                h.m_vEnd= v;
        }
    }

#if UNITY_EDITOR
    public override bool OnDrawGo(Component comp, Handle h, string title = null)
    {
        return DrawGoField<RectTransform>(comp, h, title);
    }
    //框架，绘制属性(不包含游戏对象),syncGo的话结束值变化会同步到m_go
    public override void OnDraw(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        DrawStartEndV3(comp, h, true, true, true, true, false, syncGo, GetSize);
        DrawCommonDuation(comp, h);
    }

    
    //框架，绘制属性(不包含游戏对象)，syncGo的话结束值变化会同步到m_go。这里是最小化绘制，只绘制最需要的属性
    public override void OnDrawMin(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        DrawStartEndV3(comp, h, false, false, false, false, true, syncGo, GetSize);   
    }

    //框架，绘制属性(不包含游戏对象)，syncGo的话结束值变化会同步到m_go。这里是最小化绘制，只绘制最需要的属性
    public override void OnDrawMid(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        DrawStartEndV3(comp, h, true, true, false, true, false, syncGo, GetSize);
        DrawCommonDuationMid(comp, h);
    }
#endif
}
