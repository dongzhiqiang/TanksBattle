//-----------------------------------------------------------------
//                          操作的实现类
//-----------------------------------------------------------------
//描述: 由于unity序列化的一些限制，必须独立出这个类，并且这个类只实
//      现操作，不持有任何数据.数据在回调函数中从Handle类取
//-----------------------------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class IHandle
{
    protected bool m_minShowCurve = false;
    bool m_isPass = false;
    bool m_isStop = false;
    bool IsPass { get { return m_isPass; } set { m_isPass = value; } }
    public virtual bool IsDurationValid { get { return false; } }
    
    //捕获开始值
    public virtual void OnUseNowStart(Handle h){}

    public virtual void OnStart(Handle h) { IsPass = false; }

    public void Update(Handle h, float factor, bool stopIfPlay)
    {
        m_isStop = stopIfPlay;
        if (stopIfPlay)
        {
            OnUpdate(h, factor);
            return;
        }
        
        if (!IsPass)
            OnUpdate(h, factor);
    }

    public virtual void OnUpdate(Handle h, float factor) { if (!IsDurationValid) End(h); }

    public void End(Handle h)
    {
        if (m_isStop)
        {
            OnEnd(h);
            return;
        }

        if (!IsPass)
        {
            OnEnd(h);
            IsPass = true;
        }
    }
    //设到结束
    public virtual void OnEnd(Handle h) { }

    //不在运行中时，Handle的类型或者m_go改变的时候会刷新值,对非持续性的默认都重置
    public virtual void OnReset(Handle h, bool resetBegin = true, bool resetEnd = false) { }
    

#if UNITY_EDITOR
    //框架，绘制游戏对象
    public virtual bool OnDrawGo(Component comp, Handle h, string title = null)
    {
        return false;
    }

    //框架，绘制属性(不包含游戏对象),syncGo的话结束值变化会同步到m_go。这里会绘制所有属性
    public virtual void OnDraw(Component comp, Handle h,  System.Action<Handle.WndType,object> onOpenWnd, bool syncGo = false)
    {
        
    }

    //框架，绘制属性(不包含游戏对象)，syncGo的话结束值变化会同步到m_go。这里是最小化绘制，只绘制最需要的属性
    public virtual void OnDrawMin(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {

    }

    //框架，绘制属性(不包含游戏对象)，syncGo的话结束值变化会同步到m_go。这里绘制的属性会比OnDrawMin多，比OnDraw少
    public virtual void OnDrawMid(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {

    }

    //绘制曲线
    public void DrawAnimationCurve(Component comp, Handle h,string text, string key = null,float space=0)
    {
        if (!IsDurationValid)
            return;
        if (!EditorUtil.DrawHeader(text, key, space))
        {
            GUI.changed = false;
            AnimationCurve curve = UnityEditor.EditorGUILayout.CurveField("变化曲线", h.m_animationCurve, GUILayout.Width(150f), GUILayout.Height(30f));
            if (GUI.changed)
            {
                h.m_animationCurve = curve;
                EditorUtil.SetDirty(comp);
            }
        }
    }

    //绘制即时操作
    public static void DrawCommonImmediate(Component comp, Handle h)
    {
        if (EditorUtil.DrawHeader("即时属性"))
        {
            using (new AutoContent())
            {
                GUI.changed = false;
                float delay = UnityEditor.EditorGUILayout.FloatField("延迟", h.m_delay);
                if (GUI.changed)
                {
                    EditorUtil.RegisterUndo("Handle Change", comp);
                    h.m_delay = delay;
                    EditorUtil.SetDirty(comp);
                }
            }
            
        }
    }

    //绘制持续性操作
    public static void DrawCommonDuation(Component comp, Handle h)
    {
        if (EditorUtil.DrawHeader("持续属性"))
        {
            using (new AutoContent())
            {
                GUI.changed = false;
                float delay = UnityEditor.EditorGUILayout.FloatField("延迟", h.m_delay);
                float duration = UnityEditor.EditorGUILayout.FloatField("持续时间", h.m_duration);
                float rate = UnityEditor.EditorGUILayout.FloatField("倍速", h.m_rate);
                bool isRealtime = UnityEditor.EditorGUILayout.Toggle("真实时间", h.m_isRealtime);
                int playType = UnityEditor.EditorGUILayout.Popup("类型", (int)h.m_playType, Handle.PlayTypeName);
                int endCount = h.m_endCount;
                if (h.IsEndCountValid)
                {
                    endCount = UnityEditor.EditorGUILayout.IntField("循环次数", h.m_endCount);
                }
                AnimationCurve curve = UnityEditor.EditorGUILayout.CurveField("变化曲线", h.m_animationCurve, GUILayout.Width(150f), GUILayout.Height(30f));
                if (GUI.changed)
                {
                    EditorUtil.RegisterUndo("Handle Change", comp);
                    h.m_delay = delay;
                    h.m_duration = duration;
                    h.m_rate = rate;
                    h.m_isRealtime = isRealtime;
                    h.m_playType = (Handle.PlayType)playType;
                    h.m_animationCurve = curve;
                    h.m_endCount = endCount;
                    EditorUtil.SetDirty(comp);
                }
            }

        }
    }

    //绘制曲线，带隐藏显示按钮
    public void DrawCommonDuationMid(Component comp, Handle h)
    {
        GUI.changed = false;
        float duration = UnityEditor.EditorGUILayout.FloatField("持续时间", h.m_duration);
        int playType = UnityEditor.EditorGUILayout.Popup("类型", (int)h.m_playType, Handle.PlayTypeName);
        int endCount = h.m_endCount;
        AnimationCurve curve = h.m_animationCurve;

        if (h.IsEndCountValid)
            endCount = UnityEditor.EditorGUILayout.IntField("循环次数", h.m_endCount);

        //这里加一个可以设置播放曲线的按钮
        if(!h.m_isDurationInvalid ){
            using (new AutoBeginHorizontal())
            {
                if (m_minShowCurve)
                    curve = UnityEditor.EditorGUILayout.CurveField("变化曲线", h.m_animationCurve, GUILayout.Width(150f), GUILayout.Height(30f));
                else
                    UnityEditor.EditorGUILayout.PrefixLabel("变化曲线");
                if (GUILayout.Button(m_minShowCurve ? "\u25BC" : "\u25BA", GUILayout.Width(20)))
                    m_minShowCurve = !m_minShowCurve;
            }
        }
        
        if (GUI.changed)
        {
            EditorUtil.RegisterUndo("Handle Change", comp);
            h.m_duration = duration;
            h.m_playType = (Handle.PlayType)playType;
            h.m_endCount = endCount;
            h.m_animationCurve = curve;
            EditorUtil.SetDirty(comp);
        }
    }
    //绘制游戏对象
    public bool DrawGoField(Component comp, Handle h, string title = "对象")
    {
        GUILayout.BeginHorizontal();
        GameObject newGo = (GameObject)UnityEditor.EditorGUILayout.ObjectField(title, h.m_go, typeof(GameObject), true);
        if (GUILayout.Button(UnityEditor.EditorGUIUtility.IconContent(h.ingore ? "ViewToolOrbit" : "ViewToolOrbit On"), UnityEditor.EditorStyles.toolbarButton, GUILayout.Width(25)))
        {
            h.ingore = !h.ingore;
            EditorUtil.SetDirty(comp);
        }

        GUILayout.EndHorizontal();
        if (h.m_go != newGo)
        {
            EditorUtil.RegisterUndo("Handle Change", comp);
            h.m_go = newGo;
            if (Application.isEditor && !UnityEditor.EditorApplication.isPlaying)
            {
                OnReset(h);
            }              
            EditorUtil.SetDirty(comp);
            return true;
        }
        return false;
    }

    //绘制component
    public bool DrawGoField<T>(Component comp, Handle h, string title = "对象") where T : Component
    {
        
        GameObject oldGo = h.m_go;
        GameObject newGo = null;
        GUILayout.BeginHorizontal();
        T t = (T)UnityEditor.EditorGUILayout.ObjectField( title, oldGo == null ? null : oldGo.GetComponent<T>(), typeof(T), true);
        newGo = t == null ? null : t.gameObject;
        if (GUILayout.Button(UnityEditor.EditorGUIUtility.IconContent(h.ingore ? "ViewToolOrbit" : "ViewToolOrbit On"), UnityEditor.EditorStyles.toolbarButton,GUILayout.Width(25)))
        {
            h.ingore =!h.ingore;
            EditorUtil.SetDirty(comp);
        } 
        
        GUILayout.EndHorizontal();
        if (oldGo != newGo )
        {
            EditorUtil.RegisterUndo("Handle Change", comp);
            h.m_go = newGo;
            if (Application.isEditor && !UnityEditor.EditorApplication.isPlaying)
            {
                OnReset(h);
            }        
            EditorUtil.SetDirty(comp);
            return true;
        }
        
        return false;
    }



    public void DrawStartEndSlider(Component comp, Handle h, bool haveBegin, bool haveBeginTitle, bool haveUseNowStart, bool haveEndTitle, bool showCurveWithEnd, bool syncGo, float leftValue, float rightValue, System.Func<Handle, float> syncFun, System.Action onDrawBeforeEnd = null)
    {
        GUI.changed = false;
        float begin = h.m_fBegin;
        float end = h.m_fEnd;
        bool isUseNowStart = h.m_isUseNowStart;
        AnimationCurve curve = h.m_animationCurve;

        if (haveBegin)
        {
            isUseNowStart = !haveUseNowStart ? isUseNowStart : UnityEditor.EditorGUILayout.Toggle("无开始值", h.m_isUseNowStart);
            if (!isUseNowStart)
            {
                using (new AutoBeginHorizontal())
                {
                    if (haveBeginTitle) UnityEditor.EditorGUILayout.PrefixLabel("开始");
                    begin = UnityEditor.EditorGUILayout.Slider("", h.m_fBegin, 0f, 1f);
                    //这里加一个可以同步的按钮
                    if (haveBeginTitle&&h.m_go && GUILayout.Button("同步", GUILayout.Width(35)))
                    {
                        begin = syncFun(h);
                    }
                }
            }
        }
        using (new AutoBeginHorizontal())
        {
            //这里加一个可以设置播放曲线的按钮
            if (!h.m_isDurationInvalid && showCurveWithEnd && GUILayout.Button(m_minShowCurve ? "\u25BC" : "\u25BA", GUILayout.Width(20)))
                m_minShowCurve = !m_minShowCurve;

            if (onDrawBeforeEnd != null)
                onDrawBeforeEnd();

            if (haveEndTitle) UnityEditor.EditorGUILayout.PrefixLabel("结束");
            end = UnityEditor.EditorGUILayout.Slider("", h.m_fEnd, 0f, 1f);
            //这里加一个可以同步的按钮
            if (haveEndTitle &&h.m_go && GUILayout.Button("同步", GUILayout.Width(35)))
                end = syncFun(h);
        }
        if (!h.m_isDurationInvalid && showCurveWithEnd && m_minShowCurve)
            curve = UnityEditor.EditorGUILayout.CurveField("变化曲线", h.m_animationCurve, GUILayout.Width(150f), GUILayout.Height(30f));
        if (GUI.changed)
        {
            if (h.m_fEnd != end && syncGo && h.m_go)
            {
                h.m_fEnd = end;
                End(h);
                EditorUtil.SetDirty(h.m_go);
            }

            EditorUtil.RegisterUndo("Handle Change", comp);
            h.m_fBegin = begin;
            h.m_fEnd = end;
            h.m_isUseNowStart = isUseNowStart;
            h.m_animationCurve = curve;
            EditorUtil.SetDirty(comp);
        }
    }

    public void DrawStartEndInt(Component comp, Handle h, bool haveBegin, bool haveBeginTitle, bool haveUseNowStart, bool haveEndTitle, bool showCurveWithEnd, bool syncGo, System.Func<Handle, int> syncFun,System.Action onDrawBeforeEnd = null)
    {
        GUI.changed = false;
        int begin = h.m_iBegin;
        int end = h.m_iEnd;
        bool isUseNowStart = h.m_isUseNowStart;
        AnimationCurve curve = h.m_animationCurve;

        if (haveBegin)
        {
            isUseNowStart = !haveUseNowStart ? isUseNowStart : UnityEditor.EditorGUILayout.Toggle("无开始值", h.m_isUseNowStart);
            if (!isUseNowStart)
            {
                using (new AutoBeginHorizontal())
                {
                    if (haveBeginTitle) UnityEditor.EditorGUILayout.PrefixLabel("开始");
                    begin = UnityEditor.EditorGUILayout.IntField( "", h.m_iBegin);
                    //这里加一个可以同步的按钮
                    if (haveBeginTitle && h.m_go && GUILayout.Button("同步", GUILayout.Width(35)))
                    {
                        begin = syncFun(h);
                    }
                }
            }
        }
        using (new AutoBeginHorizontal())
        {
            //这里加一个可以设置播放曲线的按钮
            if (!h.m_isDurationInvalid && showCurveWithEnd && GUILayout.Button(m_minShowCurve ? "\u25BC" : "\u25BA", GUILayout.Width(20)))
                m_minShowCurve = !m_minShowCurve;

            if (onDrawBeforeEnd != null)
                onDrawBeforeEnd();

            if (haveEndTitle) UnityEditor.EditorGUILayout.PrefixLabel("结束");
            end = UnityEditor.EditorGUILayout.IntField("", h.m_iEnd);
            //这里加一个可以同步的按钮
            if (haveEndTitle && h.m_go && GUILayout.Button("同步", GUILayout.Width(35)))
                end = syncFun(h);
        }
        if (!h.m_isDurationInvalid && showCurveWithEnd && m_minShowCurve)
            curve = UnityEditor.EditorGUILayout.CurveField("变化曲线", h.m_animationCurve, GUILayout.Width(150f), GUILayout.Height(30f));
        if (GUI.changed)
        {
            if (h.m_iEnd != end && syncGo && h.m_go)
            {
                h.m_iEnd = end;
                End(h);
                EditorUtil.SetDirty(h.m_go);
            }

            EditorUtil.RegisterUndo("Handle Change", comp);
            h.m_iBegin = begin;
            h.m_iEnd = end;
            h.m_isUseNowStart = isUseNowStart;
            h.m_animationCurve = curve;
            EditorUtil.SetDirty(comp);
        }
    }

    public void DrawStartEndV3(Component comp, Handle h, bool haveBegin, bool haveBeginTitle, bool haveUseNowStart, bool haveEndTitle, bool showCurveWithEnd, bool syncGo, System.Func<Handle, Vector3> syncFun, System.Action onDrawBeforeEnd = null)
    {
        GUI.changed = false;
        Vector3 begin = h.m_vBegin;
        Vector3 end = h.m_vEnd;
        bool isUseNowStart = h.m_isUseNowStart;
        AnimationCurve curve = h.m_animationCurve;

        if (haveBegin)
        {
            isUseNowStart = !haveUseNowStart ? isUseNowStart : UnityEditor.EditorGUILayout.Toggle("无开始值", h.m_isUseNowStart);
            if (!isUseNowStart)
            {
                using (new AutoBeginHorizontal())
                {
                    if (haveBeginTitle) UnityEditor.EditorGUILayout.PrefixLabel("开始");
                    begin = UnityEditor.EditorGUILayout.Vector3Field("", h.m_vBegin); 
                    //这里加一个可以同步的按钮
                    if (haveBeginTitle && h.m_go && GUILayout.Button("同步", GUILayout.Width(35)))
                    {
                        begin = syncFun(h);
                    }
                }
            }
        }
        using (new AutoBeginHorizontal())
        {
            //这里加一个可以设置播放曲线的按钮
            if (!h.m_isDurationInvalid &&showCurveWithEnd && GUILayout.Button(m_minShowCurve ? "\u25BC" : "\u25BA", GUILayout.Width(20)))
                m_minShowCurve = !m_minShowCurve;

            if (onDrawBeforeEnd != null)
                onDrawBeforeEnd();

            if (haveEndTitle) UnityEditor.EditorGUILayout.PrefixLabel("结束");
            end = UnityEditor.EditorGUILayout.Vector3Field("", h.m_vEnd);
            //这里加一个可以同步的按钮
            if (haveEndTitle && h.m_go && GUILayout.Button("同步", GUILayout.Width(35)))
                end = syncFun(h);
        }
        if (!h.m_isDurationInvalid && showCurveWithEnd && m_minShowCurve)
            curve = UnityEditor.EditorGUILayout.CurveField("变化曲线", h.m_animationCurve, GUILayout.Width(150f), GUILayout.Height(30f));
        if (GUI.changed)
        {
            if (h.m_vEnd != end && syncGo && h.m_go)
            {
                h.m_vEnd = end;
                OnEnd(h);
                EditorUtil.SetDirty(h.m_go);
            }

            EditorUtil.RegisterUndo("Handle Change", comp);
            h.m_vBegin = begin;
            h.m_vEnd = end;
            h.m_isUseNowStart = isUseNowStart;
            h.m_animationCurve = curve;
            EditorUtil.SetDirty(comp);
        }
    }

    public void DrawStartEndColor(Component comp, Handle h, bool haveBegin, bool haveBeginTitle, bool haveUseNowStart, bool haveEndTitle, bool showCurveWithEnd, bool syncGo, System.Func<Handle, Color> syncFun, System.Action onDrawBeforeEnd = null)
    {
        GUI.changed = false;
        Color begin = h.m_cBegin;
        Color end = h.m_cEnd;
        bool isUseNowStart = h.m_isUseNowStart;
        AnimationCurve curve = h.m_animationCurve;

        if (haveBegin)
        {
            isUseNowStart = !haveUseNowStart ? isUseNowStart : UnityEditor.EditorGUILayout.Toggle("无开始值", h.m_isUseNowStart);
            if (!isUseNowStart)
            {
                using (new AutoBeginHorizontal())
                {
                    if (haveBeginTitle) UnityEditor.EditorGUILayout.PrefixLabel("开始");
                    begin = UnityEditor.EditorGUILayout.ColorField( "", h.m_cBegin);
                    //这里加一个可以同步的按钮
                    if (haveBeginTitle && h.m_go && GUILayout.Button("同步", GUILayout.Width(35)))
                    {
                        begin = syncFun(h);
                    }
                }
            }
        }
        using (new AutoBeginHorizontal())
        {
            //这里加一个可以设置播放曲线的按钮
            if (!h.m_isDurationInvalid && showCurveWithEnd && GUILayout.Button(m_minShowCurve ? "\u25BC" : "\u25BA", GUILayout.Width(20)))
                m_minShowCurve = !m_minShowCurve;

            if (onDrawBeforeEnd != null)
                onDrawBeforeEnd();

            if (haveEndTitle) UnityEditor.EditorGUILayout.PrefixLabel("结束");
            end = UnityEditor.EditorGUILayout.ColorField( "", h.m_cEnd); 
            //这里加一个可以同步的按钮
            if (haveEndTitle && h.m_go && GUILayout.Button("同步", GUILayout.Width(35)))
                end = syncFun(h);
        }
        if (!h.m_isDurationInvalid && showCurveWithEnd && m_minShowCurve)
            curve = UnityEditor.EditorGUILayout.CurveField("变化曲线", h.m_animationCurve, GUILayout.Width(150f), GUILayout.Height(30f));
        if (GUI.changed)
        {
            if (h.m_cEnd != end && syncGo && h.m_go)
            {
                h.m_cEnd = end;
                End(h);
                EditorUtil.SetDirty(h.m_go);
            }

            EditorUtil.RegisterUndo("Handle Change", comp);
            h.m_cBegin = begin;
            h.m_cEnd = end;
            h.m_isUseNowStart = isUseNowStart;
            h.m_animationCurve = curve;
            EditorUtil.SetDirty(comp);
        }
    }
#endif
    
    
}
