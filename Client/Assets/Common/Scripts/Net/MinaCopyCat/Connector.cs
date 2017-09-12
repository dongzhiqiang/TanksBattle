#region Header
/**
 * 名称：连接器
 
 * 日期：2015.11.3
 * 描述：
 *      逻辑流程:connect()->begin()->运行中->dispose(),如果不是这个流程，那么状态逻辑会报错
 *      负责和服务端链接和断开连接
 *      发动消息给服务端
 *      setHandler用于设置供上层监听的接口
 *      setFilter用于设置编解码器，否则上层监听到的是字节数组
 * **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
namespace NetCore
{
    public enum enConnectorState
    {
        NotConnect, // 未连接状态，初始状态
        Connecting, // 连接中
        Connected,  // 连接成功
        //Closing   // 关闭中，主要是主动关闭
    }

    public partial class Connector : IoConnector
    {
        #region Fields
        const int ONCE_SIZE = 2048; // 最大发包大小,2K

        TimeMgr.Timer m_timeoutTimers;//用于超时通知的定时器
        List<TimeMgr.Timer> m_errorTimers = new List<TimeMgr.Timer>() ;//用于异常的定时器
        TimeMgr.Timer m_recvTimers;//用于检测收包的定时器
        object m_lock = new object(); // 线程同步对象
        Socket m_socket;
        enConnectorState m_state = enConnectorState.NotConnect;
        IoHandler m_handle;

        bool m_isIOThreadActive= false;
        bool m_needAutoRelogin = false; //是否要自动重连
        Thread m_receiveThread;
        Thread m_sendThread;
        List<Message> m_recvMessageList = new List<Message>();  // 接收消息队列
        List<Message> m_procTempMsgList = new List<Message>();  // 处理消息时的临时队列
        List<Message> m_sendMessageList = new List<Message>();  // 发送消息队列
        List<Message> m_sendTempMsgList = new List<Message>();  // 发送消息时的临时队列
        List<Message> m_pendingMsgList  = new List<Message>();  // 重连成功后待重发的数据
        #endregion


        #region Properties
        public enConnectorState State { get{return m_state;}}
        public bool NeedAutoRelogin 
        { 
            get 
            { 
                return m_needAutoRelogin; 
            } 
            set 
            { 
                m_needAutoRelogin = value;
                //不用自动重连就清待重发列表
                if (!m_needAutoRelogin)
                {
                    ClearPendingMsgList();
                }
            } 
        }        
        #endregion


        #region Constructors
        public Connector()
        {
    
        }
        #endregion

        #region IoConnector IoService
        /**
        * Connects to the specified remote address.
        *
        * @return the {@link ConnectFuture} instance which is completed when the
        *         connection attempt initiated by this call succeeds or fails.
        */
        public void connect(string address, int port)
        {
            if (m_state != enConnectorState.NotConnect)
            {
                Debuger.Log("状态异常.当前状态:{0}", m_state);
                return;
            }
            try
            {
                m_state = enConnectorState.Connecting;

                ResetEncode();
                ResetDecode();

                m_timeoutTimers = TimeMgr.instance.AddTimer(5f, OnTimeOut);

                m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IAsyncResult result = m_socket.BeginConnect(address, port, OnConnect, m_socket);
                Debuger.Log("连接服务器:{0}:{1}", address, port);
            }
            catch (SocketException e)
            {
                dispose();
                Debuger.LogError(e.Message);
                if (m_handle != null)
                    m_handle.OnConnectFail();
                
            }
            catch (Exception e)
            {
                dispose();
                Debuger.LogError(e.Message);
                if (m_handle != null)
                    m_handle.OnConnectFail();
            }
        }

        /**
         * Returns <tt>true</tt> if and if only all resources of this processor
         * have been disposed.
         */
        public bool isDisposed()
        {
            return m_state == enConnectorState.NotConnect;
        }

        /**
         * Releases any resources allocated by this service.  Please note that
         * this method might block as long as there are any sessions managed by
         * this service.
         */
        public void dispose()
        {
            if (m_state != enConnectorState.Connecting && m_state != enConnectorState.Connected)
            {
                //Debuger.Log("状态异常.当前状态:{0}", m_state);
                return;
            }
            lock (m_lock)
            {
                m_state = enConnectorState.NotConnect;
                if (m_timeoutTimers != null)
                {
                    m_timeoutTimers.Release();
                    m_timeoutTimers = null;
                }

                if (m_recvTimers != null)
                {
                    m_recvTimers.Release();
                    m_recvTimers = null;
                }

                if (m_errorTimers.Count != 0)
                {
                    foreach (TimeMgr.Timer t in m_errorTimers)
                    {
                        t.Release();
                    }
                    m_errorTimers.Clear();
                }

                //关闭io线程
                EndIOThread();

                if (m_socket != null)
                {
                    try
                    {
                        if (m_socket.Connected)
                            m_socket.Shutdown(SocketShutdown.Both);

                        m_socket.Close();
                    }
                    catch (System.Exception e)
                    {
                        Debuger.LogError(e.Message);
                    }
                    m_socket = null;
                    Debuger.Log("与服务器断开了");
                }
            }
        }

        public IoHandler getHandler()
        {
            return m_handle;
        }

        //设置上层处理的回调
        public void setHandler(IoHandler handler)
        {
            m_handle = handler;
        }

        public bool isActive()
        {
            return m_socket != null && m_socket.Connected;
        }

        
        #endregion

        #region IO线程相关
        void BeginIOThread()
        {
            if (m_isIOThreadActive)
            {
                EndIOThread();//尝试关掉老的io线程
            }
            m_isIOThreadActive = true;
            m_receiveThread = Util.SafeCreateThread(ReceiveThread);
            m_sendThread = Util.SafeCreateThread(SendThread);
            m_receiveThread.Start();
            m_sendThread.Start();

            //Debuger.Log("io线程开始 receiveThread:{0} receiveMessageThread:{1} ", m_receiveThread.ManagedThreadId, m_sendThread.ManagedThreadId);
        }

        void EndIOThread()
        {
            m_isIOThreadActive =false;
            Util.SafeAbortThread(m_receiveThread);
            Util.SafeAbortThread(m_sendThread);
            m_receiveThread = null;
            m_sendThread = null;

            //未处理的消息放回池里
            //这里没用锁，因为其它线程都结束了
            //Message.put(m_recvMessageList);   //已接收的消息还可以继续处理，不用归还
            if (m_needAutoRelogin)
            {
                //注意顺序
                m_pendingMsgList.AddRange(m_sendTempMsgList);
                m_pendingMsgList.AddRange(m_sendMessageList);
                m_sendTempMsgList.Clear();
                m_sendMessageList.Clear();
            }
            else
            {
                Message.put(m_sendTempMsgList);
                Message.put(m_sendMessageList);
            }
        }

        #endregion

        #region Private Methods
        
        //只能在主线程中调用，有时候io线程会报错，那么要先通过TimeMgr.AddTimer在下一帧在主线程中调用
        void OnError(object param)
        {
            object[] pp = (object[])param;
            SocketError errorCode = (SocketError)pp[0];
            string info = (string)pp[1];
            Debuger.LogError("网络连接异常，错误码：{0}，错误描述：{1}，当前状态:{2}", errorCode, info, m_state);
            if (m_state != enConnectorState.Connecting && m_state != enConnectorState.Connected)
                return;
            dispose();
            if (m_handle != null)
                m_handle.OnConnError(errorCode, info);
        }

        void OnTimeOut()
        {
            if (m_state != enConnectorState.Connecting)
            {
                Debuger.LogError("OnTimeOut() 状态异常.当前状态:{0}", m_state);
                return;
            }

            dispose();
            if (m_handle != null)
                m_handle.OnConnectFail();
        }

        //注意这个不是在主线程中被调用的
        void OnConnect(IAsyncResult result)
        {
            lock(m_lock)
            {
                if (m_timeoutTimers != null)
                {
                    m_timeoutTimers.Release();
                    m_timeoutTimers = null;
                }
            }
            
            Socket socket = (Socket)result.AsyncState;

            // Windows handles async sockets differently than other platforms, it seems.
            // If a socket is closed, OnConnectResult() is never called on Windows.
            // On the mac it does get called, however, and if the socket is used here
            // then a null exception gets thrown because the socket is not usable by this point.
            if (socket == null)
            {
                Util.SafeLogError("sock == null");
                return;
            }

            if (m_socket == null || (socket != m_socket))
            {
                Util.SafeLogError("mSocket == null || (sock != mSocket)");
                return;
            }

            try
            {
                m_socket.EndConnect(result);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Util.SafeLogError("EndConnect fail {0} {1}", ex.SocketErrorCode, ex.Message);
                Util.SafeDo(OnTimeOut);
                
                return;
            }
            //主线中再进入状态切换
            Util.SafeDo(OnBegin);

        }
        // 进入连接态，创建io线程收发数据
        void OnBegin()
        {
            if (m_state != enConnectorState.Connecting)
            {
                Debuger.LogError("Begin()状态异常.当前状态:{0}", m_state);
                return;
            }
            m_state = enConnectorState.Connected;
            m_socket.SendBufferSize = 2048;

            //开启主线程中收包用的定时器
            m_recvTimers = TimeMgr.instance.AddTimer(0, OnUpdate, -1, -1);

            //开启io线程
            BeginIOThread();

            //通知上层逻辑
            if (m_handle != null)
                m_handle.OnConnectOK();
        }

        //监听收到包
        void OnUpdate()
        {
#if UNITY_EDITOR
            //编译中的话，先把io线程关了
            if (UnityEditor.EditorApplication.isCompiling)
                EndIOThread();
#endif
            if (m_recvMessageList.Count == 0)
                return;

            lock(m_recvMessageList)
            {                
                m_procTempMsgList.AddRange(m_recvMessageList);    
                m_recvMessageList.Clear();
            }

            for(int i=0;i < m_procTempMsgList.Count; ++i)
            {
                if (m_handle != null)
                    m_handle.MessageReceived(m_procTempMsgList[i]);
            }
            m_procTempMsgList.Clear();
        }

        //用于其他线程出错时dispose主线程
        void AddError(SocketError errorCode, string info)
        {
            lock (m_lock)
            {
                m_errorTimers.Add(TimeMgr.instance.AddTimer(0f, OnError, new object[] { errorCode, info }));//主线程下个update中调用
            }
        }
        #endregion

        //模块和命令见MODULE类
        public bool Send(byte module, int command, object obj)
        {
            Message message = m_handle.MessageSend(module, command, obj);
            if (message == null)
                return false;

            if (m_state != enConnectorState.Connected)
            {
                //Debuger.LogError("发送不了 socket可能断开了，或者还没有连上");
                //没网也没关系，放到待发送队列
                m_pendingMsgList.Add(message);
            }
            else
            {
                lock (m_sendMessageList)
                {
                    m_sendMessageList.Add(message);
                }
            }
            return true;
        }

        public bool SendJsonString(byte module, int command, string jsonStr)
        {
            Message message = m_handle.MessageSendJson(module, command, jsonStr);
            if (message == null)
                return false;

            if (m_state != enConnectorState.Connected)
            {
                //没网也没关系，放到待发送队列
                m_pendingMsgList.Add(message);
            }
            else
            {
                lock (m_sendMessageList)
                {
                    m_sendMessageList.Add(message);
                }
            }
            return true;
        }

        public void SendPendingMsgList()
        {
            lock (m_sendMessageList)
            {
                m_sendMessageList.AddRange(m_pendingMsgList);
            }
        }

        public void ClearPendingMsgList()
        {
            Message.put(m_pendingMsgList);
        }
    }
}
