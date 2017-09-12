using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;


public class LogUtil{
    private const int MAX_LOG_COUNT= 10;//超过这里数量就会删掉一半先
    private static string PathName;
    private static string FileName;
    private static string FullPath;

    private static Thread LogTask;
    private static  Queue<string> LogColQueue;//自定义线程安全的Queue
    private static  object LockObj=new object();
    private static bool m_isInit;
    static AutoResetEvent LogEvent = new AutoResetEvent(false);//如果有新的日志写入，那么日志线程则执行，否则挂起
    static volatile bool m_isNeedClose=false;

    #region Private Methods
    static void ThreadFun()
    {
        string[] files = Directory.GetFiles(PathName, "*.log", SearchOption.AllDirectories);
        //Debuger.Log("{0}", string.Join("\n", files));
        //如果日志文件超过10个，删除掉老的
        if (files.Length > MAX_LOG_COUNT)
        {
            Array.Sort(files);//时间升序
            int removeCount = files.Length - MAX_LOG_COUNT / 2;//剩下最大数量的一半
            for (int i = 0; i < removeCount; ++i)
            {
                File.Delete(files[i]);
            }
            Log(string.Format("删除了{0}个老日志", removeCount));
        }
        files = null;

        //循环写日志
        string _msg = null;
        while (!m_isNeedClose)
        {
            LogEvent.WaitOne();//有新的日志才写到本地
            while (LogColQueue.Count > 0)
            {
                lock (LockObj)
                {
                    _msg = LogColQueue.Dequeue();
                }
                using (StreamWriter sw = new StreamWriter(FullPath, true, Encoding.UTF8))
                {
                    sw.Write(_msg);
                }
            }
        }

        //用于判断非正常关闭的情况
        using (StreamWriter sw = new StreamWriter(FullPath, true, Encoding.UTF8))
            sw.Write("日志关闭");
    }

    static void LogCallback(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Warning)
            return;
        if (type == LogType.Log)//log就不打印堆栈了
            Log(condition, type);
        else
            Log(condition + '\n' + stackTrace, type);
    }
    #endregion


    public static void Init()
    {
        if (m_isInit)
        {
            Debuger.LogError("本地日志重复初始化");
            return;
        }
        m_isNeedClose = false;
        m_isInit = true;
        
        //一些用到unity的函数最好在这里初始化
#if UNITY_EDITOR
        PathName = Application.dataPath.Substring(0, Application.dataPath.Length - 7) + "/Log";//E:/MySvn/GOW20150819/Client/Assets

#elif UNITY_ANDROID
        //尽量先放sd卡目录下，方便获取
        if (Directory.Exists("/sdcard"))
        {
            try{
                string appSdPath = "/sdcard/"+Application.bundleIdentifier;
                PathName = appSdPath+"/Log";
                if (!Directory.Exists(appSdPath))
                    Directory.CreateDirectory(appSdPath);
            }
            catch{
                PathName = Application.temporaryCachePath+"/Log";///data/data/xxx.xxx.xxx/cache
            }
        }
        else
            PathName = Application.temporaryCachePath+"/Log";///data/data/xxx.xxx.xxx/cache
#else
        PathName = Application.temporaryCachePath+"/Log";///data/data/xxx.xxx.xxx/cache
#endif
        
        if (!Directory.Exists(PathName))
            Directory.CreateDirectory(PathName);

        DateTime now = DateTime.Now;
        FileName = string.Format("{0}-{1:D2}-{2:D2} {3:D2}-{4:D2}.log", now.Year,now.Month,now.Day,now.Hour,now.Minute);
        FullPath = PathName + "/"+ FileName;

        LogColQueue = new Queue<string>();
        Application.logMessageReceivedThreaded += LogCallback;

        //启动线程
        LogTask =Util.SafeCreateThread(ThreadFun);
        LogTask.Start();

        Log("日志系统启动");
    }

    public static void Close()
    {
        if (m_isInit)
        {
            Application.logMessageReceivedThreaded -= LogCallback;
            m_isInit = false;
        }

        if (LogTask!=null)
        {
            m_isNeedClose = true;
            LogEvent.Set();//让休眠的日志线程判断是不是要退出
            LogTask.Join();
            LogTask = null;
        }
    }

    /// <summary>
    /// 记录日志
    /// </summary>
    /// <param name="msg">日志内容</param>
    public static void Log(string msg, LogType type = LogType.Log)
    {
        if (!m_isInit)
            return;
        lock (LockObj)
        {
            LogColQueue.Enqueue(string.Format("{0} [{1}]: {2}\n", DateTime.Now.ToString("HH:mm:ss"), type, msg));
        }
        LogEvent.Set();//让休眠的日志线程写日志
    }

}

