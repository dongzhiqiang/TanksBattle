using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CameraChangeEventCfg : SkillEventCfg
{
    public string cameraName;
    public float stayTime;

    public override enSkillEventType Type { get { return enSkillEventType.cameraChange; } }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {

        switch (col)
        {
            case 0: if (h(ref r, "镜头名", COL_WIDTH * 4)) onTip("要切换的镜头名"); return false;
            case 1: if (h(ref r, "持续时间", COL_WIDTH * 3)) onTip("持续时间后切回镜头"); return false;
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
                    cameraName = EditorGUI.TextField(r, cameraName);
                    r.x += r.width;
                }; return false;
            case 1:
                {
                    r.width = COL_WIDTH * 3;
                    stayTime = EditorGUI.FloatField(r, GUIContent.none, stayTime);
                    r.x += r.width;
                }; return false;
            default: return true;
        }
    }
#endif
    
    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {
        CameraTriggerGroup group = CameraTriggerMgr.instance.CurGroup;
        if (group == null)
        {
            Debug.LogError("当前无镜头组");
            return false;
        }

        CameraTrigger trigger = group.GetTriggerByName(cameraName);
        if (trigger == null)
        {
            Debug.LogError("没找到镜头名 ： "+ cameraName);
            return false;
        }
        trigger.m_info.duration = stayTime;
        trigger.m_info.priority = 11;
        CameraMgr.instance.Add(trigger.m_info);

        return true;
    }

}
