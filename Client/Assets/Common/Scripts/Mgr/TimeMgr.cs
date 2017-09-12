#region Header
/**
 * 名称：时间管理
 
 * 日期：2015.9.24
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public class TimeMgr : SingletonMonoBehaviour<TimeMgr>
{
    //定时器
    public class Timer
    {
        public static int s_idCounter = 0;
        public int id;
        public float interval;
        public float delay;//-1表明和interval一样
        public int num;//-1表明一直循环
        public Action<System.Object> onTimer;
        public System.Object param;

        public System.DateTime lastTime;
        public bool isRemove;
        public int counter;

        public Timer(float interval, Action<System.Object> onTimer, System.Object param, float delay = -1, int num = -1)
        {
            this.id = ++s_idCounter;
            this.interval = interval;
            this.delay = delay;
            this.num = num;
            this.onTimer = onTimer;
            this.param = param;
            isRemove =false;
            lastTime = System.DateTime.Now;//Time.unscaledTime 只能在主线程中调用
            counter =-1;
        }

        public void Release()
        {
            if (!TimeMgr.s_debugDestroy)
                TimeMgr.instance.RemoveTimer(this);
        }

        public override string ToString()
        {
            return string.Format("{0} 首次延迟:{1} 间隔:{2} 执行到第几次:{3}",lastTime,delay,interval,num);
        }
    }

    

    //计划任务,未实现
    //public class Scheduler
    //{
    //    //年月日时分秒等，参考cron表达式
    //}
    //用于时间缩放的冲突处理类
    public class TimeScaleHandle : IShareThingHandle
    {
        float scale;
        float duration;
        float beginTime;

        public bool IsOver { get { return duration<0?false:(Time.unscaledTime - beginTime) >=duration; } }

        public TimeScaleHandle(float scale, float duration)
        {
            this.scale = scale;
            this.duration = duration;
            this.beginTime = Time.unscaledTime;
        }

        public override void OnLast(IShareThingHandle prev)
        {

            Time.timeScale = scale;
        }

        public override void OnOverlay() { }

        public override void OnEmpty()
        {
            Time.timeScale = 1;
        }
    }
    #region Fields
    public int m_pauseCounter = 0;//暂停计数，设置成public界面上可以看到

    long m_serverTimeDelta = 0;         //服务器时间相对于客户端时间的差值
    int m_serverTimeZoneOffset;         //服务器时区的本地时间相对于UTC的分钟差（东半区就是正，西半区就是负）
    float m_logicTime=0;
    float m_logicTimeDelta=0;
    ShareThing m_timeScale= new ShareThing();
    LinkedList<Timer> m_timers= new LinkedList<Timer>();
    List<Timer> m_tempTimers = new List<Timer>();//用于定时器触发时回调的临时变量
    List<LinkedListNode<IShareThingHandle>> m_removeTimeScaleHandles = new List<LinkedListNode<IShareThingHandle>>();//用于时间缩放处理自动结束
    System.Object m_lock = new System.Object();
    System.DateTime m_startTime;

    public static DateTime s_dateTime1970 = new DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
    public static bool s_sundayFirst = false;
    public static int s_dayBreakPoint = 0;
    #endregion


    #region Properties
    public bool IsPause { get { return IsStop||m_pauseCounter > 0; } }
    public bool IsStop { get{return Time.timeScale == 0;}}

    //逻辑时间，用于游戏逻辑(ai、移动)
    public float logicTime { get { return this.m_logicTime; } }
    public float logicDelta { get { return this.m_logicTimeDelta; } }

    //普通时间，受缩放影响
    public float time { get { return Time.time; } }
    public float delta { get { return Time.deltaTime; } }

    //真实时间,不受缩放影响
    public float realTime { get { return Time.realtimeSinceStartup; } }
    public float realDelta { get { return Time.unscaledDeltaTime; } }
    #endregion


    #region Constructors
    public TimeMgr()
    {
        m_startTime = System.DateTime.Now;//由于要支持其他线程可以调用定时器，所以不能用unity的Time的接口
        m_serverTimeZoneOffset = GetClientTimeZoneOffset();
    }

    #endregion

    #region Mono Frame
    void Start()
    {

    }
    
    // Update is called once per frame
    void Update()
    {
        if (IsPause)
        {
            m_logicTimeDelta = 0;
        }
        else
        {
            m_logicTimeDelta = Time.deltaTime;
            m_logicTime += m_logicTimeDelta;
        }

        System.DateTime now = System.DateTime.Now;
        //定时器回调
        lock (m_lock)
        {
            LinkedListNode<Timer> node = m_timers.First;
            Timer timer;
            LinkedListNode<Timer> cur;
            while (node != null)
            {
                timer = node.Value;
                cur = node;
                node = node.Next;
                System.TimeSpan span = now - timer.lastTime;
                if ((timer.counter != -1 && span.TotalSeconds >= timer.interval) ||
                    (timer.counter == -1 && span.TotalSeconds >= (timer.delay == -1 ? timer.interval : timer.delay)))
                {
                    ++timer.counter;
                    timer.lastTime = now;
                    //timer.onTimer(timer.param);回调中可能会删除别的定时器，所以移到外面回调
                    m_tempTimers.Add(timer);//回调中可能会删除别的定时器，所以移到外面回调
                    if (timer.num != -1 && (timer.counter + 1) >= timer.num)
                        RemoveTimer(cur);
                    //Debuger.Log("定时器触发:{0}",timer.ToString());
                }
            }
        }
        
        if (m_tempTimers.Count != 0)
        {
            for (int i = 0; i < m_tempTimers.Count; ++i)
                m_tempTimers[i].onTimer(m_tempTimers[i].param);
            
            m_tempTimers.Clear();
        }

        //时间缩放处理自动结束
        LinkedListNode<IShareThingHandle> handleNode = m_timeScale.m_handles.First;
        while(handleNode!= null)
        {
            if(((TimeScaleHandle)handleNode.Value).IsOver)
                m_removeTimeScaleHandles.Add(handleNode);
            handleNode = handleNode.Next;
        }
        if (m_removeTimeScaleHandles.Count != 0)
        {
            m_timeScale.Remove(m_removeTimeScaleHandles);
            m_removeTimeScaleHandles.Clear();
        }
    }
    #endregion



    #region Private Methods
    void OnTimerAdapter(System.Object param)
    {
        Action a = (Action)param;
        a();
    }
    void RemoveTimer(LinkedListNode<Timer> node)
    {
        if (node.Value.isRemove)
        {
            Debuger.LogError("逻辑错误");
        }

       node.Value.isRemove = true;
       m_timers.Remove(node);
       
        
    }


    #endregion


    

    //让游戏逻辑暂停:不刷怪，怪不寻路 ,动画不暂停
    public void AddPause()
    {
        if (m_pauseCounter == 0)
        {
#if !ART_DEBUG
            CombatMgr.instance.StopAllLogic();

            EventMgr.FireAll(MSG.MSG_FRAME, MSG_FRAME.FRAME_PAUSE_CHANGE, true);
#endif
        }
        
        ++m_pauseCounter;
        
    }

    public void SubPause()
    {
        --m_pauseCounter;
        if (m_pauseCounter == 0)
        {
#if !ART_DEBUG
            EventMgr.FireAll(MSG.MSG_FRAME, MSG_FRAME.FRAME_PAUSE_CHANGE, false);
#endif
        }
        
        if (m_pauseCounter < 0)
        {
            m_pauseCounter = 0;
            Debuger.LogError("逻辑出错，暂停计数小于0");
        }
    }
    public void ResetPause()
    {
        m_pauseCounter= 0;
    }

    //设置时间缩放，如果是0的话，游戏完全暂停。注意如果没有设置持续时间的话，用完要自己释放
    public TimeScaleHandle AddTimeScale(float scale, float duration, int priority = 0)
    {
        TimeScaleHandle handle = new TimeScaleHandle(scale,duration);
        m_timeScale.Add(handle, priority);
        return handle;
    }


    //delay多少秒后执行第一次，然后每间隔interval多少秒执行一次
    //delay=-1,则和interval一样。num=-1则没有次数限制
    public Timer AddTimer(float interval,Action<System.Object> onTimer,System.Object param,float delay=-1, int num=1)
    {
        //这里单位从秒转为tick
        Timer t = new Timer(interval, onTimer, param, delay, num);
        lock (m_lock)
        {
            m_timers.AddLast(t);
        }
        
        return t;
    }

    //delay多少秒后执行第一次，然后每间隔interval多少秒执行一次
    //delay=-1,则和interval一样。num=-1则没有次数限制
    public Timer AddTimer(float interval, Action onTimer, float delay = -1, int num = 1)
    {
        
        return AddTimer(interval,OnTimerAdapter,onTimer,delay,num);
    }

    public void RemoveTimer(Timer t)
    {
        lock (m_lock)
        {
            if (t.isRemove)
                return;
            LinkedListNode<Timer> node =m_timers.FindLast(t);
            if (node == null)
            {
                Debuger.Log("逻辑错误");
                return;
            }
            RemoveTimer(node);   
        }        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sundayFirst"></param>
    /// <param name="dayBreakPoint"></param>
    public void SetParameter(int sundayFirst, int dayBreakPoint)
    {
        s_sundayFirst = sundayFirst != 0;
        s_dayBreakPoint = dayBreakPoint;
    }

    /// <summary>
    /// 设置服务端的时间信息
    /// </summary>
    /// <param name="ts">服务端时间戳</param>
    /// <param name="tzOff">服务端的本地时间与UTC时间的分钟差</param>
    /// <returns></returns>
    public void SetServerTimeInfo(long ts, int tzOff)
    {
        m_serverTimeDelta = ts - GetTrueTimestamp();
        m_serverTimeZoneOffset = tzOff;
        var hour = tzOff / 60;
        var minute = tzOff % 60;
    }

    /// <summary>
    /// 把时间对象转为服务端时区的时间对象
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public DateTime ConvertToServerDateTime(DateTime dt)
    {
        var diff = GetServerTimeZoneOffset() - GetClientTimeZoneOffset();
        return dt.AddMinutes(diff);
    }

    /// <summary>
    /// 时间对象转时间戳，单位秒
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public long GetTimestampFromDate(System.DateTime date)
    {
        //这种情况下，是0时区的本地时间
        if (date.Kind == DateTimeKind.Utc)
            return (long)date.Subtract(s_dateTime1970).TotalSeconds;
        //这种情况下，可能是客户端时区（Kind为Local），也可能是服务端时区（Unspecified）
        else
            return (long)date.ToUniversalTime().Subtract(s_dateTime1970).TotalSeconds;
    }

    /// <summary>
    /// 时间对象转时间戳，单位毫秒
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public long GetTimestampMSFromDate(System.DateTime date)
    {
        //这种情况下，是0时区的本地时间
        if (date.Kind == DateTimeKind.Utc)
            return (long)date.Subtract(s_dateTime1970).TotalMilliseconds;
        //这种情况下，可能是客户端时区（Kind为Local），也可能是服务端时区（Unspecified）
        else
            return (long)date.ToUniversalTime().Subtract(s_dateTime1970).TotalMilliseconds;
    }

    /// <summary>
    /// 时间戳(单位秒)转客户端时区的时间对象
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public System.DateTime TimestampToClientDateTime(long timestamp)
    {
        return s_dateTime1970.AddSeconds(timestamp).ToLocalTime();
    }

    /// <summary>
    /// 时间戳(单位秒)转服务端时区的时间对象
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public System.DateTime TimestampToServerDateTime(long timestamp)
    {
        return ConvertToServerDateTime(TimestampToClientDateTime(timestamp));
    }

    /// <summary>
    /// 时间戳(单位毫秒)转客户端时区的时间对象
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public System.DateTime TimestampMSToClientDateTime(long timestamp)
    {
        return s_dateTime1970.AddMilliseconds(timestamp).ToLocalTime();
    }

    /// <summary>
    /// 时间戳(单位毫秒)转服务端时区的时间对象
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public System.DateTime TimestampMSToServerDateTime(long timestamp)
    {
        return ConvertToServerDateTime(TimestampMSToClientDateTime(timestamp));
    }

    /// <summary>
    /// 获取服务器的时间戳，单位秒
    /// </summary>
    /// <returns></returns>
    public long GetTimestamp()
    {
        return GetTrueTimestamp() + m_serverTimeDelta;
    }

    /// <summary>
    /// 获取服务器的时间戳，单位毫秒
    /// </summary>
    /// <returns></returns>
    public long GetTimestampMS()
    {
        return GetTrueTimestampMS() + m_serverTimeDelta * 1000;
    }

    /// <summary>
    /// 获取客户端的真实时间戳，单位秒
    /// </summary>
    /// <returns></returns>
    public long GetTrueTimestamp()
    {
        return GetTimestampFromDate(GetTrueDateTime());
    }

    /// <summary>
    /// 获取客户端的真实时间戳，单位毫秒
    /// </summary>
    /// <returns></returns>
    public long GetTrueTimestampMS()
    {
        return GetTimestampMSFromDate(GetTrueDateTime());
    }

    /// <summary>
    /// 获取服务端时间戳对应的客户端时区的时间对象
    /// </summary>
    /// <returns></returns>
    public System.DateTime GetClientDateTime()
    {
        if (m_serverTimeDelta == 0)
            return GetTrueDateTime();
        else
            return GetTrueDateTime().AddSeconds(m_serverTimeDelta);
    }

    /// <summary>
    /// 获取服务端时间戳对应的服务端时区的时间对象
    /// </summary>
    /// <returns></returns>
    public System.DateTime GetServerDateTime()
    {
        return ConvertToServerDateTime(GetClientDateTime());
    }

    /// <summary>
    /// 获取客户端真实时间戳对应的客户端时区的时间对象
    /// </summary>
    /// <returns></returns>
    public System.DateTime GetTrueDateTime()
    {
        return System.DateTime.Now;
    }

    /// <summary>
    /// 用服务端时区判断是否同一天
    /// </summary>
    /// <param name="ts1"></param>
    /// <param name="ts2"></param>
    /// <returns></returns>
    public bool IsSameDay(long ts1, long ts2)
    {
        if (ts1 == ts2)
            return true;

        //把时间戳移到0点
        ts1 -= s_dayBreakPoint;
        ts2 -= s_dayBreakPoint;

        var d1 = TimestampToServerDateTime(ts1);
        var d2 = TimestampToServerDateTime(ts2);

        return d1.Date == d2.Date;
    }

    /// <summary>
    /// 用客户端时区判断是否同一天
    /// </summary>
    /// <param name="ts1"></param>
    /// <param name="ts2"></param>
    /// <returns></returns>
    public bool IsClientSameDay(long ts1, long ts2)
    {
        if (ts1 == ts2)
            return true;

        //把时间戳移到0点
        ts1 -= s_dayBreakPoint;
        ts2 -= s_dayBreakPoint;

        var d1 = TimestampToClientDateTime(ts1);
        var d2 = TimestampToClientDateTime(ts2);

        return d1.Date == d2.Date;
    }

    /// <summary>
    /// 用服务端时间和服务端时区判断是否今天
    /// </summary>
    /// <param name="ts1"></param>
    /// <param name="ts2"></param>
    /// <returns></returns>
    public bool IsToday(long ts)
    {
        return IsSameDay(ts, GetTimestamp());
    }

    /// <summary>
    /// 用服务端时间和客户端时区判断是否今天
    /// </summary>
    /// <param name="ts1"></param>
    /// <param name="ts2"></param>
    /// <returns></returns>
    public bool IsClientToday(long ts)
    {
        return IsClientSameDay(ts, GetTimestamp());
    }

    /// <summary>
    /// 用客户端真实时间和客户端时区判断是否今天
    /// </summary>
    /// <param name="ts1"></param>
    /// <param name="ts2"></param>
    /// <returns></returns>
    public bool IsTrueToday(long ts)
    {
        return IsClientSameDay(ts, GetTrueTimestamp());
    }

    /// <summary>
    /// 用服务端时区判断是否同一周
    /// </summary>
    /// <param name="ts1"></param>
    /// <param name="ts2"></param>
    /// <returns></returns>    
    public bool IsSameWeek(long ts1, long ts2)
    {
        if (ts1 == ts2)
            return true;

        var d1 = TimestampToServerDateTime(ts1);
        var d2 = TimestampToServerDateTime(ts2);

        var wd1 = (int)d1.DayOfWeek;  //周日是0，周六是6
        var wd2 = (int)d2.DayOfWeek;  //周日是0，周六是6

        //把日期移到开始
        //如果开始日期是周日，那就移到周日
        if (s_sundayFirst)
        {
            ts1 -= wd1 * 86400;
            ts2 -= wd2 * 86400;
        }
        //否则就认为开始日期是周一，移到周一
        else
        {
            ts1 -= ((wd1 == 0 ? 7 : wd1) - 1) * 86400;
            ts2 -= ((wd2 == 0 ? 7 : wd2) - 1) * 86400;
        }

        d1 = TimestampToServerDateTime(ts1);
        d2 = TimestampToServerDateTime(ts2);

        return d1.Date == d2.Date;
    }

    /// <summary>
    /// 用客户端时区判断是否同一周
    /// </summary>
    /// <param name="ts1"></param>
    /// <param name="ts2"></param>
    /// <returns></returns>    
    public bool IsClientSameWeek(long ts1, long ts2)
    {
        if (ts1 == ts2)
            return true;

        var d1 = TimestampToClientDateTime(ts1);
        var d2 = TimestampToClientDateTime(ts2);

        var wd1 = (int)d1.DayOfWeek;  //周日是0，周六是6
        var wd2 = (int)d2.DayOfWeek;  //周日是0，周六是6

        //把日期移到开始
        //如果开始日期是周日，那就移到周日
        if (s_sundayFirst)
        {
            ts1 -= wd1 * 86400;
            ts2 -= wd2 * 86400;
        }
        //否则就认为开始日期是周一，移到周一
        else
        {
            ts1 -= ((wd1 == 0 ? 7 : wd1) - 1) * 86400;
            ts2 -= ((wd2 == 0 ? 7 : wd2) - 1) * 86400;
        }

        d1 = TimestampToClientDateTime(ts1);
        d2 = TimestampToClientDateTime(ts2);

        return d1.Date == d2.Date;
    }

    /// <summary>
    /// 用服务端时间和服务端时区判断是否本周
    /// </summary>
    /// <param name="ts1"></param>
    /// <param name="ts2"></param>
    /// <returns></returns>
    public bool IsThisWeek(long ts)
    {
        return IsSameWeek(ts, GetTimestamp());
    }

    /// <summary>
    /// 用服务端时间和客户端时区判断是否本周
    /// </summary>
    /// <param name="ts1"></param>
    /// <param name="ts2"></param>
    /// <returns></returns>
    public bool IsClientThisWeek(long ts)
    {
        return IsClientSameWeek(ts, GetTimestamp());
    }

    /// <summary>
    /// 用客户端真实时间和客户端时区判断是否本周
    /// </summary>
    /// <param name="ts1"></param>
    /// <param name="ts2"></param>
    /// <returns></returns>
    public bool IsTrueThisWeek(long ts)
    {
        return IsClientSameWeek(ts, GetTrueTimestamp());
    }

    /// <summary>
    /// 用服务端时区判断是否同一月
    /// </summary>
    /// <param name="ts1"></param>
    /// <param name="ts2"></param>
    /// <returns></returns>
    public bool IsSameMonth(long ts1, long ts2)
    {
        if (ts1 == ts2)
            return true;

        var d1 = TimestampToServerDateTime(ts1);
        var d2 = TimestampToServerDateTime(ts2);

        return d1.Year == d2.Year && d1.Month == d2.Month;
    }

    /// <summary>
    /// 用客户端时区判断是否同一月
    /// </summary>
    /// <param name="ts1"></param>
    /// <param name="ts2"></param>
    /// <returns></returns>
    public bool IsClientSameMonth(long ts1, long ts2)
    {
        if (ts1 == ts2)
            return true;

        var d1 = TimestampToClientDateTime(ts1);
        var d2 = TimestampToClientDateTime(ts2);

        return d1.Year == d2.Year && d1.Month == d2.Month;
    }

    /// <summary>
    /// 用服务端时间和服务端时区判断是否本月
    /// </summary>
    /// <param name="ts1"></param>
    /// <param name="ts2"></param>
    /// <returns></returns>
    public bool IsThisMonth(long ts)
    {
        return IsSameMonth(ts, GetTimestamp());
    }

    /// <summary>
    /// 用服务端时间和客户端时区判断是否本月
    /// </summary>
    /// <param name="ts1"></param>
    /// <param name="ts2"></param>
    /// <returns></returns>
    public bool IsClientThisMonth(long ts)
    {
        return IsClientSameMonth(ts, GetTimestamp());
    }

    /// <summary>
    /// 用客户端真实时间和客户端时区判断是否本月
    /// </summary>
    /// <param name="ts1"></param>
    /// <param name="ts2"></param>
    /// <returns></returns>
    public bool IsTrueThisMonth(long ts)
    {
        return IsClientSameMonth(ts, GetTrueTimestamp());
    }

    /// <summary>
    /// 用服务端时区判断是否同一年
    /// </summary>
    /// <param name="ts1"></param>
    /// <param name="ts2"></param>
    /// <returns></returns>
    public bool IsSameYear(long ts1, long ts2)
    {
        if (ts1 == ts2)
            return true;

        var d1 = TimestampToServerDateTime(ts1);
        var d2 = TimestampToServerDateTime(ts2);

        return d1.Year == d2.Year;
    }

    /// <summary>
    /// 用客户端时区判断是否同一年
    /// </summary>
    /// <param name="ts1"></param>
    /// <param name="ts2"></param>
    /// <returns></returns>
    public bool IsClientSameYear(long ts1, long ts2)
    {
        if (ts1 == ts2)
            return true;

        var d1 = TimestampToClientDateTime(ts1);
        var d2 = TimestampToClientDateTime(ts2);

        return d1.Year == d2.Year;
    }

    /// <summary>
    /// 用服务端时间和服务端时区判断是否今年
    /// </summary>
    /// <param name="ts1"></param>
    /// <param name="ts2"></param>
    /// <returns></returns>
    public bool IsThisYear(long ts)
    {
        return IsSameYear(ts, GetTimestamp());
    }

    /// <summary>
    /// 用服务端时间和客户端时区判断是否今年
    /// </summary>
    /// <param name="ts1"></param>
    /// <param name="ts2"></param>
    /// <returns></returns>
    public bool IsClientThisYear(long ts)
    {
        return IsClientSameYear(ts, GetTimestamp());
    }

    /// <summary>
    /// 用客户端真实时间和客户端时区判断是否今年
    /// </summary>
    /// <param name="ts1"></param>
    /// <param name="ts2"></param>
    /// <returns></returns>
    public bool IsTrueThisYear(long ts)
    {
        return IsClientSameYear(ts, GetTrueTimestamp());
    }

    /// <summary>
    /// 获取当前设备的本地时间跟UTC时间相差的【分钟数】，可正可负
    /// </summary>
    /// <returns></returns>
    public int GetClientTimeZoneOffset()
    {
        TimeSpan ts = DateTime.Now - DateTime.UtcNow;
        return (int)ts.TotalMinutes;
    }

    /// <summary>
    /// 服务端的本地时间跟UTC时间相差的【分钟数】，可正可负
    /// </summary>
    /// <returns></returns>
    public int GetServerTimeZoneOffset()
    {
        return m_serverTimeZoneOffset;
    }

    /// <summary>
    /// 把服务端的时、分转成客户端的时、分
    /// </summary>
    /// <param name="hour"></param>
    /// <param name="minute"></param>
    public void AdjustServerTimeToClient(ref int hour, ref int minute)
    {
        var minutes = hour * 60 + minute;
        minutes = AdjustServerTimeToClient(minutes);
        hour = minutes / 60;
        minute = minutes % 60;
    }

    /// <summary>
    /// 把服务端的分钟数转成客户端的分钟数
    /// </summary>
    /// <param name="minutes"></param>
    /// <returns></returns>
    public int AdjustServerTimeToClient(int minutes)
    {
        var diff = GetClientTimeZoneOffset() - GetServerTimeZoneOffset();
        minutes = (minutes + diff + 24 * 60) % (24 * 60);
        return minutes;
    }

    /// <summary>
    /// 用以服务器时区为准的分钟数的开始、结束时间
    /// </summary>
    /// <param name="startMinutes"></param>
    /// <param name="endMinutes"></param>
    /// <returns></returns>
    public bool IsNowBetweenTime(int startMinutes, int endMinutes)
    {
        var curDate = GetServerDateTime();
        var curMinutes = curDate.Hour * 60 + curDate.Minute;
        return (startMinutes <= endMinutes && (curMinutes >= startMinutes && curMinutes < endMinutes))
            ||
            (startMinutes > endMinutes && (curMinutes >= startMinutes || curMinutes < endMinutes));
    }

    /// <summary>
    /// 用以服务器时区为准的时、分的开始、结束时间
    /// </summary>
    /// <param name="startHour"></param>
    /// <param name="startMinute"></param>
    /// <param name="endHour"></param>
    /// <param name="endMinute"></param>
    /// <returns></returns>
    public bool IsNowBetweenTime(int startHour, int startMinute, int endHour, int endMinute)
    {
        return IsNowBetweenTime(startHour * 60 + startMinute, endHour * 60 + endMinute);
    }

    /// <summary>
    /// 用以客户端时区为准的分钟数的开始、结束时间
    /// </summary>
    /// <param name="startMinutes"></param>
    /// <param name="endMinutes"></param>
    /// <returns></returns>
    public bool IsNowBetweenClientTime(int startMinutes, int endMinutes)
    {
        var curDate = GetClientDateTime();
        var curMinutes = curDate.Hour * 60 + curDate.Minute;
        return (startMinutes <= endMinutes && (curMinutes >= startMinutes && curMinutes < endMinutes))
            ||
            (startMinutes > endMinutes && (curMinutes >= startMinutes || curMinutes < endMinutes));
    }

    /// <summary>
    /// 用以客户端时区为准的时、分的开始、结束时间
    /// </summary>
    /// <param name="startHour"></param>
    /// <param name="startMinute"></param>
    /// <param name="endHour"></param>
    /// <param name="endMinute"></param>
    /// <returns></returns>
    public bool IsNowBetweenClientTime(int startHour, int startMinute, int endHour, int endMinute)
    {
        return IsNowBetweenClientTime(startHour * 60 + startMinute, endHour * 60 + endMinute);
    }
}