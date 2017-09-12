#region Header
/**
 * 名称：网络管理
 
 * 日期：2015.9.16
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NetCore;
using System.Reflection;
using System.Net.Sockets;

public class NetMgr : Singleton<NetMgr>
{
   

    #region Fields
    MessageHandle m_msgHandle = new MessageHandle();
    public Connector m_connector = null;
    public Dictionary<int, ModuleDefinition> m_modules = new Dictionary<int, ModuleDefinition>();

    public AccountHandler AccountHandler;
    public GMHandler GMHandler;
    public ItemHandler ItemHandler;
    public EquipHandler EquipHandler;
    public RoleHandler RoleHandler;
    public PetHandler PetHandler;
    public LevelHandler LevelHandler;
    public ActivityHandler ActivityHandler;
    public RankHandler RankHandler;
    public WeaponHandler WeaponHandler;
    public MailHandler MailHandler;  //邮件
    public SystemHandler SystemHandler;
    public OpActivityHandler OpActivityHandler;
    public SocialHandler SocialHandler;
    public TaskHandler TaskHandler;
    public FlameHandler FlameHandler;
    public CorpsHandler CorpsHandler;
    public ChatHandler ChatHandler;
    public ShopHandler ShopHandler;
    public EliteLevelHandler EliteLevelHandler;
    public TreasureHandler TreasureHandler;

    public Dictionary<string, DateTime> m_waitResponse = new Dictionary<string, DateTime>(); //等候回复的消息，键：模块_命令，值：发送时间
    public List<string> m_toDeleteWaitItems = new List<string>();   //用于m_waitResponse的延迟删除
    public TimeMgr.Timer m_waitTimer = null;
    #endregion


    #region Properties
    public Connector Connector { get{ return m_connector; }}
    public enConnectorState State { get { return Connector.State; } }
    public bool NeedAutoRelogin { get { return Connector.NeedAutoRelogin; } set { Connector.NeedAutoRelogin = value; } }
    #endregion


    #region Constructors
    #endregion

    #region Private Methods
    CommandDefinition RegisterCommandDef(int command, ModuleDefinition modDef, MethodInfo info, bool hasErrorCode, bool hasErrorMsg)
    {
        CommandDefinition cmdDef;
        cmdDef = modDef.cmdDefs.Get(command);
        if (cmdDef != null)
        {
            Debuger.LogError("不能重复注册协议:{0} 模块:{1} 原有的方法:{2} 重复的方法:{3}", command, modDef.module, cmdDef.methodInfo.Name, info.Name);
            return null;
        }

        cmdDef = new CommandDefinition();
        cmdDef.parent = modDef;
        cmdDef.command = command;
        cmdDef.methodInfo = info;
        cmdDef.hasErrorCode = hasErrorCode;
        cmdDef.hasErrorMsg = hasErrorMsg;

        //找出参数类型
        ParameterInfo[] pi =info.GetParameters();
        if (hasErrorCode)
        {
            if (hasErrorMsg)
            {
                if (pi != null && (pi.Length == 2 || pi.Length == 3) && pi[0].ParameterType == typeof(int) && pi[1].ParameterType == typeof(string))
                {
                    cmdDef.bodyType = pi.Length >= 3 ? pi[2].ParameterType : null;
                }
                else
                {
                    Debuger.LogError("消息处理方法参数不对，模块:{0}，命令：{1}，方法名：{2}", modDef.module, command, info.Name);
                    return null;
                }
            }
            else
            {
                if (pi != null && (pi.Length == 1 || pi.Length == 2) && pi[0].ParameterType == typeof(int))
                {
                    cmdDef.bodyType = pi.Length >= 2 ? pi[1].ParameterType : null;
                }
                else
                {
                    Debuger.LogError("消息处理方法参数不对，模块:{0}，命令：{1}，方法名：{2}", modDef.module, command, info.Name);
                    return null;
                }
            }
        }
        else
        {
            if (pi == null || pi.Length <= 0)
            {
                cmdDef.bodyType = null;
                
            }
            else if (pi.Length == 1)
            {
                cmdDef.bodyType = pi[0].ParameterType;
            }
            else
            {
                Debuger.LogError("网络消息处理方法参数个数不能超过1个，模块:{0}，命令：{1}，方法名：{2}", modDef.module, command, info.Name);
                return null;
            }
        }

        //如果是需要序列化的类，那么要向序列化编解码器注册下
        if (ProtocolCoder.CanRegister(cmdDef.bodyType))
            ProtocolCoder.instance.Register(cmdDef.bodyType);

        modDef.cmdDefs.Add(command, cmdDef);
        return cmdDef;
    }

    ModuleDefinition RegisterModuleDef(byte module, object facade)
    {
        ModuleDefinition modDef;
        modDef = m_modules.Get(module);
        if (modDef != null)
        {
            Debuger.LogError("不能重复注册模块:{0} 原有的:{1} 重复的:{2}", module, modDef.type.Name, facade.GetType().Name);
            return null;
        }

        modDef = new ModuleDefinition();
        modDef.module = module;
        modDef.facade = facade;
        modDef.type = facade.GetType();
        m_modules.Add(module, modDef);

        //找到特性，注册协议处理函数
        MethodInfo[] methodInfos = modDef.type.GetMethods(BindingFlags.NonPublic|BindingFlags.Public | BindingFlags.Instance);
        object[] attrs;
        NetHandlerAttribute attr;
        foreach (MethodInfo info in methodInfos)
        {
            attrs = info.GetCustomAttributes(typeof(NetHandlerAttribute), false);
            if (attrs == null || attrs.Length == 0)
                continue;
            attr = (NetHandlerAttribute)attrs[0];
            RegisterCommandDef(attr.command, modDef, info, attr.hasErrorCode, attr.hasErrorMsg);
        }
        return modDef;
    }

    void OnCheckResponse()
    {
        if (m_waitResponse.Count <= 0)
        {
            if (UIMgr.instance.Get<UIWaitResponse>().IsOpen)
                UIMgr.instance.Close<UIWaitResponse>();
            return;
        }

        DateTime curTime = TimeMgr.instance.GetTrueDateTime();

        foreach (var item in  m_waitResponse)
        {
            var passTime = (curTime - item.Value).TotalMilliseconds;
            //必须写在前面
            if (passTime > 30000)
            {
                m_toDeleteWaitItems.Add(item.Key);
                UIMessageBox.Open(LanguageCfg.Get("net_no_request_long_time"), () => { });
            }
            else if (passTime > 500)
            {
                if (!UIMgr.instance.Get<UIWaitResponse>().IsOpen)
                    UIMgr.instance.Open<UIWaitResponse>();
            }
        }

        if (m_toDeleteWaitItems.Count > 0)
        {
            for (var i = 0; i < m_toDeleteWaitItems.Count; ++i)
                m_waitResponse.Remove(m_toDeleteWaitItems[i]);
            m_toDeleteWaitItems.Clear();
        }
    }
    #endregion

    public void Init(){
        m_connector = new Connector();
        m_connector.setHandler(m_msgHandle);
        m_msgHandle.SetCommandRegister(GetCommandDefinition);
        m_msgHandle.SetFilter(OnMessageFilter);
        
        //每个模块的门面类
        AccountHandler = new AccountHandler();
        GMHandler = new GMHandler();
        ItemHandler = new ItemHandler();
        EquipHandler = new EquipHandler();
        RoleHandler = new RoleHandler();
        PetHandler = new PetHandler();
        LevelHandler = new LevelHandler();
        ActivityHandler = new ActivityHandler();
        RankHandler = new RankHandler();
        WeaponHandler = new WeaponHandler();
        MailHandler = new MailHandler();
        SystemHandler = new SystemHandler();
        OpActivityHandler = new OpActivityHandler();
        SocialHandler = new SocialHandler();
        TaskHandler = new TaskHandler();
        FlameHandler = new FlameHandler();
        CorpsHandler = new CorpsHandler();
        ChatHandler = new ChatHandler();
        ShopHandler = new ShopHandler();
        EliteLevelHandler = new EliteLevelHandler();
        TreasureHandler = new TreasureHandler();

        //添加定时器
        m_waitTimer = TimeMgr.instance.AddTimer(0.1f, OnCheckResponse, -1, -1);

        //找到特性，注册模块
        TimeCheck check = new TimeCheck();
        ProtocolCoder.instance.Indent = 0;
        ProtocolCoder.instance.Log.Remove(0, ProtocolCoder.instance.Log.Length);
        ProtocolCoder.instance.Log.Append("NetMgr 解析需要序列化的类\n");

        Type t = this.GetType();
        FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
        object[] attrs;
        foreach (FieldInfo f in fields)
        {
            attrs = f.FieldType.GetCustomAttributes(typeof(NetModuleAttribute), false);
            if (attrs == null || attrs.Length == 0)
                continue;

            RegisterModuleDef(((NetModuleAttribute)attrs[0]).module, f.GetValue(this));
        }

        ProtocolCoder.instance.Log.AppendFormat("总耗时:{0}", check.delayMS);
        //Debuger.Log(ProtocolCoder.instance.Log.ToString());
    }

    public void Connect(string ip,int port)
    {
        if (!m_connector.isDisposed())
        {
            Debuger.Log("正在连接中，或者已经连接上了。必须断开才能重新连接");
            return;
        }
        m_connector.connect(ip, port);
    }

    /// <summary>
    /// 仅给关闭程序时用
    /// </summary>
    public void Dispose()
    {
        if (m_waitTimer != null)
        {
            m_waitTimer.Release();
            m_waitTimer = null;
        }

        Close();
    }

    public void Close()
    {
        //精除重发列表
        ClearPendingMsgList();
        //主动关闭的不要自动重连
        NeedAutoRelogin = false;
        //清除等待消息列表
        m_waitResponse.Clear();
        //关闭等待窗口
        if (UIMgr.instance.Get<UIWaitResponse>().IsOpen)
            UIMgr.instance.Close<UIWaitResponse>();
        //关闭网络
        if (!m_connector.isDisposed())
            m_connector.dispose();
    }

    public CommandDefinition GetCommandDefinition(int module, int command)
    {
        ModuleDefinition modDef = m_modules.Get(module);
        if (modDef == null)
        {
            Debuger.LogError("找不到模块面门 模块:{0}  协议:{1}", module, command);
            return null;
        }

        CommandDefinition cmdDef = modDef.cmdDefs.Get(command);
        if (cmdDef == null)
        {
            Debuger.LogError("{0}找不到服务端发来的协议的处理.协议:{1}", modDef.type.Name, command);
            return null;
        }

        return cmdDef;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="module"></param>
    /// <param name="command"></param>
    /// <param name="obj"></param>
    /// <param name="noSendIfNoResponse">如果没有返回结果，不能再发起这个请求</param>
    /// <param name="resendIfFail">如果网络失败，这个要不要重发</param>
    public void Send(byte module, int command, object obj, bool waitResponse = true)
    {
        string key = module + "_" + command;
        if (waitResponse && m_waitResponse.ContainsKey(key))
            return;
        if (m_connector.Send(module, command, obj))
        {
            if (waitResponse)
                m_waitResponse[key] = TimeMgr.instance.GetTrueDateTime();
        }
    }

    /// <summary>
    /// 主要用于测试，不要直接用
    /// </summary>
    /// <param name="module"></param>
    /// <param name="command"></param>
    /// <param name="jsonStr"></param>
    /// <param name="waitResponse"></param>
    public void SendJsonString(byte module, int command, string jsonStr, bool waitResponse = true)
    {
        string key = module + "_" + command;
        if (waitResponse && m_waitResponse.ContainsKey(key))
            return;
        if (m_connector.SendJsonString(module, command, jsonStr))
        {
            if (waitResponse)
                m_waitResponse[key] = TimeMgr.instance.GetTrueDateTime();
        }
    }

    public bool OnMessageFilter(int module, int command)
    {
        string key = module + "_" + command;
        m_waitResponse.Remove(key);
        return PlayerStateMachine.Instance.NetMsgFilter(module, command);
    }

    public void SetOnConnectOK(System.Action callback)
    {
        m_msgHandle.SetOnConnectOK(callback);
    }

    public void SetOnConnectFail(System.Action callback)
    {
        m_msgHandle.SetOnConnectFail(callback);
    }

    public void SetOnConnError(System.Action<SocketError, string> callback)
    {
        m_msgHandle.SetOnConnError(callback);
    }

    public void SendPendingMsgList()
    {
        if (m_connector != null)
            m_connector.SendPendingMsgList();
    }

    public void ClearPendingMsgList()
    {
        if (m_connector != null)
            m_connector.ClearPendingMsgList();
    }

    public void ClearWaitResponse()
    {
        //清除等待消息列表
        m_waitResponse.Clear();
        //关闭等待窗口
        if (UIMgr.instance.Get<UIWaitResponse>().IsOpen)
            UIMgr.instance.Close<UIWaitResponse>();
    }
}