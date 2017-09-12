#region Header
/**
 * 名称：时间缩放事件
 
 * 日期：2016.2.26
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

//时间缩放
public class TimeScaleEventCfg : SkillEventCfg
{
    public float scale= 1;//时间缩放
    public float duration = 1;//持续时间
    
    public override enSkillEventType Type { get { return enSkillEventType.timeScale; } }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r,  SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {
        switch (col)
        {
            case 0: if (h(ref r, "缩放", COL_WIDTH * 2)) onTip("时间缩放"); return false;
            case 1: if (h(ref r, "时间", COL_WIDTH * 2)) onTip("持续时间"); return false;
            default:return true;
        }
    }
    public override bool DrawGrid(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int row, int col, int totalRow, ref bool change, Transform tran)
    {
        switch (col)
        {
            case 0:
                {
                    r.width = COL_WIDTH * 2;
                    scale = EditorGUI.FloatField(r, GUIContent.none, scale);
                    r.x += COL_WIDTH * 2;
                }; return false;
            case 1:
                {
                    r.width = COL_WIDTH * 2;
                    duration = EditorGUI.FloatField(r, GUIContent.none, duration);
                    r.x += COL_WIDTH * 2;
                }; return false;
            default: return true;
        }
    }
#endif
    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {
        if (scale <= 0 )
        {
            Debuger.LogError("时间缩放事件，缩放必须大于0:", source.Cfg.id);
            return false;
        }
        if (duration <= 0)
        {
            Debuger.LogError("时间缩放事件，持续时间不能必须填:", source.Cfg.id);
            return false;
        }

        if (source != target)
        {
            Debuger.LogError("时间缩放事件，作用对象必须为释放者本身，有其他需求请与程序沟通");
            return false;
        }
        if (!target.IsHero)
        {
            return false;
        }

        TimeMgr.instance.AddTimeScale(scale, duration);
        return true;
    }
}