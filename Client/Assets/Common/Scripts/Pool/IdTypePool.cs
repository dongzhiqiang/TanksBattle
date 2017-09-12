using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/*
 * *********************************************************
 * 名称：Id类型对象池
 
 * 日期：2015.8.24
 * 描述：用于复用对象，防止频繁的new导致的碎片。
 *       这里的对象必须是继承自IdType
 * 
 * 这里的Get效率不如new 当然，实际操作一般不是耗时在new上。
 * 用EventMgr测试了下，大概多耗时3%。
 * 
 * *********************************************************
 * 注意，用对象池非常危险，如果一个对象的函数执行中有遍历过程，
 * 遍历到某一步回收了这个对象，那么每次遍历之后都要判断下是不是被回收了
 * 不然的话会造成很多奇怪的错误，比如不应该改变的状态被改变了，或者空指针等问题
 * 
 * 不用对象池的代码写法:
void Update(){
    for(int i=0;i<100;++i)
        SomeHandle();
}
 * 使用对象池的代码的写法： 
void Update(){
    int poolId = this.Id;
    for(int i=0;i<100;++i){
        SomeHandle();
        if(this.IsDestroy(poolId))
            return;
    }
}
 */

public abstract class IdType
{
    int _poolId;
    bool _isInPool;//是不是已经回收到对象池里了
    PoolBase _pool;
    //这个接口不是太安全，有可能出现放回对象池然后被别人取出的情况，尽量用IsDestroy()
    public bool IsInPool { get { return _isInPool; } set { _isInPool = value; } }//对象池
    public int Id { get { return _poolId; } set { _poolId = value; } }
    public PoolBase Pool { get { return _pool;}set{_pool = value;} }
    

    public virtual void OnInit() { }
    public virtual void OnClear() { }

    public void Put()
    {
        if (IsInPool)return;

        Pool.Put2(this);
    }

    //是不是已经被销毁
    public bool IsDestroy(int curId)
    {
        return curId!=_poolId || _isInPool;
    }
}

public class PoolBase: IPool
{
    const int Check_Leak_Count = 500;//泄露检测，超过这个数量就报错
    static int s_idCounter = 0;//FIX,这个值必须是全局值不然一些用到继承的类将维护不了唯一id

    Stack<IdType> m_pool = new Stack<IdType>();
    int m_counter = 0;

    //名字
    public string Name { get { return this.GetType().ToString(); } }

    //在池中的对象
    public int Count { get { return m_pool.Count; } }

    //总数量
    public int TotalCount { get { return m_counter; } }

    public PoolBase()
    {
        if(PoolMgr.instance != null)
            PoolMgr.instance.AddPool(this);
    }

    public T Get<T>() where T : IdType, new()
    {
#if UNITY_EDITOR
        if (PoolMgr.instance!= null &&!string.IsNullOrEmpty(PoolMgr.instance.m_debugIdTypePool) && typeof(T).Name.Contains(PoolMgr.instance.m_debugIdTypePool))
        {
            Debuger.LogError("从对象池申请:{0}", typeof(T).Name);
        }
#endif

        //从池中获取对象，没有就new一个
        T it;
        if (m_pool.Count == 0)
        {
            it = new T();
            it.Pool = this;
            ++m_counter;
            if (m_counter > Check_Leak_Count)//检查泄露
                Debuger.LogError(string.Format("类型对象池({0})分配的{1}过多，请检查是不是没有put",typeof(T).ToString(), m_counter));
        }
        else
            it = (T)m_pool.Pop();

      
        it.Id = ++s_idCounter;
        it.IsInPool = false;
        it.OnInit();
        return it;
    }

    public void Put2(IdType it)
    {
#if UNITY_EDITOR
        if (PoolMgr.instance != null && !string.IsNullOrEmpty(PoolMgr.instance.m_debugIdTypePool) && it.GetType().Name.Contains(PoolMgr.instance.m_debugIdTypePool))
        {
            Debuger.LogError("放回对象池:{0}", it.GetType().Name);
        }
#endif
        if (it.IsInPool == true)
        {
            Debuger.LogError("类型对象池,重复放入对象：" + it.GetType().ToString());
            return;
        }

        it.OnClear();
        it.IsInPool = true;
        m_pool.Push(it);
    }

    public void Clear()
    {
        m_pool.Clear();
    }
}


public class IdTypePool<T> :PoolBase
    where T : IdType, new()
{
    static IdTypePool<T> s_instance = null;
    static IdTypePool<T> Instance
    {
        get{
            if(s_instance == null)
                s_instance = new IdTypePool<T>();
            return s_instance;
        }
    }
    public static T Get()
    {

        return Instance.Get<T>();
    }

    public  static void Put(T t){

        Instance.Put2(t);
    }
    
}

//线程安全的对象池
public class SynchronizedIdTypePool<T> : PoolBase
    where T : IdType, new()
{
    static IdTypePool<T> s_instance = null;
    static object s_lockObject = new object();
    static IdTypePool<T> Instance
    {
        get
        {
            lock(s_lockObject)
            {
                if (s_instance == null)
                    s_instance = new IdTypePool<T>();
            }
            
            return s_instance;
        }
    }
    public static T Get()
    {
        T t;
        lock(s_lockObject)
        {
            t = Instance.Get<T>();
        }
        return t;
    }

    public static void Put(T t)
    {
        lock (s_lockObject)
        {
            Instance.Put2(t);
        }
    }

}
