using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class HandleRotate : IHandle
{
    public override bool IsDurationValid { get { return true; } }

    Vector3 GetRotate(Handle h)
    {
        if (!h.m_go)
        {
            Debuger.LogError("游戏对象为空");
            return Vector3.zero;
        }
        Transform t = h.m_go.transform;
        return t.localEulerAngles;
    }

    void SetRotate(Handle h, Vector3 euler)
    {
        if (!h.m_go)
        {
            Debuger.LogError("游戏对象为空");
            return;
        }
        Transform t = h.m_go.transform;
        t.localEulerAngles = euler;
    }

    void SetRotate(Handle h,Quaternion q)
    {
        if (!h.m_go)
        {
            Debuger.LogError("游戏对象为空");
            return;
        }
        Transform t = h.m_go.transform;
        t.localRotation= q;
    }
    public override void OnUseNowStart(Handle h)
    {
        if (h.m_go == null)
            return;

        h.m_vBeginNow = GetRotate(h);
    }

    public override void OnUpdate(Handle h, float factor) {
        if (h.m_go == null)
            return;
        if (h.m_b1)//最近夹角
        {
            if (h.m_isUseNowStart)
                SetRotate(h, Quaternion.SlerpUnclamped(Quaternion.Euler(h.m_vBeginNow), Quaternion.Euler(h.m_vEnd), factor));
            else
                SetRotate(h, Quaternion.SlerpUnclamped(Quaternion.Euler(h.m_vBegin), Quaternion.Euler(h.m_vEnd), factor));              
        }
        else
        {
            if (h.m_isUseNowStart)
            {
                SetRotate(h, Quaternion.Euler(new Vector3(
                        Mathf.Lerp(h.m_vBeginNow.x, h.m_vEnd.x, factor),
                        Mathf.Lerp(h.m_vBeginNow.y, h.m_vEnd.y, factor),
                        Mathf.Lerp(h.m_vBeginNow.z, h.m_vEnd.z, factor))));
               
            }
            else
            {
                SetRotate(h, Quaternion.Euler(new Vector3(
                        Mathf.Lerp(h.m_vBegin.x, h.m_vEnd.x, factor),
                        Mathf.Lerp(h.m_vBegin.y, h.m_vEnd.y, factor),
                        Mathf.Lerp(h.m_vBegin.z, h.m_vEnd.z, factor))));
               
            }
            //if (h.m_go.name == "di")
            //    Debuger.Log("factor:{0:F1}  Lerp:{1:F1} LerpUnClamp:{2:F1} LerpAngle:{3:F1}",
            //       factor,
            //       Mathf.Lerp(h.m_vBeginNow.z, h.m_vEnd.z, factor),
            //       Mathf.LerpUnclamped(h.m_vBeginNow.z, h.m_vEnd.z, factor),
            //       Mathf.LerpAngle(h.m_vBeginNow.z, h.m_vEnd.z, factor)
            //       );
        }
        
        
    }

    public override void OnEnd(Handle h)
    {
        if (h.m_go == null)
            return;
        SetRotate(h, h.m_vEnd);        
    }

    //不在运行中时，Handle的类型或者m_go改变的时候会刷新值
    public override void OnReset(Handle h, bool resetBegin = true, bool resetEnd = false)
    {
        if (h.m_go)
        {
            Vector3 v =  GetRotate(h);
            if(resetBegin)
                h.m_vBegin =v;

            if(resetEnd)
                h.m_vEnd= v;
        }
    }

#if UNITY_EDITOR
    public override bool OnDrawGo(Component comp, Handle h, string title = null)
    {
        return DrawGoField<Transform>(comp, h, title);
    }

    //框架，绘制属性(不包含游戏对象),syncGo的话结束值变化会同步到m_go
    public override void OnDraw(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        GUI.changed = false;
        bool isQuatSlerp = UnityEditor.EditorGUILayout.Toggle("最近夹角", h.m_b1);
        if (GUI.changed)
        {
            EditorUtil.RegisterUndo("Handle Change", comp);
            h.m_b1 = isQuatSlerp;
            EditorUtil.SetDirty(comp);
        }
        DrawStartEndV3(comp, h, true, true, true, true, false, syncGo, GetRotate);
        DrawCommonDuation(comp, h);
    }


    //框架，绘制属性(不包含游戏对象)，syncGo的话结束值变化会同步到m_go。这里是最小化绘制，只绘制最需要的属性
    public override void OnDrawMin(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        DrawStartEndV3(comp, h, false, false, false, false, true, syncGo, GetRotate);
        //if (m_minShowCurve)
        //{
        //    GUI.changed = false;
        //    bool isQuatSlerp = UnityEditor.EditorGUILayout.Toggle("最近夹角", h.m_b1);
        //    if (GUI.changed)
        //    {
        //        EditorUtil.RegisterUndo("Handle Change", comp);
        //        h.m_b1 = isQuatSlerp;
        //        EditorUtil.SetDirty(comp);
        //    }
        //}
    }

    //框架，绘制属性(不包含游戏对象)，syncGo的话结束值变化会同步到m_go。这里是最小化绘制，只绘制最需要的属性
    public override void OnDrawMid(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        GUI.changed = false;
        bool isQuatSlerp = UnityEditor.EditorGUILayout.Toggle("最近夹角", h.m_b1);
        if (GUI.changed)
        {
            EditorUtil.RegisterUndo("Handle Change", comp);
            h.m_b1 = isQuatSlerp;
            EditorUtil.SetDirty(comp);
        }
        DrawStartEndV3(comp, h, true, true, false, true, false, syncGo, GetRotate);
        DrawCommonDuationMid(comp, h);
    }

#endif
}
