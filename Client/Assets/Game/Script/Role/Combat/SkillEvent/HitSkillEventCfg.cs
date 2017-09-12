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

//被击
public class HitSkillEventCfg : SkillEventCfg
{
    public float duration = 0.3f;//僵直时间
    public float floatSpeed = 5;//被击初速度: 浮空中二次被击的初速度
    public float floatAccelerated = 0;//被击加速度: 浮空中二次被击的加速度
    public HitFloatSkillEventCfg groundFloat = new HitFloatSkillEventCfg();
    
    public override enSkillEventType Type { get{return enSkillEventType.hit;} }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r,  SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {
        switch (col)
        {
            case 0: if (h(ref r, "僵直", COL_WIDTH * 2)) onTip("僵直时间"); return false;
            case 1: if (h(ref r, "速度", COL_WIDTH * 2)) onTip("被击初速度: 浮空中二次被击的初速度"); return false;
            case 2: if (h(ref r, "加速度", COL_WIDTH * 2)) onTip("被击加速度: 浮空中二次被击的加速度"); return false;
            case 3: if (h(ref r, "上升初速", COL_WIDTH * 2)) onTip("倒地时转浮空，浮空初速度,上升阶段的初始速度"); return false;
            case 4: if (h(ref r, "上升加速", COL_WIDTH * 3)) onTip("倒地时转浮空上升加速度,上升阶段的加速度"); return false;
            case 5: if (h(ref r, "反转速度", COL_WIDTH * 3)) onTip("倒地时转浮空，如果上升过程中达到这个速度就下落"); return false;
            case 6: if (h(ref r, "下落初速", COL_WIDTH * 3)) onTip("倒地时转浮空，下落初速度"); return false;
            case 7: if (h(ref r, "下落加速", COL_WIDTH * 3)) onTip("倒地时转浮空，下落加速度,下落阶段的加速度"); return false;
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
                    duration = EditorGUI.FloatField(r, GUIContent.none, duration);
                    r.x += COL_WIDTH * 2;
                }; return false;
            case 1:
                {
                    r.width = COL_WIDTH * 2;
                    floatSpeed = EditorGUI.FloatField(r, GUIContent.none, floatSpeed);
                    r.x += COL_WIDTH * 2;
                }; return false;
            case 2:
                {
                    r.width = COL_WIDTH * 2;
                    floatAccelerated = EditorGUI.FloatField(r, GUIContent.none, floatAccelerated);
                    r.x += COL_WIDTH * 2;
                }; return false;
            case 3:
                {
                    r.width = COL_WIDTH * 2;
                    groundFloat.speed = EditorGUI.FloatField(r, GUIContent.none, groundFloat.speed);
                    r.x += r.width;
                }; return false;
            case 4:
                {
                    r.width = COL_WIDTH * 3;
                    groundFloat.acceleratedUp= EditorGUI.FloatField(r, GUIContent.none, groundFloat.acceleratedUp);
                    r.x += r.width;
                }; return false;
            case 5:
                {
                    r.width = COL_WIDTH * 3;
                    groundFloat.reverseSpeed= EditorGUI.FloatField(r, GUIContent.none, groundFloat.reverseSpeed);
                    r.x += r.width;
                }; return false;
            case 6:
                {
                    r.width = COL_WIDTH * 3;
                    groundFloat.speedDown= EditorGUI.FloatField(r, GUIContent.none, groundFloat.speedDown);
                    r.x += r.width;
                }; return false;
            case 7:
                {
                    r.width = COL_WIDTH * 3;
                    groundFloat.acceleratedDown = EditorGUI.FloatField(r, GUIContent.none, groundFloat.acceleratedDown);
                    r.x += r.width;
                }; return false;
            default: return true;
        }
    }
#endif
    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {
     

        //先获取武器的僵直系数
        WeaponCfg weapon = source.FightWeapon;
        float weaponBehitRate = weapon==null?1:weapon.behitRate;

        BehitCxt cxt = IdTypePool<BehitCxt>.Get();
        cxt.cfg = this;
        cxt.duration = this.duration * target.Cfg.behitRate * weaponBehitRate;
        return target.RSM.GotoState(enRoleState.beHit, cxt,false,true);
    }
}