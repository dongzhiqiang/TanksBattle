/**********************************************************
 * 名称：处理序列编辑器用到的ui变量
 
 * 日期：2015.8.13
 * 描述：
 * *********************************************************/
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class HandleSequenceWindow : EditorWindow
{
    public const int wToolbarSmallBtn = 28;
    public const int wToolbarSpace = 30;
    public const float minUnit = 1 / 20f;//最小单位
    public const int hToolBar = 18;//工具栏高度
    public const int hMiddle = 200;//中间的高度
    public const int wLeft = 180;//左边的宽度 
    public const int hTimeLine = 18;//时间轴的高度
    public const int hTimeLineHandle = 18;//时间处理
    public const int hHandles = hMiddle - hTimeLine - hTimeLineHandle;
    public const int handleCount = hHandles / (hHandleRow + 2);
    public const int hHandleRow = 12;//一行的高度
    public const int spiltLeftTight = 6;//左右两边的间隔
    public const int wRightScrollBar = 15;//右边的滚动条的宽度
    public const int wTimeLineHandleExpand = 40;//一个时间处理展开后的宽度

    
    public Rect TopToolbar;
    public Rect RectTimeLine;
    public Rect RectHandles;
    public Vector2 ScrollHandles = Vector2.zero;
    public Vector2 ScrollBottom = Vector2.zero;
    public Vector2[] ScrolBottomCol = new  Vector2[40];
    public Rect RectSubHandle
    {
        get
        {
            return new Rect(RectHandles.x, RectHandles.yMax, RectHandles.width, hMiddle - hTimeLine - hTimeLineHandle);
        }
    }

    public bool CanDragTimeLine = false;//当前是不是正在拖动时间线
    public float _curTime = 0f;//当前时间
    public float CurTime
    {
        get { return _curTime;}
        set
        {
            _curTime = Mathf.Clamp(value, 0, m_handle.Duration);
            m_handle.SetTime(_curTime,true,true,false);
        }
    }


    public Handle CurDowmHandles = null;
    public Handle CurDragHandles = null;
    public float DragHandlesTime = 0;//要拖动处理组到哪个时间段

    public Handle CurDowmSubHandle = null;
    public Handle CurDragSubHandle = null;
    public float DragSubHandleTime = 0;//要拖动处理到哪个时间段

    public bool CanSelect = false;//当前是不是正在选择区域
    public float SelectLeft = 0;
    public float SelectRight = 0;
    public float SelectOffsetTime = 0f;//以时间度量
    public bool CanDragSelect = false;//当前是不是正在拖动区域
    public List<Handle> CurSelectHandles = null;

    public Handle PreRemoveSubHandle = null;
    public Handle PreRemoveSubHandles = null;

}