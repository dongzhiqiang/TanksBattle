#region Header
/**
 * 名称：技能事件
 
 * 日期：2015.1.14
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
public class UseSkillEventCfg : SkillEventCfg
{
    public enum enTarget
    {
        target,//事件当前的作用对象,如果设置了是敌人使用技能，那么技能目标则为自己
        skillTarget,//技能的目标，如果技能目标为空的情况下重新查找
        refind,//重新查找
        skillTargetIfExist,//技能的目标，如果技能目标为空的情况下不释放技能
    }
    public static string[] TargetTypeName = new string[] { "作用对象", "技能目标", "重新查找", "技能目标(判空)" };

    public bool self= true;//是自己使用技能，还是target使用技能
    public string skillId ="";//技能id
    public enTarget targetType = enTarget.target;

    public override enSkillEventType Type { get { return enSkillEventType.skill; } }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {
        switch (col)
        {
            case 0: if (h(ref r, "自己", COL_WIDTH * 2)) onTip("是自己使用技能，还是target使用技能"); return false;
            case 1: if (h(ref r, "技能id", COL_WIDTH * 8)) onTip("技能id"); return false;
            case 2: if (h(ref r, "技能目标", COL_WIDTH *4)) onTip(@"作用对象:事件当前的作用对象,如果设置了是敌人使用技能，那么技能目标则为自己
技能目标:技能的目标，如果技能目标为空的情况下重新查找
重新查找:按照技能自动朝向的方式查找
技能目标(判空):技能的目标，如果技能目标为空的情况下不释放技能
"); return false;
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
                    r.width = COL_WIDTH * 8;
                    skillId =EditorGUI.TextField(r,skillId);
                    
                    r.x += r.width;
                }; return false;
            case 2:
                {
                    r.width = COL_WIDTH * 4;
                    targetType = (enTarget)EditorGUI.Popup(r, (int)targetType, TargetTypeName);
                    r.x += r.width;
                }; return false;


               
            default: return true;
        }
    }
#endif


    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {
        Role r = self?source:target;
        Role skillTarget = null;
        switch (targetType)
        {
            case enTarget.target: skillTarget = self ? target: source; break;
            case enTarget.skillTarget:
                {
                    skillTarget = eventFrame.EventGroup.Target;
                }; break;
            case enTarget.refind: skillTarget = null; break;
            case enTarget.skillTargetIfExist:
                {
                    if (eventFrame.EventGroup.Target != null )
                        skillTarget = eventFrame.EventGroup.Target;
                    else
                        return false;
                }; break;
            default:
                {
                    Debuger.LogError("未知的类型:{0}", targetType);
                    return false;
                }
        }

        return r.CombatPart.Play(skillId, skillTarget, true, false) == CombatPart.enPlaySkill.normal;
    }
}