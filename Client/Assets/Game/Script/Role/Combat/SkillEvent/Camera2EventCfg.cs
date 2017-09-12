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
public class Camera2EventCfg : SkillEventCfg
{
    
    public float moveTime = 0.5f;//切换时间
    public float stayTime = 1f;//停留时间
    public float overTime = 0.5f;//结束时间，-1的话用原镜头的切换时间
    public float fovRate = 1;//视野，-1的话则用当前镜头的参数
    public int cameraPriority =20;//优先级
    
    public float blur = 0f; //径向模糊
    public float blurDuration = 0f; //径向模糊时间
    public float blurBeginSmooth = 0.5f; //模糊开始的渐变时间
    public float blurEndSmooth = 0.15f; //模糊结束的渐变时间
    public Vector3 blurOffset = Vector3.up; //径向模糊的中心偏移，不偏移的话就是主角的位置，相对于主角正向

    public override enSkillEventType Type { get { return enSkillEventType.camera2; } }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r,  SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {
        switch (col)
        {
            case 0: if (h(ref r, "切换时间", COL_WIDTH * 3)) onTip("切换时间，总时间=切换时间+停留时间+结束时间"); return false;
            case 1: if (h(ref r, "停留时间", COL_WIDTH * 3)) onTip("停留时间，总时间=切换时间+停留时间+结束时间"); return false;
            case 2: if (h(ref r, "结束时间", COL_WIDTH * 3)) onTip("结束时间,即结束渐变回原镜头所花的时间，-1的话用原镜头的切换时间，总时间=切换时间+停留时间+结束时间"); return false;
            case 3: if (h(ref r, "视野倍率", COL_WIDTH * 3)) onTip("视野倍率"); return false;
            case 4: if (h(ref r, "优先级", COL_WIDTH * 3)) onTip("镜头优先级，过渡中的场景镜头是10，没有过渡中的场景镜头是0，默认镜头是-10，通关结算转镜是50"); return false;
            case 5: if (h(ref r, "模糊", COL_WIDTH * 2)) onTip("径向模糊的强度"); return false;
            case 6: if (h(ref r, "模糊时间", COL_WIDTH * 2)) onTip("径向模糊的时间"); return false;
            case 7: if (h(ref r, "模糊开始", COL_WIDTH * 2)) onTip("模糊开始的渐变时间"); return false;
            case 8: if (h(ref r, "模糊结束", COL_WIDTH * 2)) onTip("模糊结束的渐变时间"); return false;
            case 9: if (h(ref r, "模糊偏移", COL_WIDTH * 7)) onTip("径向模糊的中心偏移，不偏移的话就是主角的位置，相对于主角正向"); return false;
            default:return true;
        }
    }
    public override bool DrawGrid(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int row, int col, int totalRow, ref bool change, Transform tran)
    {
        switch (col)
        {
            case 0:
                {
                    r.width = COL_WIDTH * 3;
                    moveTime = EditorGUI.FloatField(r, GUIContent.none, moveTime);
                    r.x += r.width;
                }; return false;
            case 1:
                {
                    r.width = COL_WIDTH * 3;
                    stayTime = EditorGUI.FloatField(r, GUIContent.none, stayTime);
                    r.x += r.width;
                }; return false;
            case 2:
                {
                    r.width = COL_WIDTH * 3;
                    overTime = EditorGUI.FloatField(r, GUIContent.none, overTime);
                    r.x += r.width;
                }; return false;
            case 3:
                {
                    r.width = COL_WIDTH * 3;
                    fovRate = EditorGUI.FloatField(r, GUIContent.none, fovRate);
                    r.x += r.width;
                }; return false;
            case 4:
                {
                    r.width = COL_WIDTH * 3;
                    cameraPriority = EditorGUI.IntField(r, GUIContent.none, cameraPriority);
                    r.x += r.width;
                }; return false;
            case 5:
                {
                    r.width = COL_WIDTH * 2;
                    blur = EditorGUI.FloatField(r, GUIContent.none, blur);
                    r.x += r.width;
                }; return false;
            case 6:
                {
                    r.width = COL_WIDTH * 2;
                    blurDuration = EditorGUI.FloatField(r, GUIContent.none, blurDuration);
                    r.x += r.width;
                }; return false;
            case 7:
                {
                    r.width = COL_WIDTH * 2;
                    blurBeginSmooth = EditorGUI.FloatField(r, GUIContent.none, this.blurBeginSmooth);
                    r.x += r.width;
                }; return false;
            case 8:
                {
                    r.width = COL_WIDTH * 2;
                    blurEndSmooth = EditorGUI.FloatField(r, GUIContent.none, this.blurEndSmooth);
                    r.x += r.width;
                }; return false;
            case 9:
                {
                    r.width = COL_WIDTH * 7;
                    blurOffset = EditorGUI.Vector3Field(r, GUIContent.none, blurOffset);
                    r.x += r.width;
                }; return false;
            default: return true;
        }
    }
#endif
    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {
        if (source != target)
        {
            Debuger.LogError("推镜事件2，作用对象必须为释放者本身，有其他需求请与程序沟通");
            return false;
        }
        if (!target.IsHero)
        {
            Debuger.LogError("推镜事件2，作用对象必须为主角，有其他需求请与程序沟通");
            return false;
        }
        Transform t = target.transform;
        if (t == null)
        {
            Debuger.LogError("推镜事件2，可能作用对象已经销毁");
            return false;
        }


        CameraMgr.instance.ScaleFov(moveTime, stayTime, overTime, fovRate, cameraPriority, blur, blurDuration, blurBeginSmooth, blurEndSmooth, blurOffset, target.RoleModel.Model);
        return true;
    }
}