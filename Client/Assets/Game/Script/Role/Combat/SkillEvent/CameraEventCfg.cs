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
public class CameraEventCfg : SkillEventCfg
{
    public Vector3 offset = Vector3.zero;//偏移，相对于作用对象前方
    public float moveTime = 0.5f;//切换时间
    public float stayTime = 1f;//停留时间
    public float overTime = 0.5f;//结束时间，-1的话用原镜头的切换时间
    public float fov = -1;//视野，-1的话则用当前镜头的参数
    public float horizontalAngle = -1;//水平角,相对于作用对象前方，-1的话则用当前镜头的参数
    public float verticalAngle = -1;//高度角，-1的话则用当前镜头的参数
    public float distance=-1;//距离，-1的话则用当前镜头的参数
    public float blur = 0f; //径向模糊
    public float blurDuration = 0f; //径向模糊时间
    public float blurBeginSmooth= 0.5f; //模糊开始的渐变时间
    public float blurEndSmooth = 0.15f; //模糊结束的渐变时间
    public Vector3 blurOffset = Vector3.up; //径向模糊的中心偏移，不偏移的话就是主角的位置，相对于主角正向

    public override enSkillEventType Type { get { return enSkillEventType.camera; } }
#if UNITY_EDITOR
    public override bool DrawHeader(ref Rect r,  SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int col, System.Action<string> onTip, HeaderButton h)
    {
        switch (col)
        {
            case 0: if (h(ref r, "偏移", COL_WIDTH * 7)) onTip("偏移，相对于作用对象前方"); return false;
            case 1: if (h(ref r, "切换时间", COL_WIDTH * 3)) onTip("切换时间，总时间=切换时间+停留时间+结束时间"); return false;
            case 2: if (h(ref r, "停留时间", COL_WIDTH * 3)) onTip("停留时间，总时间=切换时间+停留时间+结束时间"); return false;
            case 3: if (h(ref r, "结束时间", COL_WIDTH * 3)) onTip("结束时间,即结束渐变回原镜头所花的时间，-1的话用原镜头的切换时间，总时间=切换时间+停留时间+结束时间"); return false;
            case 4: if (h(ref r, "视野", COL_WIDTH * 2)) onTip("视野，-1的话则用当前镜头的参数"); return false;
            case 5: if (h(ref r, "水平角", COL_WIDTH * 2)) onTip("水平角,相对于作用对象前方，-1的话则用当前镜头的参数"); return false;
            case 6: if (h(ref r, "高度角", COL_WIDTH * 2)) onTip("高度角，-1的话则用当前镜头的参数"); return false;
            case 7: if (h(ref r, "距离", COL_WIDTH * 2)) onTip("距离，-1的话则用当前镜头的参数"); return false;
            case 8: if (h(ref r, "模糊", COL_WIDTH * 2)) onTip("径向模糊的强度"); return false;
            case 9: if (h(ref r, "模糊时间", COL_WIDTH * 2)) onTip("径向模糊的时间"); return false;
            case 10: if (h(ref r, "模糊开始", COL_WIDTH * 2)) onTip("模糊开始的渐变时间"); return false;
            case 11: if (h(ref r, "模糊结束", COL_WIDTH * 2)) onTip("模糊结束的渐变时间"); return false;
            case 12: if (h(ref r, "模糊偏移", COL_WIDTH * 7)) onTip("径向模糊的中心偏移，不偏移的话就是主角的位置，相对于主角正向"); return false;
            default:return true;
        }
    }
    public override bool DrawGrid(ref Rect r, SkillEventFrameCfg frameCfg, SkillEventGroupCfg g, int row, int col, int totalRow, ref bool change, Transform tran)
    {
        switch (col)
        {
            case 0:
                {
                    r.width = COL_WIDTH * 7;
                    offset = EditorGUI.Vector3Field(r, GUIContent.none, offset);
                    r.x += r.width;
                }; return false;
            case 1:
                {
                    r.width = COL_WIDTH * 3;
                    moveTime = EditorGUI.FloatField(r, GUIContent.none, moveTime);
                    r.x += r.width;
                }; return false;
            case 2:
                {
                    r.width = COL_WIDTH * 3;
                    stayTime = EditorGUI.FloatField(r, GUIContent.none, stayTime);
                    r.x += r.width;
                }; return false;
            case 3:
                {
                    r.width = COL_WIDTH * 3;
                    overTime = EditorGUI.FloatField(r, GUIContent.none, overTime);
                    r.x += r.width;
                }; return false;
            case 4:
                {
                    r.width = COL_WIDTH * 2;
                    fov = EditorGUI.FloatField(r, GUIContent.none, fov);
                    r.x += r.width;
                }; return false;
            case 5:
                {
                    r.width = COL_WIDTH * 2;
                    horizontalAngle = EditorGUI.FloatField(r, GUIContent.none, horizontalAngle);
                    r.x += r.width;
                }; return false;
            case 6:
                {
                    r.width = COL_WIDTH * 2;
                    verticalAngle = EditorGUI.FloatField(r, GUIContent.none, verticalAngle);
                    r.x += r.width;
                }; return false;
            case 7:
                {
                    r.width = COL_WIDTH * 2;
                    distance = EditorGUI.FloatField(r, GUIContent.none, distance);
                    r.x += r.width;
                }; return false;
            case 8:
                {
                    r.width = COL_WIDTH * 2;
                    blur = EditorGUI.FloatField(r, GUIContent.none, blur);
                    r.x += r.width;
                }; return false;
            case 9:
                {
                    r.width = COL_WIDTH * 2;
                    blurDuration = EditorGUI.FloatField(r, GUIContent.none, blurDuration);
                    r.x += r.width;
                }; return false;
            case 10:
                {
                    r.width = COL_WIDTH * 2;
                    blurBeginSmooth = EditorGUI.FloatField(r, GUIContent.none, this.blurBeginSmooth);
                    r.x += r.width;
                }; return false;
            case 11:
                {
                    r.width = COL_WIDTH * 2;
                    blurEndSmooth = EditorGUI.FloatField(r, GUIContent.none, this.blurEndSmooth);
                    r.x += r.width;
                }; return false;
            case 12:
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
            Debuger.LogError("推镜事件，作用对象必须为释放者本身，有其他需求请与程序沟通");
            return false;
        }
        if (target.transform !=CameraMgr.instance.GetFollow())
        {
            //Debuger.Log("推镜事件，作用对象必须为主角，竞技场等副本中");
            return false;
        }
        Transform t = target.transform;
        if (t == null)
        {
            Debuger.LogError("推镜事件，可能作用对象已经销毁");
            return false;
        }


        CameraMgr.instance.Still(t.position, t.forward, offset, moveTime, stayTime,
                fov, horizontalAngle, verticalAngle, distance, overTime, blur, blurDuration, blurBeginSmooth, blurEndSmooth,blurOffset,  target.RoleModel.Model);
        return true;
    }
}