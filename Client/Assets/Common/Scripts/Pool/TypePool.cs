/*
 * *********************************************************
 * 名称：类型对象池
 
 * 日期：2016.5.17
 * 描述：注意主要用于list、dict等类型，自定义的类型建议还是继承自IdTypePool，更安全些
 */

using System;
using System.Collections.Generic;

public abstract class TypePool : IPool
{
    //名字
    public string Name { get { return this.GetType().ToString(); } }

    //在池中的对象
    public abstract int Count { get; }

    //总数量
    public abstract int TotalCount { get; }

    public TypePool()
    {
        if(PoolMgr.instance!=null)
            PoolMgr.instance.AddPool(this);
    }
    public abstract void Clear();
}

public class TypePool<T> : TypePool
    where T :  new()
{
    static TypePool<T> s_instance = null;
    static TypePool<T> Instance
    {
        get
        {
            if (s_instance == null)
                s_instance = new TypePool<T>();
            return s_instance;
        }
    }


    // Object pool to avoid allocations.
    const int Check_Leak_Count = 2000;//泄露检测，超过这个数量就报错
    Stack<T> m_ListPool = new Stack<T>();
    int m_idCounter =0;

    //在池中的对象
    public override int Count { get { return m_ListPool.Count; } }
    //总数量
    public override int TotalCount { get { return m_idCounter; } }
    T Get2()
    {
        T element;
        if (m_ListPool.Count == 0)
        {
            element = new T();
            ++m_idCounter;
            if (m_idCounter > Check_Leak_Count)//检查泄露
                Debuger.LogError(string.Format("list对象池({0})分配的{1}过多，请检查是不是没有put", typeof(T).ToString(), m_idCounter));
        }
        else
            element = m_ListPool.Pop();
        return element;
    }

    //注意放回来之前要自己清空下
    void Put2(T element)
    {
        m_ListPool.Push(element);
    }
    public override void Clear()
    {
        m_ListPool.Clear();
    }

    public static T Get()
    {
        return Instance.Get2();
    }

    public static void Put(T element)
    {
        Instance.Put2(element);
    }

}