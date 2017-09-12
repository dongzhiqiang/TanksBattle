#region Header
/**
 * 名称：mono类模板
 
 * 日期：201x.x.x
 * 描述：新建继承自mono的类的时候建议用这个模板
 **/
#endregion
//using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NetCore;

namespace TestNetCore
{
    public class TestClientSocket{
        public static IoBuffer s_stream = new IoBuffer(8 * 1024);
        //顺便把处理列表放在这里
        public static Dictionary<byte, Dictionary<byte, System.Action<IoBuffer>>> s_handles = new Dictionary<byte, Dictionary<byte, System.Action<IoBuffer>>>();

        public TestServerSocket m_serverSocket;
        public static void Add(byte msg, byte code, System.Action<IoBuffer> onRecv)
        {
            s_handles.GetNewIfNo(msg)[code] = onRecv;

        }

        //连接服务器，获取socket
        public static TestClientSocket Connect(string ip="127.0.0.1",int port = 9527) { 
            TestClientSocket clientSocket =new TestClientSocket();
            clientSocket.m_serverSocket = TestServer.instance.OnConnect(clientSocket);
            return clientSocket;
        }

        //发送消息给服务器，这里直接返回了
        public void Send(byte msg, byte code, IoBuffer stream)
        {
            // 包长度  序列号  指令 模块 数据
            //    4      1       1    1    X

            m_serverSocket.OnRecv(msg, code, stream);
        }
        
        public void OnRecv(byte msg, byte code, IoBuffer stream){
            var hs = s_handles.Get(msg);
            if(hs== null){
                Debuger.LogError("这个模块没有注册网络消息{0}",msg);
                return;
            }
            var h = hs.Get(code);
            if(h== null){
                Debuger.LogError("msg:{0} code:{1} 没有注册网络消息{0}",msg,code);
                return;
            }
            h(stream);
        }

        public IoBuffer GetStream()
        {   
            s_stream.Reset();
            return s_stream;
        }
    }

    public class TestServerSocket
    {

        //顺便把处理列表放在这里
        public static Dictionary<byte, Dictionary<byte, System.Action<TestServerSocket, IoBuffer>>> s_handles = new Dictionary<byte, Dictionary<byte, System.Action<TestServerSocket, IoBuffer>>>();
        public static IoBuffer s_roleStream = new IoBuffer(8 * 1024);
        public static IoBuffer s_outStream = new IoBuffer(8 * 1024);

        public TestClientSocket m_clientSocket;
        public TestRole m_hero = new TestRole();


        public static void Add(byte msg, byte code, System.Action<TestServerSocket, IoBuffer> onRecv)
        {
            s_handles.GetNewIfNo(msg)[code] = onRecv;
        }

        //发送消息给服务器，这里直接返回了
        public void Send(byte msg, byte code, IoBuffer stream)
        {
            if (TestMSG.TMSG_ROLE != msg || (TMSG_ROLE.SYNC != code && TMSG_ROLE.LOGIN != code))
                CheckSync();//如果角色有变化，先发角色变化

            m_clientSocket.OnRecv(msg, code, stream);
        }

        public void OnRecv(byte msg, byte code, IoBuffer stream)
        {
            s_outStream.Reset();

            var hs = s_handles.Get(msg);
            if (hs == null)
            {
                Debuger.LogError("这个模块没有注册网络消息{0}", msg);
                return;
            }
            var h = hs.Get(code);
            if (h == null)
            {
                Debuger.LogError("msg:{0} code:{1} 没有注册网络消息{0}", msg, code);
                return;
            }
            h(this, stream);
            CheckSync();//可能h(this, stream)没有发业务消息，这个时候再检查一次
        }

        public IoBuffer GetStream()
        {
            s_outStream.Reset();
            return s_outStream;
        }

        //检查角色数据变化
        public void CheckSync()
        {
            if(!m_hero.ValueChange)
                return;
            //如果角色有变化，先发送角色
            s_roleStream.Reset();
            m_hero.SerializeChange(s_roleStream);
            Send(TestMSG.TMSG_ROLE, TMSG_ROLE.SYNC, s_roleStream);
        }
    }
}