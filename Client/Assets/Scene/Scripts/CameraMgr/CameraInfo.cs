using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * *********************************************************
 * 名称：相机管理器
 
 * 日期：2015.9.7
 * 描述：主要用于相机切换
 * 这里有个需求还没有实现，当主角和boss对战的时候相机位于两者之间更容易观察
 * *********************************************************
 */

//相机信息，记录着相机渐变的信息和渐变后的信息
//这个类记录的信息实际上是两个过程，一个是渐变过程(优先级为durationPriority)，另一个是渐变后的永久过程(优先级为followPriority)
//如果设置了isDurationOverIfOverlay，add的时候不在顶层的话渐变过程不被加上
//如果设置了isOverAfterDuration，那么永久过程不被加上
//如果OnLast了(重新排到最顶)，那么会重新渐变
[System.Serializable]
public class CameraInfo 
{
    //看着的点的类型
    public enum enLookType
    {
        follow,//跟随
        still,//静止
        betweenTwo,//跟随对象和敌人之间
        stillLook,//盯着
        followLook,//前后移动跟随，左右移动转动
        path,//轨道
        followBehind,//跟随在后面看敌人
    }

    //重叠和重新置顶的时候的操作
    public enum enDurationType
    {
        resetWhenLast,//当镜头置顶时，重置渐变时间
        overWhenOverlay,//当镜头被其他镜头覆盖的结束
        keepOverTimeWhenLast,//当镜头置顶的时，通过缩短渐变时间，以在结束前快速渐变好镜头
    }

    public static string[] LookTypeName = new string[] { "跟随", "不跟随","跟随对象与敌人之间","盯着跟随对象","前后跟随左右转动","轨道", "跟随对象盯住目标" };//尽量填两个字以下
    public static string[] DuratioTypeName = new string[] { "镜头置顶时重置渐变时间", "镜头被其他镜头覆盖时结束", "镜头置顶时适配结束时间" };
    public const int Camera_Duration_Priority = 10;         //渐变镜头的默认优先级
    public const int Camera_Follow_Priority = 0;            //永久镜头的默认优先级
    //public const int Camera_Default_Priority = -10;         //默认永久镜头的默认优先级
    public const int Camera_Editor_Priority = 100;          //镜头编辑器同步用的优先级，建议其他地方的都比较这个优先级低
    


    //镜头类型,及通用参数
    public enLookType lookType = enLookType.follow;
    public Vector3 refPos = Vector3.zero;                   //参考点。可能做为看的点，有的类型不需要
    public Vector3 offset = new Vector3(0, 1f, 0);          //看的点的偏移，相对于相机水平前方
    public float distance = 6;                              //看的点与相机的距离
    public float verticalAngle = 50;                        //高度夹角
    public float horizontalAngle = 0;                       //水平旋转角
    public float fov = 60f;                                 //视野角度field of view
    public float duration = -1;                           //多久时间后结束

    //锁定方向
    public bool uselock = false;                            //锁定到某个方向
    public Vector3 lockEuler = Vector3.zero;                //要锁定的方向

    //进入下一个镜头的渐变时间由自己控制
    public bool useOverDuration = false;
    public float overDuationSmooth = 1.0f;

    //跟随对象和敌人之间的特有参数
    public bool useBetweenTwoLimit = false;
    public float betweenTwoLimit = 2f;                      //如果计算出的看的点

    //盯着的特有参数
    public bool useStilllookLimit = false;
    public float stillLookLimit = 20;                       //如果是盯着，那么跟随者与相机超过这个距离，那么相机还是会跟随过去
    
    //渐变过程相关
    public enDurationType durationType = enDurationType.resetWhenLast;//渐变类型
    public bool isDurationInvalid = false;                  //渐变镜头是不是即时的
    public float durationSmooth = 1.0f;                     //渐变时间
    public int durationPriority = Camera_Duration_Priority; //切换过程中的优先级
    public AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 2f), new Keyframe(1f, 1f,0f, 0f));

    //距离衰减计算出的渐变速度，注意和渐变过程的计算出的渐变速度是取两者的最大值(也就是取慢的那个)
    public bool useDisSmooth = false;
    public float disSmooth =0.5f;
    public float disSmoothLimit = 8f;
    

    //是否受战斗镜头影响
    public bool isBattleDisturb = true;

    //永久过程相关
    public int priority = Camera_Follow_Priority;           //切换完成后的跟随中的优先级
    //public float smooth = 0.05f;                            //渐变结束速度
    public bool isOverAfterDuration = false;                //渐变后就结束，永久过程不用加

    //镜头轨道参数
    public CameraPath cameraPath;
    public float pathLag = 0;

    //跟随对象盯着目标相关参数
    public string targetId = "";
    public string bone = "";
    public Vector3 bornOffset = Vector3.zero;

    public float blur =0f;
    public float blurDuration = 0f;
    public float blurBeginSmooth= 0.5f;
    public float blurEndSmooth = 0.15f;
    public Transform blurTarget;
    public Vector3 blurOffset;

    //用于编辑器窗口
    public bool isExpand=true;

    public bool NeedShowRefPos
    {
        get {
            return uselock || lookType == CameraInfo.enLookType.still || lookType == CameraInfo.enLookType.stillLook;
        }
    }
    public CameraInfo() { }
    public CameraInfo(CameraInfo info)
    {
        if (info == null)
        {
            Debuger.LogError("用于复制的相机为空");
            return;
        }

        //看的点和相对位置信息相关，用于计算相机的最终位置
        this.lookType =info.lookType;
        this.refPos = info.refPos;
        this.offset = info.offset;
        this.distance = info.distance;
        this.verticalAngle = info.verticalAngle;
        this.horizontalAngle = info.horizontalAngle;
        this.fov = info.fov;
        this.uselock = info.uselock;
        this.lockEuler = info.lockEuler;
        this.duration = info.duration;

        //跟随对象和敌人之间的特有参数
        this.useBetweenTwoLimit = info.useBetweenTwoLimit;
        this.betweenTwoLimit = info.betweenTwoLimit;

        //盯着的特有参数
        this.useStilllookLimit = info.useStilllookLimit;
        this.stillLookLimit = info.stillLookLimit;

        //渐变过程相关
        this.durationType = info.durationType;
        this.durationSmooth = info.durationSmooth;
        this.durationPriority = info.durationPriority;
        this.isDurationInvalid = info.isDurationInvalid;
        this.animationCurve = new AnimationCurve(info.animationCurve.keys);

        //永久过程相关
        this.priority = info.priority;
        //this.smooth = info.smooth;
        this.isOverAfterDuration = info.isOverAfterDuration;

        
        //用于编辑器窗口
        this.isExpand= info.isExpand;
    }

}
