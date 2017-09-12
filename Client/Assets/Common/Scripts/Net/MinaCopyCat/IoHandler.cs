#region Header
/**
 * 名称：IoHandler
 
 * 日期：2015.11.3
 * 描述：
 **/
#endregion
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

namespace NetCore
{
    public interface IoHandler
    {
        void OnConnectOK();
        void OnConnectFail();
        void OnConnError(SocketError error, string info);
        void MessageReceived(Message message);
        Message MessageSend(byte module, int command, object obj);
        Message MessageSendJson(byte module, int command, string jsonStr);
    }
}