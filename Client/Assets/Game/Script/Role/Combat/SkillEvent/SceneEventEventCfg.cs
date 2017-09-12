#region Header
/**
 * 名称：触发关卡事件
 
 * 日期：2016.7.4
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class SceneEventEventCfg : SkillEventCfg
{
    public string sceneEvent = "";
    

    public override enSkillEventType Type { get { return enSkillEventType.sceneEvent; } }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {
        switch (col)
        {
            case 0: if (h(ref r, "关卡事件名", COL_WIDTH * 4)) onTip("要触发的关卡事件名"); return false;
            default: return true;
        }
    }
    public override bool DrawGrid(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int row, int col, int totalRow, ref bool change, Transform tran)
    {
        switch (col)
        {
            case 0:
                {
                    r.width = COL_WIDTH * 4;
                    sceneEvent = EditorGUI.TextField(r, sceneEvent);
                    r.x += r.width;
                }; return false;
            
            default: return true;
        }
    }
#endif

    
    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {
        SceneEventMgr.instance.FireAction(sceneEvent);
        return true;
    }
}