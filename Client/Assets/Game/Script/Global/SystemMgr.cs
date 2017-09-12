#region Header
/**
 * 名称：系统管理
 
 * 日期：2015.9.16
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
/*
//代理，用于设置的时候广播
public class SystemProxy
{
    Action<string,int,bool> onChange3;
    Action<bool> onChange1;
    Action onChange;

    public readonly string SysType;
    public readonly int Sys;
    
    public bool Active{get;set;}
    public bool Bind { get; set; }//用于检查有没有绑定过子系统，绑定只能执行一次

    public SystemProxy(string type ,int sys,bool active)
    {
        SysType = type;
        Sys =sys;
        Active = active;
        Bind = false;
    }

    public void Add(Action<string,int,bool> a){
        if (onChange3 == null){
            onChange3 = a;
            return;
        }

        if(onChange3.GetInvocationList().Length>10)
            Debuger.LogError("监听者太多，是不是重复监听了");  
        onChange3+=a;
    }

    public void Add(Action<bool> a)
    {
        if (onChange1 == null)
        {
            onChange1 = a;
            return;
        }

        if (onChange3.GetInvocationList().Length > 10)
            Debuger.LogError("监听者太多，是不是重复监听了");
        onChange1 += a;
    }

    public void Add(Action a)
    {
        if (onChange == null)
        {
            onChange = a;
            return;
        }

        if (onChange3.GetInvocationList().Length > 10)
            Debuger.LogError("监听者太多，是不是重复监听了");
        onChange += a;
    }

    public void Fire(){
        if(onChange3 != null)
            onChange3(SysType, Sys, Active);
        if (onChange1 != null)
            onChange1(Active);
        if (onChange != null)
            onChange();
    }
}

public class SystemMgr
{
    #region Fields
    static Dictionary<string,Dictionary<int,SystemProxy>> s_typeProxys = new Dictionary<string,Dictionary<int,SystemProxy>>();
    #endregion


    #region Properties
    
    #endregion


    #region Constructors
    static SystemMgr (){
        Dictionary<int,SystemProxy> proxys = new Dictionary<int,SystemProxy>();
        string sysType = typeof(enSysIcon).ToString();
        s_typeProxys[sysType] = proxys;
        for (int i = (int)(enSysIcon.min) + 1; i < (int)(enSysIcon.max); ++i)
            proxys[i] = new SystemProxy(sysType,i, true);

        proxys = new Dictionary<int, SystemProxy>();
        sysType = typeof(enSysFlag).ToString();
        s_typeProxys[sysType] = proxys;
        for (int i = (int)(enSysFlag.min) + 1; i < (int)(enSysFlag.max); ++i)
            proxys[i] = new SystemProxy(sysType,i, false);

        proxys = new Dictionary<int, SystemProxy>();
        sysType = typeof(enSysOpen).ToString();
        s_typeProxys[sysType] = proxys;
        for (int i = (int)(enSysOpen.min) + 1; i < (int)(enSysOpen.max); ++i)
            proxys[i] = new SystemProxy(sysType,i, false);
    }

    
    #endregion


    #region Static Methods
    public static SystemProxy Get(string type, int sys)
    {
        Dictionary<int, SystemProxy> proxys = s_typeProxys.Get(type);
        if (proxys == null)
        {
            Debuger.LogError("不包含这个类型的系统枚举:" + type);
            return null;
        }

        SystemProxy proxy = proxys.Get(sys);
        if (proxys == null)
        {
            Debuger.LogError("{0}没有这个枚举值:{1}", type, sys);
            return null;
        }
        return proxy;
    }
    static bool IsActive(string type, int sys)
    {
        SystemProxy proxy = Get(type, sys);
        return proxy == null?false:proxy.Active;
    }

    static void SetActive(string type, int sys, bool active)
    {
        SystemProxy proxy = Get(type, sys);
        if (proxy == null) return;
        if (proxy.Bind)
        {
            Debuger.LogError("{0}的:{1} 被绑定的系统不能SetActive");
            return;
        }
        if (proxy.Active == active) return;//一样就不用刷新了
        proxy.Active = active;
        proxy.Fire();
    }

    //监听
    static void Add(string type, int sys, Action<string, int, bool> a)
    {
        SystemProxy proxy = Get(type, sys);
        if (proxy == null) return;
        proxy.Add(a);
    }
    static void Add(string type, int sys, Action<bool> a)
    {
        SystemProxy proxy = Get(type, sys);
        if (proxy == null) return;
        proxy.Add(a);
    }
    static void Add(string type, int sys, Action a)
    {
        SystemProxy proxy = Get(type, sys);
        if (proxy == null) return;
        proxy.Add(a);
    }

    //绑定
    static void Bind(string type, int sys, params int[] subSyss)
    {
        if(subSyss == null || subSyss.Length == 0)
        {
            Debuger.LogError("{0}的:{1} 要绑定的子系统一个也没有", type, sys);  

        }
        SystemProxy proxy = Get(type, sys);
        if (proxy == null) return;
        if (proxy.Bind)
        {
            Debuger.LogError("{0}的:{1} 被绑定了两次，只能绑定一次");
            return;
        }
        proxy.Bind=true;

        //绑定的委托
        Action a = ()=>{
            for (int i = 0; i < subSyss.Length; ++i)
            {
                SystemProxy subProxy = Get(type, subSyss[i]);    
                if(subProxy ==null)continue;
                if (subProxy.Active)
                {
                    if (!proxy.Active)
                    {
                        proxy.Active = true;
                        proxy.Fire();
                    }
                        
                    return;
                }
            }
            if (proxy.Active)
            {
                proxy.Active = false;
                proxy.Fire();
            }
                
        };

        //添加监听
        for (int i = 0; i < subSyss.Length; ++i)
        {
            SystemProxy subProxy = Get(type, subSyss[i]);
            if (subProxy == null) {
                Debuger.LogError("{0}的:{1} 没有子系统", type, sys,subSyss[i]);
                continue;
            }
            subProxy.Add(a);
        }

        //先设置一次系统
        a();
    }

    
    public static bool IsActive(enSysIcon sys){return IsActive(sys.GetType().ToString(), (int)sys);}

    public static bool IsActive(enSysFlag sys){return IsActive(sys.GetType().ToString(), (int)sys);}

    public static bool IsActive(enSysOpen sys){return IsActive(sys.GetType().ToString(), (int)sys);}

    
    public static void SetActive(enSysIcon sys, bool active) { SetActive(sys.GetType().ToString(), (int)sys,active); }
    public static void SetActive(enSysFlag sys, bool active) { SetActive(sys.GetType().ToString(), (int)sys, active); }
    public static void SetActive(enSysOpen sys, bool active) { SetActive(sys.GetType().ToString(), (int)sys, active); }
    
    //监听
    public static void Add(enSysIcon sys, Action<string, int, bool> a) { Add(sys.GetType().ToString(), (int)sys, a); }
    public static void Add(enSysIcon sys, Action<bool> a) { Add(sys.GetType().ToString(), (int)sys, a); }
    public static void Add(enSysIcon sys, Action a) { Add(sys.GetType().ToString(), (int)sys, a); }

    public static void Add(enSysFlag sys, Action<string, int, bool> a) { Add(sys.GetType().ToString(), (int)sys, a); }
    public static void Add(enSysFlag sys, Action<bool> a) { Add(sys.GetType().ToString(), (int)sys, a); }
    public static void Add(enSysFlag sys, Action a) { Add(sys.GetType().ToString(), (int)sys, a); }

    public static void Add(enSysOpen sys, Action<string, int, bool> a) { Add(sys.GetType().ToString(), (int)sys, a); }
    public static void Add(enSysOpen sys, Action<bool> a) { Add(sys.GetType().ToString(), (int)sys, a); }
    public static void Add(enSysOpen sys, Action a) { Add(sys.GetType().ToString(), (int)sys, a); }

    //将一个系统和其他系统绑定
    public static void Bind(enSysIcon sys, params enSysIcon[] subSyss) { Bind(sys.GetType().ToString(), (int)sys, System.Array.ConvertAll<enSysIcon, int>(subSyss, subsys=>(int)subsys)); }
    public static void Bind(enSysFlag sys, params enSysFlag[] subSyss) { Bind(sys.GetType().ToString(), (int)sys, System.Array.ConvertAll<enSysFlag, int>(subSyss, subsys => (int)subsys)); }
    public static void Bind(enSysOpen sys, params enSysOpen[] subSyss) { Bind(sys.GetType().ToString(), (int)sys, System.Array.ConvertAll<enSysOpen, int>(subSyss, subsys => (int)subsys)); }

    #endregion


    #region Private Methods
    
    #endregion

    



}

*/