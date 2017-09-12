#region Header
/**
 * 名称：帧事件
 
 * 日期：2015.9.28
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

//击飞
public class HitFlySkillEventCfg : SkillEventCfg
{
    public float speed = 6;//击飞初速度,上升阶段的初始速度
    public float acceleratedUp = 0;//上升加速度,上升阶段的加速度
    public float acceleratedDown = 0;//下落加速度,下落阶段的加速度
    public float groundDuration = 0.5f;//倒地时间
    public float reverseSpeed = 4;//反转速度
    public float speedDown = -8;//浮空初速度,上升阶段的初始速度
    public float hitDuration = 0.3f;//被击僵直时间
    public override enSkillEventType Type { get { return enSkillEventType.hitFly; } }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {
        switch (col)
        {
            case 0: if (h(ref r, "初速度", COL_WIDTH * 2)) onTip("击飞初速度,上升阶段的初始速度。"); return false;
            case 1: if (h(ref r, "上升加速", COL_WIDTH * 3)) onTip("上升加速度,上升阶段的加速度"); return false;
            case 2: if (h(ref r, "反转速度", COL_WIDTH * 3)) onTip("如果上升过程中达到这个速度就下落"); return false;
            case 3: if (h(ref r, "下落初速", COL_WIDTH * 3)) onTip("下落初速度"); return false;
            case 4: if (h(ref r, "下落加速", COL_WIDTH * 3)) onTip("下落加速度,下落阶段的加速度"); return false;
            case 5: if (h(ref r, "倒地时间", COL_WIDTH * 3)) onTip("倒地时间"); return false;
            case 6: if (h(ref r, "被击僵直", COL_WIDTH * 3)) onTip("被击僵直时间。有的角色不能浮空，这个时候会把浮空事件转换为被击事件"); return false;
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
                    speed = EditorGUI.FloatField(r, GUIContent.none, speed);
                    r.x += r.width;
                }; return false;
            case 1:
                {
                    r.width = COL_WIDTH * 3;
                    acceleratedUp = EditorGUI.FloatField(r, GUIContent.none, acceleratedUp);
                    r.x += r.width;
                }; return false;
            case 2:
                {
                    r.width = COL_WIDTH * 3;
                    reverseSpeed = EditorGUI.FloatField(r, GUIContent.none, reverseSpeed);
                    r.x += r.width;
                }; return false;
            case 3:
                {
                    r.width = COL_WIDTH * 3;
                    speedDown = EditorGUI.FloatField(r, GUIContent.none, speedDown);
                    r.x += r.width;
                }; return false;
            case 4:
                {
                    r.width = COL_WIDTH * 3;
                    acceleratedDown = EditorGUI.FloatField(r, GUIContent.none, acceleratedDown);
                    r.x += r.width;
                }; return false;
            case 5:
                {
                    r.width = COL_WIDTH * 3;
                    groundDuration = EditorGUI.FloatField(r, GUIContent.none, groundDuration);
                    r.x += r.width;
                }; return false;
            case 6:
                {
                    r.width = COL_WIDTH * 3;
                    hitDuration = EditorGUI.FloatField(r, GUIContent.none, hitDuration);
                    r.x += r.width;
                }; return false;
            default: return true;
        }
    }
#endif

    HitSkillEventCfg hitSkillEvent = null;

    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {
        //击飞、浮空是否要转化为被击
        if (target.Fire(MSG_ROLE.CHANGE_HIT_EVENT, this))
        {
            if (speedDown > 0)
                Debuger.LogError("击飞下落初速度不能为0:{0} {1}", source.Cfg.id, eventFrame.EventGroup.Name);

            BeFlyCxt cxt = IdTypePool<BeFlyCxt>.Get();
            cxt.cfg = this;
            return target.RSM.GotoState(enRoleState.beHit, cxt, false, true);
        }
        else
        {
            if (hitSkillEvent == null)
                hitSkillEvent = new HitSkillEventCfg();
            hitSkillEvent.duration = hitDuration;

            BehitCxt cxt = IdTypePool<BehitCxt>.Get();
            cxt.cfg = hitSkillEvent;
            return target.RSM.GotoState(enRoleState.beHit, cxt, false, true);
        }   
    }
}