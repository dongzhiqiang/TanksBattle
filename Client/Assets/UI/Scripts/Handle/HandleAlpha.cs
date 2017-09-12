using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class HandleAlpha : IHandle
{
    public override bool IsDurationValid { get { return true; } }

    float GetAlpha(Handle h)
    {
        if (!h.m_go)
        {
            Debuger.LogError("游戏对象为空");
            return 1f;
        }

        CanvasGroup c = h.m_go.GetComponent<CanvasGroup>();
        if(c)
            return c.alpha;

        Graphic g = h.m_go.GetComponent<Graphic>();
        if (g)
            return g.color.a;

        return 1f;
    }

    void SetAlpha(Handle h,float a)
    {
        if (!h.m_go)
        {
            Debuger.LogError("游戏对象为空");
            return ;
        }

        CanvasGroup c = h.m_go.GetComponent<CanvasGroup>();
        if (c)
        {
            c.alpha =a;
        }
        Graphic g = h.m_go.GetComponent<Graphic>();
        if (g)
        {
            Color clr =g.color;
            clr.a = a;
            g.color = clr;
            return;
        }
        
    }

    //开始时调用，可以捕获开始值
    public override void OnUseNowStart(Handle h)
    {
        if (h.m_go == null)
            return;
        
        h.m_fBeginNow = GetAlpha(h);
    }
    
    public override void OnUpdate(Handle h, float factor) {
        if (h.m_go == null)
            return;
        if (h.m_isUseNowStart)
            SetAlpha(h, Mathf.Lerp(h.m_fBeginNow, h.m_fEnd, factor));
        else
            SetAlpha(h, Mathf.Lerp(h.m_fBegin, h.m_fEnd, factor));
                
    }

    public override void OnEnd(Handle h)
    {
        if (h.m_go == null)
            return;
        SetAlpha(h, h.m_fEnd);        
    }

    //不在运行中时，Handle的类型或者m_go改变的时候会刷新值
    public override void OnReset(Handle h, bool resetBegin = true, bool resetEnd = false)
    {
        if (h.m_go)
        {
            float a = GetAlpha(h);
            if (resetBegin)
                h.m_fBegin = a;
            if(resetEnd)
                h.m_fEnd= a;
        }
    }

#if UNITY_EDITOR
    public override bool OnDrawGo(Component comp, Handle h, string title = null)
    {
        return DrawGoField(comp, h, title);
    }

    //框架，绘制属性(不包含游戏对象),syncGo的话结束值变化会同步到m_go
    public override void OnDraw(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        DrawStartEndSlider(comp, h, true, true, true, true,false, syncGo, 0f, 1f, GetAlpha);
        DrawCommonDuation(comp, h);
    }

    
    //框架，绘制属性(不包含游戏对象)，syncGo的话结束值变化会同步到m_go。这里是最小化绘制，只绘制最需要的属性
    public override void OnDrawMin(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        DrawStartEndSlider(comp, h, false, false, false, false, true, syncGo, 0f, 1f, GetAlpha);
    }

    
    //框架，绘制属性(不包含游戏对象)，syncGo的话结束值变化会同步到m_go。这里是最小化绘制，只绘制最需要的属性
    public override void OnDrawMid(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        DrawStartEndSlider(comp, h, true, true, false, true, false, syncGo, 0f, 1f, GetAlpha);
        DrawCommonDuationMid(comp, h);
    }
#endif
}
