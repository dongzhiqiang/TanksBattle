#region Header
/**
 * 名称：接收数据线程
 
 * 日期：2015.11.3
 * 描述：
 *      负责拆包然后ReceiveMessageThread()才能收包
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
    
    public partial class Connector : IoConnector
    {
        #region Fields
        bool m_inReceiving = false;
        int m_waitingLength = 0;
        #endregion

        void ReceiveThread()
        {
            IoBuffer temBuffer = new IoBuffer(ONCE_SIZE * 2);

            while (m_isIOThreadActive)
            {
                //如果空间不够
                if(temBuffer.WriteRemain < ONCE_SIZE)
                {
                    //如果移到太靠后的位置了，那么整体挪到开始的位置
                    if(temBuffer.ReadPos > ONCE_SIZE * 3)
                        temBuffer.MoveToFirst();
                    //否则再申请点
                    else
                        temBuffer.EnsureCapacity(ONCE_SIZE);
                }

                try
                {
                    SocketError error = SocketError.Success;
                    int length = m_socket.Receive(temBuffer.Buffer, temBuffer.WritePos, ONCE_SIZE, SocketFlags.None, out error);
                    if (error != SocketError.Success)//接收不成功的话可能因为延迟很高，暂时的处理是掉线,经常出现的话可以考虑改为重新接收
                    {
                        AddError(error, "m_socket.Receive error");
                        return;
                    }
                    if (length == 0)
                    {
                        AddError(error, "Socket shutdown gracefully");
                        return;
                    }
                    temBuffer.WritePos += length;

                    //解码
                    decode(temBuffer, m_recvMessageList);
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    AddError(e.SocketErrorCode,e.Message+"\n"+e.StackTrace);//告诉主线程，然后主线程会传给上层
                    return;
                }
                catch (Exception e)
                {
                    AddError(SocketError.SocketError, e.Message + "\n" + e.StackTrace);//告诉主线程，然后主线程会传给上层
                    return;
                }

                //如果完全读取完成了重置下
                if(temBuffer.ReadSize == 0)
                {
                    temBuffer.Reset();
                }
            }
        }

        void decode(IoBuffer inBuf, List<Message> outObjList)
        {

            while (inBuf.ReadSize > 0)
            {
                //包头
                if (!m_inReceiving)
                {
                    // 检查包头数据是否足够，这里应该一种防止包出错的机制，如果上个包出错，这里可以找到当前包的包头
                    while (true)
                    {
                        if (inBuf.ReadSize < Message.PACKAGE_PRE_LENGTH)
                            return;
                        if (inBuf.PeekInt32() == Message.PACKAGE_INDETIFIER)
                        {
                            // 已检测到数据头
                            inBuf.Skip(4);
                            break;
                        }
                        else
                        {
                            // 跳过这个字节
                            inBuf.Skip(1);
                        }
                    }

                    //找到前导字节了，取本包长度
                    var packLen = inBuf.ReadInt32();
                    //如果没有数据，那至少应该8字节，数据长度字段4字节 + 检验和4字节
                    if (packLen < 8)
                    {
                        Util.SafeLogError("收到的数据包总长小于8， 包总长：{0}", packLen);
                        continue;
                    }
                    m_waitingLength = packLen - 4;   //减去数据长度字段4字节
                    m_inReceiving = true;
                }

                //包体
                if (m_inReceiving)
                {
                    if (inBuf.ReadSize < m_waitingLength)
                        return;

                    var msgLen = m_waitingLength - 4;  //减去检验和4字节
                    m_inReceiving = false;
                    m_waitingLength = 0;

                    //获得检验和
                    var checkSum1 = inBuf.ReadInt32();
                    //计算检验和
                    var checkSum2 = Message.BPHash(inBuf.Buffer, inBuf.ReadPos, msgLen);
                    //检验和不对？
                    if (checkSum1 != checkSum2)
                    {
                        //跳过这个包
                        inBuf.Skip(msgLen);
                        Util.SafeLogError("数据包检验和不正确， checkSum1：{0}，checkSum2：{0}", checkSum1, checkSum2);
                        continue;
                    }

                    //解密数据
                    Encrypt.Tea16.DecryptInplace(inBuf.Buffer, inBuf.ReadPos, msgLen);
                    var msgObj = Message.FromIOBuffer(inBuf, msgLen);
                    //如果解析正确，加入消息列表
                    if (msgObj != null)
                    {
                        lock (outObjList)
                        {
                            outObjList.Add(msgObj);
                        }                        
                    }
                }
            }
        }

        void ResetDecode()
        {
            m_inReceiving = false;
            m_waitingLength = 0;
        }
    }
}
