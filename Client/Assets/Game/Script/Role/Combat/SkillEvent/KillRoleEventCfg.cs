#region Header
/**
 * 名称：杀死角色事件
 
 * 日期：2016.6.28
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

//技能
public class KillRoleEventCfg : SkillEventCfg
{
    public bool self= true;//是自己使用技能，还是target使用技能
    public bool destroy = false;//直接销毁还是带
    
    public override enSkillEventType Type { get { return enSkillEventType.killRole; } }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {
        switch (col)
        {
            case 0: if (h(ref r, "自己", COL_WIDTH * 2)) onTip("是自己使用技能，还是target使用技能"); return false;
            case 1: if (h(ref r, "销毁", COL_WIDTH * 2)) onTip("是进入死亡状态还是直接销毁"); return false;
            default: return true;
        }
    }

    public override bool DrawGrid(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int row, int col, int totalRow, ref bool change, Transform tran)
    {
        switch (col)
        {
            case 0:
                {
                    r.width = COL_WIDTH * 2;
                    self = EditorGUI.Toggle(r, GUIContent.none, self);
                    r.x += r.width;
                }; return false;
            case 1:
                {
                    r.width = COL_WIDTH * 2;
                    destroy = EditorGUI.Toggle(r, GUIContent.none, destroy);
                    r.x += r.width;
                }; return false;
            default: return true;
        }
    }
#endif


    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {
        Role r = self?source:target;
        r.DeadPart.Handle(destroy);
        return true;
    }
}