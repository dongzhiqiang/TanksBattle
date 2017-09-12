#region Header
/**
 * 名称：角色寻路辅助类
 
 * 日期：2016.1.2
 * 描述：
 *  在寻路插件和角色寻路功能之间提供的方便的接口，提供多点寻路、碰撞回避的功能
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;


public class RolePath
{
    public enum enPathType
    {
        once,
        pingPong,//来回
        loop,//循环
    }

    //一些用于调整的常量和变量
    public static Vector3 Invalid_Pos = new Vector3(10000, 10000, 10000);

    public const float Pick_Next_Waypoint_Distance = 0.5f;
    public const float Pick_Next_Waypoint_Distance_Sq = Pick_Next_Waypoint_Distance * Pick_Next_Waypoint_Distance;
    public const float End_Reached_Distance = 0.2f;
    public const float End_Reached_Distance_Sq = End_Reached_Distance * End_Reached_Distance;

    public const float Stuck_Check_Angle = 25;//如果真实位移和目标位移角度相差这么多就算卡住
    public const float Stuck_Stop_Distance=1.5f;//如果和目标点在这个距离内卡住了，那么就停下来，在这里距离外会回避
    public const float Stuck_Stop_Distance_Sq=Stuck_Stop_Distance*Stuck_Stop_Distance;//如果和目标点在这个距离内卡住了，那么就停下来，在这里距离外会回避
    public const int Stuck_Stop_Limit = 3;//卡住停止的次数上限，如果卡住了这么多次，并且Stuck_Stop_Distance内，那么算到达目的地了
    public const int Stuck_Avoid_Limit = 1;//卡住回避的次数上限，如果卡住了这么多次，并且Stuck_Stop_Distance外，那么寻路加上回避点
    public const int Stuck_Avoid_Duration_Limit = 1;//回避不能太频繁，上次回避后超过这个值才可以再回避
    public const float Stuck_Avoid_Distance = 1;//用于调整回避距离

    //调试用
    public static bool s_canStuckAvoid = true;
    public static bool s_canStuckStop = true;
    public static bool s_debug= false;

    #region Fields
    Transform m_root;
    Seeker m_seeker;
    bool m_reached =false;//不需要寻路或者寻路到达了为true,寻路中为false
    int m_loopCounter = 0;
    enPathType m_pathType = enPathType.once;
    Path m_path;
    Vector3 m_lastFoundWaypointPosition;
    float m_lastFoundWaypointTime = -9999;
    int m_currentWaypointIndex = 0;
    Vector3 m_curTargetPos = Invalid_Pos;//当前的目标点
    Vector3 m_lastSearchPos = Invalid_Pos;
    float m_lastSearchTime = 0;
    LinkedList<Vector3> m_poss = new LinkedList<Vector3>();//需要寻路的点的堆栈
    List<Vector3> m_initPoss = new List<Vector3>();
    bool m_pathCompleted = true;
    bool m_inGetPathMode = false; //是否处于只获取路径的模式，这时不会引发走动
    Action<List<Vector3>> m_getPathModeCallback = null;

    //碰撞回避
    int m_stuckCount=0;
    float m_lastAvoidTime= 0;//回避不能太频繁，上次回避后超过一段时间才可以再回避
    Vector3 m_avoidPos = Invalid_Pos;
    Vector3 m_avoidPos2 = Invalid_Pos;//1个点回避有时候会比较突然，这里加多一个点过度
    
    #endregion


    #region Properties
    public Vector3 CurTargetPos { get{return m_curTargetPos;}}
    public bool Reached { get{return m_reached;}}//不需要寻路或者寻路到达了为true,寻路中为false
    //public bool CanStuckAvoid { get{return m_canStuckAvoid;} set{m_canStuckAvoid = value;}}
    //public bool CanStuckStop { get { return m_canStuckStop; } set { m_canStuckStop = value; } }
    #endregion


    #region frame 上层必须调用的框架性接口
    public RolePath(Seeker seeker,Transform root)
    {
        this.m_seeker =seeker;
        this.m_root = root;
        if (m_seeker == null)//检错下
            Debuger.LogError("Seeker 为null");
        Clear();
    }

    public void OnEnable()
    {        
        Clear();
        m_pathCompleted = true;
        m_lastFoundWaypointPosition = m_root.position;
        //Make sure we receive callbacks when paths complete
        m_seeker.pathCallback += OnPathComplete;
    }

    public void OnDisable()
    {
        
        // Abort calculation of path
        if (m_seeker != null && !m_seeker.IsDone()) m_seeker.GetCurrentPath().Error();

        // Release current path
        if (m_path != null) m_path.Release(this);
        m_path = null;

        //Make sure we receive callbacks when paths complete
        m_seeker.pathCallback -= OnPathComplete;
    }
    #endregion


    #region Private Methods
    void OnPathComplete(Path _p)
    {
        m_pathCompleted = true;
        ABPath p = _p as ABPath;
        if (p == null) throw new System.Exception("This function only handles ABPaths, do not use special path types");

        //Claim the new path
        p.Claim(this);

        //已经到达了，那么这条寻路路线就不需要了
        if (m_reached)
        {
            //Debuger.Log("已经到达了，不需要这条寻路路线");
            p.Release(this);//这里要释放下，让path回收

            if (m_inGetPathMode)
                OnGetPath(false);

            return;
        }

        // Path couldn't be calculated of some reason.
        // More info in p.errorLog (debug string)
        if (p.error)
        {
            p.Release(this);

            if (m_inGetPathMode)
                OnGetPath(false);

            return;
        }

        //Release the previous path
        if (m_path != null) m_path.Release(this);

        //Replace the old path
        m_path = p;

        if (m_inGetPathMode)
        {
            OnGetPath(true);
            return;
        }

        //Reset some variables
        m_currentWaypointIndex = 0;
        
        //由于是异步操作，角色可能已经走了一段距离了，这个时候迭代到最近的寻路点
        //The next row can be used to find out if the path could be found or not
        //If it couldn't (error == true), then a message has probably been logged to the console
        //however it can also be got using p.errorLog
        //if (p.error)
        Vector3 p1 = Time.time - m_lastFoundWaypointTime < 0.3f ? m_lastFoundWaypointPosition : p.originalStartPoint;
        Vector3 p2 = m_root.position;
        Vector3 dir = p2 - p1;
        float magn = dir.magnitude;
        dir /= magn;
        int steps = (int)(magn / Pick_Next_Waypoint_Distance);
        for (int i = 0; i <= steps; i++)
        {
            CalculateOffset(p1,5,0.05f);
            p1 += dir;
        }
    }

    void Clear()
    {
        m_reached = true;
        m_lastSearchPos = Invalid_Pos;
        //m_curTargetPos = Invalid_Pos;这个点不用清空，一般到达后还要判断下
        m_initPoss.Clear();
        m_poss.Clear();
        m_loopCounter = 0;
        m_stuckCount = 0;
        m_lastAvoidTime = 0;
        m_avoidPos = Invalid_Pos;
        m_avoidPos2 = Invalid_Pos;
        m_lastSearchTime = 0;
        m_inGetPathMode = false;
        m_getPathModeCallback = null;
    }
    void SearchPath()
    {
        if (m_curTargetPos == Invalid_Pos )
        {
            Debuger.LogError("寻路的目标点没有设置进来");
            return;
        }

        if(m_reached)
        {
            Debuger.LogError("逻辑错误，已经到达了却请求寻路");
            return;
        }

        //回避

        
        //如果上次寻路还没完成，那么这次的就不要再寻了
        if (!m_pathCompleted)
        {
            //Debuger.Log("寻路还没有返回就有新的寻路需求了");
            if (m_lastSearchTime == 0 || Time.time - m_lastSearchTime<0.5){//这里加个小的超时判断以免底层一直不complete
                return;
            }
            
        }

        m_lastSearchTime = Time.time;//无论需不需要搜索都要设置下这个时间，因为Clear()会清空它，如果不设置会导致一些判断出错
        //如果目标点变化不大，那么不用更新了
        if (m_lastSearchPos != Invalid_Pos && Util.XZSqrMagnitude(m_lastSearchPos, m_curTargetPos) < 0.01)
            return;

        m_lastSearchPos = m_curTargetPos;
        m_pathCompleted = false;

        //We should search from the current position
        m_seeker.StartPath(m_root.position, m_lastSearchPos);
        
    }

    /** Calculates target point from the current line segment.
     * \param p Current position
     * \param a Line segment start
     * \param b Line segment end
     * The returned point will lie somewhere on the line segment.
     * \see #forwardLook
     * \todo This function uses .magnitude quite a lot, can it be optimized?
     */
    Vector3 CalculateTargetPoint(Vector3 p, Vector3 a, Vector3 b)
    {
        a.y = p.y;
        b.y = p.y;

        float magn = (a - b).magnitude;
        if (magn == 0) return a;

        float closest = AstarMath.Clamp01(AstarMath.NearestPointFactor(a, b, p));
        Vector3 point = (b - a) * closest + a;
        float distance = (point - p).magnitude;

        float lookAhead = Mathf.Clamp(1 - distance, 0.0F, 1);

        float offset = lookAhead / magn;
        offset = Mathf.Clamp(offset + closest, 0.0F, 1.0F);
        return (b - a) * offset + a;
    }

    bool SearchNext()
    {
        //有下一个寻路点则加上去
        if (m_poss.Count != 0)
        {
            m_curTargetPos = m_poss.First.Value;
            m_poss.RemoveFirst();
            SearchPath();//寻路
            return true;
        }
        //寻路类型是循环的话加回要循环的点
        else if (m_pathType == enPathType.loop)
        {
            m_curTargetPos = m_initPoss[0];
            for (int i = 1; i < m_initPoss.Count; ++i)
                m_poss.AddLast(m_initPoss[i]);
            ++m_loopCounter;
            SearchPath();//寻路
            return true;
        }
        //寻路类型是来回点话要加回点
        else if (m_pathType == enPathType.pingPong)
        {
            bool forward = m_loopCounter % 2 == 1;
            m_curTargetPos = forward ? m_initPoss[1] : m_initPoss[m_initPoss.Count - 2];//这里是设置倒数第二个点，而不是倒数第一个
            if (forward)
            {
                for (int i = 2; i <= m_initPoss.Count -1; ++i)
                    m_poss.AddLast(m_initPoss[i]);
            }
            else
            {
                for (int i = m_initPoss.Count - 3; i >= 0; --i)
                    m_poss.AddLast(m_initPoss[i]);
            }

            ++m_loopCounter;
            SearchPath();//寻路
            return true;
        }
        
        return false;
    }
    #endregion

    public void GetPath(Vector3 destPos, Action<List<Vector3>> resultCallback)
    {
        //强制正在走的停掉
        Stop();
        //初始化值
        m_reached = false;
        m_pathCompleted = false;
        m_inGetPathMode = true;
        m_getPathModeCallback = resultCallback;
        m_curTargetPos = destPos;
        //开始寻路
        m_seeker.StartPath(m_root.position, destPos);
    }

    private void OnGetPath(bool ok)
    {
        if (!m_inGetPathMode)
        {
            Debuger.LogError("不是处于获取路径的模式");
            return;
        }

        var destPos = m_curTargetPos;
        var callback = m_getPathModeCallback;

        m_reached = true;
        m_pathCompleted = true;
        m_inGetPathMode = false;
        m_getPathModeCallback = null;
        m_curTargetPos = Invalid_Pos;

        if (ok)
        {
            callback(m_path.vectorPath);
        }
        else
        {
            var fakePath = new List<Vector3>();
            fakePath.Add(m_root.position);
            fakePath.Add(destPos);
            callback(fakePath);
        }
    }

    //寻路到某个点
    public void Move(Vector3 pos)
    {
        //获取路径模式下不要执行这个
        if (m_inGetPathMode)
            return;

        //寻路优化
        Vector3 lastSearchPos = m_lastSearchPos;
        float lastSearchTime = m_lastSearchTime;
        bool reachBefore = m_reached;

        //回避处理
        bool needAvoid = reachBefore&&m_lastAvoidTime != 0 && Time.time - m_lastAvoidTime <= Stuck_Avoid_Duration_Limit && (m_avoidPos != Invalid_Pos || m_avoidPos2 != Invalid_Pos);
        Vector3 avoidPos =m_avoidPos;
        Vector3 avoidPos2 = m_avoidPos2;
        float lastAvoidTime = m_lastAvoidTime;
       
        Clear();
        //有回避点的话，那么第一个寻路的点就是回避点，否则就是设置进来的点的第一个。
        if (needAvoid)
        {
            m_lastAvoidTime = lastAvoidTime;
            m_avoidPos2 = avoidPos2;
            m_avoidPos = avoidPos;

            if (m_avoidPos!= Invalid_Pos){
                m_curTargetPos = avoidPos;
                m_poss.AddLast(avoidPos2);
            }
            else//可能第一个已经寻过了
            {
                m_curTargetPos = m_avoidPos2;
            }
            
            m_poss.AddLast(pos);
        }
        else
        {
            m_curTargetPos = pos;
        }
        m_pathType = enPathType.once;
        m_reached = false;


        //寻路优化
        if (!reachBefore)
        {
            m_lastSearchTime = lastSearchTime;
            m_lastSearchPos = lastSearchPos;
        }
        SearchPath();
    }

    //沿着一堆寻路点顺序寻路
    public void Move(List<Vector3> poss,enPathType pathType = enPathType.once)
    {
        //获取路径模式下不要执行这个
        if (m_inGetPathMode)
            return;

        if (poss.Count == 0)
        {
            Debuger.LogError("寻路目标点不能为空");
            Stop();
            return;
        }
        else if (poss.Count == 1) //一个的情况下不能多点寻路
        {
            Move(poss[0]);
            return;
        }

        //回避处理
        bool reachBefore = m_reached;
        bool needAvoid = reachBefore && m_lastAvoidTime != 0 && Time.time - m_lastAvoidTime <= Stuck_Avoid_Duration_Limit && (m_avoidPos != Invalid_Pos || m_avoidPos2 != Invalid_Pos);
        Vector3 avoidPos = m_avoidPos;
        Vector3 avoidPos2 = m_avoidPos2;
        float lastAvoidTime = m_lastAvoidTime;
        
        Clear();
        //有回避点的话，那么第一个寻路的点就是回避点，否则就是设置进来的点的第一个
        if (needAvoid )
        {
            m_lastAvoidTime = lastAvoidTime;
            m_avoidPos2 = avoidPos2;
            m_avoidPos = avoidPos;
            if (m_avoidPos != Invalid_Pos)
            {
                m_curTargetPos = avoidPos;
                m_poss.AddLast(avoidPos2);
            }
            else//可能第一个已经寻过了
            {
                m_curTargetPos = m_avoidPos2;
            }

            for (int i = 0; i < poss.Count; ++i)
                m_poss.AddLast(poss[i]);
        }
        else
        {
            m_curTargetPos = poss[0];
            for (int i = 1; i < poss.Count; ++i)
                m_poss.AddLast(poss[i]);
        }
        m_initPoss.AddRange(poss);
        m_pathType = pathType;
        m_reached = false;
        

        //寻路
        SearchPath();
    }

    //停止寻路
    public void Stop()
    {
        m_pathCompleted = true;
        Clear();    
    }

    //返回寻路方向2d(大小为这个update的时间内的运动大小)，如果是zero说明没有寻路方向(可能是还没寻到路或者寻路完了)，调用完这个函数之后可以用Reached属性判断是不是寻路完成了
    public Vector3 CalculateOffset(Vector3 currentPosition,float speed,float delta)
    {
        //获取路径模式下不要执行这个
        if (m_inGetPathMode)
            return Vector3.zero;

        if (m_path == null || m_path.vectorPath == null || m_path.vectorPath.Count == 0 || m_curTargetPos == Invalid_Pos) return Vector3.zero;

        List<Vector3> vPath = m_path.vectorPath;

        if (vPath.Count == 1)
            vPath.Insert(0, currentPosition);
        if (m_currentWaypointIndex >= vPath.Count) { m_currentWaypointIndex = vPath.Count - 1; }
        if (m_currentWaypointIndex <= 1) m_currentWaypointIndex = 1;

        //找到最近的寻路点
        while (true)
        {
            if (m_currentWaypointIndex < vPath.Count - 1)
            {
                //There is a "next path segment"
                float dist = Util.XZSqrMagnitude(vPath[m_currentWaypointIndex], currentPosition);
                //Mathfx.DistancePointSegmentStrict (vPath[currentWaypointIndex+1],vPath[currentWaypointIndex+2],currentPosition);
                if (dist < Pick_Next_Waypoint_Distance_Sq)
                {
                    m_lastFoundWaypointPosition = currentPosition;
                    m_lastFoundWaypointTime = Time.time;
                    m_currentWaypointIndex++;
                }
                else
                    break;
            }
            else
                break;
        }

        //计算出目标参考点，由当前点和寻路点决定
        Vector3 targetPosition = CalculateTargetPoint(currentPosition, vPath[m_currentWaypointIndex - 1], vPath[m_currentWaypointIndex]);
        Vector3 dir = targetPosition - currentPosition;
        dir.y = 0;
        Vector3 dirForward = m_curTargetPos - currentPosition;
        dirForward.y = 0;

        //判断是不是到了
        if (m_currentWaypointIndex == vPath.Count - 1 && dir.sqrMagnitude <= End_Reached_Distance_Sq)
        {
            //有时候路径还没有算出来，而距离比较短，这个时候可能会出错，这种情况下使用方向替代
            if (!m_pathCompleted && Util.XZSqrMagnitude(currentPosition, m_curTargetPos) > End_Reached_Distance_Sq)
            {
                dir = dirForward;
                
            }
            else
            {
                //寻路过的回避点要清空下，用于上面move判断
                if (m_avoidPos == m_curTargetPos)
                    m_avoidPos = Invalid_Pos;
                else if (m_avoidPos2 == m_curTargetPos)
                    m_avoidPos2 = Invalid_Pos;

                if (!SearchNext())//没有下一个寻路点，那么到达
                {
                    Stop();
                    //Send a move request, this ensures gravity is applied
                    return Vector3.zero;
                }                
            }
        }
        
        //算出deltaTime内应该移动多远
        float d = speed * delta;
        if (dirForward != Vector3.zero &&dirForward.sqrMagnitude < d * d)//如果离得目标点位置很近，那么不能走过头
            dir = dirForward;
        else
            dir = dir.normalized * d;

        return dir;
    }


    
    //卡住检查，可能需要挺下来或者做一些回避寻路，调用完后可能Reached==true,外部要注意判断下
    public void CheckStuck(Vector3 currentPosition,Vector3 offset,Vector3 trueOffset)
    {
        //获取路径模式下不要执行这个
        if (m_inGetPathMode)
            return;

        if(m_reached)
        {
            Debuger.LogError("不是寻路状态不能检查是不是卡住");
            return;
        }

        if(s_canStuckAvoid == false&& s_canStuckStop==false)
            return;

        //目标位移是无效的情况下，不需要检测
        if(Mathf.Approximately(offset.x,0) &&Mathf.Approximately(offset.z,0))
            return;

        //短期内回避过了就不要再回避了
        if (m_lastAvoidTime != 0 && Time.time - m_lastAvoidTime < Stuck_Avoid_Duration_Limit)
            return;
        //刚开始寻路的时候也不要检测,不然角色会往奇怪的方向走
        if (!m_pathCompleted)
            return;

        //检查卡住
        bool stuck=false;
        offset.y = 0;
        trueOffset.y = 0;
        float angle= trueOffset == Vector3.zero ? 90:Vector3.Angle(offset, trueOffset); 
        if(trueOffset.sqrMagnitude < offset.sqrMagnitude*0.49  )//距离走少于70%就算卡住
        {
            stuck = true;
            if(s_debug)Debuger.LogError("距离不足的卡住");
        }
        else if(angle > Stuck_Check_Angle)//如果真实位移方向和目标方向相差太大，那么算卡住
        {
            stuck = true;
            if(s_debug)Debuger.LogError("角度偏差的卡住：{0}", angle);
        }
        

        //相关卡住处理
        if(stuck)
        {
            ++m_stuckCount;

            //如果离目标比较近了，寻路下一个点，如果不是，那么左右移动下来规避卡住的问题
            if (Util.XZSqrMagnitude(currentPosition, m_curTargetPos) < Stuck_Stop_Distance_Sq)
            {
                if (s_canStuckStop &&m_stuckCount >= Stuck_Stop_Limit)
                {
                    m_stuckCount = 0;
                    if (!SearchNext())//没有下一个寻路点，那么到达
                        Stop();
                }
            }
            else
            {
                if (s_canStuckAvoid)
                {
                    m_stuckCount = 0;

                    //计算回避方向,如果真实方向没有的情况下当作相对方向的垂直方向
                    Vector3 avoidDir = angle < 90 ? trueOffset : new Vector3(offset.z, 0, -offset.x);
                    if (angle > 90) angle = 90;

                    //计算回避距离，角度越大，那么回避的距离就越短，角度比较小的时候可以回避长一点
                    float distance = 0.6f + ((90f - angle) / 90f) * Stuck_Avoid_Distance;
                    m_avoidPos = currentPosition + avoidDir.normalized * distance;
                    if (s_debug) Debug.DrawLine(currentPosition, m_avoidPos, Color.red, 3);

                    //一个回避点不够，这里计算多一个点，让回避更平滑
                    Quaternion from = Quaternion.LookRotation(avoidDir);
                    Quaternion to = Quaternion.LookRotation(offset);
                    Quaternion sample = Quaternion.Lerp(from, to, 0.5f);
                    Vector3 dir = sample * Vector3.forward;
                    m_avoidPos2 = m_avoidPos + dir * distance * 2;
                    if (s_debug) Debug.DrawLine(m_avoidPos, m_avoidPos2, Color.red, 3);

                    m_lastAvoidTime = Time.time;

                    //当前的寻路点往后排，寻路到回避点
                    m_poss.AddFirst(m_curTargetPos);
                    m_curTargetPos = m_avoidPos;
                    m_poss.AddFirst(m_avoidPos2);
                    SearchPath();
                }
               
            }
        }
        else
        {
            m_stuckCount = 0;
        }


    }

}
