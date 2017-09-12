#region Header
/**
 * 名称：对象池管理器
 
 * 日期：2016.6.6
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public interface IPool
{
    //名字
    string Name { get; }

    //在池中的对象
    int Count { get; }

    //总数量
    int TotalCount { get; }

    //清空到剩下一个
    void Clear();
}

public class PoolMgr : SingletonMonoBehaviour<PoolMgr>
{
    const float COLLECT_TIME = 3 * 60;

    public string m_debugIdTypePool = "";

    SortedDictionary<string, IPool> m_pools = new SortedDictionary<string, IPool>();
    float m_lastGCCollect;


    public SortedDictionary<string, IPool> Pools { get { return m_pools; } }

    public void AddPool(IPool pool)
    {
        if (m_pools.ContainsKey(pool.Name))
        {
            Debuger.LogError("重复注册对象池:{0}", pool.Name);
            return;
        }
        m_pools.Add(pool.Name, pool);
    }

    public void Clear()
    {
        //foreach (var p in m_pools.Values)
        //{
        //    p.Clear();
        //}
        GameObjectPool.GetPool(GameObjectPool.enPool.Fx).Clear();
        GameObjectPool.GetPool(GameObjectPool.enPool.Role).Clear();
        GameObjectPool.GetPool(GameObjectPool.enPool.Other).Clear();
        GCCollect();
        
    }


    public void GCCollect(bool immidiaty =true)
    {
        if(!immidiaty&& Time.realtimeSinceStartup - m_lastGCCollect< COLLECT_TIME)
        {
            return;
        }

        System.GC.Collect();//垃圾回收下
        m_lastGCCollect = Time.realtimeSinceStartup;
    }

}