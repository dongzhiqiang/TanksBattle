using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class HandleColor : IHandle
{
    public override bool IsDurationValid { get { return true; } }

    Color GetColor(Handle h)
    {
        if (!h.m_go)
        {
            Debuger.LogError("游戏对象为空");
            return Color.white;
        }
        Graphic g = h.m_go.GetComponent<Graphic>();
        if (g)
            return g.color;

        Renderer r = h.m_go.GetComponent<Renderer>();
        if (r && r.material)
            return r.material.color;

        SpriteRenderer s = h.m_go.GetComponent<SpriteRenderer>();
        if (s )
            return s.color;

        Light l = h.m_go.GetComponent<Light>();
        if(l)
            return l.color;

        return Color.white;
    }

    void SetColor(Handle h,Color c)
    {
        if (!h.m_go)
        {
            Debuger.LogError("游戏对象为空");
            return ;
        }
        Graphic g = h.m_go.GetComponent<Graphic>();
        if (g)
        {
            g.color=c;
            return;
        }
             

        Renderer r = h.m_go.GetComponent<Renderer>();
        if (r && r.material)
        {
            r.material.color = c;
            return;
        }

        SpriteRenderer s = h.m_go.GetComponent<SpriteRenderer>();
        if (s)
        {
            s.color = c;
            return;
        }

        Light l = h.m_go.GetComponent<Light>();
        if (l)
        {
            l.color = c;
            return;
        }
    }
    //开始时调用，可以捕获开始值
    public override void OnUseNowStart(Handle h)
    {
        if (h.m_go == null)
            return;

        h.m_cBeginNow = GetColor(h);
    }

    public override void OnUpdate(Handle h, float factor) {
        if (h.m_go == null)
            return;
        if (h.m_isUseNowStart)
            SetColor(h, Color.Lerp(h.m_cBeginNow, h.m_cEnd, factor) );
        else
            SetColor(h, Color.Lerp(h.m_cBegin, h.m_cEnd, factor));
                
    }

    public override void OnEnd(Handle h)
    {
        if (h.m_go == null)
            return;
        SetColor(h, h.m_cEnd);        
    }

    //不在运行中时，Handle的类型或者m_go改变的时候会刷新值
    public override void OnReset(Handle h, bool resetBegin = true, bool resetEnd = false)
    {
        if (h.m_go)
        {
            Color c =  GetColor(h);
            if(resetBegin)
                h.m_cBegin =c;

            if(resetEnd)
                h.m_cEnd= c;
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
        DrawStartEndColor(comp, h, true, true, true, true, false, syncGo, GetColor);
        DrawCommonDuation(comp, h);
    }

    
    //框架，绘制属性(不包含游戏对象)，syncGo的话结束值变化会同步到m_go。这里是最小化绘制，只绘制最需要的属性
    public override void OnDrawMin(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        DrawStartEndColor(comp, h, false, false, false, false, true, syncGo, GetColor);
    }

    //框架，绘制属性(不包含游戏对象)，syncGo的话结束值变化会同步到m_go。这里是最小化绘制，只绘制最需要的属性
    public override void OnDrawMid(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        DrawStartEndColor(comp, h, true, true, false, true, false, syncGo, GetColor);
        DrawCommonDuationMid(comp, h);
    }
#endif
}
