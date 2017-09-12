using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * *********************************************************
 * 名称：相机管理器
 
 * 日期：2015.9.7
 * 描述：主要用于相机切换
 * 
 * *********************************************************
 */
public class CameraHandle : IShareThingHandle
{

    public static int s_idCounter = 0;

    public CameraInfo m_info;
    public float m_beginTime;
    public bool m_firstDurationInvalid;//头一次立即切
    public float m_duration;//真实的渐变时间
    public bool m_isDuation;//是不是在渐变中
    Transform m_follow = null;

    int m_id;
    bool m_isFirst;
    float m_lastPercent;
    Transform m_target = null;

    public CameraInfo.enLookType Type { get { return m_info.lookType; } }

    public bool IsDurationInvalid { get { return m_info.isDurationInvalid || m_firstDurationInvalid; } }

    //渐变到多少了
    public float Factor { get { return m_duration == 0 ? 1 : Mathf.Clamp01((Time.unscaledTime - m_beginTime) / m_duration); } }

    public float CurTime { get { return m_beginTime == -1 ? 0 : Time.unscaledTime - m_beginTime; } }

    public float Fov { get { return m_info.fov; } }

    public Vector3 FollowPos { get { return m_follow == null ? CameraMgr.instance.GetFollowPos() : m_follow.position; } }
    public Vector3 TargetPos { get { return m_target == null ? Vector3.zero : m_target.position; } }

    //是不是需要跟随者
    public bool NeedFollow
    {
        get
        {
            switch (m_info.lookType)
            {

                case CameraInfo.enLookType.follow: return true;
                case CameraInfo.enLookType.still: return false;
                case CameraInfo.enLookType.betweenTwo: return true;
                case CameraInfo.enLookType.stillLook: return true;
                case CameraInfo.enLookType.followLook: return true;
                case CameraInfo.enLookType.path: return true;
                case CameraInfo.enLookType.followBehind: return true;
                default:
                    {
                        Debuger.LogError("未知的类型:" + m_info.lookType);

                    }; return true;
            }
        }
    }
    public float Distance
    {
        get
        {
            float dis;
            switch (m_info.lookType)
            {

                case CameraInfo.enLookType.follow: { dis = m_info.distance; } break;
                case CameraInfo.enLookType.still: { dis = m_info.distance; } break;
                case CameraInfo.enLookType.betweenTwo: { dis = m_info.distance; } break;
                case CameraInfo.enLookType.stillLook: { dis = (LookPos - StillLookPos).magnitude; } break;
                case CameraInfo.enLookType.followLook: { dis = m_info.distance; } break;
                case CameraInfo.enLookType.followBehind: { dis = m_info.distance; } break;
                case CameraInfo.enLookType.path:
                    {
                        if (m_info.cameraPath == null)
                        {
                            dis = m_info.distance;
                            break;
                        }
                        float nearestPercent = m_info.cameraPath.GetNearestPoint(CameraPos, false, 5);
                        m_lastPercent = nearestPercent;
                        Vector3 backwards = -m_info.cameraPath.GetPathDirection(nearestPercent, true);
                        Vector3 pos = m_info.cameraPath.GetPathPosition(nearestPercent, false);
                        Vector3 camPos = pos + backwards * m_info.pathLag;
                        dis = (LookPos - camPos).magnitude;
                    }
                    break;
                default:
                    {
                        Debuger.LogError("未知的类型:" + m_info.lookType);
                        dis = m_info.distance;
                    }; break;
            }

            return dis;
        }
    }

    //看着的点(带偏移)
    public Vector3 LookPos
    {
        get
        {
            Vector3 lookPos;
            switch (m_info.lookType)
            {

                case CameraInfo.enLookType.follow: { lookPos = FollowPos; } break;
                case CameraInfo.enLookType.still: { lookPos = m_info.refPos; } break;
                case CameraInfo.enLookType.betweenTwo: { lookPos = BetweenPos; } break;
                case CameraInfo.enLookType.stillLook: { lookPos = FollowPos; } break;
                case CameraInfo.enLookType.followLook: { lookPos = FollowPos; } break;
                case CameraInfo.enLookType.followBehind: { lookPos = FollowPos; } break;
                case CameraInfo.enLookType.path: { lookPos = FollowPos; } break;
                default:
                    {
                        Debuger.LogError("未知的类型:" + m_info.lookType);
                        lookPos = m_info.refPos;
                    }; break;
            }
            lookPos += Quaternion.Euler(0, m_info.horizontalAngle, 0) * m_info.offset;

            //方向上的锁定
            if (m_info.uselock)
            {
                Vector3 refPos = m_info.refPos + Quaternion.Euler(0, m_info.horizontalAngle, 0) * m_info.offset;
                lookPos = refPos + Vector3.Project((lookPos - refPos), Quaternion.Euler(m_info.lockEuler) * Vector3.forward);
            }

            return lookPos;
        }
    }


    //public Vector3 Pos
    //{
    //    get
    //    {
    //        Vector3 pos;
    //        switch (m_info.lookType)
    //        {

    //            case CameraInfo.enLookType.follow: { pos = CameraMgr.instance.NewLookPos - StillForward * m_info.distance; } break;
    //            case CameraInfo.enLookType.still: { pos = StillPos; } break;
    //            case CameraInfo.enLookType.betweenTwo: { pos = CameraMgr.instance.NewLookPos - StillForward * m_info.distance; } break;
    //            case CameraInfo.enLookType.stillLook: { pos = StillLookPos; } break;
    //            case CameraInfo.enLookType.followLook: { pos = FollowLookPos; } break;
    //            default:
    //                {
    //                    Debuger.LogError("未知的类型:" + m_info.lookType);
    //                    pos = CameraMgr.instance.NewLookPos - StillForward * m_info.distance;
    //                }; break;
    //        }
    //        return pos;
    //    }
    //}
    public Vector3 Euler
    {
        get
        {
            Vector3 euler;
            switch (m_info.lookType)
            {

                case CameraInfo.enLookType.follow: { euler = new Vector3(m_info.verticalAngle, m_info.horizontalAngle, 0); } break;
                case CameraInfo.enLookType.still: { euler = new Vector3(m_info.verticalAngle, m_info.horizontalAngle, 0); } break;
                case CameraInfo.enLookType.betweenTwo: { euler = new Vector3(m_info.verticalAngle, m_info.horizontalAngle, 0); } break;
                case CameraInfo.enLookType.stillLook: { euler = Quaternion.LookRotation(LookPos - StillLookPos).eulerAngles; } break;
                case CameraInfo.enLookType.followLook: { euler = Quaternion.LookRotation(LookPos - FollowLookPos).eulerAngles; } break;
                case CameraInfo.enLookType.followBehind: { euler = Quaternion.LookRotation(TargetPos - LookPos).eulerAngles; } break;
                case CameraInfo.enLookType.path:
                    {
                        if (m_info.cameraPath == null)
                        {
                            euler = new Vector3(m_info.verticalAngle, m_info.horizontalAngle, 0);
                            break;
                        }
                        float nearestPercent = m_info.cameraPath.GetNearestPoint(CameraPos, false, 5);
                        Vector3 backwards = -m_info.cameraPath.GetPathDirection(nearestPercent, true);
                        Vector3 pos = m_info.cameraPath.GetPathPosition(nearestPercent, false);
                        Vector3 camPos = pos + backwards * m_info.pathLag;
                        euler = Quaternion.LookRotation(LookPos - camPos).eulerAngles;
                    }
                    break;
                default:
                    {
                        Debuger.LogError("未知的类型:" + m_info.lookType);
                        euler = new Vector3(m_info.verticalAngle, m_info.horizontalAngle, 0);
                    }; break;
            }

            return euler;
        }
    }

    public Quaternion Rotate { get { return Quaternion.Euler(Euler); } }

    Vector3 StillForward { get { return Quaternion.Euler(m_info.verticalAngle, m_info.horizontalAngle, 0) * Vector3.forward; } }

    Vector3 CameraPos { get { return FollowPos + Quaternion.Euler(0, m_info.horizontalAngle, 0) * m_info.offset - StillForward * m_info.distance; } }

    //由refPos、offset、distance和角度算出来的相机的位置
    Vector3 StillPos { get { return m_info.refPos + Quaternion.Euler(0, m_info.horizontalAngle, 0) * m_info.offset - StillForward * m_info.distance; } }

    Vector3 BetweenPos
    {
        get
        {
            Transform t = CameraMgr.instance.m_tranEnemyDebug;
            Transform caTran = CameraMgr.instance.Tran;
            Vector3 lookPos = FollowPos;
            if (t != null)
            {
                //要计算距离限制，如果跟随者在相机正下方就是betweenTwoLimit，如果离相机的角度越远那么limit越大
                Vector3 cameraDir = lookPos - caTran.position;//相机看向跟随者
                cameraDir = Vector3.ProjectOnPlane(cameraDir, CameraMgr.instance.HorizontalRightlDir);//投影到相机前方
                float angle = Vector3.Angle(cameraDir, new Vector3(cameraDir.x, 0, cameraDir.z));//和水平面的夹角
                Vector3 link = (t.position - lookPos) / 2;//中点,相对于跟随对象

                if (!m_info.useBetweenTwoLimit)
                    lookPos = lookPos + link;
                else
                {
                    float maxLimit = m_info.betweenTwoLimit * (1 + Mathf.Tan((90 - angle) * Mathf.Deg2Rad));//注意近大远小

                    if (link.sqrMagnitude <= maxLimit * maxLimit)//没有超出限制
                        lookPos = lookPos + link;
                    else
                        lookPos = lookPos + link.normalized * maxLimit;
                }
            }
            return lookPos;
        }
    }

    Vector3 StillLookPos
    {
        get
        {
            Vector3 stillPos = StillPos;
            Vector3 link = CameraMgr.instance.SmoothLookPos - stillPos;
            if (m_info.useStilllookLimit)//距离限制
            {
                link.y = 0;//注意要保证相机的y轴不变
                if (link.sqrMagnitude <= m_info.stillLookLimit * m_info.stillLookLimit)
                    return stillPos;
                else
                    return stillPos + link - link.normalized * m_info.stillLookLimit;
            }
            //没有距离限制
            else
                return stillPos;
        }
    }

    Vector3 FollowLookPos
    {
        get
        {
            CameraMgr caMgr = CameraMgr.instance;
            Vector3 horizontalRightlDir = caMgr.HorizontalRightlDir;
            Vector3 lastLookPos = caMgr.LastLookPos;
            Quaternion q;
            if (CameraMgr.instance.IsSampleDurationInvalid)
            {
                q = Quaternion.Euler(m_info.verticalAngle, m_info.horizontalAngle, 0);
            }
            else
                q = caMgr.transform.rotation;

            Vector3 lookPos = LookPos;
            Vector3 link = lookPos - lastLookPos;
            if (link != Vector3.zero)
            {
                Vector3 p = Vector3.Project(link, horizontalRightlDir);
                bool sign = Vector3.Dot(horizontalRightlDir, p) >= 0;
                float d = p.magnitude;

                if (d != 0)
                {
                    float angle = Mathf.Atan(d / m_info.distance) * Mathf.Rad2Deg;
                    q = q * Quaternion.Euler(0, sign ? angle : -angle, 0);
                }
            }

            return lookPos - q * Vector3.forward * m_info.distance;
        }
    }

    //duration=-1则默认用info的durationSmooth
    public CameraHandle(CameraInfo info, bool firstDurationInvalid, float duration = -1)
    {
        m_id = ++s_idCounter;
        this.m_info = info;
        m_isFirst = true;

        Reset(firstDurationInvalid, duration);
    }


    //firstDuration= true马上切，duration=-1则默认用info的durationSmooth
    public void Reset(bool firstDurationInvalid, float duration = -1)
    {
        this.m_isDuation = true;
        this.m_beginTime = Time.unscaledTime;
        this.m_firstDurationInvalid = firstDurationInvalid;
        this.m_duration = duration == -1 ? m_info.durationSmooth : duration;
    }

    public void Reset()
    {
        this.m_isDuation = true;
        this.m_beginTime = Time.unscaledTime;
    }

    public void ResetKeepOverTime()
    {
        this.m_isDuation = true;

        float endTime = Time.unscaledTime + this.m_duration;
        this.m_beginTime = Time.unscaledTime;
        if (endTime <= this.m_beginTime)
        {
            this.m_firstDurationInvalid = true;
        }
        else
        {
            this.m_firstDurationInvalid = false;
            this.m_duration = endTime - this.m_beginTime;
        }
    }



    public override void OnLast(IShareThingHandle prev)
    {
        if (CameraMgr.instance)
        {
            CameraMgr.instance.m_curId = m_id;
            CameraMgr.instance.m_curCameraInfo = m_info;
            CameraMgr.instance.m_isDuration = true;
            CameraMgr.instance.m_samlpeCounter = 0;
        }

        CameraHandle prevHandle = prev as CameraHandle;
        //注意只有在被覆盖然后置顶的时候才需要重置相关东西和提升优先级
        if (m_isFirst || m_info.durationType == CameraInfo.enDurationType.overWhenOverlay)
            return;

        if (m_info.durationType == CameraInfo.enDurationType.resetWhenLast)
        {
            if (prevHandle != null && prevHandle.m_info.useOverDuration)
            {
                if (prevHandle.m_info.blur != 0 && prevHandle.m_info.blurDuration != 0)//推镜过程模糊
                {
                    CameraMgr.instance.PlayRadialBlur(prevHandle.m_info.blur, prevHandle.m_info.blurTarget, prevHandle.m_info.blurDuration, prevHandle.m_info.offset, prevHandle.m_info.blurBeginSmooth, prevHandle.m_info.blurEndSmooth);
                }

                Reset(prevHandle.m_info.isDurationInvalid, prevHandle.m_info.overDuationSmooth);
            }
            else
            {
                if (m_info.blur != 0 && m_info.blurDuration != 0)//推镜过程模糊
                {
                    CameraMgr.instance.PlayRadialBlur(m_info.blur, m_info.blurTarget, m_info.blurDuration, m_info.offset, m_info.blurBeginSmooth, m_info.blurEndSmooth);
                }
                Reset(false);
            }
                

        }
        else if (m_info.durationType == CameraInfo.enDurationType.keepOverTimeWhenLast)
        {
            ResetKeepOverTime();
        }

        //提升优先级
        this.m_shareThing.Change(this, m_info.durationPriority);
    }

    public override void OnOverlay()
    {
        m_isFirst = false;
    }

    public override void OnEmpty()
    {
        if (CameraMgr.instance)
        {
            CameraMgr.instance.m_curId = -1;
        }
    }

    public void OnDrawGL(DrawGL draw)
    {
        Gizmos.color = Color.white;

        Vector3 pos = m_info.refPos + Quaternion.Euler(0, m_info.horizontalAngle, 0) * m_info.offset;
        if (m_info.NeedShowRefPos)
        {
            draw.DrawSphere(Color.white, m_info.refPos, 0.5f);
            draw.DrawSphere(Color.yellow, pos, 0.5f);
        }


        if (m_info.uselock)//方向上的锁定
            draw.DrawLine(Color.yellow, pos, Quaternion.Euler(m_info.lockEuler) * Vector3.forward, 5);

        //Quaternion rotate = Quaternion.Euler(m_info.verticalAngle, m_info.horizontalAngle, 0);
        //Vector3 forward = rotate * Vector3.forward;
        //draw.DrawSphere(Color.red, StillPos, 0.5f);
        //draw.DrawLine(Color.red, StillPos, forward, 5);
    }

    public void SetFollow(Transform t)
    {
        m_follow = t;
    }

    public void SetTarget(Transform t)
    {
        m_target = t;
    }

}