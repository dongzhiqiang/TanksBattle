using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
/*
 * *********************************************************
 * 名称：消息管理器
 
 * 日期：2015.7.11
 * 描述：
 * *********************************************************
 */

public class EventNotifier : IdType
{
    public delegate void OnRemoveOb(EventObserver observer);
    public Dictionary<int, Dictionary<int, EventObserver>> observersByMsgCode = new Dictionary<int, Dictionary<int, EventObserver>>();
    public Dictionary<int, EventObserver> observersById = new Dictionary<int, EventObserver>();
    public bool isRemove=false;//是不是已经删除，这个值是给消息管理器用的，外部不要修改
    public object _parent;
    public OnRemoveOb onRemoveOb;

    public EventNotifier() { }
    public T GetParent<T>()
    {
        return (T)_parent;
    }

    public void SetParent(object p)
    {
        _parent=p;
    }
    public void Init()
    {

        isRemove = false;
    }

    public override void OnClear()
    {
        observersByMsgCode.Clear();
        observersById.Clear();
        _parent = null;
        onRemoveOb = null;
    }
    
    public void Add(EventObserver observer)
    {
        Dictionary<int, EventObserver> obs = observersByMsgCode.GetNewIfNo(observer.Key);
        if (obs.Count > 100)
        {
            Debuger.LogError("EventMgr同一个对象的同一个消息的监听者过多，是不是泄露：{0} {1} {2}", observer.msg, observer.code, Util.GetDelegateName(observer.GetDelegate()));
        }

        
        obs.Add(observer.Id, observer);
        observersById.Add(observer.Id, observer);
    }

    public void Remove(EventObserver observer)
    {
        Dictionary<int, EventObserver> obs = null;
        if (observersByMsgCode.TryGetValue(observer.Key, out obs))
        {
            if (!obs.Remove(observer.Id))
                Debuger.LogError(string.Format("删除了不存在的观察者2：{0}", observer.Key));
        }
        else
            Debuger.LogError(string.Format("无效的key：{0}", observer.Key));

        if(!observersById.Remove(observer.Id))
            Debuger.LogError(string.Format("删除了不存在的观察者：{0}", observer.Key));

        if (onRemoveOb!=null)
            onRemoveOb(observer);
    }
    public void Remove()
    {
        EventMgr.Remove(this);
    }

    public void Fire(int msg, int code, object param)
    {
        EventMgr.Fire(this,msg,code,param);
    }

    public string LogObservers()
    {
        string log="";
        foreach(var pair in observersByMsgCode){
            if(pair.Value.Count == 0)
                continue;
            log += string.Format("msg:{0} code:{1} {2}个监听\n", pair.Key / 100000, pair.Key % 100000, pair.Value.Count);
            foreach(var ob in pair.Value.Values){
                log += string.Format("      {0}\n", Util.GetDelegateName(ob.GetDelegate()));
            
            }
        }
        return log;
    }

    public int Add(int msg, int code, EventObserver.OnFire onFire) { return EventMgr.Add(this, msg, code, onFire); }
    public int Add(int msg, int code, EventObserver.OnFire1 onFire) { return EventMgr.Add(this, msg, code, onFire); }
    public int Add(int msg, int code, EventObserver.OnFire2 onFire) { return EventMgr.Add(this, msg, code, onFire); }
    public int Add(int msg, int code, EventObserver.OnFire3 onFire){return EventMgr.Add(this, msg, code, onFire);}
    public int Add(int msg, int code, EventObserver.OnFireOb onFire) { return EventMgr.Add(this, msg, code, onFire); }
    public int AddVote(int msg, int code, EventObserver.OnVote onVote) { return EventMgr.AddVote(this, msg, code, onVote); }
}


public class EventObserver : IdType
{
    enum enFire{
        min,
        onFire,
        onFire1,
        onFire2,
        onFire3,
        onFireOb,
        onVote,
        max,
    }
    public delegate void OnFire();
    public delegate void OnFire1(object param);
    public delegate void OnFire2(object param1, object param2);
    public delegate void OnFire3(object param1, object param2, object param3);
    public delegate void OnFireOb(object param1, object param2, object param3,EventObserver observer);
    public delegate bool OnVote(object param1, object param2, object param3, EventObserver observer);


    public EventNotifier notifier;//派发者，如果是全局监听的话，派发者会动态变化
    public int msg;
    public int code;
    public int removeCounter = 0;//是不是已经删除，这个值是给消息管理器用的，外部不要修改
    public bool isAll;//是不是全局监听
    enFire fireType;
    
    public OnFire onFire;
    public OnFire1 onFire1;
    public OnFire2 onFire2;
    public OnFire3 onFire3;
    public OnFireOb onFireOb;
    public OnVote onVote;//如果返回true，则继续往下执行
    
    public int Key { get{return msg*100000+code;}}
    
    

    public EventObserver() {  }
    
    public void Init(EventNotifier notifier, int msg, int code, bool isAll,object onFire)
    {
        if (onFire is OnFire){this.onFire = (OnFire)onFire;fireType = enFire.onFire;}
        else if (onFire is OnFire1) { this.onFire1 = (OnFire1)onFire; fireType = enFire.onFire1; }
        else if (onFire is OnFire2) { this.onFire2 = (OnFire2)onFire; fireType = enFire.onFire2; }
        else if (onFire is OnFire3) { this.onFire3 = (OnFire3)onFire; fireType = enFire.onFire3; }
        else if (onFire is OnFireOb) { this.onFireOb = (OnFireOb)onFire; fireType = enFire.onFireOb; }
        else if (onFire is OnVote) { this.onVote = (OnVote)onFire; fireType = enFire.onVote; }
        

        this.notifier = notifier;
        this.msg = msg;
        this.code = code;
        this.isAll = isAll;
        removeCounter =0;
    }



    public override void OnClear()
    {
        notifier = null;
        onFire = null;
        onFire1 = null;
        onFire2 = null;
        onFire3 = null;
        onFireOb = null;
        onVote = null;
    }

    public bool OnHandleFire(object param1, object param2, object param3)
    {
        switch (fireType)
        {
            case enFire.onFire:if(onFire!=null) onFire();return true;
            case enFire.onFire1: if (onFire1 != null) onFire1(param1); return true;
            case enFire.onFire2: if (onFire2 != null) onFire2(param1, param2); return true;
            case enFire.onFire3: if (onFire3 != null) onFire3(param1, param2, param3); return true;
            case enFire.onFireOb: if (onFireOb != null) onFireOb(param1, param2, param3, this); return true;
            case enFire.onVote: if (onVote != null) return onVote(param1, param2, param3, this); else return true;
            default: Debuger.LogError("事件管理器.未知的回调类型:{0}",fireType);return true;
        }
    }

    public Delegate GetDelegate()
    {
        switch (fireType)
        {
            case enFire.onFire: return onFire;
            case enFire.onFire1: return onFire1;
            case enFire.onFire2: return onFire2;
            case enFire.onFire3:  return onFire3;
            case enFire.onFireOb:  return onFireOb;
            case enFire.onVote: return onVote;
            default: Debuger.LogError("事件管理器,GetDelegate().未知的回调类型:{0}", fireType); return null;
        }
    }

    public T GetParent<T>()
    {
        return notifier == null ? default(T) : notifier.GetParent<T>();
    }
}

public class EventMgr
{
    public const int Invalid_Id =0;
    static EventNotifier m_allNotifier=null;
    static EventNotifier AllNotifier//用于监听全局
    {
        get
        {
            if (m_allNotifier == null)
                m_allNotifier = Get();
            return m_allNotifier;
        }
    }

    static EventNotifier m_allFireNotifier = null;
    static EventNotifier AllFireNotifier//监听广播全局
    {
        get
        {
            if (m_allFireNotifier == null)
                m_allFireNotifier = Get();
            return m_allFireNotifier;
        }
    }

    //发送者，这里用不同的索引方式记录，提升效率
    static Dictionary<int, EventNotifier> m_notifiersById = new Dictionary<int, EventNotifier>(); 

    //观察者，这里用不同的索引方式记录，提升效率
    static Dictionary<int, EventObserver> m_observersById = new Dictionary<int, EventObserver>();
    
    //避免消息遍历执行中删除导致的出错
    static int m_fireCount = 0;//正在fire中的数量
    static List<EventObserver> m_delayRemovesObservers = new List<EventObserver>();
    static List<EventNotifier> m_delayRemovesNotifiers = new List<EventNotifier>();

    public static EventNotifier Get()
    {
        EventNotifier notifier = IdTypePool<EventNotifier>.Get();
        notifier.Init();
        m_notifiersById.Add(notifier.Id, notifier);
        return notifier;
    }

    //监听,onFire返回false表示否决(之后的监听者不执行)
    static int DoAdd(EventNotifier notifier, int msg, int code, object onFire)
    {
        if (notifier == null || !m_notifiersById.ContainsKey(notifier.Id))
        {
            Debuger.LogError(string.Format("观察对象找不到：{0}", msg + code));
            return Invalid_Id;
        }

        EventObserver observer = IdTypePool<EventObserver>.Get();
        observer.Init(notifier, msg, code, false, onFire);
        notifier.Add(observer);
        AllFireNotifier.Add(observer);
        m_observersById.Add(observer.Id, observer);
        return observer.Id;
    }
    public static int Add(EventNotifier notifier, int msg, int code, EventObserver.OnFire onFire) { return DoAdd(notifier, msg, code, onFire); }
    public static int Add(EventNotifier notifier, int msg, int code, EventObserver.OnFire1 onFire) { return DoAdd(notifier, msg, code, onFire); }

    public static int Add(EventNotifier notifier, int msg, int code, EventObserver.OnFire2 onFire) { return DoAdd(notifier, msg, code, onFire); }

    public static int Add(EventNotifier notifier, int msg, int code, EventObserver.OnFire3 onFire) { return DoAdd(notifier, msg, code, onFire); }

    public static int Add(EventNotifier notifier, int msg, int code, EventObserver.OnFireOb onFire) { return DoAdd(notifier, msg, code, onFire); }

    public static int AddVote(EventNotifier notifier, int msg, int code, EventObserver.OnVote onVote) { return DoAdd(notifier, msg, code, onVote); }
    static int DoAddAll(int msg, int code, object onFire)
    {
        EventObserver observer = IdTypePool<EventObserver>.Get();
        observer.Init(null, msg, code, true, onFire);
        AllNotifier.Add(observer);
        m_observersById.Add(observer.Id, observer);
        return observer.Id;
    }

    //监听全局,onFire返回false表示否决之后执行的监听者
    public static int AddAll(int msg, int code, EventObserver.OnFire onFire) { return DoAddAll(msg, code, onFire); }
    public static int AddAll(int msg, int code, EventObserver.OnFire1 onFire) { return DoAddAll(msg, code, onFire); }
    public static int AddAll(int msg, int code, EventObserver.OnFire2 onFire) { return DoAddAll(msg, code, onFire); }
    public static int AddAll(int msg, int code, EventObserver.OnFire3 onFire) { return DoAddAll(msg, code, onFire); }
    public static int AddAll(int msg, int code, EventObserver.OnFireOb onFire) { return DoAddAll(msg, code, onFire); }
    public static int AddVote(int msg, int code, EventObserver.OnVote onVote) { return DoAddAll(msg, code, onVote); }

    //取消监听
    public static void Remove(int observerId)
    {
        if(observerId == Invalid_Id)
            return;
        EventObserver ob=m_observersById.Get(observerId);
        if(ob == null)
            return;
        Remove(ob);
    }

    //取消监听
    public static void Remove(EventObserver observer)
    {
        if (observer.IsInPool)
        {
            Debuger.LogError(string.Format("重复删除监听者：{0} {1}", observer.Id, Util.GetDelegateName(observer.GetDelegate())));
            return;
        }
        if (observer.removeCounter > 0 && m_fireCount >0)
        {
        //    Debuger.LogError(string.Format("重复删除监听者：{0} {1}", observer.Id,  Util.GetDelegateName(observer.GetDelegate())));
            return;
        }
        observer.removeCounter = 1;
        if (m_fireCount > 0)
        {
            m_delayRemovesObservers.Add(observer);
            return;
        }
        

        if(!observer.isAll){
            observer.notifier.Remove(observer);
            AllFireNotifier.Remove(observer);
        }
        else
            AllNotifier.Remove(observer);
        if (!m_observersById.Remove(observer.Id))
            Debuger.LogError(string.Format("删除了不存在的监听者：{0}", observer.Id));

        IdTypePool<EventObserver>.Put(observer);
    }

    //删除一个发送者，这个时候连它的监听者也会被删除
    public static void Remove(EventNotifier notifier)
    {
        notifier.isRemove = true;
        if (m_fireCount >0)
        {
            m_delayRemovesNotifiers.Add(notifier);//fire中的话，等到fire后才删除
            return;
        }

        if (notifier.observersById.Count != 0)//如果不加这一行，会导致foreach里有GC Alloc
        {
            foreach (EventObserver observer in notifier.observersById.Values)
            {
                AllFireNotifier.Remove(observer);
                if (!m_observersById.Remove(observer.Id))
                    Debuger.LogError(string.Format("删除了不存在的监听者：{0}", observer.Id));
                IdTypePool<EventObserver>.Put(observer);
            }
        }
        if (!m_notifiersById.Remove(notifier.Id))
            Debuger.LogError(string.Format("删除了不存在的发送者：{0}", notifier.Id));

        
        IdTypePool<EventNotifier>.Put(notifier);
        
    }

    //广播事件,返回false表示有人否决
    public static bool Fire(EventNotifier notifier, int msg, int code, object param1 = null, object param2 = null, object param3 = null)
    {
        if (notifier !=null && notifier.isRemove)//已经删除的要再发送的时候报个错
        {
            Debuger.LogError(string.Format("广播者已经被删除但是仍然fire：{0} {1} {2}", notifier.Id, msg, code));
            return true;
        }
            
        ++m_fireCount;
        bool ret = true;
        try  
        {
            
            int key =msg*100000 + code;
            Dictionary<int, EventObserver> obs;
            if (AllNotifier.observersByMsgCode.TryGetValue(key, out obs))
            {
                if (obs.Count != 0)//如果不加这一行，会导致foreach里有GC Alloc
                {
                    foreach (EventObserver observer in obs.Values)
                    {
                        if (observer.removeCounter>0)
                        {
                            ++observer.removeCounter;
                            if (observer.removeCounter>10)
                                Debuger.LogError(string.Format("监听者者已经被删除但是仍然fire：{0} {1} {2} {3}", observer.Id, msg, code, Util.GetDelegateName(observer.GetDelegate())));
                            continue;
                        }
                            
                        observer.notifier = notifier;//这里全局监听的notifier是动态变化的
                        if (!observer.OnHandleFire(param1, param2, param3))
                        {
                            ret = false;
                            goto Label_Break;
                        }
                    }       
                }
            }

            if (notifier.observersByMsgCode.TryGetValue(key, out obs))
            {
                if (obs.Count != 0)//如果不加这一行，会导致foreach里有GC Alloc
                {
                    foreach (EventObserver observer in obs.Values)
                    {
                        if (observer.removeCounter > 0)
                        {
                            ++observer.removeCounter;
                            if (observer.removeCounter > 10)
                                Debuger.LogError(string.Format("监听者已经被删除但是仍然fire2：{0} {1} {2} {3}", observer.Id, msg, code, Util.GetDelegateName(observer.GetDelegate())));
                            continue;
                        }

                        if (!observer.OnHandleFire(param1, param2, param3))
                        {
                            ret = false;
                            goto Label_Break;
                        }
                    }
                        
                }
            }
            
        Label_Break: ;
        }
        //catch (System.Exception err)
        //{
        //    Debuger.LogError(err.StackTrace + ":" + err.Message);
        //}
        finally
        {
            --m_fireCount;
            if (m_fireCount <= 0 && m_delayRemovesObservers.Count != 0 )
            {
                for (int i = 0; i < m_delayRemovesObservers.Count; ++i)
                {
                    Remove(m_delayRemovesObservers[i]);
                }

                m_delayRemovesObservers.Clear();
            }
            if (m_fireCount <= 0 && m_delayRemovesNotifiers.Count != 0)
            {
                for (int i = 0; i < m_delayRemovesNotifiers.Count; ++i)
                {
                    Remove(m_delayRemovesNotifiers[i]);
                }
                m_delayRemovesNotifiers.Clear();
            }
        }

        return ret;
    }

    //广播全局事件，对同一个事件的所有监听者,返回false的话表示有人否决
    public static bool FireAll(int msg, int code, object param1 = null, object param2 = null, object param3 = null)
    {
        return Fire(AllFireNotifier, msg, code, param1, param2, param3);
    }
}