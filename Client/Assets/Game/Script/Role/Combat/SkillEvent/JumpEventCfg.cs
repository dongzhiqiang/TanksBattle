#region Header
/**
 * 名称：跳起事件
 
 * 日期：2016.1.13
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

//跳起
public class JumpEventCfg : SkillEventCfg
{
    public float duration = 1f;//跳起时间
    public float speed = 6;//跳起初速度
    public float accelerate = 0;//跳起加速度
    public float fallSpeed = 0;//下落初速度
    public float fallAccelerate = 0;//下落加速度
    public float firstHangDuration = 2;//首次滞空时间
    public float hangDuration = 1;//滞空时间    
    public float cameraOverTime = 0.5f;//相机结束渐变回之前镜头的时间

    public override enSkillEventType Type { get { return enSkillEventType.jump; } }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {
        switch (col)
        {
            case 0: if (h(ref r, "起跳时间", COL_WIDTH * 3)) onTip("起跳时间"); return false;
            case 1: if (h(ref r, "跳起初速", COL_WIDTH * 3)) onTip("跳起初速度"); return false;
            case 2: if (h(ref r, "跳起加速", COL_WIDTH * 3)) onTip("跳起加速度"); return false;
            case 3: if (h(ref r, "下落初速", COL_WIDTH * 3)) onTip("下落初速度"); return false;
            case 4: if (h(ref r, "下落加速", COL_WIDTH * 3)) onTip("下落加速度"); return false;
            case 5: if (h(ref r, "首滞空时间", COL_WIDTH * 4)) onTip("首次滞空时间"); return false;
            case 6: if (h(ref r, "滞空时间", COL_WIDTH * 3)) onTip("滞空时间"); return false;
            case 7: if (h(ref r, "推镜结束时间", COL_WIDTH * 3)) onTip("相机结束渐变回之前镜头的时间"); return false;
            default: return true;
        }
    }
    public override bool DrawGrid(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int row, int col, int totalRow, ref bool change, Transform tran)
    {
        switch (col)
        {
            case 0:
                {
                    r.width = COL_WIDTH * 3;
                    duration = EditorGUI.FloatField(r, GUIContent.none, duration);
                    r.x += r.width;
                }; return false;
            case 1:
                {
                    r.width = COL_WIDTH * 3;
                    speed = EditorGUI.FloatField(r, GUIContent.none, speed);
                    r.x += r.width;
                }; return false;
            case 2:
                {
                    r.width = COL_WIDTH * 3;
                    accelerate = EditorGUI.FloatField(r, GUIContent.none, accelerate);
                    r.x += r.width;
                }; return false;
            case 3:
                {
                    r.width = COL_WIDTH * 3;
                    fallSpeed = EditorGUI.FloatField(r, GUIContent.none, fallSpeed);
                    r.x += r.width;
                }; return false;
            case 4:
                {
                    r.width = COL_WIDTH * 3;
                    fallAccelerate = EditorGUI.FloatField(r, GUIContent.none, fallAccelerate);
                    r.x += r.width;
                }; return false;
            case 5:
                {
                    r.width = COL_WIDTH * 4;
                    firstHangDuration = EditorGUI.FloatField(r, GUIContent.none, firstHangDuration);
                    r.x += r.width;
                }; return false;
            case 6:
                {
                    r.width = COL_WIDTH * 3;
                    hangDuration = EditorGUI.FloatField(r, GUIContent.none, hangDuration);
                    r.x += r.width;
                }; return false;
            case 7:
                {
                    r.width = COL_WIDTH * 3;
                    cameraOverTime = EditorGUI.FloatField(r, GUIContent.none, cameraOverTime);
                    r.x += r.width;
                }; return false;
            default: return true;
        }
    }
#endif



    public override bool OnHandle(Role source, Role target, SkillEventFrame eventFrame)
    {
        if(target!= source)
        {
            Debuger.LogError("跳起事件的作用对象必须为释放者");
            return false;
        }
        TranPart tranPart = target.TranPart;
        TranPartCxt cxt = tranPart.AddCxt();
        if (cxt == null)
            return false;

        RoleStateFall fall =target.RSM.StateFall;
        fall.FirstHangDuration =firstHangDuration;
        fall.HangDuration = hangDuration;
        fall.FallSpeed =fallSpeed;
        fall.FallAccelerate = fallAccelerate;
        fall.CameraOverTime =cameraOverTime;
        
        

        //设置为空中状态
        target.RSM.IsAir = true;

        cxt.moveType = TranPartCxt.enMove.dir;
        cxt.SetMoveDir(Vector3.up, enValidAxis.vertical);
        cxt.speed = speed;
        cxt.accelerate = accelerate;
        cxt.duration = duration;
        cxt.minSpeed = 0;//不允许速度为负数，不然会有下坠感

        

        return true;
    }
}